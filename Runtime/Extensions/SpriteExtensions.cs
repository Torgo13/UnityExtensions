using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace UnityExtensions
{
    public static class SpriteExtensions
    {
        public static Mesh GenerateMeshFromSprite(this Sprite sprite)
        {
            var mesh = new Mesh
            {
                name = sprite.name.Replace('.', '_'),
            };
            
            sprite.GenerateMeshFromSprite(ref mesh);

            return mesh;
        }
        
        public static void GenerateMeshFromSprite(this Sprite sprite, ref Mesh mesh)
        {
            Vector2[] spriteVertices = sprite.vertices;
            var vertices = new NativeArray<Vector3>(spriteVertices.Length, Allocator.Temp);

            for (int i = 0; i < spriteVertices.Length; i++)
            {
                vertices[i] = spriteVertices[i]; // Automatically sets z to 0
            }
            
            mesh.Clear(keepVertexLayout: false);
            mesh.indexFormat = IndexFormat.UInt16;

            mesh.SetVertices(vertices);
            mesh.SetTriangles(sprite.triangles, submesh: 0);
            mesh.uv = sprite.uv;

            var forward = Vector3.forward;
            
            // Re-use vertices NativeArray for normals
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = forward;
            }

            mesh.SetNormals(vertices);
            vertices.Dispose();

            mesh.RecalculateBounds();
            mesh.Optimize();
        }
    }
}
