using System;
using UnityEngine;

public static class Dbg
{
    public static bool IsDebugBuild;
    
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    public static void LogColorDebug(object message, Color color)
    {
        if (IsDebugBuild)
        {
            LogColor(message, color);
        }
    }
    
    public static void LogColor(object message, Color color)
    {
        Debug.Log($"<color={ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }
    
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
    
    public static void LogException(Exception ex)
    {
        Debug.LogException(ex);
    }

    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    public static void LogMemoryLog(string str)
    {
        //Debug.Log("#### Memory  " + str);
    }
}