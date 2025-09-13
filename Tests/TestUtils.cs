using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace PKGE.Tests
{
    public static class TestUtils
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture.tests/Tests/Editor/TestUtils.cs
        #region Unity.LiveCapture.Tests.Editor
        /// <summary>
        /// Delays a unity test until the given number of frame updates have occured.
        /// </summary>
        public static IEnumerator WaitForPlayerLoopUpdates(int frames)
        {
            while (frames > 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif // UNITY_EDITOR

                yield return null;
                frames--;
            }
        }
        #endregion // Unity.LiveCapture.Tests.Editor

        //https://github.com/Unity-Technologies/Unity.Mathematics/blob/1.2.5/src/Tests/Tests/Shared/TestUtils.cs
        #region Unity.Mathematics.Tests
        public static void AreEqual(bool expected, bool actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(int expected, int actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(uint expected, uint actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(long expected, long actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(ulong expected, ulong actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void AreEqual(float expected, float actual, float delta = 0.0f)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        public static void AreEqual(double expected, double actual, double delta = 0.0)
        {
            Assert.AreEqual(expected, actual, delta);
        }

        // int
        public static void AreEqual(Vector2Int expected, Vector2Int actual)
        {
            AreEqual(expected.x, actual.x);
            AreEqual(expected.y, actual.y);
        }

        public static void AreEqual(Vector3Int expected, Vector3Int actual)
        {
            AreEqual(expected.x, actual.x);
            AreEqual(expected.y, actual.y);
            AreEqual(expected.z, actual.z);
        }

        // float
        public static void AreEqual(Vector2 expected, Vector2 actual, float delta = 0.0f)
        {
            AreEqual(expected.x, actual.x, delta);
            AreEqual(expected.y, actual.y, delta);
        }

        public static void AreEqual(Vector3 expected, Vector3 actual, float delta = 0.0f)
        {
            AreEqual(expected.x, actual.x, delta);
            AreEqual(expected.y, actual.y, delta);
            AreEqual(expected.z, actual.z, delta);
        }

        public static void AreEqual(Vector4 expected, Vector4 actual, float delta = 0.0f)
        {
            AreEqual(expected.x, actual.x, delta);
            AreEqual(expected.y, actual.y, delta);
            AreEqual(expected.z, actual.z, delta);
            AreEqual(expected.w, actual.w, delta);
        }

        public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual, float delta = 0.0f)
        {
            AreEqual(expected.GetColumn(0), actual.GetColumn(0), delta);
            AreEqual(expected.GetColumn(1), actual.GetColumn(1), delta);
            AreEqual(expected.GetColumn(2), actual.GetColumn(2), delta);
            AreEqual(expected.GetColumn(3), actual.GetColumn(3), delta);
        }

        public static void AreEqual(Quaternion expected, Quaternion actual, float delta = 0.0f)
        {
            AreEqual(expected[0], actual[0], delta);
            AreEqual(expected[1], actual[1], delta);
            AreEqual(expected[2], actual[2], delta);
            AreEqual(expected[3], actual[3], delta);
        }

        public static void AreEqual(Transform expected, Transform actual, float delta = 0.0f)
        {
            AreEqual(expected.rotation, actual.rotation, delta);
            AreEqual(expected.position, actual.position, delta);
        }

        public static void IsTrue(bool condition)
        {
            AreEqual(true, condition);
        }

        public static void IsFalse(bool condition)
        {
            AreEqual(false, condition);
        }
        #endregion // Unity.Mathematics.Tests
    }
}
