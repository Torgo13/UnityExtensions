using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> that invokes a callback when it is destroyed.
    /// </summary>
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.OnDestroyNotifier.html")]
    public class OnDestroyNotifier : MonoBehaviour
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/OnDestroyNotifier.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Called when this behavior is destroyed.
        /// </summary>
        public Action<OnDestroyNotifier> Destroyed { private get; set; }

        void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
        #endregion // Unity.XR.CoreUtils
    }
}