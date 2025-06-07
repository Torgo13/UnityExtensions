using Unity.Mathematics;

namespace UnityExtensions.Packages
{
    public struct RotationSampler
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/RotationSampler.cs
        #region Unity.Kinematica.Editor
        public FloatSampler x;
        public FloatSampler y;
        public FloatSampler z;
        public FloatSampler w;

        public const int NumCurves = 4;

        public static RotationSampler CreateEmpty(quaternion defaultRotation)
        {
            return new RotationSampler()
            {
                x = FloatSampler.CreateEmpty(defaultRotation.value.x),
                y = FloatSampler.CreateEmpty(defaultRotation.value.y),
                z = FloatSampler.CreateEmpty(defaultRotation.value.z),
                w = FloatSampler.CreateEmpty(defaultRotation.value.w)
            };
        }

        public quaternion DefaultValue => new quaternion(x.DefaultValue, y.DefaultValue, z.DefaultValue, w.DefaultValue);

        public quaternion this[int index] => new quaternion(x[index], y[index], z[index], w[index]);

        public quaternion Evaluate(float sampleTimeInSeconds)
        {
            return new quaternion(
                x.Evaluate(sampleTimeInSeconds),
                y.Evaluate(sampleTimeInSeconds),
                z.Evaluate(sampleTimeInSeconds),
                w.Evaluate(sampleTimeInSeconds));
        }
        #endregion // Unity.Kinematica.Editor
    }
}
