using System.Collections.Generic;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Extension methods for the <see cref="Bounds"/> type.
    /// </summary>
    public static class BoundsExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/BoundsExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Returns a whether the given bounds are contained completely within this one.
        /// </summary>
        /// <remarks>If a boundary value is the same for both <see cref="Bounds"/> objects,
        /// that boundary is considered to be within the <paramref name="outerBounds"/>.</remarks>
        /// <param name="outerBounds">The outer bounds which may contain the inner bounds.</param>
        /// <param name="innerBounds">The inner bounds that may or may not fit within outerBounds.</param>
        /// <returns><see langword="true"/> if outerBounds completely encloses innerBounds.</returns>
        public static bool ContainsCompletely(this Bounds outerBounds, Bounds innerBounds)
        {
            var outerBoundsMax = outerBounds.max;
            var outerBoundsMin = outerBounds.min;
            var innerBoundsMax = innerBounds.max;
            var innerBoundsMin = innerBounds.min;
            return outerBoundsMax.x >= innerBoundsMax.x && outerBoundsMax.y >= innerBoundsMax.y && outerBoundsMax.z >= innerBoundsMax.z
                && outerBoundsMin.x <= innerBoundsMin.x && outerBoundsMin.y <= innerBoundsMin.y && outerBoundsMin.z <= innerBoundsMin.z;
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/com.unity.demoteam.hair/blob/75a7f446209896bc1bce0da2682cfdbdf30ce447/Runtime/Utility/Extensions.cs
        #region Unity.DemoTeam.Hair
        public static Bounds WithPadding(this Bounds bounds, float padding)
        {
            return new Bounds(bounds.center, bounds.size + new Vector3(2.0f * padding, 2.0f * padding, 2.0f * padding));
        }

        public static Bounds WithScale(this Bounds bounds, float scale)
        {
            return new Bounds(bounds.center, bounds.size * scale);
        }

        public static Bounds ToCube(this Bounds bounds)
        {
            return new Bounds(bounds.center, bounds.size.Abs().MaxComponent() * Vector3.one);
        }
        #endregion // Unity.DemoTeam.Hair
    }
}
