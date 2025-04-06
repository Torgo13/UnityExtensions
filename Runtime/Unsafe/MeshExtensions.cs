using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityExtensions.Unsafe
{
    public static class MeshExtensions
    {
        #region UnsafeList

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVertices(this Mesh mesh, Unity.Collections.LowLevel.Unsafe.UnsafeList<Vector3> inVertices)
        {
            mesh.SetVertices(inVertices.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUVs<T>(this Mesh mesh, int channel, Unity.Collections.LowLevel.Unsafe.UnsafeList<T> uvs) where T : unmanaged
        {
            mesh.SetUVs(channel, uvs.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetNormals(this Mesh mesh, Unity.Collections.LowLevel.Unsafe.UnsafeList<Vector3> inNormals)
        {
            mesh.SetNormals(inNormals.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, Unity.Collections.LowLevel.Unsafe.UnsafeList<Color> inColors)
        {
            mesh.SetColors(inColors.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, Unity.Collections.LowLevel.Unsafe.UnsafeList<Color32> inColors)
        {
            mesh.SetColors(inColors.AsNativeArray());
        }

        #endregion // UnsafeList
    }
}
