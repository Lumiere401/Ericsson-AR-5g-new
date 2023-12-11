using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using M2MqttUnity.Examples;
public class MenuSelection : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler
{
    // Start is called before the first frame update
    public TouchScreenKeyboard keyboard;
    public M2MqttUnityTest MQTTManager;
    private InputField currentInputField;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("state" + TouchScreenKeyboard.visible);

        if (currentInputField != null && TouchScreenKeyboard.visible)
        {
            currentInputField.text = keyboard.text;
        }
    }

    public void KeyboardClose()
    {
        MQTTManager.SetUiMessage("keyboardClose");
        string temp_str = currentInputField.text;
        if (currentInputField.name == "AddressInputField")
        {
            MQTTManager.SetUiMessage("Update IP into: " + temp_str);
            MQTTManager.ResetBrokerAddress(temp_str);
        }
        if (currentInputField.name == "PortInputField")
        {
            MQTTManager.SetUiMessage("Update port into: " + temp_str);
            MQTTManager.ResetBrokerPort(temp_str);
        }
        if (currentInputField.name == "TopicInputField")
        {
            MQTTManager.SetUiMessage("Update topic into: " + temp_str);
            MQTTManager.ResetTopic(temp_str);
        }
    }

    // Check if a point is inside the rectangle formed by the four corners of a RectTransform
    bool IsPointInsideRectangle(Vector3 point, RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Check if the point is within the rectangle by comparing its coordinates
        return point.x >= corners[0].x && point.x <= corners[2].x &&
               point.y >= corners[0].y && point.y <= corners[2].y;
    }

    Selectable FindClosestUIElement(Canvas canvas, Vector3 hitPoint)
    {
        Selectable[] uiElements = canvas.GetComponentsInChildren<Selectable>();
        Selectable closestUIElement = null;
        float closestDistance = float.MaxValue;

        foreach (Selectable uiElement in uiElements)
        {
            RectTransform rectTransform = uiElement.GetComponent<RectTransform>();

            // Check if the hit point is within the rectangle formed by the four corners
            if (IsPointInsideRectangle(hitPoint, rectTransform))
            {
                float distance = Vector2.Distance(hitPoint, rectTransform.position);

                // Update the closest UI element if this one is closer
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUIElement = uiElement;
                }
            }
        }

        return closestUIElement;
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {

    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {

    }

    void IMixedRealityPointerHandler.OnPointerDown(
         MixedRealityPointerEventData eventData)
    {
        foreach (var source in MixedRealityToolkit.InputSystem.DetectedInputSources)
        {
            // Ignore anything that is not a hand because we want articulated hands
            if (source.SourceType == Microsoft.MixedReality.Toolkit.Input.InputSourceType.Hand)
            {
                foreach (var p in source.Pointers)
                {
                    if (p is IMixedRealityNearPointer)
                    {
                        // Ignore near pointers, we only want the rays
                        continue;
                    }
                    if (p.Result != null)
                    {
                        var startPoint = p.Position;
                        var endPoint = p.Result.Details.Point;
                        var hitObject = p.Result.Details.Object;
                        if (hitObject)
                        {
                            if (hitObject.name == "TestUI")
                            {
                                GameObject TestUI = hitObject;
                                Canvas canvas = TestUI.GetComponentInChildren<Canvas>();
                                if (canvas != null)
                                {
                                    // Find the closest button to the hit point
                                    Selectable closestUIElement = FindClosestUIElement(canvas, endPoint);
                             
                                    if (closestUIElement != null && closestUIElement.interactable && closestUIElement.name != "ConsoleInputField")
                                    {
                                        // Trigger the UI element's interaction
                                        if (closestUIElement is Button)
                                        {
                                            Button button = (Button)closestUIElement;
                                            button.onClick.Invoke();
                                            Debug.Log("Button Clicked");
                                        }
                                        else if (closestUIElement is InputField)
                                        {
                                            InputField inputField = (InputField)closestUIElement;
                                            currentInputField = inputField;
                                            if (inputField.name == "TopicInputField")
                                            {
                                                keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default, false, false, false, false);
                                            }
                                            else
                                            {
                                                keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.NumberPad, false, false, false, false);
                                            }
                                            Debug.Log("InputField Clicked with text: " + inputField.text);
                                            // Trigger input field-specific actions
                                        }
                                        else if (closestUIElement is Toggle)
                                        {
                                            Toggle toggle = (Toggle)closestUIElement;
                                            toggle.isOn = !toggle.isOn; // Example toggle action
                                            Debug.Log("Toggle Clicked");

                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
    }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    { }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {

    }


}
