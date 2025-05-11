using System.Collections.Generic;
using Unity.Collections;
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
                if (bounds.HasValue)
                {
                    goBounds.Encapsulate(bounds.Value);
                }

                bounds = goBounds;
            }

            return bounds ?? new Bounds();
        }
        
        /// <inheritdoc cref="GetBounds(System.Collections.Generic.List{UnityEngine.GameObject})"/>
        public static Bounds GetBounds(GameObject[] gameObjects)
        {
            Bounds? bounds = null;
            foreach (var gameObject in gameObjects)
            {
                var goBounds = GetBounds(gameObject.transform);
                if (bounds.HasValue)
                {
                    goBounds.Encapsulate(bounds.Value);
                }

                bounds = goBounds;
            }

            return bounds ?? new Bounds();
        }

        /// <summary>
        /// Get the aggregated bounds of an array of transforms and their children.
        /// </summary>
        /// <param name="transforms">The list of transforms.</param>
        /// <returns>The aggregated bounds.</returns>
        public static Bounds GetBounds(List<Transform> transforms)
        {
            Bounds? bounds = null;
            foreach (var t in transforms)
            {
                var goBounds = GetBounds(t);
                if (bounds.HasValue)
                {
                    goBounds.Encapsulate(bounds.Value);
                }

                bounds = goBounds;
            }

            return bounds ?? new Bounds();
        }
        
        /// <inheritdoc cref="GetBounds(System.Collections.Generic.List{UnityEngine.GameObject})"/>
        public static Bounds GetBounds(Transform[] transforms)
        {
            Bounds? bounds = null;
            foreach (var t in transforms)
            {
                var goBounds = GetBounds(t);
                if (bounds.HasValue)
                {
                    goBounds.Encapsulate(bounds.Value);
                }

                bounds = goBounds;
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
            List<Renderer> renderers = ListPool<Renderer>.Get();
            
            transform.GetComponentsInChildren(renderers);
            var b = GetBounds(renderers);
            
            ListPool<Renderer>.Release(renderers);

            // As a fallback when there are no bounds, collect all transform positions
            if (b.size == Vector3.zero)
            {
                var transforms = ListPool<Transform>.Get();
                transform.GetComponentsInChildren(transforms);

                if (transforms.Count > 0)
                    b.center = transforms[0].position;

                foreach (var t in transforms)
                {
                    b.Encapsulate(t.position);
                }

                ListPool<Transform>.Release(transforms);
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

            var points = new NativeArray<Vector3>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
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
                if (newMin.x > points[i].x) newMin.x = points[i].x;
                if (newMax.x < points[i].x) newMax.x = points[i].x;
                
                if (newMin.y > points[i].y) newMin.y = points[i].y;
                if (newMax.y < points[i].y) newMax.y = points[i].y;
                
                if (newMin.z > points[i].z) newMin.z = points[i].z;
                if (newMax.z < points[i].z) newMax.z = points[i].z;
            }

            points.Dispose();

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
                return ret;
            }

            Bounds bounds = CalcLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                bounds.Encapsulate(CalcLocalBounds(renderers[i], transform));
            }

            ret.center = bounds.center;
            float max = System.Math.Max(bounds.size.x, System.Math.Max(bounds.size.y, bounds.size.z));
            ret.size = new Vector3(max, max, max);  

            return ret;
        }
        #endregion // Unity.HLODSystem
        
        //https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Editor/SpaceManager/QuadTreeSpaceSplitter.cs
        #region Unity.HLODSystem.SpaceManager
        public static bool CalculateBounds(GameObject obj, Transform transform, out Bounds result)
        {
            using var _0 = ListPool<MeshRenderer>.Get(out var renderers);
            obj.GetComponentsInChildren(renderers);
            if (renderers.Count == 0)
            {
                result = default;
                return false;
            }

            result = CalcLocalBounds(renderers[0], transform);
            for (int i = 1; i < renderers.Count; ++i)
            {
                result.Encapsulate(CalcLocalBounds(renderers[i], transform));
            }

            return true;
        }
        #endregion // #region Unity.HLODSystem.SpaceManager
    }
}
