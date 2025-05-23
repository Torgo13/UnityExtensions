using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// This class contains useful extension methods for the Animator component.
    /// </summary>
    public static class AnimatorExtensions
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/AnimatorExtensions.cs
        #region Unity.LiveCapture
        /// <summary>
        /// Attempts to calculate the path from the specified Animator to the specified Transform.
        /// </summary>
        /// <param name="animator">The Animator to calculate the path from.</param>
        /// <param name="transform">The Transform to calculate the path to.</param>
        /// <param name="path">The calculated path.</param>
        /// <returns>True if the path is valid; false otherwise.</returns>
        public static bool TryGetAnimationPath(this Animator animator, Transform transform, out string path)
        {
            path = string.Empty;

            if (animator == null
                || transform == null
                || transform.root != animator.transform.root)
                return false;

            var names = new List<string>();
            var root = animator.transform;

            while (transform != null && transform != root)
            {
                names.Add(transform.name);
                transform = transform.parent;
            }

            if (transform == root)
            {
                path = string.Join('/', names.Reverse<string>());
                return true;
            }

            return false;
        }
        #endregion // Unity.LiveCapture
    }
}
