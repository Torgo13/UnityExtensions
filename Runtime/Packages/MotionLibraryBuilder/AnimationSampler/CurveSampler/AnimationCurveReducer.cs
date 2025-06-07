using UnityEngine;
using UnityEngine.Pool;
using Unity.Mathematics;

namespace UnityExtensions.Packages
{
    public static class AnimationCurveReducer
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/AnimationCurveReducer.cs
        #region CurveSampler
        static public float ReduceWithMaximumAbsoluteError(AnimationCurve curve, float maxError)
        {
            Keyframe[] keys = curve.keys;
            float error = ReduceWithMaximumAbsoluteError(ref keys, maxError);
            curve.keys = keys;
            return error;
        }

        static public float ReduceWithMaximumAbsoluteError(ref Keyframe[] keyframes, float maxError)
        {
            int numKeys = keyframes.Length;
            using var _0 = ListPool<Keyframe>.Get(out var newKeyFrames);
            newKeyFrames.EnsureCapacity(numKeys);
            int prevKeyIndex = 0;
            int currentKeyIndex = 1;
            int nextKeyIndex = 2;
            float conversionError = 0.0f;

            newKeyFrames.Add(keyframes[0]);
            while (nextKeyIndex < (numKeys))
            {
                Keyframe prev = keyframes[prevKeyIndex];
                Keyframe next = keyframes[nextKeyIndex];

                for (int i = currentKeyIndex; i > prevKeyIndex; i--)
                {
                    Keyframe test = keyframes[i];
                    float value = KeyframeUtilities.Evaluate(test.time, prev, next);
                    float error = math.abs(value - test.value);
                    if (error >= maxError)
                    {
                        newKeyFrames.Add(keyframes[currentKeyIndex]);
                        prevKeyIndex = currentKeyIndex;
                    }
                }

                currentKeyIndex++;
                nextKeyIndex++;
            }

            newKeyFrames.Add(keyframes[numKeys - 1]);

            keyframes = newKeyFrames.ToArray();
            return conversionError;
        }

        static public float ReduceWithMaximumLocalRelativeError(ref Keyframe[] keyframes, float maxError)
        {
            int numKeys = keyframes.Length;
            using var _0 = ListPool<Keyframe>.Get(out var newKeyFrames);
            newKeyFrames.EnsureCapacity(numKeys);
            int prevKeyIndex = 0;
            int currentKeyIndex = 1;
            int nextKeyIndex = 2;
            float conversionError = 0.0f;

            newKeyFrames.Add(keyframes[0]);
            while (nextKeyIndex < (numKeys))
            {
                Keyframe prev = keyframes[prevKeyIndex];
                Keyframe next = keyframes[nextKeyIndex];

                float valueRange = math.abs(next.value - prev.value);

                for (int i = currentKeyIndex; i > prevKeyIndex; i--)
                {
                    Keyframe test = keyframes[i];
                    float value = KeyframeUtilities.Evaluate(test.time, prev, next);
                    float error = math.abs(value - test.value) / valueRange;
                    if (error >= maxError)
                    {
                        newKeyFrames.Add(keyframes[currentKeyIndex]);
                        prevKeyIndex = currentKeyIndex;
                    }
                }

                currentKeyIndex++;
                nextKeyIndex++;
            }

            newKeyFrames.Add(keyframes[numKeys - 1]);

            keyframes = newKeyFrames.ToArray();
            return conversionError;
        }

        static public float ReduceWithMaximumRelativeError(ref Keyframe[] keyframes, float maxError)
        {
            int numKeys = keyframes.Length;
            using var _0 = ListPool<Keyframe>.Get(out var newKeyFrames);
            newKeyFrames.EnsureCapacity(numKeys);
            int prevKeyIndex = 0;
            int currentKeyIndex = 1;
            int nextKeyIndex = 2;
            float conversionError = 0.0f;

            newKeyFrames.Add(keyframes[0]);
            while (nextKeyIndex < (numKeys))
            {
                Keyframe prev = keyframes[prevKeyIndex];
                Keyframe cur = keyframes[currentKeyIndex];
                Keyframe next = keyframes[nextKeyIndex];

                float valueRange = math.abs(cur.value) / 2;

                for (int i = currentKeyIndex; i > prevKeyIndex; i--)
                {
                    Keyframe test = keyframes[i];
                    float value = KeyframeUtilities.Evaluate(test.time, prev, next);
                    float error = math.abs(value - test.value) / valueRange;
                    if (error >= maxError)
                    {
                        newKeyFrames.Add(keyframes[currentKeyIndex]);
                        prevKeyIndex = currentKeyIndex;
                    }
                }

                currentKeyIndex++;
                nextKeyIndex++;
            }

            newKeyFrames.Add(keyframes[numKeys - 1]);

            keyframes = newKeyFrames.ToArray();
            return conversionError;
        }
        #endregion // CurveSampler
    }
}
