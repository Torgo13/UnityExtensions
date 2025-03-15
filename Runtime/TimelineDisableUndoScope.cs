using System;
using System.Reflection;
using System.Linq;

namespace UnityExtensions
{
    public static class TimelineDisableUndoScope
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/TimelineDisableUndoScope.cs
        #region Unity.LiveCapture
        public static IDisposable Create()
        {
#if TIMELINE_1_7_0_OR_NEWER
            return TimelineDisableUndoScope170.Create();
#else
            return new TimelineDisableUndoScopeLegacy();
#endif
        }
        #endregion // Unity.LiveCapture
    }

#if TIMELINE_1_7_0_OR_NEWER
    public static class TimelineDisableUndoScope170
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/TimelineDisableUndoScope.cs
        #region Unity.LiveCapture
        const string s_AssemblyName = "Unity.Timeline.Editor";
        const string s_TypeName = "UnityEditor.Timeline.UndoExtensions+DisableTimelineUndoScope";
        static Type s_ScopeType;

        static TimelineDisableUndoScope170()
        {
            s_ScopeType = Type.GetType($"{s_TypeName}, {s_AssemblyName}");
        }

        static public IDisposable Create()
        {
            return Activator.CreateInstance(s_ScopeType) as IDisposable;
        }
        #endregion // Unity.LiveCapture
    }
#else
    public class TimelineDisableUndoScopeLegacy : IDisposable
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/TimelineDisableUndoScope.cs
        #region Unity.LiveCapture
        const string s_TypeStr = "UnityEngine.Timeline.TimelineUndo+DisableUndoGuard";
        const string s_FieldStr = "enableUndo";
        static FieldInfo s_EnableUndo = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => string.Equals(t.FullName, s_TypeStr))
            ?.GetField(s_FieldStr, BindingFlags.NonPublic | BindingFlags.Static);

        bool m_Disposed;

        static void SetUndoEnabled(bool value)
        {
            s_EnableUndo.SetValue(null, value);
        }

        public TimelineDisableUndoScopeLegacy()
        {
            SetUndoEnabled(false);
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(TimelineDisableUndoScopeLegacy));
            }

            SetUndoEnabled(true);

            m_Disposed = true;
        }
        #endregion // Unity.LiveCapture
    }
#endif
}
