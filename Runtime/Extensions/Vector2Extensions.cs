using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for the <see cref="Vector2"/> type.
    /// </summary>
    public static class Vector2Extensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/Vector2Extensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        ///  Returns the component-wise inverse of this vector [1/x, 1/y].
        /// </summary>
        /// <param name="vector">The vector to invert.</param>
        /// <returns>The inverted vector.</returns>
        public static Vector2 Inverse(this Vector2 vector)
        {
            return new Vector2(1.0f / vector.x, 1.0f / vector.y);
        }

        /// <summary>
        /// Returns the smallest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned.</param>
        /// <returns>The minimum value.</returns>
        public static float MinComponent(this Vector2 vector)
        {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the largest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned.</param>
        /// <returns>The maximum value.</returns>
        public static float MaxComponent(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the component-wise absolute value of this vector [abs(x), abs(y)].
        /// </summary>
        /// <param name="vector">The vector whose absolute value will be returned.</param>
        /// <returns>The component-wise absolute value of this vector.</returns>
        public static Vector2 Abs(this Vector2 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            return vector;
        }
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Returns a new Vector2 that multiplies each component of both input vectors together.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">Vector2 used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector2 Multiply(this Vector2 value, Vector2 scale)
        {
            return new Vector2(value.x * scale.x, value.y * scale.y);
        }

        /// <summary>
        /// Returns a new `Vector2` that divides each component of the input value by each component of the scale value.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector2` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        /// <exception cref="System.DivideByZeroException">Thrown if scale parameter has any 0 values. Consider using <see cref="SafeDivide"/>.</exception>
        public static Vector2 Divide(this Vector2 value, Vector2 scale)
        {
            return new Vector2(value.x / scale.x, value.y / scale.y);
        }

        /// <summary>
        /// Returns a new `Vector2` that divides each component of the input value by each component of the scale value.
        /// If any divisor is 0 or the output of the division is a `NaN`, then the output of that component will be zero.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector2` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector2 SafeDivide(this Vector2 value, Vector2 scale)
        {
            float x = Mathf.Approximately(scale.x, 0f) ? 0f : value.x / scale.x;
            if (float.IsNaN(x))
            {
                x = 0f;
            }

            float y = Mathf.Approximately(scale.y, 0f) ? 0f : value.y / scale.y;
            if (float.IsNaN(y))
            {
                y = 0f;
            }
            return new Vector2(x, y);
        }
    }
}
