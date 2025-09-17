
#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using PKGE.Mathematics;
using float3 = UnityEngine.Vector3;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public struct HermitCurve
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/HermitCurve.cs
        #region Unity.Kinematica
        public static HermitCurve Create(float3 inPos, float3 inTangent, float3 outPos, float3 outTangent, float curvature)
        {
            float length = math.distance(inPos, outPos);

            HermitCurve curve = new HermitCurve()
            {
                p0 = inPos,
                p1 = outPos,
                m0 = math.normalizesafe(inTangent, float3.zero) * math.min(length, curvature),
                m1 = math.normalizesafe(outTangent, float3.zero) * math.min(length, curvature)
            };

            curve.subdivs = math.max((int)math.floor(length / SubdivLength), 3);
            curve.ComputeCurveLength();

            return curve;
        }

        static float SubdivLength => 1.0f;

        public readonly float3 InPosition => p0;
        public readonly float3 OutPosition => p1;
        public readonly float3 InTangent => m0;
        public readonly float3 OutTangent => m1;

        public readonly float3 SegmentDirection => math.normalizesafe(p1 - p0, float3.zero);

        public readonly float CurveLength => curveLength;

        public readonly float3 EvaluatePosition(float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2.0f * t3 - 3.0f * t2 + 1.0f;
            float h10 = t3 - 2.0f * t2 + t;
            float h01 = -2.0f * t3 + 3.0f * t2;
            float h11 = t3 - t2;

            return p0 * h00 + m0 * h10 + p1 * h01 + m1 * h11;
        }

        public readonly float3 EvaluateTangent(float t)
        {
            float t2 = t * t;

            float h00 = 6.0f * (t2 - t);
            float h10 = 3.0f * t2 - 4.0f * t + 1.0f;
            float h01 = 6.0f * (t - t2);
            float h11 = 3.0f * t2 - 2.0f * t;

            return p0 * h00 + m0 * h10 + p1 * h01 + m1 * h11;
        }

        public SplinePoint EvaluatePointAtDistance(float distance)
        {
            float t = DistanceToTime(distance);
            return new SplinePoint()
            {
                position = EvaluatePosition(t),
                tangent = EvaluateTangent(t),
            };
        }

        public readonly float DistanceToTime(float distance)
        {
            float remainingDistance = distance;

            float dt = 1.0f / subdivs;
            float3 point = EvaluatePosition(0.0f);
            for (int i = 0; i < subdivs; ++i)
            {
                float3 nextPoint = EvaluatePosition((i + 1) * dt);
                float segmentLength = math.distance(point, nextPoint);

                if (segmentLength <= 0.0f)
                {
                    continue;
                }

                if (remainingDistance < segmentLength)
                {
                    return (i + remainingDistance / segmentLength) * dt;
                }

                point = nextPoint;
                remainingDistance -= segmentLength;
            }

            return 1.0f;
        }

        public void ComputeCurveLength()
        {
            curveLength = 0.0f;

            float dt = 1.0f / subdivs;
            float3 point = EvaluatePosition(0.0f);
            for (int i = 0; i < subdivs; ++i)
            {
                float3 nextPoint = EvaluatePosition((i + 1) * dt);
                curveLength += math.distance(point, nextPoint);
                point = nextPoint;
            }
        }

        float3 p0; // start position
        float3 p1; // end position
        float3 m0; // start tangent
        float3 m1; // end tangent

        int     subdivs;
        float   curveLength;
        #endregion // Unity.Kinematica

        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/Hermite.cs
        #region CurveSampler
        public static float Evaluate(float t, float p0, float m0, float m1, float p1)
        {
            // Unrolled the equations to avoid precision issue.
            // (2 * t^3 -3 * t^2 +1) * p0 + (t^3 - 2 * t^2 + t) * m0 + (-2 * t^3 + 3 * t^2) * p1 + (t^3 - t^2) * m1

            var a = 2.0f * p0 + m0 - 2.0f * p1 + m1;
            var b = -3.0f * p0 - 2.0f * m0 + 3.0f * p1 - m1;
            var c = m0;
            var d = p0;

            return t * (t * (a * t + b) + c) + d;
        }
        #endregion // CurveSampler
    }
}
