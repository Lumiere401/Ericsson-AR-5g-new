using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugTextDisplay : MonoBehaviour
{
    TMP_Text textField;
    private void Start()
    {
        XRDebug.Log("XRDebug working");
        textField = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textField.text = XRDebug.debugOutput;
    }
}
