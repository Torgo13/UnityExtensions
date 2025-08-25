using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityExtensions
{
    static class Debug
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/Stubs/Unity/Debug.cs
        #region Unity.Entities
        public static ILogger unityLogger => UnityEngine.Debug.unityLogger;

        public static bool developerConsoleEnabled
        {
            get { return UnityEngine.Debug.developerConsoleEnabled; }
            set { UnityEngine.Debug.developerConsoleEnabled = value;}
        }

        public static bool developerConsoleVisible
        {
            get { return UnityEngine.Debug.developerConsoleVisible; }
            set { UnityEngine.Debug.developerConsoleVisible = value;}
        }

        public static bool isDebugBuild => UnityEngine.Debug.isDebugBuild;

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) => UnityEngine.Debug.DrawLine(start, end, color, duration);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawLine(Vector3 start, Vector3 end, Color color) => UnityEngine.Debug.DrawLine(start, end, color);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawLine(Vector3 start, Vector3 end) => UnityEngine.Debug.DrawLine(start, end);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) => UnityEngine.Debug.DrawRay(start, dir, color, duration);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawRay(Vector3 start, Vector3 dir, Color color) => UnityEngine.Debug.DrawRay(start, dir, color);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DrawRay(Vector3 start, Vector3 dir) => UnityEngine.Debug.DrawRay(start, dir);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Break() => UnityEngine.Debug.Break();

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void DebugBreak() => UnityEngine.Debug.DebugBreak();

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Log(object message) => UnityEngine.Debug.Log(message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Log(object message, UnityObject context) => UnityEngine.Debug.Log(message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogFormat(UnityObject context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogFormat(LogType logType, LogOption logOptions, UnityObject context, string format, params object[] args) => UnityEngine.Debug.LogFormat(logType, logOptions, context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogError(object message) => UnityEngine.Debug.LogError(message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogError(object message, UnityObject context) => UnityEngine.Debug.LogError(message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogErrorFormat(string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogErrorFormat(UnityObject context, string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void ClearDeveloperConsole() => UnityEngine.Debug.ClearDeveloperConsole();

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogException(Exception exception) => UnityEngine.Debug.LogException(exception);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogException(Exception exception, UnityObject context) => UnityEngine.Debug.LogException(exception, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogWarning(object message, UnityObject context) => UnityEngine.Debug.LogWarning(message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogWarningFormat(string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogWarningFormat(UnityObject context, string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition) => UnityEngine.Debug.Assert(condition);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, UnityObject context) => UnityEngine.Debug.Assert(condition, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, object message) => UnityEngine.Debug.Assert(condition, message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, string message) => UnityEngine.Debug.Assert(condition, message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, object message, UnityObject context) => UnityEngine.Debug.Assert(condition, message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, string message, UnityObject context) => UnityEngine.Debug.Assert(condition, message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void AssertFormat(bool condition, string format, params object[] args) => UnityEngine.Debug.AssertFormat(condition, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void AssertFormat(bool condition, UnityObject context, string format, params object[] args) => UnityEngine.Debug.AssertFormat(condition, context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogAssertion(object message) => UnityEngine.Debug.LogAssertion(message);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogAssertion(object message, UnityObject context) => UnityEngine.Debug.LogAssertion(message, context);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogAssertionFormat(string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void LogAssertionFormat(UnityObject context, string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(context, format, args);

#if CONDITIONAL_DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
#endif // CONDITIONAL_DEBUG
        public static void Assert(bool condition, string format, params object[] args) => UnityEngine.Debug.AssertFormat(condition, format, args);
        #endregion // Unity.Entities
    }
}
