#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using float3 = UnityEngine.Vector3;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public struct PositionSampler
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/PositionSampler.cs
        #region Unity.Kinematica.Editor
        public FloatSampler x;
        public FloatSampler y;
        public FloatSampler z;

        public const int NumCurves = 3;

        public static PositionSampler CreateEmpty(float3 defaultPosition)
        {
            return new PositionSampler()
            {
                x = FloatSampler.CreateEmpty(defaultPosition.x),
                y = FloatSampler.CreateEmpty(defaultPosition.y),
                z = FloatSampler.CreateEmpty(defaultPosition.z)
            };
        }

        public float3 DefaultPosition => new float3(x.DefaultValue, y.DefaultValue, z.DefaultValue);

        public float3 this[int index] => new float3(x[index], y[index], z[index]);

        public float3 Evaluate(float sampleTimeInSeconds)
        {
            return new float3(
                x.Evaluate(sampleTimeInSeconds),
                y.Evaluate(sampleTimeInSeconds),
                z.Evaluate(sampleTimeInSeconds));
        }
        #endregion // Unity.Kinematica.Editor
    }
}
