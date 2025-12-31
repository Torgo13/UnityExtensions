using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using PKGE.Mathematics;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public static class AnimationCurveBake
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/AnimationCurveBake.cs
        #region CurveSampler
        public enum InterpolationMode : byte
        {
            ConstantPre,
            ConstantPost,
            Linear,
            Auto,
            ClampedAuto
        };

        public readonly struct SampleRange
        {
            public readonly int startFrameIndex;
            public readonly int numFrames;

            public SampleRange(int startFrameIndex, int numFrames)
            {
                this.startFrameIndex = startFrameIndex;
                this.numFrames = numFrames;
            }
        }

        public static int Bake(AnimationCurve curve, float frameRate, InterpolationMode mode = InterpolationMode.Auto)
        {
            var keys = new NativeArray<Keyframe>(curve.keys, Allocator.Temp);
            return Bake(ref keys, frameRate, mode);
        }

        public static int Bake(ref Keyframe[] keys, float frameRate, InterpolationMode mode = InterpolationMode.Auto)
        {
            var nativeKeys = new NativeArray<Keyframe>(keys, Allocator.Temp);
            int frameCount = Bake(ref nativeKeys, frameRate, mode);
            nativeKeys.CopyTo(keys);
            return frameCount;
        }

        /// <exception cref="System.InvalidOperationException">Not Implemented</exception>
        public static int Bake(ref NativeArray<Keyframe> keys, float frameRate, in InterpolationMode mode = InterpolationMode.Auto)
        {
            switch (mode)
            {
                case InterpolationMode.Linear:
                case InterpolationMode.Auto:
                case InterpolationMode.ClampedAuto:
                    break;
                default:
                    throw new System.InvalidOperationException("Not Implemented");
            }

            var result = new NativeArray<int>(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            new BakeJob
            {
                result = result,
                keys = keys,
                frameRate = frameRate,
                mode = mode,
            }.Run();

            int frameCount = result[0];
            result.Dispose();
            return frameCount;
        }

        [Unity.Burst.BurstCompile]
        private struct BakeJob : IJob
        {
            [WriteOnly] public NativeArray<int> result;
            public NativeArray<Keyframe> keys;
            public float frameRate;
            public InterpolationMode mode;

            public void Execute()
            {
                float duration = keys[keys.Length - 1].time - keys[0].time;
                int frameCount = (int)math.ceil(frameRate * duration);
                var bakedKeys = new NativeArray<Keyframe>(frameCount, Allocator.Temp);
                var sampleRange = new SampleRange(startFrameIndex: 0, numFrames: frameCount);

                ThreadSafe.Bake(keys, frameRate, ref bakedKeys, sampleRange);

                switch (mode)
                {
                    case InterpolationMode.Linear:
                        KeyframeUtilities.AlignTangentsLinear(ref bakedKeys);
                        break;
                    case InterpolationMode.Auto:
                        KeyframeUtilities.AlignTangentsSmooth(ref bakedKeys);
                        break;
                    case InterpolationMode.ClampedAuto:
                        KeyframeUtilities.AlignTangentsClamped(ref bakedKeys);
                        break;
                    default:
                        throw new System.InvalidOperationException("Not Implemented");
                }

                keys.CopyFrom(bakedKeys);
                bakedKeys.Dispose();
                result[0] = frameCount;
            }
        }

        public static class ThreadSafe
        {
            [Unity.Burst.BurstCompile(CompileSynchronously = true)]
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

            public static BakeJob ConfigureBake(Curve[] curves, float frameRate, Curve[] outCurves, Allocator alloc, in SampleRange sampleRange)
            {
                return ConfigureBake(new NativeArray<Curve>(curves, Allocator.Temp), frameRate,
                    new NativeArray<Curve>(outCurves, Allocator.Temp), alloc, sampleRange);
            }

            /// <remarks>Dispose of <see cref="BakeJob.curves"/> and <see cref="BakeJob.outCurves"/>
            /// after completing <see cref="BakeJob"/>.</remarks>
            public static BakeJob ConfigureBake(in NativeArray<Curve> curves, float frameRate, in NativeArray<Curve> outCurves, Allocator alloc, in SampleRange sampleRange)
            {
                var job = new BakeJob();
                job.curves = new NativeArray<NativeArray<Keyframe>>(curves.Length, alloc);
                job.frameRate = frameRate;
                job.outCurves = new NativeArray<NativeArray<Keyframe>>(curves.Length, alloc);
                job.sampleRange = sampleRange;

                for (int i = 0; i < curves.Length; i++)
                {
                    job.curves[i] = curves[i].Keys;
                    //int frameCount = (int)math.ceil(frameRate * curves[i].Duration);
                    //var outCurve = new Curve(frameCount, alloc);
                    //outCurves[i] = outCurve;
                    job.outCurves[i] = outCurves[i].Keys;
                }

                return job;
            }

            public static BakeJob ConfigureBake(Curve[] curves, float frameRate, Curve[] outCurves, Allocator alloc)
            {
                return ConfigureBake(new NativeArray<Curve>(curves, Allocator.Temp), frameRate,
                    new NativeArray<Curve>(outCurves, Allocator.Temp), alloc);
            }
            
            public static BakeJob ConfigureBake(NativeArray<Curve> curves, float frameRate, NativeArray<Curve> outCurves, Allocator alloc)
            {
                SampleRange sampleRange = new SampleRange(startFrameIndex: 0, numFrames: curves[0].Length);
                return ConfigureBake(curves, frameRate, outCurves, alloc, sampleRange);
            }

            public static void Bake(ref Curve curve, float frameRate, ref Curve outCurve, in SampleRange sampleRange)
            {
                NativeArray<Keyframe> keys = curve.Keys;
                NativeArray<Keyframe> outKeys = outCurve.Keys;
                Bake(keys, frameRate, ref outKeys, sampleRange);
                //  KeyframeUtilities.AlignTangentsSmooth(outKeys, sampleRange);
            }

            public static void Bake(in NativeArray<Keyframe> keyframes, float frameRate, ref NativeArray<Keyframe> bakedFrames, in SampleRange sampleRange)
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
