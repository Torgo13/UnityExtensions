using System.Threading.Tasks;
using UnityEngine;

namespace PKGE
{
    public static class AwaitableExtensions
    {
#if UNITY_6000_0_OR_NEWER
        //https://docs.unity3d.com/Documentation/Manual/async-awaitable-examples.html
        #region Unity
        public static async Task AsTask(this Awaitable a)
        {
            await a;
        }

        public static async Task<T> AsTask<T>(this Awaitable<T> a)
        {
            return await a;
        }
        #endregion // Unity
#endif // UNITY_6000_0_OR_NEWER
    }
}