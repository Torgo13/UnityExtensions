using System.Collections.Generic;
using UnityEngine;

namespace PKGE.Tests
{
    //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/master/Tests/Runtime/Scripts/PointInPolygonPerformanceTest.cs
    class PointInPolygonPerformanceTest : PerformanceTest
    {
        static readonly List<Vector3> k_TestHexagon = new List<Vector3>
        {
            new Vector3(4f, 0f, 4f), new Vector3(3f, 0f, 4f), new Vector3(2f, 0f, 5f),
            new Vector3(3f, 0f, 6f), new Vector3(4f, 0f, 6f), new Vector3(5f, 0f, 5f)
        };

        Vector3[] m_TestPoints;

        protected override void SetupData()
        {
            Random.InitState(2000);
            m_TestPoints = TestData.RandomXZVector3Array(m_CallCount);
            m_MethodLabel = "GeometryUtils.PointInPolygon(p, vertices)";
        }

        protected override void RunTestFrame()
        {
            foreach (var p in m_TestPoints)
            {
                m_Timer.Restart();
                GeometryUtils.PointInPolygon(p, k_TestHexagon);
                m_Timer.Stop();
                m_ElapsedTicks += m_Timer.ElapsedTicks;
            }
        }
    }

    //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/master/Tests/Runtime/Scripts/ConvexHullPerformanceTest.cs
    class ConvexHullPerformanceTest : PerformanceTest
    {
        const int k_ExampleCount = 3;
        const int k_PointsPerExample = 128;

        List<List<Vector3>> m_Cases = new List<List<Vector3>>();

        List<Vector3> m_Hull = new List<Vector3>(64);

        protected override void SetupData()
        {
            m_MethodLabel = "GeometryUtils.ConvexHull2D()";
            m_CallCount *= k_ExampleCount;

            Random.InitState(100);
            for (var i = 0; i < k_ExampleCount; i++)
            {
                var list = new List<Vector3>();
                var range = Mathf.Sqrt((i + 1) * 5) * 0.5f;
                for (var j = 0; j < k_PointsPerExample; j++)
                {
                    var x = Random.Range(-range, range);
                    var z = Random.Range(-range, range);
                    list.Add(new Vector3(x, 0f, z));
                }

                m_Cases.Add(list);
            }
        }

        protected override void RunTestFrame()
        {
            foreach (var c in m_Cases)
            {
                m_Timer.Restart();
                GeometryUtils.ConvexHull2D(c, m_Hull);
                m_Timer.Stop();
                m_ElapsedTicks += m_Timer.ElapsedTicks;
            }
        }
    }

    static class TestData
    {
        public static Vector2[] RandomVector2Array(int length, float range = 0.0001f)
        {
            var array = new Vector2[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
            }

            return array;
        }

        public static Vector3[] RandomXZVector3Array(int length, float range = 0.0001f)
        {
            var array = new Vector3[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
            }

            return array;
        }
    }
}
