using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

#if PACKAGE_MATHEMATICS
using Unity.Mathematics;
#endif // PACKAGE_MATHEMATICS

namespace PKGE.Packages
{
    public static class AnimationCurveBake
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/AnimationCurveBake.cs
        #region CurveSampler
        public enum InterpolationMode
        {
            ConstantPre,
            ConstantPost,
            Linear,
            Auto,
            ClampedAuto
        };

        public struct SampleRange
        {
            public int startFrameIndex;
            public int numFrames;
        }

        public static int Bake(AnimationCurve curve, float frameRate, InterpolationMode mode)
        {
            var keys = new NativeArray<Keyframe>(curve.keys, Allocator.Temp);

            float duration = keys[keys.Length - 1].time - keys[0].time;
            int frameCount = (int)math.ceil(frameRate * duration);
            var bakedKeys = new NativeArray<Keyframe>(frameCount, Allocator.Temp);

            switch (mode)
            {
                case InterpolationMode.Linear:
                    KeyframeUtilities.AlignTangentsLinear(bakedKeys);
                    break;
                case InterpolationMode.Auto:
                    KeyframeUtilities.AlignTangentsSmooth(bakedKeys);
                    break;
                case InterpolationMode.ClampedAuto:
                    KeyframeUtilities.AlignTangentsClamped(bakedKeys);
                    break;
                default:
                    throw new System.InvalidOperationException("Not Implemented");
            }

            curve.keys = bakedKeys.ToArray();
            keys.Dispose();
            bakedKeys.Dispose();
            return frameCount;
        }

        public static int Bake(ref Keyframe[] keys, float frameRate, InterpolationMode mode = InterpolationMode.Auto)
        {
            var nativeKeys = new NativeArray<Keyframe>(keys, Allocator.Temp);
            float duration = keys[keys.Length - 1].time - keys[0].time;
            int frameCount = (int)math.ceil(frameRate * duration);
            var bakedKeys = new NativeArray<Keyframe>(frameCount, Allocator.Temp);
            var sampleRange = new SampleRange() { startFrameIndex = 0, numFrames = frameCount };
            
            ThreadSafe.Bake(ref nativeKeys, frameRate, ref bakedKeys, ref sampleRange);

            switch (mode)
            {
                case InterpolationMode.Linear:
                    KeyframeUtilities.AlignTangentsLinear(bakedKeys);
                    break;
                case InterpolationMode.Auto:
                    KeyframeUtilities.AlignTangentsSmooth(bakedKeys);
                    break;
                case InterpolationMode.ClampedAuto:
                    KeyframeUtilities.AlignTangentsClamped(bakedKeys);
                    break;
                default:
                    throw new System.InvalidOperationException("Not Implemented");
            }

            keys = bakedKeys.ToArray();
            bakedKeys.Dispose();
            return frameCount;
        }

        public static class ThreadSafe
        {
#if PACKAGE_BURST
            [Unity.Burst.BurstCompile(CompileSynchronously = true)]
#endif // PACKAGE_BURST
            public struct BakeJob : IJobParallelFor
            {
                [ReadOnly] public NativeArray<NativeArray<Keyframe>> curves;
                [NativeDisableParallelForRestriction]
                public NativeArray<NativeArray<Keyframe>> outCurves;
                [ReadOnly] public SampleRange sampleRange;

                [ReadOnly] public float frameRate;

                public void Execute(int index)
                {
                    NativeArray<Keyframe> keyframes = curves[index];
                    NativeArray<Keyframe> bakedFrames = outCurves[index];

                    //Bake(ref curve, frameRate, ref outCurve, ref sampleRange);
                    int numKeys = keyframes.Length;
                    float start = keyframes[0].time;
                    float end = keyframes[numKeys - 1].time;
                    //float duration = end - start;
                    float frame = 1 / frameRate;
                    //int numFrames = bakedFrames.Length;

                    for (int i = 0; i < sampleRange.numFrames; i++)
                    {
                        int frameIndex = sampleRange.startFrameIndex + i;
                        float time = math.clamp(start + frameIndex * frame, start, end);

                        CurveSampling.ThreadSafe.EvaluateWithinRange(keyframes, time, 0, numKeys - 1,
                            out float value);

                        bakedFrames[frameIndex] = new Keyframe(time, value);
                    }
                }
            }

            public static BakeJob ConfigureBake(Curve[] curves, float frameRate, Curve[] outCurves, Allocator alloc, SampleRange sampleRange)
            {
                var job = new BakeJob();
                job.curves = new NativeArray<NativeArray<Keyframe>>(curves.Length, alloc);
                job.frameRate = frameRate;
                job.outCurves = new NativeArray<NativeArray<Keyframe>>(curves.Length, alloc);
                job.sampleRange = sampleRange;

                for (int i = 0; i < curves.Length; i++)
                {
                    job.curves[i] = new CurveData(curves[i], alloc).array;
                    //int frameCount = (int)math.ceil(frameRate * curves[i].Duration);
                    //var outCurve = new Curve(frameCount, alloc);
                    //outCurves[i] = outCurve;
                    job.outCurves[i] = new CurveData(outCurves[i], alloc).array;
                }

                return job;
            }

            public static BakeJob ConfigureBake(Curve[] curves, float frameRate, Curve[] outCurves, Allocator alloc)
            {
                SampleRange sampleRange = new SampleRange()
                {
                    startFrameIndex = 0,
                    numFrames = curves[0].Length
                };

                return ConfigureBake(curves, frameRate, outCurves, alloc, sampleRange);
            }

            public static void Bake(ref Curve curve, float frameRate, ref Curve outCurve, ref SampleRange sampleRange)
            {
                NativeArray<Keyframe> keys = curve.Keys;
                NativeArray<Keyframe> outKeys = outCurve.Keys;
                Bake(ref keys, frameRate, ref outKeys, ref sampleRange);
                //  KeyframeUtilities.AlignTangentsSmooth(outKeys, sampleRange);
            }

            public static void Bake(ref NativeArray<Keyframe> keyframes, float frameRate, ref NativeArray<Keyframe> bakedFrames, ref SampleRange sampleRange)
            {
                int numKeys = keyframes.Length;
                float start = keyframes[0].time;
                float end = keyframes[numKeys - 1].time;
                //float duration = end - start;
                float frame = 1 / frameRate;
                //int numFrames = bakedFrames.Length;

                for (int i = 0; i < sampleRange.numFrames; i++)
                {
                    int frameIndex = sampleRange.startFrameIndex + i;
                    float time = math.clamp(start + frameIndex * frame, start, end);

                    CurveSampling.ThreadSafe.EvaluateWithinRange(keyframes, time, 0, numKeys - 1,
                        out float value);

                    bakedFrames[frameIndex] = new Keyframe(time, value);
                }
            }
        }
        #endregion // CurveSampler
    }
}
