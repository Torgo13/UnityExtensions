using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
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
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/04be7e86357c5f3e11726b6ac9c33bd4fe1c3040/com.unity.hlod/Runtime/Utils/BoundsUtils.cs
        #region Unity.HLODSystem.Utils
        public static Bounds CalculateLocalBounds(Renderer renderer, Transform transform)
        {
            Bounds bounds = renderer.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Matrix4x4 matrix = transform.worldToLocalMatrix;

            var points = new Unity.Collections.NativeArray<Vector3>(8, Unity.Collections.Allocator.Temp);
            points[0] = new Vector3(min.x, min.y, min.z);
            points[1] = new Vector3(max.x, min.y, min.z);
            points[2] = new Vector3(min.x, min.y, max.z);
            points[3] = new Vector3(max.x, min.y, max.z);
            points[4] = new Vector3(min.x, max.y, min.z);
            points[5] = new Vector3(max.x, max.y, min.z);
            points[6] = new Vector3(min.x, max.y, max.z);
            points[7] = new Vector3(max.x, max.y, max.z);

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = matrix.MultiplyPoint(points[i]);
            }

            Vector3 newMin = points[0];
            Vector3 newMax = points[0];

            for (int i = 1; i < points.Length; ++i)
            {
                if (newMin.x > points[i].x)
                    newMin.x = points[i].x;
                if (newMax.x < points[i].x)
                    newMax.x = points[i].x;

                if (newMin.y > points[i].y)
                    newMin.y = points[i].y;
                if (newMax.y < points[i].y)
                    newMax.y = points[i].y;

                if (newMin.z > points[i].z)
                    newMin.z = points[i].z;
                if (newMax.z < points[i].z)
                    newMax.z = points[i].z;
            }

            Bounds newBounds = new Bounds();
            newBounds.SetMinMax(newMin, newMax);
            return newBounds;
        }
        #endregion // Unity.HLODSystem.Utils
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/04be7e86357c5f3e11726b6ac9c33bd4fe1c3040/com.unity.hlod/Runtime/HLOD.cs
        #region Unity.HLODSystem
        public static Bounds GetBounds(List<Renderer> renderers, Transform transform)
        {
            Bounds ret = new Bounds();
            if (renderers.Count == 0)
            {
                ret.center = Vector3.zero;
                ret.size = Vector3.zero;
                return ret;
            }

            Bounds bounds = CalculateLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                bounds.Encapsulate(CalculateLocalBounds(renderers[i], transform));
            }

            ret.center = bounds.center;
            float max = System.Math.Max(bounds.size.x, System.Math.Max(bounds.size.y, bounds.size.z));
            ret.size = new Vector3(max, max, max);

            return ret;
        }
        #endregion // Unity.HLODSystem
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/04be7e86357c5f3e11726b6ac9c33bd4fe1c3040/com.unity.hlod/Editor/SpaceManager/QuadTreeSpaceSplitter.cs
        #region Unity.HLODSystem.SpaceManager
#nullable enable
        public static Bounds? CalculateBounds(GameObject obj, Transform transform)
        {
            using var _0 = UnityEngine.Pool.ListPool<MeshRenderer>.Get(out var renderers);
            obj.GetComponentsInChildren(renderers);
            if (renderers.Count == 0)
            {
                return null;
            }

            Bounds result = CalculateLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                result.Encapsulate(CalculateLocalBounds(renderers[i], transform));
            }

            return result;
        }
#nullable restore
        #endregion // Unity.HLODSystem.SpaceManager
    }
}
