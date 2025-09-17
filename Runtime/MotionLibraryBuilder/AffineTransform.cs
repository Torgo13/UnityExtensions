using System;
using System.Runtime.InteropServices;
using UnityEngine;
using PKGE;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
using static Unity.Mathematics.math;
#else
using PKGE.Mathematics;
using static PKGE.Mathematics.math;
using float3 = UnityEngine.Vector3;
using quaternion = UnityEngine.Quaternion;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct AffineTransform
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Math/AffineTransform.cs
        #region Unity.Mathematics
        public float3 t;
        public quaternion q;

        public AffineTransform(float3 t_, quaternion q_)
        {
            t = t_;
            q = q_;
        }

        public static AffineTransform Create(float3 t, quaternion q)
        {
            return new AffineTransform(t, q);
        }

        public static AffineTransform CreateGlobal(Transform transform)
        {
            return Create(transform.position, transform.rotation);
        }

        public static AffineTransform identity
        {
            get
            {
                return new AffineTransform(
                    new float3(0.0f, 0.0f, 0.0f),
#if INCLUDE_MATHEMATICS
                    Unity.Mathematics.quaternion.identity);
#else
                    quaternion.identity);
#endif // INCLUDE_MATHEMATICS
            }
        }

        public readonly float3 transform(float3 p)
        {
#if INCLUDE_MATHEMATICS
            return rotate(q, p) + t;
#else
            return (float3)rotate(q, p) + t;
#endif // INCLUDE_MATHEMATICS
        }

        public readonly float3 transformDirection(float3 d)
        {
            return rotate(q, d);
        }

        public readonly float3 inverseTransform(float3 p)
        {
            return inverse().transform(p);
        }

        public static AffineTransform operator*(AffineTransform lhs, AffineTransform rhs)
        {
            return new AffineTransform(
                lhs.transform(rhs.t), mul(lhs.q, rhs.q));
        }

        public static AffineTransform operator*(AffineTransform lhs, float scale)
        {
            return new AffineTransform(lhs.t * scale, lhs.q);
        }

        // Return the inverse of this transform
        // v'=R*v+T
        // v'-T=R*v
        // inverse(R)*(v' - T) = inverse(R)*R*v = v
        // v = -inverse(R)*T +inverse(R)*(v')
        public readonly AffineTransform inverse()
        {
            quaternion inverseQ = conjugate(q);
            return new AffineTransform(
                rotate(inverseQ, -t), inverseQ);
        }

        // returns this.Inverse() * rhs
        public readonly AffineTransform inverseTimes(AffineTransform rhs)
        {
            quaternion inverseQ = conjugate(q);
            return new AffineTransform(
                rotate(inverseQ, rhs.t - t),
                mul(inverseQ, rhs.q));
        }

        public readonly AffineTransform alignHorizontally()
        {
            quaternion alignRotation = PKGE.MathematicsExtensions.forRotation(transformDirection(new float3(0.0f, 1.0f, 0.0f)), new float3(0.0f, 1.0f, 0.0f));

            return new AffineTransform(
                t, mul(alignRotation, q));
        }

        public readonly float3 Forward => transformDirection(new float3(0.0f, 0.0f, 1.0f));
#endregion // Unity.Mathematics
    }
}
