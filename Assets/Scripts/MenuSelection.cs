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
        
        if (currentInputField != null && TouchScreenKeyboard.visible)
        {
            Debug.Log("keyboard" + keyboard.active);
            currentInputField.text = keyboard.text;
        }
        if (currentInputField != null && (keyboard.status == TouchScreenKeyboard.Status.Done || keyboard.status == TouchScreenKeyboard.Status.Canceled))
        {
            if (currentInputField.name == "AddressInputField")
            {
                MQTTManager.SetUiMessage("Update IP into: " + currentInputField.text);
                MQTTManager.SetBrokerAddress(currentInputField.text);
            }
            if (currentInputField.name == "PortInputField")
            {
                MQTTManager.SetUiMessage("Update port into: " + currentInputField.text);
                MQTTManager.SetBrokerPort(currentInputField.text);
            }
        }
    }

    public void KeyboardClose()
    {
        
    }
    Selectable FindClosestUIElement(Canvas canvas, Vector3 hitPoint)
    {
        Selectable[] uiElements = canvas.GetComponentsInChildren<Selectable>();
        Selectable closestUIElement = null;
        float closestDistance = float.MaxValue;

        foreach (Selectable uiElement in uiElements)
        {
            // Get the UI element's position in screen space
            Vector3 uiElementScreenPos = uiElement.transform.position;

            // Calculate the distance from the hit point to the UI element in screen space
            float distance = Vector3.Distance(hitPoint, uiElementScreenPos);

            // Update the closest UI element if this one is closer
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUIElement = uiElement;
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
                                    Debug.Log(closestUIElement.name);
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
                                            keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.NumberPad, false, false, false, false);
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
