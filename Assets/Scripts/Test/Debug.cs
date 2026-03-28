using System.Diagnostics;
using UnityEngine;

namespace CustomTIJI
{
    public static class Debug
    {
        const string SYMBOL = "TEST_DEBUG";

        [Conditional(SYMBOL)]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional(SYMBOL)]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional(SYMBOL)]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional(SYMBOL)]
        public static void LogFormat(string message)
        {
            UnityEngine.Debug.LogFormat(message);
        }

        [Conditional(SYMBOL)]
        public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f)
        {
            if (color == null)
                color = Color.white;
            UnityEngine.Debug.DrawRay(start, dir, (Color)color, duration);
        }

        [Conditional(SYMBOL)]
        public static void DrawLine(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f)
        {
            if (color == null)
                color = Color.white;
            UnityEngine.Debug.DrawLine(start, dir, (Color)color, duration);
        }
    }
}