#nullable enable
using System;
using System.Runtime.InteropServices;
using Interlocked = System.Threading.Interlocked;

namespace TCGE
{
    /// <summary>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AtomicInt : IEquatable<AtomicInt>
    {
        public int value;

        public static implicit operator AtomicInt(int value) => new AtomicInt { value = value };
        public static implicit operator int(AtomicInt value) => value.value;

        public static AtomicInt operator ++(AtomicInt value)
            => Interlocked.Increment(ref value.value);
        public static AtomicInt operator --(AtomicInt value)
            => Interlocked.Decrement(ref value.value);

        public static AtomicInt operator +(AtomicInt value) => value;
        public static AtomicInt operator -(AtomicInt value) => new AtomicInt { value = -value.value };

        public static AtomicInt operator +(AtomicInt left, AtomicInt right)
            => Interlocked.Add(ref left.value, right.value);

        public static AtomicInt operator -(AtomicInt left, AtomicInt right)
            => Interlocked.Add(ref left.value, -right.value);

        /// <summary>
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.compareexchange?view=netstandard-2.1#system-threading-interlocked-compareexchange(system-int32@-system-int32-system-int32)"/>
        /// </summary>
        public int Mul(int mul)
        {
            int initialValue, computedValue;
            do
            {
                // Save the current running total in a local variable.
                initialValue = value;

                // Multiply the new value with the running total.
                computedValue = initialValue * mul;

                // CompareExchange compares value to initialValue. If
                // they are not equal, then another thread has updated the
                // running total since this loop started. CompareExchange
                // does not update value. CompareExchange returns the
                // contents of value, which do not equal initialValue,
                // so the loop executes again.
            } while (initialValue != Interlocked.CompareExchange(
                ref value, computedValue, initialValue));
            // If no other thread updated the running total, then 
            // value and initialValue are equal when CompareExchange
            // compares them, and computedValue is stored in value.
            // CompareExchange returns the value that was in value
            // before the update, which is equal to initialValue, so the 
            // loop ends.

            // The function returns computedValue, not value, because
            // value could be changed by another thread between
            // the time the loop ends and the function returns.
            return computedValue;
        }

        /// <inheritdoc cref="Mul(int)"/>
        /// <exception cref="DivideByZeroException"></exception>
        public int Div(int div)
        {
            if (div == 0)
            {
                throw new DivideByZeroException();
            }

            int initialValue, computedValue;
            do
            {
                initialValue = value;
                computedValue = initialValue / div;
            } while (initialValue != Interlocked.CompareExchange(
                ref value, computedValue, initialValue));

            return computedValue;
        }

        #region Unity.Collections.LowLevel.Unsafe
        /// <summary>
        /// Atomically adds a value to this counter. The result will not be greater than a maximum value.
        /// </summary>
        /// <param name="addValue">The value to add to this counter.</param>
        /// <param name="max">The maximum which the result will not be greater than.</param>
        /// <returns>The original value before the addition.</returns>
        public int AddSat(int addValue, int max = int.MaxValue)
        {
            int oldVal;
            int newVal = value;
            do
            {
                oldVal = newVal;
                newVal = newVal >= max ? max : Math.Min(max, newVal + addValue);
                newVal = Interlocked.CompareExchange(ref value, newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        /// <summary>
        /// Atomically subtracts a value from this counter. The result will not be less than a minimum value.
        /// </summary>
        /// <param name="subValue">The value to subtract from this counter.</param>
        /// <param name="min">The minimum which the result will not be less than.</param>
        /// <returns>The original value before the subtraction.</returns>
        public int SubSat(int subValue, int min = int.MinValue)
        {
            int oldVal;
            int newVal = value;
            do
            {
                oldVal = newVal;
                newVal = newVal <= min ? min : Math.Max(min, newVal - subValue);
                newVal = Interlocked.CompareExchange(ref value, newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != min);

            return oldVal;
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        public int IncrementWrap(int min = int.MinValue, int max = int.MaxValue)
        {
            UnityEngine.Assertions.Assert.IsTrue(max > min);

            int oldVal;
            int newVal = value;
            do
            {
                oldVal = newVal++;
                if (newVal > max)
                {
                    newVal = min;
                }

                newVal = Interlocked.CompareExchange(ref value, newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        public readonly bool Equals(AtomicInt other) => this == other;
        public readonly override bool Equals(object? obj) => obj is AtomicInt other && Equals(other);
        public static bool operator ==(AtomicInt left, AtomicInt right) => left.value == right.value;
        public static bool operator !=(AtomicInt left, AtomicInt right) => !(left == right);

        public readonly override string ToString() => value.ToString();
        public readonly override int GetHashCode() => value.GetHashCode();
    }

    /// <summary>
    /// A 32-bit atomic counter.
    /// </summary>
    /// <remarks>Rather than have its own int, a counter *points* to an int.
    /// This arrangement lets counters in different jobs share reference to the same underlying int.</remarks>
    public struct AtomicCounter32
    {
        #region Unity.Collections.LowLevel.Unsafe
        /// <summary>
        /// The int that is modified by this counter.
        /// </summary>
        /// <value>The int that is modified by this counter.</value>
        public Unity.Collections.NativeArray<int> Counter;

        /// <summary>
        /// Initializes and returns an instance of AtomicCounter32.
        /// </summary>
        /// <param name="ptr">A pointer to the int to be modified by this counter.</param>
        public AtomicCounter32(Unity.Collections.NativeArray<int> ptr)
        {
            UnityEngine.Assertions.Assert.IsTrue(ptr.IsCreated);
            UnityEngine.Assertions.Assert.IsTrue(ptr.Length > 0);

            Counter = ptr;
        }

        /// <summary>
        /// Non-atomically sets this counter to a value.
        /// </summary>
        /// <param name="value">The value to set. Defaults to 0</param>
        public void Reset(int value = 0)
        {
            Counter[0] = value;
        }

        /// <summary>
        /// Atomically adds a value to this counter.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The original value before the add.</returns>
        public readonly int Add(int value)
        {
            return Interlocked.Add(ref PKGE.ArrayExtensions.AsRef(Counter), value) - value;
        }

        /// <summary>
        /// Atomically subtracts a value from this counter.
        /// </summary>
        /// <param name="value">The value to subtract.</param>
        /// <returns>The original value before the subtract.</returns>
        public readonly int Sub(int value) => Add(-value);

        /// <summary>
        /// Atomically adds a value to this counter. The result will not be greater than a maximum value.
        /// </summary>
        /// <param name="value">The value to add to this counter.</param>
        /// <param name="max">The maximum which the result will not be greater than.</param>
        /// <returns>The original value before the add.</returns>
        public int AddSat(int value, int max = int.MaxValue)
        {
            int oldVal;
            int newVal = Counter[0];
            do
            {
                oldVal = newVal;
                newVal = newVal >= max ? max : Math.Min(max, newVal + value);
                newVal = Interlocked.CompareExchange(ref PKGE.ArrayExtensions.AsRef(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        /// <summary>
        /// Atomically subtracts a value from this counter. The result will not be less than a minimum value.
        /// </summary>
        /// <param name="value">The value to subtract from this counter.</param>
        /// <param name="min">The minimum which the result will not be less than.</param>
        /// <returns>The original value before the subtract.</returns>
        public int SubSat(int value, int min = int.MinValue)
        {
            int oldVal;
            int newVal = Counter[0];
            do
            {
                oldVal = newVal;
                newVal = newVal <= min ? min : Math.Max(min, newVal - value);
                newVal = Interlocked.CompareExchange(ref PKGE.ArrayExtensions.AsRef(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != min);

            return oldVal;
        }
        #endregion // Unity.Collections.LowLevel.Unsafe
    }

    /// <summary>
    /// A 64-bit atomic counter.
    /// </summary>
    /// <remarks>Rather than have its own long, a counter *points* to a long.
    /// This arrangement lets counters in different jobs share reference to the same underlying long.</remarks>
    public struct AtomicCounter64
    {
        #region Unity.Collections.LowLevel.Unsafe
        /// <summary>
        /// The long that is modified by this counter.
        /// </summary>
        /// <value>The long that is modified by this counter.</value>
        public Unity.Collections.NativeArray<long> Counter;

        /// <summary>
        /// Initializes and returns an instance of AtomicCounter64.
        /// </summary>
        /// <param name="ptr">A pointer to the long to be modified by this counter.</param>
        public AtomicCounter64(Unity.Collections.NativeArray<long> ptr)
        {
            UnityEngine.Assertions.Assert.IsTrue(ptr.IsCreated);
            UnityEngine.Assertions.Assert.IsTrue(ptr.Length > 0);

            Counter = ptr;
        }

        /// <summary>
        /// Non-atomically sets this counter to a value.
        /// </summary>
        /// <param name="value">The value to set. Defaults to 0</param>
        public void Reset(long value = 0)
        {
            Counter[0] = value;
        }

        /// <summary>
        /// Atomically adds a value to this counter.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The original value before the add.</returns>
        public readonly long Add(long value)
        {
            return Interlocked.Add(ref PKGE.ArrayExtensions.AsRef(Counter), value) - value;
        }

        /// <summary>
        /// Atomically subtracts a value from this counter.
        /// </summary>
        /// <param name="value">The value to subtract.</param>
        /// <returns>The original value before the subtract.</returns>
        public readonly long Sub(long value) => Add(-value);

        /// <summary>
        /// Atomically adds a value to this counter. The result will not be greater than a maximum value.
        /// </summary>
        /// <param name="value">The value to add to this counter.</param>
        /// <param name="max">The maximum which the result will not be greater than.</param>
        /// <returns>The original value before the add.</returns>
        public long AddSat(long value, long max = long.MaxValue)
        {
            long oldVal;
            long newVal = Counter[0];
            do
            {
                oldVal = newVal;
                newVal = newVal >= max ? max : Math.Min(max, newVal + value);
                newVal = Interlocked.CompareExchange(ref PKGE.ArrayExtensions.AsRef(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        /// <summary>
        /// Atomically subtracts a value from this counter. The result will not be less than a minimum value.
        /// </summary>
        /// <param name="value">The value to subtract from this counter.</param>
        /// <param name="min">The minimum which the result will not be less than.</param>
        /// <returns>The original value before the subtract.</returns>
        public long SubSat(long value, long min = long.MinValue)
        {
            long oldVal;
            long newVal = Counter[0];
            do
            {
                oldVal = newVal;
                newVal = newVal <= min ? min : Math.Max(min, newVal - value);
                newVal = Interlocked.CompareExchange(ref PKGE.ArrayExtensions.AsRef(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != min);

            return oldVal;
        }
        #endregion // Unity.Collections.LowLevel.Unsafe
    }
}
