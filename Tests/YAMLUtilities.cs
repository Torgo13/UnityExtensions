using UnityEngine;
using System;

namespace UnityExtensions.Tests
{
    static class YAMLUtilities
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Tests/Editor/TestFramework/YAMLUtilities.cs
        #region UnityEditor.Rendering.TestFramework
        public static string ToYAML(this Quaternion v) => FormattableString.Invariant($"{{x: {v.x}, y: {v.y}, z: {v.z}, w: {v.w}}}");
        public static string ToYAML(this Vector3 v) => FormattableString.Invariant($"{{x: {v.x}, y: {v.y}, z: {v.z}}}");
        public static string ToYAML(this Color v) => FormattableString.Invariant($"{{r: {v.r}, g: {v.g}, b: {v.b}, a: {v.a}}}");
        #endregion // UnityEditor.Rendering.TestFramework
    }
}
