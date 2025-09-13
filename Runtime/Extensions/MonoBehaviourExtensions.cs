using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="MonoBehaviour"/> objects.
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/MonoBehaviourExtensions.cs
        #region Unity.XR.CoreUtils
#if UNITY_EDITOR
        /// <summary>
        /// Starts running this <see cref="MonoBehaviour"/> while in edit mode.
        /// </summary>
        /// <remarks>
        /// This function sets <see cref="MonoBehaviour.runInEditMode"/> to <see langword="true"/>, which, if the behaviour is
        /// currently enabled, calls <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">OnDisable</see>
        /// and then <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">OnEnable</see>.
        /// </remarks>
        /// <param name="behaviour">The behaviour</param>
        public static void StartRunInEditMode(this MonoBehaviour behaviour)
        {
            behaviour.runInEditMode = true;
        }

        /// <summary>
        /// Stops this <see cref="MonoBehaviour"/> from running in edit mode.
        /// </summary>
        /// <remarks>
        /// If this <see cref="MonoBehaviour"/> is currently enabled, this function disables it,
        /// sets <see cref="MonoBehaviour.runInEditMode"/> to <see langword="false"/>, and the re-enables it.
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">OnDisable</see> and
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">OnEnable</see> are called.
        /// If this <see cref="MonoBehaviour"/> is currently disabled, this function only sets
        /// <see cref="MonoBehaviour.runInEditMode"/> to <see langword="false"/>.
        /// </remarks>
        /// <param name="behaviour">The behaviour</param>
        public static void StopRunInEditMode(this MonoBehaviour behaviour)
        {
            var wasEnabled = behaviour.enabled;
            if (wasEnabled)
                behaviour.enabled = false;

            behaviour.runInEditMode = false;

            if (wasEnabled)
                behaviour.enabled = true;
        }
#endif
        #endregion // Unity.XR.CoreUtils
    }
}
