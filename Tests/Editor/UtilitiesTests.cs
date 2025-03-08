using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace UnityExtensions.Editor.Tests
{
    public class UtilitiesTests
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture.tests/Tests/Editor/UtilitiesTests.cs
        #region Unity.LiveCapture.Tests.Editor
        [Test]
        public void CanCreateEmptyGameObject()
        {
            var gameObject = AdditionalCoreUtils.CreateEmptyGameObject();
            var components = gameObject.GetComponents<Component>();
            Assert.IsTrue(components.Length == 1);
            Assert.IsTrue(components[0] is Transform);
            Object.DestroyImmediate(gameObject);
        }
        #endregion // Unity.LiveCapture.Tests.Editor

        private class TestClass
        {
            public int IntField;
            public string StringProperty { get; set; }
            public List<float> FloatList { get; set; }
        }

        /// <summary>
        /// Validates that the function returns null when the host type is null.
        /// </summary>
        [Test]
        public void GetMemberInfoFromPropertyPath_NullHost_ReturnsNull()
        {
            var result = EditorUtils.GetMemberInfoFromPropertyPath(null, "IntField", out Type type);
            Assert.IsNull(result);
            Assert.IsNull(type);
        }

        /// <summary>
        /// Tests that the function returns null when given an invalid property path.
        /// </summary>
        [Test]
        public void GetMemberInfoFromPropertyPath_InvalidPath_ReturnsNull()
        {
            var result = EditorUtils.GetMemberInfoFromPropertyPath(typeof(TestClass), "InvalidField", out Type type);
            Assert.IsNull(result);
            Assert.IsNull(type);
        }

        /// <summary>
        /// Tests that the function returns the correct MemberInfo and type for a field.
        /// </summary>
        [Test]
        public void GetMemberInfoFromPropertyPath_Field_ReturnsCorrectMemberInfo()
        {
            var result = EditorUtils.GetMemberInfoFromPropertyPath(typeof(TestClass), "IntField", out Type type);
            Assert.IsNotNull(result);
            Assert.AreEqual("IntField", result.Name);
            Assert.AreEqual(typeof(int), type);
        }

        /// <summary>
        /// Tests that the function returns the correct MemberInfo and type for a property.
        /// </summary>
        [Test]
        public void GetMemberInfoFromPropertyPath_Property_ReturnsCorrectMemberInfo()
        {
            var result = EditorUtils.GetMemberInfoFromPropertyPath(typeof(TestClass), "StringProperty", out Type type);
            Assert.IsNotNull(result);
            Assert.AreEqual("StringProperty", result.Name);
            Assert.AreEqual(typeof(string), type);
        }

        /// <summary>
        /// Validates that the function correctly handles array elements within a generic list
        /// and returns the correct MemberInfo and element type.
        /// </summary>
        [Test]
        public void GetMemberInfoFromPropertyPath_GenericListElement_ReturnsCorrectMemberInfo()
        {
            var result = EditorUtils.GetMemberInfoFromPropertyPath(typeof(TestClass), "FloatList.Array.data[0]", out Type type);
            Assert.IsNotNull(result);
            Assert.AreEqual("FloatList", result.Name);
            Assert.AreEqual(typeof(float), type);
        }
    }
}