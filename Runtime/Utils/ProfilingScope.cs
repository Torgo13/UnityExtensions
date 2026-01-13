// TProfilingSampler<TEnum>.Samples should just be an array. Unfortunately, Enum cannot be converted to int without generating garbage.
// This could be worked around by using Unsafe, but it's not available at the moment.
// So in the meantime we use a Dictionary with a perf hit...
#define USE_UNSAFE

#if UNITY_2020_1_OR_NEWER
#define UNITY_USE_RECORDER
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Unity.Profiling;

namespace PKGE
{
    class TProfilingSampler<TEnum> : ProfilingSampler where TEnum : Enum
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Debugging/ProfilingScope.cs
        #region UnityEngine.Rendering
#if USE_UNSAFE
        internal static readonly TProfilingSampler<TEnum>[] Samples;
#else
        internal static readonly Dictionary<TEnum, TProfilingSampler<TEnum>> Samples = new Dictionary<TEnum, TProfilingSampler<TEnum>>();
#endif
        static TProfilingSampler()
        {
            var names = EnumValues<TEnum>.Names;
#if USE_UNSAFE
            UnityEngine.Assertions.Assert.AreEqual(sizeof(int), SizeOfCache<TEnum>.Size);
            var enumValues = EnumValues<TEnum>.Values;
            var values = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<TEnum[], int[]>(ref enumValues);
            Samples = new TProfilingSampler<TEnum>[values.Max() + 1];
#else
            var values = Enum.GetValues(typeof(TEnum));
#endif

            for (int i = 0; i < names.Length; i++)
            {
                var sample = new TProfilingSampler<TEnum>(names[i]);
#if USE_UNSAFE
                Samples[values[i]] = sample;
#else
                Samples.Add((TEnum)values.GetValue(i), sample);
#endif
            }
        }

        public TProfilingSampler(string name)
            : base(name)
        {
        }
        #endregion // UnityEngine.Rendering
    }

    /// <summary>
    /// Wrapper around CPU and GPU profiling samplers.
    /// Use this along ProfilingScope to profile a piece of code.
    /// </summary>
    [IgnoredByDeepProfiler]
    public class ProfilingSampler
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Debugging/ProfilingScope.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Get the sampler for the corresponding enumeration value.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enumeration.</typeparam>
        /// <param name="marker">Enumeration value.</param>
        /// <returns>The profiling sampler for the given enumeration value.</returns>
#if DEBUG
        public static ProfilingSampler Get<TEnum>(TEnum marker)
            where TEnum : Enum
        {
#if USE_UNSAFE
            UnityEngine.Assertions.Assert.AreEqual(sizeof(int), SizeOfCache<TEnum>.Size);
            return TProfilingSampler<TEnum>.Samples[Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<TEnum, int>(ref marker)];
#else
            TProfilingSampler<TEnum>.Samples.TryGetValue(marker, out var sampler);
            return sampler;
#endif
        }
#else
        public static ProfilingSampler Get<TEnum>(TEnum marker)
            where TEnum : Enum
        {
            return null;
        }
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the profiling sampler.</param>
        public ProfilingSampler(string name)
        {
            // Caution: Name of sampler MUST not match name provide to cmd.BeginSample(), otherwise
            // we get a mismatch of marker when enabling the profiler.
#if UNITY_USE_RECORDER
            sampler = CustomSampler.Create(name, true); // Event markers, command buffer CPU profiling and GPU profiling
#else
            // In this case, we need to use the BeginSample(string) API, since it creates a new sampler by that name under the hood,
            // we need rename this sampler to not clash with the implicit one (it won't be used in this case)
            sampler = CustomSampler.Create($"Dummy_{name}");
#endif
            inlineSampler = CustomSampler.Create($"Inl_{name}"); // Profiles code "immediately"
            this.name = name;

#if UNITY_USE_RECORDER
            _recorder = sampler.GetRecorder();
            _recorder.enabled = false;
            _inlineRecorder = inlineSampler.GetRecorder();
            _inlineRecorder.enabled = false;
#endif
        }

        /// <summary>
        /// Begin the profiling block.
        /// </summary>
        /// <param name="cmd">Command buffer used by the profiling block.</param>
        public void Begin(CommandBuffer cmd)
        {
            if (cmd != null)
#if UNITY_USE_RECORDER
                if (sampler != null && sampler.isValid)
                    cmd.BeginSample(sampler);
                else
                    cmd.BeginSample(name);
#else
                cmd.BeginSample(name);
#endif
            inlineSampler?.Begin();
        }

        /// <summary>
        /// End the profiling block.
        /// </summary>
        /// <param name="cmd">Command buffer used by the profiling block.</param>
        public void End(CommandBuffer cmd)
        {
            if (cmd != null)
#if UNITY_USE_RECORDER
                if (sampler != null && sampler.isValid)
                    cmd.EndSample(sampler);
                else
                    cmd.EndSample(name);
#else
                _cmd.EndSample(name);
#endif
            inlineSampler?.End();
        }

        internal bool IsValid() { return (sampler != null && inlineSampler != null); }

        internal CustomSampler sampler { get; private set; }
        internal CustomSampler inlineSampler { get; private set; }
        /// <summary>
        /// Name of the Profiling Sampler
        /// </summary>
        public string name { get; private set; }

#if UNITY_USE_RECORDER
        readonly Recorder _recorder;
        readonly Recorder _inlineRecorder;
#endif

        /// <summary>
        /// Set to true to enable recording of profiling sampler timings.
        /// </summary>
        public bool enableRecording
        {
            set
            {
#if UNITY_USE_RECORDER
                _recorder.enabled = value;
                _inlineRecorder.enabled = value;
#endif
            }
        }

#if UNITY_USE_RECORDER
        /// <summary>
        /// GPU Elapsed time in milliseconds.
        /// </summary>
        public float gpuElapsedTime => _recorder.enabled ? _recorder.gpuElapsedNanoseconds / 1000000.0f : 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the GPU
        /// </summary>
        public int gpuSampleCount => _recorder.enabled ? _recorder.gpuSampleBlockCount : 0;
        /// <summary>
        /// CPU Elapsed time in milliseconds (Command Buffer execution).
        /// </summary>
        public float cpuElapsedTime => _recorder.enabled ? _recorder.elapsedNanoseconds / 1000000.0f : 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the CPU in the command buffer.
        /// </summary>
        public int cpuSampleCount => _recorder.enabled ? _recorder.sampleBlockCount : 0;
        /// <summary>
        /// CPU Elapsed time in milliseconds (Direct execution).
        /// </summary>
        public float inlineCpuElapsedTime => _inlineRecorder.enabled ? _inlineRecorder.elapsedNanoseconds / 1000000.0f : 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the CPU.
        /// </summary>
        public int inlineCpuSampleCount => _inlineRecorder.enabled ? _inlineRecorder.sampleBlockCount : 0;
#else
        /// <summary>
        /// GPU Elapsed time in milliseconds.
        /// </summary>
        public float gpuElapsedTime => 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the GPU
        /// </summary>
        public int gpuSampleCount => 0;
        /// <summary>
        /// CPU Elapsed time in milliseconds (Command Buffer execution).
        /// </summary>
        public float cpuElapsedTime => 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the CPU in the command buffer.
        /// </summary>
        public int cpuSampleCount => 0;
        /// <summary>
        /// CPU Elapsed time in milliseconds (Direct execution).
        /// </summary>
        public float inlineCpuElapsedTime => 0.0f;
        /// <summary>
        /// Number of times the Profiling Sampler has hit on the CPU.
        /// </summary>
        public int inlineCpuSampleCount => 0;
#endif
        // Keep the constructor private
        ProfilingSampler() { }
        #endregion // UnityEngine.Rendering
    }

#if DEBUG
    /// <summary>
    /// Scoped Profiling markers
    /// </summary>
    [IgnoredByDeepProfiler]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct ProfilingScope : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Debugging/ProfilingScope.cs
        #region UnityEngine.Rendering
        readonly CommandBuffer _cmd;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U1)]
        bool _disposed;
        readonly ProfilingSampler _sampler;

        /// <summary>
        /// Profiling Scope constructor
        /// </summary>
        /// <param name="sampler">Profiling Sampler to be used for this scope.</param>
        public ProfilingScope(ProfilingSampler sampler)
        {
            _cmd = null;
            _disposed = false;
            _sampler = sampler;
            _sampler?.Begin(_cmd);
        }

        /// <summary>
        /// Profiling Scope constructor
        /// </summary>
        /// <param name="cmd">Command buffer used to add markers and compute execution timings.</param>
        /// <param name="sampler">Profiling Sampler to be used for this scope.</param>
        public ProfilingScope(CommandBuffer cmd, ProfilingSampler sampler)
        {
            // NOTE: Do not mix with named CommandBuffers.
            // Currently, there's an issue which results in mismatched markers.
            // The named CommandBuffer will close its "profiling scope" on execution.
            // That will orphan ProfilingScope markers as the named CommandBuffer marker
            // is their "parent".
            // Resulting in following pattern:
            // exec(cmd.start, scope.start, cmd.end) and exec(cmd.start, scope.end, cmd.end)
            _cmd = cmd;
            _disposed = false;
            _sampler = sampler;
            _sampler?.Begin(_cmd);
        }

        /// <summary>
        ///  Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        // Protected implementation of Dispose pattern.
        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // As this is a struct, it could have been initialized using an empty constructor so we
            // need to make sure `cmd` isn't null to avoid a crash. Switching to a class would fix
            // this but will generate garbage on every frame (and this struct is used quite a lot).
            if (disposing)
            {
                _sampler?.End(_cmd);
            }

            _disposed = true;
        }
        #endregion // UnityEngine.Rendering
    }
#else
    /// <summary>
    /// Scoped Profiling markers
    /// </summary>
    public struct ProfilingScope : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Debugging/ProfilingScope.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Profiling Scope constructor
        /// </summary>
        /// <param name="sampler">Profiling Sampler to be used for this scope.</param>
        public ProfilingScope(ProfilingSampler sampler)
        {
        }

        /// <summary>
        /// Profiling Scope constructor
        /// </summary>
        /// <param name="cmd">Command buffer used to add markers and compute execution timings.</param>
        /// <param name="sampler">Profiling Sampler to be used for this scope.</param>
        public ProfilingScope(CommandBuffer cmd, ProfilingSampler sampler)
        {
        }

        /// <summary>
        ///  Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
        }
        #endregion // UnityEngine.Rendering
    }
#endif
}
