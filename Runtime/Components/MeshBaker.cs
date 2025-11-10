using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace PKGE
{
    public class MeshBaker : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/WaterSamples/Scripts/MeshBaker.cs
        #region UnityEngine.Rendering
        public Mesh mesh;

        [Range(0, 98)]
        public int sliceIndex;

        [Range(64, 256)]
        public int resolution = 64;

        [Range(0, 0.1f)]
        public float sliceThreshold = 0.0044f;

        // Baked data
        [HideInInspector] public List<float> slicesY;
        [HideInInspector] public Bounds bounds;

        public void Bake()
        {
            List<Vector3> vertices = ListPool<Vector3>.Get();
            mesh.GetVertices(vertices);

            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Set(vertices[i].x, Mathf.Max(vertices[i].y, 0.0f), vertices[i].z);

            // Isolate slices & bounds
            if (slicesY == null)
                slicesY = new List<float>(vertices.Count);
            else
                slicesY.Clear();
            bounds = new Bounds { min = vertices[0], max = vertices[0] };
            for (int i = 0; i < vertices.Count; i++)
            {
                bounds.Encapsulate(vertices[i]);

                bool found = false;
                foreach (float sliceY in slicesY)
                {
                    if (Mathf.Abs(sliceY - vertices[i].z) < sliceThreshold)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    slicesY.Add(vertices[i].z);
            }

            ListPool<Vector3>.Release(vertices);
            
            // Texture
            var result = new Texture2D(resolution, slicesY.Count,
                UnityEngine.Experimental.Rendering.DefaultFormat.HDR,
                UnityEngine.Experimental.Rendering.TextureCreationFlags.DontInitializePixels);
            var data = result.GetPixelData<Vector4>(mipLevel: 0);

            List<PointWithUV> slice = ListPool<PointWithUV>.Get();

            //int idx = sliceIndex;
            for (int idx = 0; idx < slicesY.Count; idx++)
            {
                GetSlice(slice, idx, out float sliceLength);

                for (int i = 0; i < resolution; i++)
                {
                    int j = 0;
                    float percentage = i / (float)(resolution - 1);
                    for (; j < slice.Count - 1; j++)
                    {
                        if (slice[j].dist / sliceLength > percentage)
                            break;
                    }

                    float startP = slice[j - 1].dist / sliceLength;
                    float endP = slice[j].dist / sliceLength;
                    Assert.IsTrue(startP <= percentage && percentage <= endP);
                    float lerpFactor = Mathf.InverseLerp(startP, endP, percentage);

                    var displacedPos = Vector3.Lerp(slice[j - 1].pos, slice[j].pos, lerpFactor);
                    var origPos = new Vector3((1.0f - percentage) * bounds.size.x + bounds.min.x, bounds.min.y, 0.0f);
                    var displacement = displacedPos - origPos;
                    Assert.IsTrue(displacement.y >= 0.0f);

                    var color = Mathf.Lerp(slice[j - 1].color.r, slice[j].color.r, lerpFactor);

                    data[idx * resolution + i] = new Vector4(
                        displacement.x / (2.0f * bounds.size.x) + 0.5f,
                        displacement.y / bounds.size.y,
                        color,
                        1f);
                }
            }

            ListPool<PointWithUV>.Release(slice);

            result.Apply();

            try
            {
                var bytes = result.EncodeToPNG();
                string path = Application.dataPath + "/Artifacts/" + mesh.name + ".png";
                var task = File.WriteAllBytesAsync(path, bytes, destroyCancellationToken);

#if UNITY_EDITOR
                task.Wait(destroyCancellationToken);
                Debug.Log("Texture file written at " + path);
#endif // UNITY_EDITOR
            }
            catch (System.Exception e) when (e is not System.Threading.Tasks.TaskCanceledException)
            {
                Debug.LogException(e);
            }
            finally
            {
                CoreUtils.Destroy(ref result, skipNullCheck: true);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PointWithUV
        {
            public Color color;
            public Vector3 pos;
            public float dist;
            public Vector2 uv;
        }

        void GetSlice(List<PointWithUV> slice, int idx, out float sliceLength)
        {
            var vertices = ListPool<Vector3>.Get();
            var colors = ListPool<Color>.Get();
            var uvs = ListPool<Vector2>.Get();
            
            mesh.GetColors(colors);
            mesh.GetVertices(vertices);
            mesh.GetUVs(channel: 0, uvs);

            // Isolate slice
            slice.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                if (Mathf.Abs(slicesY[idx] - vertices[i].z) >= sliceThreshold)
                    continue;

                PointWithUV pointWithUV = new PointWithUV();
                pointWithUV.pos = new Vector3(vertices[i].x, Mathf.Max(vertices[i].y, 0.0f), 0);
                if (i < uvs.Count)
                    pointWithUV.uv = uvs[i];
                if (i < colors.Count)
                    pointWithUV.color = colors[i];

                slice.Add(pointWithUV);
            }
            
            ListPool<Vector3>.Release(vertices);
            ListPool<Color>.Release(colors);
            ListPool<Vector2>.Release(uvs);

            slice.Sort((a, b) => a.uv.x > b.uv.x ? 1 : -1);

            // Compute Length
            sliceLength = 0.0f;
            Vector3 lastPos = slice[0].pos;
            for (int i = 1; i < slice.Count; i++)
            {
                var basePos = slice[i].pos;
                sliceLength += Vector3.Distance(lastPos, basePos);
                lastPos = basePos;

                slice[i] = new PointWithUV
                {
                    pos = slice[i].pos,
                    uv = slice[i].uv,
                    color = slice[i].color,
                    dist = sliceLength,
                };
            }
        }

        void OnDrawGizmos()
        {
            if (slicesY == null)
                return;
            
            sliceIndex = Mathf.Min(sliceIndex, slicesY.Count - 1);

            var position = transform.position;
            var offsetMin = new Vector3(bounds.min.x, bounds.min.y, 0.0f);
            var offsetMax = new Vector3(bounds.max.x, bounds.max.y, 0.0f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position + offsetMin, new Vector3(bounds.size.x, 0.0f, 0.0f));
            Gizmos.DrawRay(position + offsetMin, new Vector3(0.0f, bounds.size.y, 0.0f));
            Gizmos.DrawRay(position + offsetMax, -new Vector3(bounds.size.x, 0.0f, 0.0f));
            Gizmos.DrawRay(position + offsetMax, -new Vector3(0.0f, bounds.size.y, 0.0f));

            List<PointWithUV> slice = ListPool<PointWithUV>.Get();
            GetSlice(slice, sliceIndex, out float sliceLength);

            var lastPos = slice[0].pos;
            for (int i = 1; i < slice.Count; i++)
            {
                var displacedPos = slice[i].pos;

                float uv = slice[i].dist / sliceLength;
                Gizmos.color = new Color(uv, 0.0f, 0.0f);
                if (uv >= 0.8f && uv <= 0.85f)
                    Gizmos.color = new Color(0.0f, 1.0f, 0.0f);

                Gizmos.DrawLine(position + lastPos, position + displacedPos);

                var origPos = new Vector3((1.0f - slice[i].dist / sliceLength) * bounds.size.x + bounds.min.x, bounds.min.y, 0.0f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(position + origPos, position + displacedPos);

                lastPos = displacedPos;
            }

            ListPool<PointWithUV>.Release(slice);
        }
        #endregion // UnityEngine.Rendering
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MeshBaker))]
    public class MeshBakerEditor : UnityEditor.Editor
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/WaterSamples/Scripts/MeshBaker.cs
        #region UnityEngine.Rendering
        public override void OnInspectorGUI()
        {
            _ = DrawDefaultInspector();

            var baker = target as MeshBaker;
            if (ReferenceEquals(baker, null))
                return;
            
            if (GUILayout.Button("Bake"))
                baker.Bake();

            if (baker.slicesY != null)
            {
                GUILayout.Label("Found " + baker.slicesY.Count + " slices");
                GUILayout.Label("Bounds: " + baker.bounds.size.x + ", " + baker.bounds.size.y);
            }
        }
        #endregion // UnityEngine.Rendering
    }
#endif
}
