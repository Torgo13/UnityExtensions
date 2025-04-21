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
        const string AssemblyName = "Unity.Timeline.Editor";
        const string TypeName = "UnityEditor.Timeline.UndoExtensions+DisableTimelineUndoScope";
        static readonly Type ScopeType;

        static TimelineDisableUndoScope170()
        {
            ScopeType = Type.GetType($"{TypeName}, {AssemblyName}");
        }

        public static IDisposable Create()
        {
            return Activator.CreateInstance(ScopeType) as IDisposable;
        }
        #endregion // Unity.LiveCapture
    }
#else
    public class TimelineDisableUndoScopeLegacy : IDisposable
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/TimelineDisableUndoScope.cs
        #region Unity.LiveCapture
        const string TypeStr = "UnityEngine.Timeline.TimelineUndo+DisableUndoGuard";
        const string FieldStr = "enableUndo";
        static readonly FieldInfo EnableUndo = ReflectionUtils.GetCachedAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => string.Equals(t.FullName, TypeStr))
            ?.GetField(FieldStr, BindingFlags.NonPublic | BindingFlags.Static);

        bool _disposed;

        static void SetUndoEnabled(bool value)
        {
            EnableUndo.SetValue(null, value);
        }

        public TimelineDisableUndoScopeLegacy()
        {
            SetUndoEnabled(false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TimelineDisableUndoScopeLegacy));
            }

            SetUndoEnabled(true);
            _disposed = true;
        }
        #endregion // Unity.LiveCapture
    }
#endif
}
