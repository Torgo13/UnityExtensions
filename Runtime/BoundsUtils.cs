using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions
{
    /// <summary>
    /// Bounds related utilities
    /// </summary>
    public static class BoundsUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/BoundsUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Get the aggregated bounds of a list of GameObjects and their children.
        /// </summary>
        /// <param name="gameObjects">The list of GameObjects.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(List<GameObject> gameObjects)
        {
            Bounds? bounds = null;
            foreach (var gameObject in gameObjects)
            {
                var goBounds = GetBounds(gameObject.transform);
                if (!bounds.HasValue)
                {
                    bounds = goBounds;
                }
                else
                {
                    goBounds.Encapsulate(bounds.Value);
                    bounds = goBounds;
                }
            }

            return bounds ?? new Bounds();
        }

        /// <summary>
        /// Get the aggregated bounds of an array of transforms and their children.
        /// </summary>
        /// <param name="transforms">The array of transforms.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(Transform[] transforms)
        {
            Bounds? bounds = null;
            foreach (var t in transforms)
            {
                var goBounds = GetBounds(t);
                if (!bounds.HasValue)
                {
                    bounds = goBounds;
                }
                else
                {
                    goBounds.Encapsulate(bounds.Value);
                    bounds = goBounds;
                }
            }
            return bounds ?? new Bounds();
        }

        /// <summary>
        /// Get the aggregated bounds of a transform and its children.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(Transform transform)
        {
            List<Renderer> k_Renderers = ListPool<Renderer>.Get();
            transform.GetComponentsInChildren(k_Renderers);
            var b = GetBounds(k_Renderers);
            ListPool<Renderer>.Release(k_Renderers);

            // As a fallback when there are no bounds, collect all transform positions
            if (b.size == Vector3.zero)
            {
                List<Transform> k_Transforms = ListPool<Transform>.Get();
                transform.GetComponentsInChildren(k_Transforms);

                if (k_Transforms.Count > 0)
                    b.center = k_Transforms[0].position;

                foreach (var t in k_Transforms)
                {
                    b.Encapsulate(t.position);
                }

                ListPool<Transform>.Release(k_Transforms);
            }

            return b;
        }

        /// <summary>
        /// Get the aggregated bounds of a list of renderers.
        /// </summary>
        /// <param name="renderers">The list of renderers.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(List<Renderer> renderers)
        {
            if (renderers.Count > 0)
            {
                var first = renderers[0];
                var b = new Bounds(first.transform.position, Vector3.zero);
                foreach (var r in renderers)
                {
                    if (r.bounds.size != Vector3.zero)
                        b.Encapsulate(r.bounds);
                }

                return b;
            }

            return default;
        }

#if INCLUDE_PHYSICS_MODULE
        /// <summary>
        /// Get the aggregated bounds of a list of colliders.
        /// </summary>
        /// <param name="colliders">The list of colliders.</param>
        /// <typeparam name="T">The type of object in the list of colliders.</typeparam>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds<T>(List<T> colliders) where T : Collider
        {
            if (colliders.Count > 0)
            {
                var first = colliders[0];
                var b = new Bounds(first.transform.position, Vector3.zero);
                foreach (var c in colliders)
                {
                    if (c.bounds.size != Vector3.zero)
                        b.Encapsulate(c.bounds);
                }

                return b;
            }

            return default;
        }
#endif

        /// <summary>
        /// Gets the bounds that encapsulate a list of points.
        /// </summary>
        /// <param name="points">The list of points to encapsulate.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(List<Vector3> points)
        {
            var bounds = default(Bounds);
            if (points.Count < 1)
                return bounds;

            var minPoint = points[0];
            var maxPoint = minPoint;
            for (var i = 1; i < points.Count; ++i)
            {
                var point = points[i];
                if (point.x < minPoint.x)
                    minPoint.x = point.x;
                if (point.y < minPoint.y)
                    minPoint.y = point.y;
                if (point.z < minPoint.z)
                    minPoint.z = point.z;
                if (point.x > maxPoint.x)
                    maxPoint.x = point.x;
                if (point.y > maxPoint.y)
                    maxPoint.y = point.y;
                if (point.z > maxPoint.z)
                    maxPoint.z = point.z;
            }

            bounds.SetMinMax(minPoint, maxPoint);
            return bounds;
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Runtime/Utils/BoundsUtils.cs
        #region Unity.HLODSystem.Utils
        public static Bounds CalcLocalBounds(Renderer renderer, Transform transform)
        {
            Bounds bounds = renderer.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Matrix4x4 matrix = transform.worldToLocalMatrix;

            List<Vector3> points = ListPool<Vector3>.Get();
            points.EnsureCapacity(8);
            points.Add(new Vector3(min.x, min.y, min.z));
            points.Add(new Vector3(max.x, min.y, min.z));
            points.Add(new Vector3(min.x, min.y, max.z));
            points.Add(new Vector3(max.x, min.y, max.z));
            points.Add(new Vector3(min.x, max.y, min.z));
            points.Add(new Vector3(max.x, max.y, min.z));
            points.Add(new Vector3(min.x, max.y, max.z));
            points.Add(new Vector3(max.x, max.y, max.z));

            for (int i = 0; i < points.Count; ++i)
            {
                points[i] = matrix.MultiplyPoint(points[i]);
            }

            Vector3 newMin = points[0];
            Vector3 newMax = points[0];

            for (int i = 1; i < points.Count; ++i)
            {
                if (newMin.x > points[i].x) newMin.x = points[i].x;
                if (newMax.x < points[i].x) newMax.x = points[i].x;
                
                if (newMin.y > points[i].y) newMin.y = points[i].y;
                if (newMax.y < points[i].y) newMax.y = points[i].y;
                
                if (newMin.z > points[i].z) newMin.z = points[i].z;
                if (newMax.z < points[i].z) newMax.z = points[i].z;
            }

            ListPool<Vector3>.Release(points);

            Bounds newBounds = new Bounds();
            newBounds.SetMinMax(newMin, newMax);
            return newBounds;
        }
        #endregion // Unity.HLODSystem.Utils
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Runtime/HLOD.cs
        #region Unity.HLODSystem
        public static Bounds GetBounds(List<MeshRenderer> renderers, Transform transform)
        {
            Bounds ret = new Bounds();
            if (renderers.Count == 0)
            {
                ret.center = Vector3.zero;
                ret.size = Vector3.zero;
                ListPool<MeshRenderer>.Release(renderers);
                return ret;
            }

            Bounds bounds = CalcLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                bounds.Encapsulate(CalcLocalBounds(renderers[i], transform));
            }

            ret.center = bounds.center;
            float max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            ret.size = new Vector3(max, max, max);  

            ListPool<MeshRenderer>.Release(renderers);
            return ret;
        }
        #endregion // Unity.HLODSystem
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Editor/SpaceManager/QuadTreeSpaceSplitter.cs
        #region Unity.HLODSystem.SpaceManager
#nullable enable
        public static Bounds? CalculateBounds(GameObject obj, Transform transform)
        {
            var renderers = ListPool<MeshRenderer>.Get();
            obj.GetComponentsInChildren(renderers);
            if (renderers.Count == 0)
            {
                ListPool<MeshRenderer>.Release(renderers);
                return null;
            }

            Bounds result = CalcLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                result.Encapsulate(CalcLocalBounds(renderers[i], transform));
            }

            ListPool<MeshRenderer>.Release(renderers);
            return result;
        }
#nullable restore
        #endregion // #region Unity.HLODSystem.SpaceManager
    }
}
