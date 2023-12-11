/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{
    // Beam json serialization struture
    [System.Serializable]
    public class BeamIndexData
    {
        public String timestamp;
        public int beamIndex;
        public int sinr;
        BeamIndexData(String t, int index, int s)
        {
            this.timestamp = t;
            this.beamIndex = index;
            this.sinr = s;
        }
    }
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttUnityTest : M2MqttUnityClient
    {
        [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
        public bool autoTest = false;
        [Tooltip("Topic name that broker will subscribe")]
        public String topicSub = null;
        [Tooltip("If true, using the local data and publish on localhost")]
        public bool isLocalTesting = true;
        public TextAsset jsonFile; // Drag and drop your JSON file in the Unity Inspector.
        [Header("User Interface")]
        public InputField consoleInputField;
        public Toggle encryptedToggle;
        public Toggle localTestingToggle;
        public InputField addressInputField;
        public InputField portInputField;
        public InputField topicInputField;
        public Button connectButton;
        public Button disconnectButton;
        public Button testPublishButton;
        public Button clearButton;
        public float speed;

        private List<string> eventMessages = new List<string>();
        private Queue<BeamIndexData> beams;
        private BeamIndexData last_msg;
        private bool updateUI = false;
        private string DefaultUserName = "ldt220503";
        private string DefaultPassword = "ldt220503";
        private string DefaultTopic = "EricssonONE/egarage/gnodeb/beamtracking";
        private string DefaultAddress = "129.192.82.202";

        protected override void Awake()
        {
            if (isLocalTesting)
            {
                this.topicSub = DefaultTopic;
                this.brokerAddress = DefaultAddress;
                this.mqttUserName = DefaultUserName;
                this.mqttPassword = DefaultPassword;
            }
            base.Awake(); // Call the base class's Awake method
                          // Additional initialization specific to ChildClass
        }

        protected override void Start()
        {
            SetUiMessage("Ready.");
            beams = new Queue<BeamIndexData>();
            base.Start();
            LoadBeamData();
            updateUI = true;
        }

        public void TestPublish()
        {
            client.Publish(this.topicSub, System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Debug.Log("Test message published");
            AddUiMessage("Test message published.");
        }

        public void SetBrokerAddress(string brokerAddress)
        {
            if (addressInputField && !updateUI)
            {
                this.brokerAddress = brokerAddress;
            }
        }

        public void SetTopic(string topicName)
        {
            if (topicInputField && !updateUI)
            {
                this.topicSub = topicName;
            }
        }

        public void SetBrokerPort(string brokerPort)
        {
            if (portInputField && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
            }
        }

        public void ResetBrokerAddress(string brokerAddress)
        {
            if (addressInputField)
            {
                this.brokerAddress = brokerAddress;
            }
        }

        public void ResetTopic(string topicName)
        {
            if (topicInputField)
            {
                this.topicSub = topicName;
            }
        }

        public void ResetBrokerPort(string brokerPort)
        {
            if (portInputField)
            {
                int.TryParse(brokerPort, out this.brokerPort);
            }
        }

        public void SetLocalTesting(bool isLocalTesting)
        {
            this.isLocalTesting = isLocalTesting;
            if (isLocalTesting == false)
            {
                // Everytime when uncheck the localtesting, reload the local beamindex data
                beams.Clear();
                LoadBeamData();
            }
        }

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }


        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }


        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";

                updateUI = true;
            }
            else
            {
                Debug.Log(msg);
            }
        }

        private void LoadBeamData()
        {
            // Local pre-record beamIndex data
            if (jsonFile != null)
            {
                string jsonText = jsonFile.text;
                // You can now parse the JSON data using a JSON parser, e.g., JSONUtility or a third-party library like Newtonsoft.Json.
                // Example using JSONUtility:
                using (StringReader reader = new StringReader(jsonText))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        BeamIndexData data = JsonUtility.FromJson<BeamIndexData>(line);
                        beams.Enqueue(data);
                    }
                }
                Debug.Log("beam data loaded");
            }
            else
            {
                Debug.LogError("JSON file not found. Make sure to assign it in the Inspector.");
            }
        }

        protected IEnumerator PublishBeamIndexMessages()
        {
            if (last_msg == null)
            {
                last_msg = beams.Dequeue();
            }
            BeamIndexData msg = beams.Dequeue();
            // calculate the delay based on the message timestamp
            TimeSpan delay = DateTime.Parse(msg.timestamp) - DateTime.Parse(last_msg.timestamp);

            if (delay.TotalSeconds > 0)
            {
                yield return new WaitForSecondsRealtime((float)delay.TotalSeconds * 10.0f);
            }
            last_msg = msg;
            // publish the message to an mqtt topic
            if (client != null)
            {
                client.Publish(this.topicSub, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg)), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");

        }

        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { topicSub }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { topicSub });
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
            if (client == null)
            {
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = client.IsConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !client.IsConnected;
                }
            }
            if (addressInputField != null && connectButton != null)
            {
                addressInputField.interactable = connectButton.interactable;
                addressInputField.text = base.brokerAddress;
            }
            if (portInputField != null && connectButton != null)
            {
                portInputField.interactable = connectButton.interactable;
                portInputField.text = base.
                    brokerPort.ToString();
            }
            if (topicInputField != null && connectButton != null)
            {
                topicInputField.interactable = connectButton.interactable;
                topicInputField.text = topicSub;
                topicInputField.textComponent.fontSize = 23;
            }
            if (encryptedToggle != null && connectButton != null)
            {
                encryptedToggle.interactable = connectButton.interactable;
                encryptedToggle.isOn = isEncrypted;
            }
            if (localTestingToggle != null && connectButton != null)
            {
                localTestingToggle.interactable = connectButton.interactable;
                localTestingToggle.isOn = isLocalTesting;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // Convert the byte array to a string message
            string msg = System.Text.Encoding.UTF8.GetString(message);

            // Log the raw message for debugging
            Debug.Log("Received raw message: " + msg + "from topic" + topic);
            SetUiMessage("Received raw message: " + msg + "from topic" + topic);

            // Check if the msg is about beam index
            if (msg.Contains("beamIndex"))
            {
                // Parse the JSON string into the BeamIndexData object
                BeamIndexData beamData = JsonUtility.FromJson<BeamIndexData>(msg);

                // Log the parsed beamIndex for debugging
                Debug.Log("Parsed beamIndex: " + beamData.beamIndex);
                XRDebug.Log(msg);
                // Find the BeamIndexMapping component in the scene
                BeamIndexMapping beamIndexMapping = FindObjectOfType<BeamIndexMapping>();
                if (beamIndexMapping != null)
                {
                    // Update the beam index using the parsed data
                    beamIndexMapping.StartSmoothTransition(beamData.beamIndex);
                }
                else
                {
                    // If the component isn't found, log an error message
                    Debug.LogError("BeamIndexMapping component not found in the scene.");

                }
            }
            
        }






        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()
            Debug.Log("islocaltesting" + isLocalTesting);
            Debug.Log("currentTopic" + topicSub);
            if (isLocalTesting && client != null && beams.Count > 0)
            {
                StartCoroutine(PublishBeamIndexMessages());
            }
            if (!isLocalTesting)
            {
            }
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            if (autoTest)
            {
                autoConnect = true;
            }
        }
    }

}