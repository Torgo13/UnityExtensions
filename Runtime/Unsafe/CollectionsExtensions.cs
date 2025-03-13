using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class CollectionsExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPointer<T>(this NativeArray<T> array) where T : struct
        {
            return array.Length == 0 ? null : array.GetUnsafePtr();
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
        
        public static unsafe void* GetReadOnlyPointer<T>(this NativeArray<T> array) where T : struct
        {
            return array.Length == 0 ? null : array.GetUnsafeReadOnlyPtr();
        }
    }
}
