using System.Diagnostics;
using UnityEngine;

public static class Debug
{
    [Conditional("DV_DEBUG")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("DV_DEBUG")]
    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("DV_DEBUG")]
    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError(message);
    }

    [Conditional("DV_DEBUG")]
    public static void LogFormat(string message)
    {
        UnityEngine.Debug.LogFormat(message);
    }

    [Conditional("DV_DEBUG")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f)
    {
        if(color == null) 
            color = Color.white;
        UnityEngine.Debug.DrawRay(start, dir, (Color)color, duration);
    }

    [Conditional("DV_DEBUG")]
    public static void DrawLine(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f)
    {
        if(color == null)
            color = Color.white;
        UnityEngine.Debug.DrawLine(start, dir, (Color)color, duration);
    }
}
