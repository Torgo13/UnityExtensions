using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PKGE.Packages
{
#if INCLUDE_COLLECTIONS
    internal struct UnityObjectRefMap : IDisposable
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/Serialization/UnityObjectRef.cs
        #region Unity.Entities
        public NativeHashMap<int, int> InstanceIDMap;
        public NativeList<int> InstanceIDs;

        public readonly bool IsCreated => InstanceIDs.IsCreated && InstanceIDMap.IsCreated;

        public UnityObjectRefMap(Allocator allocator)
        {
            InstanceIDMap = new NativeHashMap<int, int>(0, allocator);
            InstanceIDs = new NativeList<int>(0, allocator);
        }

        public void Dispose()
        {
            InstanceIDMap.Dispose();
            InstanceIDs.Dispose();
        }

        public UnityEngine.Object[] ToObjectArray()
        {
            var objects = new List<UnityEngine.Object>();

            if (IsCreated && InstanceIDs.Length > 0)
                Resources.InstanceIDToObjectList(InstanceIDs.AsArray(), objects);

            return objects.ToArray();
        }

        public int Add(int instanceId)
        {
            var index = -1;
            if (instanceId != 0 && IsCreated)
            {
                if (!InstanceIDMap.TryGetValue(instanceId, out index))
                {
                    index = InstanceIDs.Length;
                    InstanceIDMap.Add(instanceId, index);
                    InstanceIDs.Add(instanceId);
                }
            }

            return index;
        }
        #endregion // Unity.Entities

        public List<UnityEngine.Object> Get(List<UnityEngine.Object> objects)
        {
            if (IsCreated && InstanceIDs.Length > 0)
                Resources.InstanceIDToObjectList(InstanceIDs.AsArray(), objects);

            return objects;
        }
    }
#endif // INCLUDE_COLLECTIONS

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct UntypedUnityObjectRef : IEquatable<UntypedUnityObjectRef>
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/Serialization/UnityObjectRef.cs
        #region Unity.Entities
        [SerializeField]
        internal int instanceId;

        public readonly bool Equals(UntypedUnityObjectRef other)
        {
            return instanceId == other.instanceId;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is UntypedUnityObjectRef other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return instanceId;
        }
        #endregion // Unity.Entities
    }

    /// <summary>
    /// A utility structure that stores a reference of an <see cref="UnityEngine.Object"/> for Entities. Allows references to be stored on unmanaged component.
    /// </summary>
    /// <typeparam name="T">Type of the Object that is going to be referenced by UnityObjectRef.</typeparam>
    /// <remarks>
    /// Stores the Object's instance ID. Also serializes asset references in subscenes the same way managed components 
    /// do with direct references to <see cref="UnityEngine.Object"/>. This is the recommended way to store references to Unity 
    /// assets in Entities because it remains unmanaged.
    /// 
    /// Serialization is supported on <see cref="IComponentData"/> <see cref="ISharedComponentData"/> and <see cref="IBufferElementData"/>.
    /// 
    /// Just as when referencing an asset in a Monobehaviour, the asset will not be collected by any asset garbage collection (such as calling <see cref="Resources.UnloadUnusedAssets()"/>).
    /// 
    /// For more information, refer to [Reference Unity objects in your code](xref:reference-unity-objects).
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct UnityObjectRef<T> : IEquatable<UnityObjectRef<T>>
        where T : Object
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/Serialization/UnityObjectRef.cs
        #region Unity.Entities
        [SerializeField]
        internal UntypedUnityObjectRef Id;

        /// <summary>
        /// Implicitly converts an <see cref="UnityEngine.Object"/> to an <see cref="UnityObjectRef{T}"/>.
        /// </summary>
        /// <param name="instance">Instance of the Object to store as a reference.</param>
        /// <returns>A UnityObjectRef referencing instance</returns>
        public static implicit operator UnityObjectRef<T>(T instance)
        {
            var instanceId = instance == null ? 0 : instance.GetInstanceID();

            return FromInstanceID(instanceId);
        }

        internal static UnityObjectRef<T> FromInstanceID(int instanceId)
        {
            var result = new UnityObjectRef<T>{Id = new UntypedUnityObjectRef{ instanceId = instanceId }};
            return result;
        }

        /// <summary>
        /// Implicitly converts an <see cref="UnityObjectRef{T}"/> to an <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="unityObjectRef">Reference used to access the Object.</param>
        /// <returns>The instance of type T referenced by unityObjectRef.</returns>
        public static implicit operator T(UnityObjectRef<T> unityObjectRef)
        {
            if (unityObjectRef.Id.instanceId == 0)
                return null;

            return (T)Resources.InstanceIDToObject(unityObjectRef.Id.instanceId);
        }

        /// <summary>
        /// Object being referenced by this <see cref="UnityObjectRef{T}"/>.
        /// </summary>
        public T Value
        {
            [ExcludeFromBurstCompatTesting("Returns managed object")]
            readonly get => this;
            [ExcludeFromBurstCompatTesting("Sets managed object")]
            set => this = value;
        }

        /// <summary>
        /// Checks if this reference and another reference are equal.
        /// </summary>
        /// <param name="other">The UnityObjectRef to compare for equality.</param>
        /// <returns>True if the two lists are equal.</returns>
        public readonly bool Equals(UnityObjectRef<T> other)
        {
            return Id.instanceId == other.Id.instanceId;
        }

        /// <summary>
        /// Checks if this object references the same UnityEngine.Object as another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True, if the <paramref name="obj"/> parameter is a UnityEngine.Object instance that points to the same
        /// instance as this.</returns>
        public override readonly bool Equals(object obj)
        {
            return obj is UnityObjectRef<T> other && Equals(other);
        }

        /// <summary>
        /// Overload of the 'bool' operator to check for the validity of the instance ID.
        /// </summary>
        /// <param name="obj">The object to check for validity.</param>
        /// <returns>True, if the instance ID is valid.</returns>
        public static implicit operator bool(UnityObjectRef<T> obj)
        {
            return obj.IsValid();
        }

        /// <summary>
        /// Computes a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override readonly int GetHashCode()
        {
            return Id.instanceId.GetHashCode();
        }

        /// <summary>
        /// Returns 'true' if the UnityObjectRef is still valid, as in the Object still exists.
        /// </summary>
        /// <returns>Valid state.</returns>
        public readonly bool IsValid()
        {
            return Resources.InstanceIDIsValid(Id.instanceId);
        }

        /// <summary>
        /// Returns true if two <see cref="UnityObjectRef{T}"/> are equal.
        /// </summary>
        /// <param name="left">The first reference to compare for equality.</param>
        /// <param name="right">The second reference to compare for equality.</param>
        /// <returns>True if the two references are equal.</returns>
        public static bool operator ==(UnityObjectRef<T> left, UnityObjectRef<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns true if two <see cref="UnityObjectRef{T}"/> are not equal.
        /// </summary>
        /// <param name="left">The first reference to compare for equality.</param>
        /// <param name="right">The second reference to compare for equality.</param>
        /// <returns>True if the two references are not equal.</returns>
        public static bool operator !=(UnityObjectRef<T> left, UnityObjectRef<T> right)
        {
            return !left.Equals(right);
        }
        #endregion // Unity.Entities
    }
}
