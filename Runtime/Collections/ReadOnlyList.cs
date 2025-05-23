using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityExtensions.Collections
{
    /// <summary>
    /// Wraps a <see cref="List{T}"/> to provide a read-only view of its memory without copying any elements.
    /// It is preferable to use this collection in API designs instead of `IReadOnlyCollection` because
    /// <see cref="GetEnumerator"/> returns a value-type enumerator and does not perform any heap allocations.
    /// </summary>
    /// <remarks>
    /// This collection is not thread-safe.
    /// </remarks>
    /// <typeparam name="T">The element type.</typeparam>
    public sealed class ReadOnlyList<T> : IReadOnlyList<T>, IEquatable<ReadOnlyList<T>>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Collections/ReadOnlyList.cs
        #region Unity.XR.CoreUtils.Collections
        static ReadOnlyList<T> _emptyList;

        readonly List<T> _list;

        /// <summary>
        /// The number of elements in the read-only list.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count => _list.Count;

        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index] => _list[index];

        /// <summary>
        /// Constructs a new instance of this class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        public ReadOnlyList(List<T> list)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
        }

        /// <summary>
        /// Returns an empty read-only list with the specified type argument.
        /// </summary>
        /// <returns>The empty read-only list.</returns>
        /// <remarks>
        /// This method caches an empty read-only list that you can re-use throughout the life cycle of your app.
        /// </remarks>
        public static ReadOnlyList<T> Empty()
        {
            if (_emptyList == null)
                _emptyList = new ReadOnlyList<T>(new List<T>(0));

            return _emptyList;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        /// <remarks>
        /// Two `ReadOnlyList` instances compare equal if they are read-only views of the same list instance.
        /// </remarks>
        public bool Equals(ReadOnlyList<T> other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || Equals(_list, other._list);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        /// <remarks>
        /// Two `ReadOnlyList` instances compare equal if they are read-only views of the same list instance.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ReadOnlyList<T>)obj);
        }

        /// <summary>
        /// Returns `true` if objects are equal by <see cref="Equals(UnityExtensions.Collections.ReadOnlyList{T})"/>.
        /// Otherwise, `false`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`true` if objects are equal. Otherwise, `false`.</returns>
        public static bool operator ==(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs)
        {
            if (lhs is null && rhs is null)
                return true;
            return lhs is not null && lhs.Equals(rhs);
        }

        /// <summary>
        /// Returns `false` if objects are equal by <see cref="Equals(UnityExtensions.Collections.ReadOnlyList{T})"/>.
        /// Otherwise, `true`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`false` if objects are equal. Otherwise, `true`.</returns>
        public static bool operator !=(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs) => !(lhs == rhs);

        /// <summary>
        /// Get a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _list != null ? _list.GetHashCode() : 0;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            sb.Append('{');
            sb.AppendLine();
            foreach (var item in _list)
            {
                if (item == null)
                {
                    sb.AppendLine("  null,");
                }
                else
                {
                    sb.Append(' ').Append(' ').Append(item).Append(',').AppendLine();
                }
            }
            sb.Append('}');
            return sb.ToString();
        }
#endregion // Unity.XR.CoreUtils.Collections
    }
}
