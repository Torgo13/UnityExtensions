using System;
using UnityEngine.Profiling;

namespace UnityExtensions
{
    // A helper to simplify profiling methods with complex flow control.
    public readonly struct CustomSamplerScope : IDisposable
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/SamplerScope.cs
        #region Unity.LiveCapture
        readonly CustomSampler m_Sampler;

        public CustomSamplerScope(CustomSampler sampler)
        {
            m_Sampler = sampler;
            m_Sampler.Begin();
        }

        public void Dispose()
        {
            m_Sampler.End();
        }
        #endregion // Unity.LiveCapture
    }
}
