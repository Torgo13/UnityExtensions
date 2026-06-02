#if PKGE_USING_UNSAFE
using System.Buffers;
using UnityEngine;
using UnityEngine.Assertions;

namespace PKGE.Unsafe
{
    public static class UITextExtensions
    {
        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/UIExtensionMethods.cs
        #region FPSSample
        public static void Format<T0>(this Text me, string format, T0 arg0)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0);
            me.Set(buf, l);
        }

#if INCLUDE_TEXTMESH_PRO
        public static void Format<T0>(this TMPro.TextMeshProUGUI me, string format, T0 arg0)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0);
            me.Set(buf, l);
        }
#endif // INCLUDE_TEXTMESH_PRO

        public static void Format<T0, T1>(this Text me, string format, T0 arg0, T1 arg1)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1);
            me.Set(buf, l);
        }

#if INCLUDE_TEXTMESH_PRO
        public static void Format<T0, T1>(this TMPro.TextMeshProUGUI me, string format, T0 arg0, T1 arg1)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1);
            me.Set(buf, l);
        }
#endif // INCLUDE_TEXTMESH_PRO

        public static void Format<T0, T1, T2>(this Text me, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1, arg2);
            me.Set(buf, l);
        }

#if INCLUDE_TEXTMESH_PRO
        public static void Format<T0, T1, T2>(this TMPro.TextMeshProUGUI me, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            using var pooledArray = DisposeArrayPool<char>.Rent(1024);
            char[] buf = pooledArray.PooledArray;

            int l = StringFormatter.Write(ref buf, 0, format, arg0, arg1, arg2);
            me.Set(buf, l);
        }
#endif // INCLUDE_TEXTMESH_PRO
        #endregion // FPSSample
    }
}
#endif // PKGE_USING_UNSAFE
