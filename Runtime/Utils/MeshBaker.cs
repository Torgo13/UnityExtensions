using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityExtensions
{
    public class MeshBaker : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/WaterSamples/Scripts/MeshBaker.cs
        #region UnityEngine.Rendering
        public Mesh mesh;

        [Range(0, 98)]
        public int sliceIndex = 0;

        [Range(64, 256)]
        public int resolution = 64;

        [Range(0, 0.1f)]
        public float sliceThreshold = 0.0044f;

        // Baked data
        [HideInInspector] public List<float> slicesY;
        [HideInInspector] public Bounds bounds;

        Texture2D result;

        public void Bake()
        {
            List<Vector3> vertices = ListPool<Vector3>.Get();
            mesh.GetVertices(vertices);

            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Set(vertices[i].x, Mathf.Max(vertices[i].y, 0.0f), vertices[i].z);

            // Isolate slices & bounds
            if (slicesY == null)
                slicesY = new List<float>();
            else
                slicesY.Clear();
            bounds = new Bounds() { min = vertices[0], max = vertices[0] };
            for (int i = 0; i < vertices.Count; i++)
            {
                bounds.Encapsulate(vertices[i]);

                bool found = false;
                for (int j = 0; j < slicesY.Count; j++)
                {
                    if (Mathf.Abs(slicesY[j] - vertices[i].z) < sliceThreshold)
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
            result = new Texture2D(resolution, slicesY.Count);
            var data = result.GetPixelData<Color>(mipLevel: 0);

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

                    displacement = new Vector3(displacement.x / (2.0f * bounds.size.x) + 0.5f, displacement.y / bounds.size.y, color);

                    data[idx * resolution + i] = new Color(displacement.x, displacement.y, displacement.z);
                }
            }

            ListPool<PointWithUV>.Release(slice);

            result.Apply();
            var bytes = result.EncodeToPNG();
            string path = Application.dataPath + "/Artifacts/" + mesh.name + ".png";
            File.WriteAllBytes(path, bytes);
            Debug.Log("Texture file written at " + path);
        }

        struct PointWithUV
        {
            public Vector3 pos;
            public Vector2 uv;
            public Color color;
            public float dist;
        }

        void GetSlice(List<PointWithUV> slice, int idx, out float sliceLength)
        {
            Vector3[] vertices = mesh.vertices;
            Color[] colors = mesh.colors;
            Vector2[] uvs = mesh.uv;

            // Isolate slice
            slice.Clear();
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = Mathf.Max(vertices[i].y, 0.0f);

                if (Mathf.Abs(slicesY[idx] - vertices[i].z) >= sliceThreshold)
                    continue;

                var pos = vertices[i];
                pos.z = 0;
                PointWithUV pointWithUV = new PointWithUV();
                pointWithUV.pos = pos;
                if (i < uvs.Length)
                    pointWithUV.uv = uvs[i];
                if (i < colors.Length)
                    pointWithUV.color = colors[i];

                slice.Add(pointWithUV);
            }

            slice.Sort((PointWithUV a, PointWithUV b) => {
                return a.uv.x > b.uv.x ? 1 : -1;
            });

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

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position + new Vector3(bounds.min.x, bounds.min.y, 0.0f), new Vector3(bounds.size.x, 0.0f, 0.0f));
            Gizmos.DrawRay(transform.position + new Vector3(bounds.min.x, bounds.min.y, 0.0f), new Vector3(0.0f, bounds.size.y, 0.0f));
            Gizmos.DrawRay(transform.position + new Vector3(bounds.max.x, bounds.max.y, 0.0f), -new Vector3(bounds.size.x, 0.0f, 0.0f));
            Gizmos.DrawRay(transform.position + new Vector3(bounds.max.x, bounds.max.y, 0.0f), -new Vector3(0.0f, bounds.size.y, 0.0f));

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

                Gizmos.DrawLine(transform.position + lastPos, transform.position + displacedPos);

                var origPos = new Vector3((1.0f - slice[i].dist / sliceLength) * bounds.size.x + bounds.min.x, bounds.min.y, 0.0f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + origPos, transform.position + displacedPos);

                lastPos = displacedPos;
            }

            ListPool<PointWithUV>.Release(slice);
        }
        #endregion // UnityEngine.Rendering
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MeshBaker))]
    public class MeshBakerEditor : Editor
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/WaterSamples/Scripts/MeshBaker.cs
        #region UnityEngine.Rendering
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            var baker = target as MeshBaker;
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
