using Unity.Collections;

namespace PKGE.Packages
{
    public struct FloatSampler
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/FloatSampler.cs
        #region Unity.Kinematica.Editor
        CurveData curveData;
        float defaultValue;

        public bool HasCurve => curveData.IsValid;

        public readonly int Length => curveData.size;

        public readonly float DefaultValue => defaultValue;

        public float this[int index]
        {
            get => HasCurve ? curveData[index].value : defaultValue;
        }

        public float Evaluate(float time)
        {
            return HasCurve ? curveData.ToCurve().Evaluate(time) : defaultValue;
        }

        public Curve SetCurve(Curve curve)
        {
            curveData = new CurveData(curve, Allocator.Persistent);
            return curve;
        }

        public static FloatSampler CreateEmpty(float defaultValue) => new FloatSampler()
        {
            curveData = CurveData.CreateInvalid(),
            defaultValue = defaultValue
        };
        #endregion // Unity.Kinematica.Editor
    }
}
