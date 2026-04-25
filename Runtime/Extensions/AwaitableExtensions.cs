using System.Threading.Tasks;
using UnityEngine;

namespace PKGE
{
    public static class AwaitableExtensions
    {
#if UNITY_6000_0_OR_NEWER
        //https://docs.unity3d.com/Documentation/Manual/async-awaitable-examples.html
        #region Unity
        public static async Task AsTask([System.Diagnostics.CodeAnalysis.NotNull] this Awaitable a)
        {
#if SAFETY
            try
            {
                await a;
            }
            catch (System.Exception e)
            {
                if (e is not System.OperationCanceledException)
                    Debug.LogException(e);
            }
#else
            await a;
#endif // SAFETY
        }

        public static async Task<T> AsTask<T>([System.Diagnostics.CodeAnalysis.NotNull] this Awaitable<T> a)
        {
#if SAFETY
            try
            {
                return await a;
            }
            catch (System.Exception e)
            {
                if (e is not System.OperationCanceledException)
                    Debug.LogException(e);
            }

            return default;
#else
            return await a;
#endif // SAFETY
        }
        #endregion // Unity
#endif // UNITY_6000_0_OR_NEWER
    }
}