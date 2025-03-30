using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for the <see cref="Vector3"/> type.
    /// </summary>
    public static class Vector3Extensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/Vector3Extensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Returns the component-wise inverse of this vector [1/x,1/y,1/z].
        /// </summary>
        /// <param name="vector">The vector to invert.</param>
        /// <returns>The inverted vector</returns>
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(1.0f / vector.x, 1.0f / vector.y, 1.0f / vector.z);
        }

        /// <summary>
        /// Returns the smallest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned.</param>
        /// <returns>The minimum value.</returns>
        public static float MinComponent(this Vector3 vector)
        {
            return Mathf.Min(Mathf.Min(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns the largest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned.</param>
        /// <returns>The maximum value.</returns>
        public static float MaxComponent(this Vector3 vector)
        {
            return Mathf.Max(Mathf.Max(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns the component-wise absolute value of this vector [abs(x), abs(y), abs(z)].
        /// </summary>
        /// <param name="vector">The vector whose absolute value will be returned</param>
        /// <returns>A vector containing the component-wise absolute values of this vector.</returns>
        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }

        /// <summary>
        /// Returns a new vector3 that multiplies each component of both input vectors together.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">Vector3 used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector3 Multiply(this Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
        }

        /// <summary>
        /// Returns a new `Vector3` that divides each component of the input value by each component of the scale value.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector3` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        /// <exception cref="System.DivideByZeroException">Thrown if scale parameter has any 0 values. Consider using <see cref="SafeDivide"/>.</exception>
        public static Vector3 Divide(this Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x / scale.x, value.y / scale.y, value.z / scale.z);
        }

        /// <summary>
        /// Returns a new `Vector3` that divides each component of the input value by each component of the scale value.
        /// If any divisor is 0 or the output of the division is a `NaN`, then the output of that component will be zero.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector3` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector3 SafeDivide(this Vector3 value, Vector3 scale)
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

            float z = Mathf.Approximately(scale.z, 0f) ? 0f : value.z / scale.z;
            if (float.IsNaN(z))
            {
                z = 0f;
            }
            return new Vector3(x, y, z);
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        /// <summary>A useful Epsilon</summary>
        const float Epsilon = 0.0001f;

        /// <summary>Much more stable for small angles than Unity's native implementation</summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>Angle between the vectors, in degrees</returns>
        public static float Angle(this Vector3 v1, Vector3 v2)
        {
#if false // Maybe this version is better?  to test....
            float a = v1.magnitude;
            v1 *= v2.magnitude;
            v2 *= a;
            return Mathf.Atan2((v1 - v2).magnitude, (v1 + v2).magnitude) * Mathf.Rad2Deg * 2;
#else
            v1.Normalize();
            v2.Normalize();
            return Mathf.Atan2((v1 - v2).magnitude, (v1 + v2).magnitude) * Mathf.Rad2Deg * 2;
#endif
        }

        /// <summary>Much more stable for small angles than Unity's native implementation</summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <param name="up">Definition of up (used to determine the sign)</param>
        /// <returns>Signed angle between the vectors, in degrees</returns>
        public static float SignedAngle(this Vector3 v1, Vector3 v2, Vector3 up)
        {
            float angle = Angle(v1, v2);
            if (Mathf.Sign(Vector3.Dot(up, Vector3.Cross(v1, v2))) < 0)
                return -angle;

            return angle;
        }

        /// <summary>Much more stable for small angles than Unity's native implementation</summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <param name="up">Definition of up (used to determine the sign)</param>
        /// <returns>Rotation between the vectors</returns>
        public static Quaternion SafeFromToRotation(this Vector3 v1, Vector3 v2, Vector3 up)
        {
            var p1 = v1.ProjectOntoPlane(up);
            var p2 = v2.ProjectOntoPlane(up);

            if (p1.sqrMagnitude < Epsilon || p2.sqrMagnitude < Epsilon)
            {
                var axis = Vector3.Cross(v1, v2);
                if (axis.AlmostZero())
                    axis = up; // in case they are pointing in opposite directions

                return Quaternion.AngleAxis(Angle(v1, v2), axis);
            }

            var pitchChange = Vector3.Angle(v2, up) - Vector3.Angle(v1, up);
            return Quaternion.AngleAxis(SignedAngle(p1, p2, up), up)
                * Quaternion.AngleAxis(pitchChange, Vector3.Cross(up, v1).normalized);
        }

        /// <summary>This is a slerp that mimics a camera operator's movement in that
        /// it chooses a path that avoids the lower hemisphere, as defined by
        /// the up param</summary>
        /// <param name="vA">First direction</param>
        /// <param name="vB">Second direction</param>
        /// <param name="t">Interpolation amoun t</param>
        /// <param name="up">Defines the up direction</param>
        /// <returns>Interpolated vector</returns>
        public static Vector3 SlerpWithReferenceUp(
            this Vector3 vA, Vector3 vB, float t, Vector3 up)
        {
            float dA = vA.magnitude;
            float dB = vB.magnitude;
            if (dA < Epsilon || dB < Epsilon)
                return Vector3.Lerp(vA, vB, t);

            Vector3 dirA = vA / dA;
            Vector3 dirB = vB / dB;
            Quaternion qA = Quaternion.LookRotation(dirA, up);
            Quaternion qB = Quaternion.LookRotation(dirB, up);
            Quaternion q = QuaternionExtensions.SlerpWithReferenceUp(qA, qB, t, up);
            Vector3 dir = q * Vector3.forward;
            return dir * Mathf.Lerp(dA, dB, t);
        }

        /// <summary>
        /// Checks if the Vector3 contains NaN for x or y.
        /// </summary>
        /// <param name="v">Vector3 to check for NaN</param>
        /// <returns>True, if any components of the vector are NaN</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }

        /// <summary>
        /// Get the closest point on a line segment.
        /// </summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static float ClosestPointOnSegment(this Vector3 p, Vector3 s0, Vector3 s1)
        {
            Vector3 s = s1 - s0;
            float len2 = Vector3.SqrMagnitude(s);
            if (len2 < Epsilon)
                return 0; // degenrate segment

            return Mathf.Clamp01(Vector3.Dot(p - s0, s) / len2);
        }

        /// <summary>
        /// Returns a non-normalized projection of the supplied vector onto a plane
        /// as described by its normal
        /// </summary>
        /// <param name="vector">The vector to project</param>
        /// <param name="planeNormal">The normal that defines the plane.  Must have a length of 1.</param>
        /// <returns>The component of the vector that lies in the plane</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
        {
            return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
        }

        /// <summary>
        /// Checks whether the vector components are the same value.
        /// </summary>
        /// <param name="v">Vector to check</param>
        /// <returns>True, if the vector elements are the same. False, otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUniform(this Vector3 v)
        {
            return System.Math.Abs(v.x - v.y) < Epsilon && System.Math.Abs(v.x - v.z) < Epsilon;
        }

        /// <summary>Is the vector within Epsilon of zero length?</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostZero(this Vector3 v)
        {
            return v.sqrMagnitude < (Epsilon * Epsilon);
        }
        #endregion // Unity.Cinemachine
    }
}
