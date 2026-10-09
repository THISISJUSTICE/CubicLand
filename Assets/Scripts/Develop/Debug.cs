using System.Diagnostics;
using UnityEngine;

namespace Commar
{
    public static class Debug
    {
        private const string SYMBOL = "TEST_DEBUG";

        [Conditional(SYMBOL)]
        public static void Log(string message, Object context = null)
        {
            if (context != null)
                UnityEngine.Debug.Log(message, context);
            else
                UnityEngine.Debug.Log(message);
        }

        [Conditional(SYMBOL)]
        public static void LogWarning(string message, Object context = null)
        {
            if (context != null)
                UnityEngine.Debug.LogWarning(message, context);
            else
                UnityEngine.Debug.LogWarning(message);
        }

        [Conditional(SYMBOL)]
        public static void LogError(string message, Object context = null)
        {
            if (context != null)
                UnityEngine.Debug.LogError(message, context);
            else
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