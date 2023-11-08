using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class XRDebug 
{
    public static string debugOutput;

    public static void Log(string message)
    {
        debugOutput += message + '\n';
    }
}
