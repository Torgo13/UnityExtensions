using NUnit.Framework;
using System;
using UnityEngine;

namespace PKGE.Editor.Tests
{
    public static class AssertUtilities
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Tests/Editor/TestFramework/AssertUtilities.cs
        #region UnityEditor.Rendering.TestFramework
        const float Epsilon = 1e-6f;

        public static void AssertAreEqual(Vector3 l, Vector3 r)
        {
            Assert.True(
                Mathf.Abs(l.x - r.x) < Epsilon
                && Mathf.Abs(l.y - r.y) < Epsilon
                && Mathf.Abs(l.z - r.z) < Epsilon
            );
        }

        public static void AssertAreEqual(Quaternion l, Quaternion r)
        {
            AssertAreEqual(l.eulerAngles, r.eulerAngles);
        }

        public static void AssertAreEqual(Matrix4x4 l, Matrix4x4 r)
        {
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 4; ++x)
                    Assert.True(Mathf.Abs(l[x, y] - r[x, y]) < Epsilon);
            }
        }
        #endregion // UnityEditor.Rendering.TestFramework
    }
}
