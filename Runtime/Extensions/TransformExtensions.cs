#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PKGE
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
        /// <param name="transform">The <see cref="Transform"/> from which to get the pose.</param>
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
        /// <param name="transform">The <see cref="Transform"/> from which to get the pose.</param>
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
        /// <param name="transform">The <see cref="Transform"/> on which to set the pose.</param>
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
        /// <param name="transform">The <see cref="Transform"/> on which to set the pose.</param>
        /// <param name="pose">Pose specifying the new position and rotation.</param>
        public static void SetWorldPose(this Transform transform, Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="pose">The <see cref="Pose"/> to transform.</param>
        /// <returns>A new <see cref="Pose"/> representing the transformed <paramref name="pose"/>.</returns>
        public static Pose TransformPose(this Transform transform, Pose pose)
        {
            return pose.GetTransformedBy(transform);
        }

        /// <summary>
        /// Inverse transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="pose">The <see cref="Pose"/> to inversely transform.</param>
        /// <returns>A new <see cref="Pose"/> representing the inversely transformed <paramref name="pose"/>.</returns>
        /// <exception cref="System.ArgumentNullException">transform</exception>
        public static Pose InverseTransformPose(this Transform transform, Pose pose)
        {
            if (transform == null)
                throw new System.ArgumentNullException(nameof(transform));

            return new Pose
            {
                position = transform.InverseTransformPoint(pose.position),
                rotation = Quaternion.Inverse(transform.rotation) * pose.rotation
            };
        }

        /// <summary>
        /// Inverse transforms a <see cref="Ray"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="ray">The <see cref="Ray"/> to inversely transform.</param>
        /// <returns>A new <see cref="Ray"/> representing the inversely transformed <paramref name="ray"/>.</returns>
        /// <exception cref="System.ArgumentNullException">transform</exception>
        public static Ray InverseTransformRay(this Transform transform, Ray ray)
        {
            if (transform == null)
                throw new System.ArgumentNullException(nameof(transform));

            return new Ray(
                transform.InverseTransformPoint(ray.origin),
                transform.InverseTransformDirection(ray.direction));
        }
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Transforms a <see cref="Ray"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="ray">The <see cref="Ray"/> to transform.</param>
        /// <returns>A new <see cref="Ray"/> representing the transformed <paramref name="ray"/>.</returns>
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

                if (System.Math.Abs(p.x - pos.x) < tolerance
                    && System.Math.Abs(p.y - pos.y) < tolerance
                    && System.Math.Abs(p.z - pos.z) < tolerance)
                    pos = p;

                if (System.Math.Abs(r.x - rot.x) < tolerance
                    && System.Math.Abs(r.y - rot.y) < tolerance
                    && System.Math.Abs(r.z - rot.z) < tolerance
                    && System.Math.Abs(r.w - rot.w) < tolerance)
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

        public static int FindAllDescendants(
            this Transform t,
            List<Transform> descendants,
            bool includeInactive = false)
        {
            t.GetComponentsInChildren(includeInactive, descendants);
            descendants.RemoveAtSwapBack(0);
            return descendants.Count;
        }
        #endregion // Unity.FilmInternalUtilities
        
        public static int GetChildCount(this Transform t, bool onlyActive = false)
        {
            var children = ListPool<Transform>.Get();
            int childrenCount = t.FindAllDescendants(children, includeInactive: !onlyActive);
            ListPool<Transform>.Release(children);

            return childrenCount;
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
        ///   <para>The non-generic, non-allocating version of <see cref="Component.GetComponentsInChildren(System.Type, bool)"/>.</para>
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="type">The type of component to search for.</param>
        /// <param name="results">A list of all found components matching the specified type.</param>
        /// <param name="includeInactive">Whether to include inactive child GameObjects in the search.
        /// The <see cref="GameObject"/> on which the method is called is always searched regardless of this parameter.</param>
        public static void GetComponentsInChildren(this Transform transform, System.Type type, List<Component> results,
            bool includeInactive = false)
        {
            transform.gameObject.GetComponentsInChildren(type, results, includeInactive);
        }

        /// <summary>
        ///   <para>The non-generic, non-allocating version of <see cref="Component.GetComponentsInParent(System.Type, bool)"/>.</para>
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> component.</param>
        /// <param name="type">The type of component to search for.</param>
        /// <param name="results">A list of all found components matching the specified type.</param>
        /// <param name="includeInactive">Whether to include inactive parent GameObjects in the search.
        /// The <see cref="GameObject"/> on which the method is called is always searched regardless of this parameter.</param>
        public static void GetComponentsInParent(this Transform transform, System.Type type, List<Component> results,
            bool includeInactive = false)
        {
            transform.gameObject.GetComponentsInParent(type, results, includeInactive);
        }

        /// <summary>
        /// Get the direct children <see cref="Transform"/>s of this <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform">The parent <see cref="Transform"/> that we will want to get the child <see cref="Transform"/>s on.</param>
        /// <param name="childTransforms">The direct children of a <see cref="Transform"/>.</param>
        public static void GetChildTransforms(this Transform transform, List<Transform> childTransforms)
        {
            var childCount = transform.childCount;
            childTransforms.EnsureCapacity(childCount);
            for (var i = 0; i < childCount; i++)
            {
                childTransforms.Add(transform.GetChild(i));
            }
        }

        public static void GetChildInstanceIDs(this Transform transform, List<EntityId> childInstanceIDs)
        {
            transform.gameObject.GetChildInstanceIDs(childInstanceIDs);
        }

        public static void SetActiveRecursively(this Transform transform, bool active)
        {
#if UNITY_6000_3_OR_NEWER
            transform.gameObject.SetActive(active);
#else
            transform.gameObject.SetActiveRecursively(active);
#endif // UNITY_6000_3_OR_NEWER
        }

        public static void SetGrandchildrenActiveRecursively(this Transform transform, bool active)
        {
            var childInstanceIDs = ListPool<EntityId>.Get();
            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
            {
                transform.GetChild(i).GetChildInstanceIDs(childInstanceIDs);
            }

            if (childInstanceIDs.Count > 0)
            {
#if UNITY_6000_3_OR_NEWER
                GameObject.SetGameObjectsActive(childInstanceIDs.AsReadOnlySpan(), active);
#else
                GameObject.SetGameObjectsActive(childInstanceIDs.Cast<EntityId, int>(), active);
#endif // UNITY_6000_3_OR_NEWER
            }

            ListPool<EntityId>.Release(childInstanceIDs);
        }

        /// <inheritdoc cref="GameObjectExtensions.GetNamedChild(GameObject, string, out Transform?)"/>
        public static bool GetNamedChild(this Transform transform, string name,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Transform? namedChild)
        {
            return transform.gameObject.GetNamedChild(name, out namedChild);
        }
    }
}
