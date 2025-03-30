using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using FrustumPlanes = UnityExtensions.Unsafe.FrustumPlanes;

namespace UnityExtensions.Tests
{
    //https://github.com/needle-mirror/com.unity.entities.graphics/blob/master/Unity.Entities.Graphics.Tests/FrustumPlanesTests.cs
    public class FrustumPlanesTests
    {
        /*
        static readonly Plane[] Planes =
        {
            new Plane(new Vector3(1.0f, 0.0f, 0.0f), -1.0f),
            new Plane(new Vector3(-1.0f, 0.0f, 0.0f),  1.0f),
            new Plane(new Vector3(0.0f, 1.0f, 0.0f), -1.0f),
            new Plane(new Vector3(0.0f, -1.0f, 0.0f),  1.0f),
            new Plane(new Vector3(0.0f, -1.0f, 1.0f),  1.0f),
            new Plane(new Vector3(0.0f, -2.0f, 0.0f),  12.0f),
            new Plane(new Vector3(1.0f, -1.0f, -7.0f), -12.0f),
            new Plane(new Vector3(0.0f, 183.0f, -7.0f), -12.0f),
            new Plane(new Vector3(0.9933293f, 0.01911314f, 0.113717f), -409.9551f),
        };

        struct AABB
        {
            public float3 Center;
            public float3 Extents;
        }

        static readonly AABB[] boxes =
        {
            new AABB { Center = new float3(0.0f, 0.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(-1.0f, 0.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(-2.0f, 0.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, -2.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, -1.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 1.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 2.0f, 0.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 0.0f, -2.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 0.0f, -1.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 0.0f, 1.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 0.0f, 2.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(1.0f, -1.0f, 1.0f), Extents = new float3(0.5f, 0.5f, 0.5f) },
            new AABB { Center = new float3(0.0f, 0.0f, 0.0f), Extents = new float3(16384.0f, 16384.0f, 16384.0f) },
            new AABB { Center = new float3(-325.303f, 391.993f, 1053.86f), Extents = new float3(22.32453f, 18.56214f, 23.49754f) },
        };

        static NativeArray<Plane> CreatePlanes(int n)
        {
            var result = new NativeArray<Plane>(n, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < n; ++i)
            {
                result[i] = Planes[i];
            }

            return result;
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void MultiPlaneTest(int planeCount)
        {
            using (var par = CreatePlanes(planeCount))
            using (var soap = FrustumPlanes.BuildSOAPlanePackets(par, Allocator.Temp))
            {
                foreach (var box in boxes)
                {
                    Assert.AreEqual(ReferenceTest(par, box), FrustumPlanes.Intersect2(soap.AsNativeArray(), box.Center, box.Extents));
                }
            }
        }

        private FrustumPlanes.IntersectResult ReferenceTest(NativeArray<Plane> par, AABB box)
        {
            FrustumPlanes.IntersectResult result;
            var temp = new NativeArray<float4>(par.Length, Allocator.Temp);

            for (int i = 0; i < par.Length; ++i)
            {
                temp[i] = new float4(par[i].normal, par[i].distance);
            }

            result = FrustumPlanes.Intersect(temp, box.Center, box.Extents);

            temp.Dispose();
            return result;
        }
        */

        [Test]
        [TestCase(0f)]
        [TestCase(1e5f)]
        [TestCase(-1e5f)]
        public void FrustumFromCamera_PlaneDistance(float zPosition)
        {
            var gameObject = new GameObject();
            var camera = gameObject.AddComponent<Camera>();

            const float nearClipPlane = 0.3f;
            const float farClipPlane = 1000f;

            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;

            gameObject.transform.position = new float3(0, 0, zPosition);

            Plane[] sourcePlanes = new Plane[6];

            using (var planes = new NativeArray<float4>(6, Allocator.Temp))
            {
                FrustumPlanes.FromCamera(camera, planes, sourcePlanes);

                var nearPlane = planes[4];
                var farPlane = planes[5];

                Assert.That(nearPlane.w, Is.EqualTo(-nearClipPlane - zPosition).Within(1e-3f));
                Assert.That(farPlane.w, Is.EqualTo(farClipPlane + zPosition).Within(1e-3f));
            }

            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }
}
