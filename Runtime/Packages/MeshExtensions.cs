using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using static Unity.Mathematics.math;
using IndexFormat = UnityEngine.Rendering.IndexFormat;

namespace UnityExtensions.Packages
{
    public static class MeshExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.probuilder/blob/master/Runtime/Core/MeshUtility.cs
        #region UnityEngine.ProBuilder
        /// <summary>
        /// Generates tangents and applies them on the specified mesh.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> mesh target.</param>
        public static void GenerateTangent(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            // http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

            // speed up math by copying the mesh arrays
            using var _0 = ListPool<int>.Get(out var triangles);
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh.GetTriangles(triangles, i); // TODO Check if this appends to the List or clears it
            }

            using var _1 = ListPool<Vector3>.Get(out var vertices);
            mesh.GetVertices(vertices);

            using var _2 = ListPool<Vector2>.Get(out var uv);
            for (int i = 0; i < 8; i++)
            {
                mesh.GetUVs(i, uv);
            }

            using var _3 = ListPool<Vector3>.Get(out var normals);
            mesh.GetNormals(normals);

            //variable definitions
            int triangleCount = triangles.Count;
            int vertexCount = vertices.Count;

            var tan1 = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
            var tan2 = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
            var tangents = new NativeArray<Vector4>(vertexCount, Allocator.Temp);

            for (int a = 0; a < triangleCount; a += 3)
            {
                int i1 = triangles[a + 0];
                int i2 = triangles[a + 1];
                int i3 = triangles[a + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (int a = 0; a < vertexCount; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];

                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a] = new Vector4(
                    t.x,
                    t.y,
                    t.z,
                    (dot(cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f
                );
            }

            mesh.SetTangents(tangents);
        }

        /// <summary>
        /// Returns a new mesh containing all attributes and values copied from the specified source mesh.
        /// </summary>
        /// <param name="source">The mesh to copy from.</param>
        /// <returns>A new <see cref="Mesh"/> object with the same values as the source mesh.</returns>
        public static Mesh DeepCopy(Mesh source)
        {
            Mesh m = new Mesh();
            CopyTo(source, m);
            return m;
        }

        /// <summary>
        /// Copies mesh attribute values from one mesh to another.
        /// </summary>
        /// <param name="source">The mesh from which to copy attribute values.</param>
        /// <param name="destination">The destination mesh to copy attribute values to.</param>
        /// <exception cref="ArgumentNullException">Throws if source or destination is null.</exception>
        public static void CopyTo(Mesh source, Mesh destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.Clear();
            destination.name = source.name;

            using (ListPool<Vector3>.Get(out var v))
            {
                source.GetVertices(v);
                destination.SetVertices(v);

                v.Clear();

                source.GetNormals(v);
                destination.SetNormals(v);
            }

            using (ListPool<int>.Get(out var t))
            {
                int subMeshCount = source.subMeshCount;
                destination.subMeshCount = subMeshCount;
                for (int i = 0; i < subMeshCount; i++)
                {
                    source.GetTriangles(t, i);
                    destination.SetTriangles(t, i);

                    t.Clear();
                }
            }

            using (ListPool<Vector2>.Get(out var u))
            {
                for (int i = 0; i < 8; i++)
                {
                    source.GetUVs(i, u);
                    destination.SetUVs(i, u);

                    u.Clear();
                }
            }

            using (ListPool<Vector4>.Get(out var tan))
            {
                source.GetTangents(tan);
                destination.SetTangents(tan);
            }

            using (ListPool<Color32>.Get(out var c))
            {
                source.GetColors(c);
                destination.SetColors(c);
            }
        }

        /// <summary>
        /// Get a mesh attribute from either the MeshFilter.sharedMesh or the MeshRenderer.additionalVertexStreams mesh. The additional vertex stream mesh has priority.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to fetch.</typeparam>
        /// <param name="gameObject">The GameObject with the MeshFilter and (optional) MeshRenderer to search for mesh attributes.</param>
        /// <param name="attributeGetter">The function used to extract mesh attribute.</param>
        /// <returns>A List of the mesh attribute values from the Additional Vertex Streams mesh if it exists and contains the attribute, or the MeshFilter.sharedMesh attribute values.</returns>
        public static T GetMeshChannel<T>(GameObject gameObject, Func<Mesh, T> attributeGetter) where T : IList
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (attributeGetter == null)
                throw new ArgumentNullException(nameof(attributeGetter));

            Mesh mesh = gameObject.TryGetComponent<MeshFilter>(out var mf) ? mf.sharedMesh : null;
            T res = default(T);

            if (mesh == null)
                return res;

            int vertexCount = mesh.vertexCount;

            Mesh vertexStream = gameObject.TryGetComponent<MeshRenderer>(out var renderer) ? renderer.additionalVertexStreams : null;

            if (vertexStream != null)
            {
                res = attributeGetter(vertexStream);

                if (res != null && res.Count == vertexCount)
                    return res;
            }

            res = attributeGetter(mesh);

            return res != null && res.Count == vertexCount ? res : default(T);
        }

        static void PrintAttribute<T>(StringBuilder sb, string title, List<T> attrib, string fmt)
        {
            if (attrib == null)
            {
                _ = sb.Append("  - ")
                    .AppendLine(title)
                    .Append(' ')
                    .Append('(')
                    .Append(')')
                    .AppendLine()
                    .AppendLine("\tnull");
                return;
            }

            //using var _0 = ListPool<T>.Get(out var list);
            //list.AddRange(attrib);

            _ = sb.Append("  - ")
                .Append(title)
                .Append(' ')
                .Append('(')
                .Append(attrib.Count)
                .Append(')')
                .AppendLine();
            //sb.AppendLine(title);

            if (attrib.Count > 0)
            {
                foreach (var value in attrib)
                {
                    _ = sb.Append("    ")
                        .AppendFormat($"    {fmt}", value)
                        .AppendLine();
                    //sb.AppendLine(string.Format($"    {fmt}", value));
                }
            }
            else
            {
                _ = sb.AppendLine("\tnull");
            }
        }

        /// <summary>
        /// Prints a detailed string summary of the mesh attributes.
        /// </summary>
        /// <param name="mesh">The mesh to print information for.</param>
        /// <returns>A tab-delimited string (positions, normals, colors, tangents, and UV coordinates).</returns>
        public static string Print(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            using var _0 = StringBuilderPool.Get(out var sb);

            using var _1 = ListPool<Vector3>.Get(out var positions);
            mesh.GetVertices(positions);
            using var _2 = ListPool<Vector3>.Get(out var normals);
            mesh.GetNormals(normals);
            using var _3 = ListPool<Color>.Get(out var colors);
            mesh.GetColors(colors);
            using var _4 = ListPool<Vector4>.Get(out var tangents);
            mesh.GetTangents(tangents);
            using var _5 = ListPool<Vector4>.Get(out var uv0);
            using var _6 = ListPool<Vector2>.Get(out var uv2);
            mesh.GetUVs(1, uv2);
            using var _7 = ListPool<Vector4>.Get(out var uv3);
            using var _8 = ListPool<Vector4>.Get(out var uv4);

            mesh.GetUVs(0, uv0);
            mesh.GetUVs(2, uv3);
            mesh.GetUVs(3, uv4);

            _ = sb.AppendLine("# Sanity Check");
            //sb.AppendLine(MeshUtility.SanityCheck(mesh));

            _ = sb.Append("# Attributes (")
                .Append(mesh.vertexCount)
                .Append(')')
                .AppendLine();

            PrintAttribute(sb, "positions", positions, "pos: {0:F2}");
            PrintAttribute(sb, "normals", normals, "nrm: {0:F2}");
            PrintAttribute(sb, "colors", colors, "col: {0:F2}");
            PrintAttribute(sb, "tangents", tangents, "tan: {0:F2}");
            PrintAttribute(sb, "uv0", uv0, "uv0: {0:F2}");
            PrintAttribute(sb, "uv2", uv2, "uv2: {0:F2}");
            PrintAttribute(sb, "uv3", uv3, "uv3: {0:F2}");
            PrintAttribute(sb, "uv4", uv4, "uv4: {0:F2}");

            _ = sb.AppendLine("# Topology");

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var topo = mesh.GetTopology(i);
                var submesh = mesh.GetIndices(i);
                _ = sb.Append("  Submesh[")
                    .Append(i)
                    .Append("] (")
                    .Append(topo)
                    .Append(')')
                    .AppendLine();
                //sb.AppendLine($"  Submesh[{i}] ({topo})");

                switch (topo)
                {
                    case MeshTopology.Points:
                        for (int n = 0; n < submesh.Length; n += 1)
                        {
                            _ = sb.Append('\t')
                                .Append(submesh[n]);
                            //sb.AppendLine(string.Format("\t{0}", submesh[n]));
                        }

                        break;
                    case MeshTopology.Lines:
                        for (int n = 0; n < submesh.Length; n += 2)
                        {
                            _ = sb.Append('\t')
                                .Append(submesh[n])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 1]);
                            //sb.AppendLine(string.Format("\t{0}, {1}", submesh[n], submesh[n + 1]));
                        }

                        break;
                    case MeshTopology.Triangles:
                        for (int n = 0; n < submesh.Length; n += 3)
                        {
                            _ = sb.Append('\t')
                                .Append(submesh[n])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 1])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 2]);
                            //sb.AppendLine(string.Format("\t{0}, {1}, {2}", submesh[n], submesh[n + 1], submesh[n + 2]));
                        }

                        break;
                    case MeshTopology.Quads:
                        for (int n = 0; n < submesh.Length; n += 4)
                        {
                            _ = sb.Append('\t')
                                .Append(submesh[n])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 1])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 2])
                                .Append(',')
                                .Append(' ')
                                .Append(submesh[n + 3]);
                            //sb.AppendLine(string.Format("\t{0}, {1}, {2}, {3}", submesh[n], submesh[n + 1], submesh[n + 2], submesh[n + 3]));
                        }

                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the number of indices this mesh contains.
        /// </summary>
        /// <param name="mesh">The source mesh to sum submesh index counts from.</param>
        /// <returns>The count of all indices contained within this mesh's submeshes.</returns>
        public static uint GetIndexCount(Mesh mesh)
        {
            uint sum = 0;

            if (mesh == null)
                return sum;

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
                sum += mesh.GetIndexCount(i);

            return sum;
        }

        /// <summary>
        /// Returns the number of triangles or quads this mesh contains. No other mesh topologies are considered.
        /// </summary>
        /// <param name="mesh">The source mesh to sum submesh primitive counts from.</param>
        /// <returns>The count of all triangles or quads contained within this mesh's submeshes.</returns>
        public static uint GetPrimitiveCount(Mesh mesh)
        {
            uint sum = 0;

            if (mesh == null)
                return sum;

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
            {
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                    sum += mesh.GetIndexCount(i) / 3;
                else if (mesh.GetTopology(i) == MeshTopology.Quads)
                    sum += mesh.GetIndexCount(i) / 4;
            }

            return sum;
        }
        #endregion // UnityEngine.ProBuilder

        //https://github.com/Unity-Technologies/com.unity.probuilder/blob/master/Runtime/Core/MeshHandles.cs
        #region UnityEngine.ProBuilder
        public static void CreatePointMesh(List<Vector3> positions, List<int> indexes, Mesh target)
        {
            int vertexCount = positions.Count;
            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            target.name = "ProBuilder::PointMesh";
            target.SetVertices(positions);
            target.subMeshCount = 1;

            target.SetIndices(indexes, MeshTopology.Points, 0);
        }

        public static void CreatePointBillboardMesh(List<Vector3> positions, Mesh target)
        {
            var pointCount = positions.Count;
            var vertexCount = pointCount * 4;

            Vector2 billboard0 = new Vector2(-1f, -1f);
            Vector2 billboard1 = new Vector2(-1f, 1f);
            Vector2 billboard2 = new Vector2(1f, -1f);
            Vector2 billboard3 = new Vector2(1f, 1f);

            var vector2List = new NativeArray<Vector2>(vertexCount, Allocator.Temp);
            var vector3List = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
            var indexList = new NativeArray<int>(vertexCount, Allocator.Temp);

            for (int i = 0; i < pointCount; i++)
            {
                vector3List[i * 4 + 0] = positions[i];
                vector3List[i * 4 + 1] = positions[i];
                vector3List[i * 4 + 2] = positions[i];
                vector3List[i * 4 + 3] = positions[i];

                vector2List[i * 4 + 0] = billboard0;
                vector2List[i * 4 + 1] = billboard1;
                vector2List[i * 4 + 2] = billboard2;
                vector2List[i * 4 + 3] = billboard3;

                indexList[i * 4 + 0] = i * 4 + 0;
                indexList[i * 4 + 1] = i * 4 + 1;
                indexList[i * 4 + 2] = i * 4 + 3;
                indexList[i * 4 + 3] = i * 4 + 2;
            }

            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            target.SetVertices(vector3List);
            target.SetUVs(0, vector2List);
            target.subMeshCount = 1;
            target.SetIndices(indexList, MeshTopology.Quads, 0);
        }

        public static void CreatePointBillboardMesh(List<Vector3> positions, List<int> indexes, Mesh target)
        {
            var pointCount = indexes.Count;
            var vertexCount = pointCount * 4;

            Vector2 billboard0 = new Vector2(-1f, -1f);
            Vector2 billboard1 = new Vector2(-1f, 1f);
            Vector2 billboard2 = new Vector2(1f, -1f);
            Vector2 billboard3 = new Vector2(1f, 1f);

            var vector2List = new NativeArray<Vector2>(vertexCount, Allocator.Temp);
            var vector3List = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
            var indexList = new NativeArray<int>(vertexCount, Allocator.Temp);

            for (int i = 0; i < pointCount; i++)
            {
                var index = indexes[i];

                vector3List[i * 4 + 0] = positions[index];
                vector3List[i * 4 + 1] = positions[index];
                vector3List[i * 4 + 2] = positions[index];
                vector3List[i * 4 + 3] = positions[index];

                vector2List[i * 4 + 0] = billboard0;
                vector2List[i * 4 + 1] = billboard1;
                vector2List[i * 4 + 2] = billboard2;
                vector2List[i * 4 + 3] = billboard3;

                indexList[i * 4 + 0] = i * 4 + 0;
                indexList[i * 4 + 1] = i * 4 + 1;
                indexList[i * 4 + 2] = i * 4 + 3;
                indexList[i * 4 + 3] = i * 4 + 2;
            }

            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            target.SetVertices(vector3List);
            target.SetUVs(0, vector2List);
            target.subMeshCount = 1;
            target.SetIndices(indexList, MeshTopology.Quads, 0);
        }
        #endregion // UnityEngine.ProBuilder

        //https://github.com/needle-mirror/com.unity.physics/blob/master/Unity.Physics/Base/Utilities/MeshUtilities.cs
        #region Unity.Physics
        public static void AppendMeshPropertiesToNativeBuffers(Mesh.MeshData meshData,
            bool trianglesNeeded, out NativeArray<float3> vertices, out NativeArray<int3> triangles,
            Allocator allocator = Allocator.Temp)
        {
            vertices = new NativeArray<float3>(meshData.vertexCount, allocator);
            var verticesV3 = vertices.Reinterpret<Vector3>();
            meshData.GetVertices(verticesV3);

            if (!trianglesNeeded)
            {
                triangles = default;
                return;
            }

            switch (meshData.indexFormat)
            {
                case IndexFormat.UInt16:
                    var indices16 = meshData.GetIndexData<ushort>();
                    var numTriangles = indices16.Length / 3;

                    triangles = new NativeArray<int3>(numTriangles, allocator);

                    int trianglesIndex = 0;
                    for (var sm = 0; sm < meshData.subMeshCount; ++sm)
                    {
                        var subMesh = meshData.GetSubMesh(sm);
                        for (int i = subMesh.indexStart, count = 0; count < subMesh.indexCount; i += 3, count += 3)
                        {
                            triangles[trianglesIndex] =
                                ((int3)new uint3(indices16[i], indices16[i + 1], indices16[i + 2]));

                            ++trianglesIndex;
                        }
                    }

                    break;
                case IndexFormat.UInt32:
                    var indices32 = meshData.GetIndexData<uint>();
                    numTriangles = indices32.Length / 3;

                    triangles = new NativeArray<int3>(numTriangles, allocator);

                    trianglesIndex = 0;
                    for (var sm = 0; sm < meshData.subMeshCount; ++sm)
                    {
                        var subMesh = meshData.GetSubMesh(sm);
                        for (int i = subMesh.indexStart, count = 0; count < subMesh.indexCount; i += 3, count += 3)
                        {
                            triangles[trianglesIndex] =
                                ((int3)new uint3(indices32[i], indices32[i + 1], indices32[i + 2]));

                            ++trianglesIndex;
                        }
                    }

                    break;
                default:
                    triangles = default;
                    break;
            }
        }
        #endregion // Unity.Physics

        #region NativeList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVertices(this Mesh mesh, NativeList<Vector3> inVertices)
        {
            mesh.SetVertices(inVertices.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUVs<T>(this Mesh mesh, int channel, NativeList<T> uvs) where T : unmanaged
        {
            mesh.SetUVs(channel, uvs.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetNormals(this Mesh mesh, NativeList<Vector3> inNormals)
        {
            mesh.SetNormals(inNormals.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, NativeList<Color> inColors)
        {
            mesh.SetColors(inColors.AsArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetColors(this Mesh mesh, NativeList<Color32> inColors)
        {
            mesh.SetColors(inColors.AsArray());
        }
        #endregion // NativeList
    }
}
