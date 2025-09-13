using System;
using UnityEngine.Profiling;

namespace PKGE
{
    // A helper to simplify profiling methods with complex flow control.
    public readonly struct CustomSamplerScope : IDisposable
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/SamplerScope.cs
        #region Unity.LiveCapture
        readonly CustomSampler _sampler;

        public CustomSamplerScope(CustomSampler sampler)
        {
            _sampler = sampler;
            _sampler.Begin();
        }

        public void Dispose()
        {
            _sampler.End();
        }
        #endregion // Unity.LiveCapture
    }
}
