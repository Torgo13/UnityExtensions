using Unity.Collections;

namespace PKGE.Packages
{
    public struct FloatSampler
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/FloatSampler.cs
        #region Unity.Kinematica.Editor
        Curve curve;
        readonly float defaultValue;

        public FloatSampler(Curve curve, float defaultValue)
        {
            this.curve = curve;
            this.defaultValue = defaultValue;
        }

        public bool HasCurve => curve.Keys.IsCreated;

        public readonly int Length => curve.Length;

        public readonly float DefaultValue => defaultValue;

        public float this[int index]
        {
            get => HasCurve ? curve.Keys[index].value : defaultValue;
        }

        public float Evaluate(float time)
        {
            return HasCurve ? curve.Evaluate(time) : defaultValue;
        }

        public Curve SetCurve(Curve curve)
        {
            this.curve = curve;
            return this.curve;
        }

        public static FloatSampler CreateEmpty(float defaultValue) => new FloatSampler(default, defaultValue);
        #endregion // Unity.Kinematica.Editor
    }
}
