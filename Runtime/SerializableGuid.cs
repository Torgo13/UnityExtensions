using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// A <c>Guid</c> that can be serialized by Unity.
    /// </summary>
    /// <remarks>
    /// The 128-bit <c>Guid</c>
    /// is stored as two 64-bit <see langword="ulong"/>s. See the creation utility,
    /// <see cref="SerializableGuidUtil"/>, for additional information.
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/SerializableGuid.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Represents <c>System.Guid.Empty</c>, a GUID whose value is all zeros.
        /// </summary>
        public static readonly SerializableGuid Empty = new SerializableGuid(0, 0);

        [SerializeField]
        [HideInInspector]
        ulong guidLow;

        [SerializeField]
        [HideInInspector]
        ulong guidHigh;

        /// <summary>
        /// Reconstructs the <c>Guid</c> from the serialized data.
        /// </summary>
        public readonly Guid Guid => GuidUtil.Compose(guidLow, guidHigh);

        /// <summary>
        /// Constructs a <see cref="SerializableGuid"/> from two 64-bit <see langword="ulong"/> values.
        /// </summary>
        /// <param name="guidLow">The low 8 bytes of the <c>Guid</c>.</param>
        /// <param name="guidHigh">The high 8 bytes of the <c>Guid</c>.</param>
        public SerializableGuid(ulong guidLow, ulong guidHigh)
        {
            this.guidLow = guidLow;
            this.guidHigh = guidHigh;
        }

        /// <summary>
        /// Gets the hash code for this SerializableGuid.
        /// </summary>
        /// <returns>The hash code.</returns>
        public readonly override int GetHashCode()
        {
            unchecked
            {
                var hash = guidLow.GetHashCode();
                return hash * 486187739 + guidHigh.GetHashCode();
            }
        }

        /// <summary>
        /// Checks if this SerializableGuid is equal to an object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if <paramref name="obj"/> is a SerializableGuid with the same field values.</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is SerializableGuid serializableGuid
                && Equals(serializableGuid);
        }

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString()"/>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString">Microsoft's documentation</a>
        /// for more details.
        /// </summary>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public readonly override string ToString() => Guid.ToString();

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString(string)"/>.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString_System_String_">Microsoft's documentation</a>
        /// for more details.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public readonly string ToString(string format) => Guid.ToString(format);

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString(string, IFormatProvider)"/>.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString_System_String_System_IFormatProvider_">Microsoft's documentation</a>
        /// for more details.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public readonly string ToString(string format, IFormatProvider provider) => Guid.ToString(format, provider);

        /// <summary>
        /// Check if this SerializableGuid is equal to another SerializableGuid.
        /// </summary>
        /// <param name="other">The other SerializableGuid</param>
        /// <returns>True if this SerializableGuid has the same field values as the other one.</returns>
        public readonly bool Equals(SerializableGuid other)
        {
            return guidLow == other.guidLow &&
                guidHigh == other.guidHigh;
        }

        /// <summary>
        /// Perform an equality operation on two SerializableGuids.
        /// </summary>
        /// <param name="lhs">The left-hand SerializableGuid.</param>
        /// <param name="rhs">The right-hand SerializableGuid.</param>
        /// <returns>True if the SerializableGuids are equal to each other.</returns>
        public static bool operator ==(SerializableGuid lhs, SerializableGuid rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Perform an inequality operation on two SerializableGuids.
        /// </summary>
        /// <param name="lhs">The left-hand SerializableGuid.</param>
        /// <param name="rhs">The right-hand SerializableGuid.</param>
        /// <returns>True if the SerializableGuids are not equal to each other.</returns>
        public static bool operator !=(SerializableGuid lhs, SerializableGuid rhs) => !lhs.Equals(rhs);
        #endregion // Unity.XR.CoreUtils
    }
}