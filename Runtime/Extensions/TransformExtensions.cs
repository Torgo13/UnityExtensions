using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for <see cref="Transform"/> components.
    /// </summary>
    public static class TransformExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/TransformExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Gets the local position and rotation as a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform from which to get the pose.</param>
        /// <returns>The local pose.</returns>
        public static Pose GetLocalPose(this Transform transform)
        {
#if HAS_GET_POSITION_AND_ROTATION
            transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            return new Pose(localPosition, localRotation);
#else
            return new Pose(transform.localPosition, transform.localRotation);
#endif
        }

        /// <summary>
        /// Gets the world position and rotation as a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform from which to get the pose.</param>
        /// <returns>The world pose.</returns>
        public static Pose GetWorldPose(this Transform transform)
        {
#if HAS_GET_POSITION_AND_ROTATION
            transform.GetPositionAndRotation(out var position, out var rotation);
            return new Pose(position, rotation);
#else
            return new Pose(transform.position, transform.rotation);
#endif
        }

        /// <summary>
        /// Sets the local position and rotation from a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform on which to set the pose.</param>
        /// <param name="pose">Pose specifying the new position and rotation.</param>
        public static void SetLocalPose(this Transform transform, Pose pose)
        {
#if HAS_SET_LOCAL_POSITION_AND_ROTATION
            transform.SetLocalPositionAndRotation(pose.position, pose.rotation);
#else
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
#endif
        }

        /// <summary>
        /// Sets the world position and rotation from a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform on which to set the pose.</param>
        /// <param name="pose">Pose specifying the new position and rotation.</param>
        public static void SetWorldPose(this Transform transform, Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="pose">The <c>Pose</c> to transform.</param>
        /// <returns>A new <c>Pose</c> representing the transformed <paramref name="pose"/>.</returns>
        public static Pose TransformPose(this Transform transform, Pose pose)
        {
            return pose.GetTransformedBy(transform);
        }

        /// <summary>
        /// Inverse transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="pose">The <c>Pose</c> to inversely transform.</param>
        /// <returns>A new <c>Pose</c> representing the inversely transformed <paramref name="pose"/>.</returns>
        /// <exception cref="System.ArgumentNullException">transform</exception>
        public static Pose InverseTransformPose(this Transform transform, Pose pose)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Pose
            {
                position = transform.InverseTransformPoint(pose.position),
                rotation = Quaternion.Inverse(transform.rotation) * pose.rotation
            };
        }

        /// <summary>
        /// Inverse transforms a <see cref="Ray"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="ray">The <c>Ray</c> to inversely transform.</param>
        /// <returns>A new <c>Ray</c> representing the inversely transformed <paramref name="ray"/>.</returns>
        /// <exception cref="System.ArgumentNullException">transform</exception>
        public static Ray InverseTransformRay(this Transform transform, Ray ray)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Ray(
                transform.InverseTransformPoint(ray.origin),
                transform.InverseTransformDirection(ray.direction));
        }
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Transforms a <see cref="Ray"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="ray">The <c>Ray</c> to transform.</param>
        /// <returns>A new <c>Ray</c> representing the transformed <paramref name="ray"/>.</returns>
        public static Ray TransformRay(this Transform transform, Ray ray)
        {
            return new Ray(
                transform.TransformPoint(ray.origin),
                transform.TransformDirection(ray.direction));
        }
        
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        public static void ConservativeSetPositionAndRotation(this Transform t, Vector3 pos, Quaternion rot)
        {
            // Avoid precision creep
            t.GetPositionAndRotation(out var position, out var rotation);
            if (position.Equals(pos) && rotation.Equals(rot))
                return;

#if UNITY_EDITOR
            // Avoid dirtying the scene with insignificant diffs
            if (Application.isPlaying)
            {
                t.SetPositionAndRotation(pos, rot);
            }
            else
            {
                // Work in local space to reduce precision mismatches
                var parent = t.parent;
                if (parent != null)
                {
                    pos = parent.InverseTransformPoint(pos);
                    rot = Quaternion.Inverse(parent.rotation) * rot;
                }

                const float tolerance = 0.0001f;
                t.GetLocalPositionAndRotation(out var p, out var r);

                if (Mathf.Abs(p.x - pos.x) < tolerance
                    && Mathf.Abs(p.y - pos.y) < tolerance
                    && Mathf.Abs(p.z - pos.z) < tolerance)
                    pos = p;

                if (Mathf.Abs(r.x - rot.x) < tolerance
                    && Mathf.Abs(r.y - rot.y) < tolerance
                    && Mathf.Abs(r.z - rot.z) < tolerance
                    && Mathf.Abs(r.w - rot.w) < tolerance)
                    rot = r;

                t.SetLocalPositionAndRotation(pos, rot);
            }
#else
            t.SetPositionAndRotation(pos, rot);
#endif
        }
        #endregion // Unity.Cinemachine
        
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/TransformExtensions.cs
        #region Unity.FilmInternalUtilities
        public static Transform FindOrCreateChild(this Transform t, string childName, bool worldPositionStays = true)
        {
            Transform childT = t.Find(childName);
            if (null != childT)
                return childT;

            GameObject go = new GameObject(childName);
            childT = go.transform;
            childT.SetParent(t, worldPositionStays);
            return childT;
        }

        public static int FindAllDescendants(this Transform t, List<Transform> descendants)
        {
            int childCount = 0;
            using var _0 = UnityEngine.Pool.ListPool<Transform>.Get(out var childEnumerator);

            int numChildren = t.childCount;
            for (int i = 0; i < numChildren; ++i)
            {
                Transform child = t.GetChild(i);
                descendants.Add(child);

                childEnumerator.Clear();
                childCount += 1 + FindAllDescendants(child, childEnumerator);
                descendants.AddRange(childEnumerator);
            }

            return childCount;
        }

        public static int FindAllActiveDescendants(this Transform t, List<Transform> activeDescendants)
        {
            int activeChildCount = 0;
            using var _0 = UnityEngine.Pool.ListPool<Transform>.Get(out var childEnumerator);

            int numChildren = t.childCount;
            for (int i = 0; i < numChildren; ++i)
            {
                Transform child = t.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    activeDescendants.Add(child);
                    ++activeChildCount;
                }

                childEnumerator.Clear();
                FindAllActiveDescendants(child, childEnumerator);
                foreach (var descendant in childEnumerator)
                {
                    if (descendant.gameObject.activeInHierarchy)
                    {
                        activeDescendants.Add(descendant);
                        ++activeChildCount;
                    }
                }
            }

            return activeChildCount;
        }
        #endregion // Unity.FilmInternalUtilities
        
        public static int GetChildCount(this Transform t)
        {
            using var _0 = ListPool<Transform>.Get(out var children);
            t.FindAllDescendants(children);
            return children.Count;
        }

        public static int GetActiveChildCount(this Transform t)
        {
            using var _0 = ListPool<Transform>.Get(out var children);
            t.FindAllActiveDescendants(children);
            return children.Count;
        }
        
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/3d486c22f22d65278f910f0835128afdb8f2a36e/com.unity.formats.alembic/Runtime/Scripts/Exporter/Utils.cs
        #region UnityEngine.Formats.Alembic.Util
        public static Matrix4x4 WorldNoScale(this Transform transform)
        {
            transform.GetPositionAndRotation(out var pos, out var rotation);
            var rot = Matrix4x4.Rotate(rotation);
            rot = rot.transpose;
            var t = rot.MultiplyPoint(-pos);
            return Matrix4x4.TRS(t, Quaternion.Inverse(rotation), Vector3.one);
        }
        #endregion // UnityEngine.Formats.Alembic.Util
        
        //https://github.com/Unity-Technologies/game-programming-patterns-demo/blob/b2b309abf65c59fd53f09a4a391396c592c99c7d/Assets/UnityTechnologies/Scripts/Utilities/ExtensionMethods.cs
        #region DesignPatterns.Utilities
        public static void ResetTransformation(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        #endregion // DesignPatterns.Utilities
        
        /// <summary>
        ///   <para>The non-generic, non-allocating version of <see cref="Component.GetComponentsInChildren(Type, bool)"/>.</para>
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="type">The type of component to search for.</param>
        /// <param name="includeInactive">Whether to include inactive child GameObjects in the search.</param>
        /// <param name="results">A list of all found components matching the specified type.</param>
        public static void GetComponentsInChildren(this Transform transform, System.Type type, List<Component> results,
            bool includeInactive = false)
        {
            if ((includeInactive || transform.gameObject.activeInHierarchy)
                && transform.TryGetComponent(type, out var root))
            {
                results.Add(root);
            }

            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
            {
                transform.GetChild(i).GetComponentsInChildren(type, results, includeInactive);
            }
        }
        
        /// <summary>
        ///   <para>The non-generic, non-allocating version of <see cref="Component.GetComponentsInParent(Type, bool)"/>.</para>
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="type">The type of component to search for.</param>
        /// <param name="includeInactive">Whether to include inactive parent GameObjects in the search.</param>
        /// <param name="results">A list of all found components matching the specified type.</param>
        public static void GetComponentsInParent(this Transform transform, System.Type type, List<Component> results,
            bool includeInactive = false)
        {
            for (var ancestor = transform; ancestor; ancestor = ancestor.parent)
            {
                if ((includeInactive || transform.gameObject.activeInHierarchy)
                    && ancestor.TryGetComponent(type, out var parent))
                {
                    results.Add(parent);
                }

                transform.GetComponentsInParent(type, results, includeInactive);
            }
        }
    }
}
