using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PKGE.Collections
{
    /// <summary>
    /// Wrapper data structure for hashset, that leans on a list for deterministic sort order
    /// </summary>
    /// <typeparam name="T">HashSetList type</typeparam>
    public class HashSetList<T> : ISerializable, IDeserializationCallback, ISet<T>, IReadOnlyCollection<T>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Collections/HashSetList.cs
        #region Unity.XR.CoreUtils.Collections
        readonly List<T> _internalList;
        readonly HashSet<T> _internalHashSet;

        // This value always matches _internalList.Count but we track it ourselves
        // as a performance optimization determined through profiling.
        int _count;

        /// <summary>
        /// Internal list count.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Mandatory field. Always false.
        /// </summary>
        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Access internal list element from index.
        /// </summary>
        /// <param name="index">Index used to access internal list.</param>
        public T this[int index] => _internalList[index];

        /// <summary>
        /// Allocates internal list and hashset.
        /// </summary>
        /// <param name="capacity">Initial list capacity</param>
        public HashSetList(int capacity = 0)
        {
            _internalList = new List<T>(capacity);
            _internalHashSet = new HashSet<T>();
        }

        /// <summary>
        /// Creates enumerator for internal list.
        /// </summary>
        /// <returns>Returns internal list enumerator.</returns>
        public List<T>.Enumerator GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        /// <summary>
        /// Gets an <see cref="IEnumerator{T}"/> for the internal list.
        /// </summary>
        /// <returns>Returns internal list enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        /// <summary>Returns a standard enumerator that iterates through the collection.</summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds item to the existing internal list.
        /// </summary>
        /// <param name="item">Item of type <c>T</c> to add.</param>
        void ICollection<T>.Add(T item)
        {
            if (_internalHashSet.Add(item))
            {
                _internalList.Add(item);
                _count++;
            }
        }

        /// <summary>
        /// Attempt to add item to internal hashset. If it is not already in the hashset, add it to the list.
        /// </summary>
        /// <param name="item">Item of type <c>T</c> to add.</param>
        /// <returns>True if the item was added to both list and hashset.</returns>
        public bool Add(T item)
        {
            bool wasAdded = _internalHashSet.Add(item);
            if (wasAdded)
            {
                _internalList.Add(item);
                _count++;
            }

            return wasAdded;
        }

        /// <summary>
        /// Attempt to remove item to internal hashset. If it is still present in the hashset,
        /// remove it from the list.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was removed from both list and hashset.</returns>
        public bool Remove(T item)
        {
            if (_count == 0)
                return false;

            bool wasRemoved = _internalHashSet.Remove(item);
            if (wasRemoved)
            {
                _internalList.Remove(item);
                _count--;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Except operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to except with.</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            _internalHashSet.ExceptWith(other);
            RefreshList();
        }

        /// <summary>
        /// <c>IntersectWith</c> operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to intersect with.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            _internalHashSet.IntersectWith(other);
            RefreshList();
        }

        /// <summary>
        /// <c>IsProperSubsetOf</c> operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to <c>IsProperSubsetOf</c> with.</param>
        /// <returns>Returns <see langword="true"/> if internal hashset is a proper subset of other,
        /// returns <see langword="false"/> otherwise.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _internalHashSet.IsProperSubsetOf(other);
        }

        /// <summary>
        /// <c>IsProperSupersetOf</c> operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to <c>IsProperSupersetOf</c> with.</param>
        /// <returns>Returns <see langword="true"/> if internal hashset is a proper superset of other,
        /// returns <see langword="false"/> otherwise.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _internalHashSet.IsProperSupersetOf(other);
        }

        /// <summary>
        /// <c>IsSubsetOf</c> operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to <c>IsSubsetOf</c> with.</param>
        /// <returns>Returns <see langword="true"/> if internal hashset is a subset of other,
        /// returns <see langword="false"/> otherwise.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _internalHashSet.IsSubsetOf(other);
        }

        /// <summary>
        /// <c>IsSupersetOf</c> operation with hashset. Regenerates internal list from new hashset.
        /// </summary>
        /// <param name="other">Enumerable to <c>IsSupersetOf</c> with.</param>
        /// <returns>Returns <see langword="true"/> if internal hashset is a superset of other,
        /// returns <see langword="false"/>.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _internalHashSet.IsSupersetOf(other);
        }

        /// <summary>
        /// <c>Overlaps</c> operation with hashset.
        /// </summary>
        /// <param name="other">Enumerable to <c>Overlaps</c> with.</param>
        /// <returns>Returns <see langword="true"/> if there is overlap, returns <see langword="false"/>.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _internalHashSet.Overlaps(other);
        }

        /// <summary>
        /// Check if set equals other <see cref="Overlaps"/> operation with hashset.
        /// </summary>
        /// <param name="other">Enumerable to <see cref="Overlaps"/> with.</param>
        /// <returns>Returns <see langword="true"/> if hash iOverlaps operation is true,
        /// returns <see langword="false"/> otherwise.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _internalHashSet.SetEquals(other);
        }

        /// <summary>
        /// <c>SymmetricExceptWith</c> between internal hashset and other <c>IEnumerable</c>. Refresh List.
        /// </summary>
        /// <param name="other">Enumerable to <c>SymmetricExceptWith</c> with.</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _internalHashSet.SymmetricExceptWith(other);
            RefreshList();
        }

        /// <summary>
        /// <c>UnionWith</c> between internal hashset and other IEnumerable. Refresh List.
        /// </summary>
        /// <param name="other">Enumerable to union with.</param>
        public void UnionWith(IEnumerable<T> other)
        {
            _internalHashSet.UnionWith(other);
            RefreshList();
        }

        /// <summary>
        /// Clear both internal hashset and list.
        /// </summary>
        public void Clear()
        {
            _internalHashSet.Clear();
            _internalList.Clear();
        }

        /// <summary>
        /// Checks if internal hashset contains item.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Returns <see langword="true"/> if internal hashset contains item,
        /// returns <see langword="false"/> otherwise.</returns>
        public bool Contains(T item)
        {
            return _internalHashSet.Contains(item);
        }

        /// <summary>
        /// Copies out internal list as array.
        /// </summary>
        /// <param name="array">Array to write to.</param>
        /// <param name="arrayIndex">Index to start from.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Calls internal hashset <c>GetObjectData</c> and refreshes list.
        /// </summary>
        /// <param name="info"><c>GetObjectData</c> info.</param>
        /// <param name="context"><c>GetObjectData</c> context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _internalHashSet.GetObjectData(info, context);
            RefreshList();
        }

        /// <summary>
        /// Deserializes sender object into internal hashset and refreshes list.
        /// </summary>
        /// <param name="sender">Object to be deserialized as set.</param>
        public void OnDeserialization(object sender)
        {
            _internalHashSet.OnDeserialization(sender);
            RefreshList();
        }

        void RefreshList()
        {
            _internalList.Clear();
            _internalList.AddRange(_internalHashSet);
            _count = _internalList.Count;
        }

        /// <summary>
        /// Exposes internal list without any allocation.
        /// </summary>
        /// <returns>Internal list structure.</returns>
        public IReadOnlyList<T> AsList()
        {
            return _internalList;
        }
        #endregion // Unity.XR.CoreUtils.Collections
    }
}
