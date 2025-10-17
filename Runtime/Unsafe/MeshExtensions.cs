#if INCLUDE_COLLECTIONS
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static class MeshExtensions
    {
        #region UnsafeList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVertices(this Mesh mesh, UnsafeList<Vector3> inVertices)
        {
            mesh.SetVertices(inVertices.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetNormals(this Mesh mesh, UnsafeList<Vector3> inNormals)
        {
            mesh.SetNormals(inNormals.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTangents(this Mesh mesh, UnsafeList<Vector4> inTangents)
        {
            mesh.SetTangents(inTangents.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, UnsafeList<Color> inColors)
        {
            mesh.SetColors(inColors.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, UnsafeList<Color32> inColors)
        {
            mesh.SetColors(inColors.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUVs<T>(this Mesh mesh, int channel, UnsafeList<T> uvs) where T : unmanaged
        {
            mesh.SetUVs(channel, uvs.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIndices(this Mesh mesh, UnsafeList<int> indices, MeshTopology topology,
            int submesh, bool calculateBounds = true, int baseVertex = 0)
        {
            mesh.SetIndices(indices.AsNativeArray(), indicesStart: 0, indices.Length, topology,
                submesh, calculateBounds, baseVertex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIndices(this Mesh mesh, UnsafeList<ushort> indices, MeshTopology topology,
            int submesh, bool calculateBounds = true, int baseVertex = 0)
        {
            mesh.SetIndices(indices.AsNativeArray(), indicesStart: 0, indices.Length, topology,
                submesh, calculateBounds, baseVertex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTriangles<T>(this Mesh mesh, UnsafeList<T> indices,
            int submesh, bool calculateBounds = true, int baseVertex = 0)
            where T : unmanaged
        {
            mesh.SetIndices(indices.AsNativeArray(), indicesStart: 0, indices.Length, MeshTopology.Triangles,
                submesh, calculateBounds, baseVertex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTriangles(this Mesh mesh, UnsafeList<int> indices,
            int submesh, bool calculateBounds = true, int baseVertex = 0)
        {
            mesh.SetIndices(indices.AsNativeArray(), indicesStart: 0, indices.Length, MeshTopology.Triangles,
                submesh, calculateBounds, baseVertex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTriangles(this Mesh mesh, UnsafeList<ushort> indices,
            int submesh, bool calculateBounds = true, int baseVertex = 0)
        {
            mesh.SetIndices(indices.AsNativeArray(), indicesStart: 0, indices.Length, MeshTopology.Triangles,
                submesh, calculateBounds, baseVertex);
        }
        #endregion // UnsafeList
    }
}
#endif // INCLUDE_COLLECTIONS
