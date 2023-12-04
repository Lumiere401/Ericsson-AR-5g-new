using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),

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

public class FakeMQTT : MonoBehaviour
{
    public TextAsset jsonFile; // Drag and drop your JSON file in the Unity Inspector.

    private List<string> eventMessages = new List<string>();
    private Queue<BeamIndexData> beams;
    private Queue<BeamIndexData> beamsFullList;
    private BeamIndexData last_msg;

    bool coroutineRunning = false;
    public float delay = 1;

    private void Start()
    {
        beams = new Queue<BeamIndexData>();
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
                beamsFullList = new Queue<BeamIndexData>(beams);
            }
        }
        else
        {
            Debug.LogError("JSON file not found. Make sure to assign it in the Inspector.");
        }
    }

    private void TestPublish(BeamIndexData msg)
    {
        //client.Publish("beam/testing", System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg)), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

        Debug.Log("Test message published");
        //AddUiMessage("Test message published.");
    }

    protected IEnumerator PublishBeamIndexMessages()
    {
        while (true)
        {
            coroutineRunning = true;
            float slowdownMultiplier = 50f; //Right now it's very fast
            if (last_msg == null)
            {
                last_msg = beams.Dequeue();
            }
            BeamIndexData msg = beams.Dequeue();
            // calculate the delay based on the message timestamp
            TimeSpan delay = DateTime.Parse(msg.timestamp) - DateTime.Parse(last_msg.timestamp);
            Debug.Log("DELAY: " + delay);

            /*if (delay.Seconds > 0)
            {*/
            yield return new WaitForSecondsRealtime(this.delay);
            /*}*/

            last_msg = msg;
            // publish the message to an mqtt topic
            //TestPublish(msg);

            // Convert the byte array to a string message
            /*string msg = System.Text.Encoding.UTF8.GetString(message);

            // Log the raw message for debugging
            Debug.Log("Received raw message: " + msg);

            // Parse the JSON string into the BeamIndexData object
            BeamIndexData beamData = JsonUtility.FromJson<BeamIndexData>(msg);
            BeamIndexData beamData = msg.beamIndex;*/

            BeamIndexMapping beamIndexMapping = FindObjectOfType<BeamIndexMapping>();
            if (beamIndexMapping != null)
            {
                // Start smooth transition to the new beam index
                beamIndexMapping.StartSmoothTransition(msg.beamIndex);
            }
            else
            {
                // If the component isn't found, log an error message
                Debug.LogError("BeamIndexMapping component not found in the scene.");
            }

            if (beams.Count <= 1)
            {
                //Debug.Log("Breaking publishing loop");
                //break;
                beams = new Queue<BeamIndexData>(beamsFullList);
            }
        }
        coroutineRunning = false;
    }


    void Update()
    {
        if (beams.Count > 0 && !coroutineRunning)
        {
            StartCoroutine(PublishBeamIndexMessages());
        }
    }
}