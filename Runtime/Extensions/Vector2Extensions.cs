using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityExtensions
{
    public static class Vector2Utils
    {
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        /// <summary>
        /// Calculates the intersection point defined by line_1 [p1, p2], and line_2 [q1, q2].
        /// </summary>
        /// <param name="p1">line_1 is defined by (p1, p2)</param>
        /// <param name="p2">line_1 is defined by (p1, p2)</param>
        /// <param name="q1">line_2 is defined by (q1, q2)</param>
        /// <param name="q2">line_2 is defined by (q1, q2)</param>
        /// <param name="intersection">If lines intersect at a single point,
        /// then this will hold the intersection point.
        /// Otherwise, it will be Vector2.positiveInfinity.</param>
        /// <returns>
        ///     0 = no intersection,
        ///     1 = lines intersect,
        ///     2 = segments intersect,
        ///     3 = lines are colinear, segments do not touch,
        ///     4 = lines are colinear, segments touch (at one or at multiple points)
        /// </returns>
        public static int FindIntersection(
            in Vector2 p1, in Vector2 p2, in Vector2 q1, in Vector2 q2,
            out Vector2 intersection)
        {
            var p = p2 - p1;
            var q = q2 - q1;
            var pq = q1 - p1;
            var pXq = p.Cross(q);
            if (Mathf.Abs(pXq) < 0.00001f)
            {
                // The lines are parallel (or close enough to it)
                intersection = Vector2.positiveInfinity;
                if (Mathf.Abs(pq.Cross(p)) < 0.00001f)
                {
                    // The lines are colinear.  Do the segments touch?
                    var dotPQ = Vector2.Dot(q, p);

                    if (dotPQ > 0 && (p1 - q2).sqrMagnitude < 0.001f)
                    {
                        // q points to start of p
                        intersection = q2;
                        return 4;
                    }

                    if (dotPQ < 0 && (p2 - q2).sqrMagnitude < 0.001f)
                    {
                        // p and q point at the same point
                        intersection = p2;
                        return 4;
                    }

                    var dot = Vector2.Dot(pq, p);
                    if (0 <= dot && dot <= Vector2.Dot(p, p))
                    {
                        if (dot < 0.0001f)
                        {
                            if (dotPQ <= 0 && (p1 - q1).sqrMagnitude < 0.001f)
                                intersection = p1; // p and q start at the same point and point away
                        }
                        else if (dotPQ > 0 && (p2 - q1).sqrMagnitude < 0.001f)
                            intersection = p2; // p points at start of q

                        return 4;   // colinear segments touch
                    }

                    dot = Vector2.Dot(p1 - q1, q);
                    if (0 <= dot && dot <= Vector2.Dot(q, q))
                        return 4;   // colinear segments overlap

                    return 3;   // colinear segments don't touch
                }

                return 0; // the lines are parallel and not colinear
            }

            var t = pq.Cross(q) / pXq;
            intersection = p1 + t * p;

            var u = pq.Cross(p) / pXq;
            if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                return 2;   // segments touch

            return 1;   // segments don't touch but lines intersect
        }
        #endregion // Unity.Cinemachine
    }
    
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

        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        /// <summary>A useful Epsilon</summary>
        const float Epsilon = 0.0001f;

        /// <summary>
        /// Checks if the Vector2 contains NaN for x or y.
        /// </summary>
        /// <param name="v">Vector2 to check for NaN</param>
        /// <returns>True, if any components of the vector are NaN</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(this Vector2 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y);
        }

        /// <summary>
        /// Get the closest point on a line segment.
        /// </summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static float ClosestPointOnSegment(this Vector2 p, Vector2 s0, Vector2 s1)
        {
            Vector2 s = s1 - s0;
            float len2 = Vector2.SqrMagnitude(s);
            if (len2 < Epsilon)
                return 0; // degenrate segment

            return Mathf.Clamp01(Vector2.Dot(p - s0, s) / len2);
        }

        /// <summary>
        /// Normalized the vector onto the unit square instead of the unit circle
        /// </summary>
        /// <param name="v">The vector to normalize</param>
        /// <returns>The normalized vector, or the zero vector if its magnitude
        /// was too small to normalize</returns>
        public static Vector2 SquareNormalize(this Vector2 v)
        {
            var d = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y));
            return d < Epsilon ? Vector2.zero : v / d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(this Vector2 v1, Vector2 v2) { return (v1.x * v2.y) - (v1.y * v2.x); }

        /// <summary>
        /// Checks whether the vector components are the same value.
        /// </summary>
        /// <param name="v">Vector to check</param>
        /// <returns>True, if the vector elements are the same. False, otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUniform(this Vector2 v)
        {
            return System.Math.Abs(v.x - v.y) < Epsilon;
        }

        /// <summary>Is the vector within Epsilon of zero length?</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostZero(this Vector2 v)
        {
            return v.sqrMagnitude < (Epsilon * Epsilon);
        }
        #endregion // Unity.Cinemachine
        
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
