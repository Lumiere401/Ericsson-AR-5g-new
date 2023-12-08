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
        public bool LocalTesting = false;
        public TextAsset jsonFile; // Drag and drop your JSON file in the Unity Inspector.
        [Header("User Interface")]
        public InputField consoleInputField;
        public Toggle encryptedToggle;
        public InputField addressInputField;
        public InputField portInputField;
        public Button connectButton;
        public Button disconnectButton;
        public Button testPublishButton;
        public Button clearButton;
        public float speed;

        private List<string> eventMessages = new List<string>();
        private Queue<BeamIndexData> beams;
        private BeamIndexData last_msg;
        private bool updateUI = false;

        protected override void Awake()
        {
            if (LocalTesting)
            {
                this.topicSub = "EricssonONE/egarage/gnodeb/beamtracking";
                this.brokerAddress = "129.192.82.202";
                this.mqttUserName = "ldt220503";
                this.mqttPassword = "ldt220503";
            }
            base.Awake(); // Call the base class's Awake method
                          // Additional initialization specific to ChildClass
        }

        protected override void Start()
        {
            SetUiMessage("Ready.");
            beams = new Queue<BeamIndexData>();
            if (LocalTesting)
            {
                base.Start();
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
            else
            {
                base.Start();
            }
            updateUI = true;
        }

        private void TestPublish(BeamIndexData msg)
        {
            client.Publish(this.topicSub, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg)), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Debug.Log("Test message published");
            //AddUiMessage("Test message published.");
        }

        public void SetBrokerAddress(string brokerAddress)
        {
            if (addressInputField && !updateUI)
            {
                this.brokerAddress = brokerAddress;
            }
        }

        public void SetBrokerPort(string brokerPort)
        {
            if (portInputField && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
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
            TestPublish(msg);
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
                addressInputField.text = brokerAddress;
            }
            if (portInputField != null && connectButton != null)
            {
                portInputField.interactable = connectButton.interactable;
                portInputField.text = brokerPort.ToString();
            }
            if (encryptedToggle != null && connectButton != null)
            {
                encryptedToggle.interactable = connectButton.interactable;
                encryptedToggle.isOn = isEncrypted;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }


        // protected override void DecodeMessage(string topic, byte[] message)
        // {
        //     string msg = System.Text.Encoding.UTF8.GetString(message);
        //     Debug.Log("Received: " + msg);
        //     StoreMessage(msg);
        //     if (topic == "M2MQTT_Unity/test")
        //     {
        //         if (autoTest)
        //         {
        //             autoTest = false;
        //             Disconnect();
        //         }
        //     }
        // }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            // Convert the byte array to a string message
            string msg = System.Text.Encoding.UTF8.GetString(message);

            // Log the raw message for debugging
            Debug.Log("Received raw message: " + msg);
            SetUiMessage("Received raw message: " + msg);
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
            if (LocalTesting && client != null && beams.Count > 0)
            {
                StartCoroutine(PublishBeamIndexMessages());
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