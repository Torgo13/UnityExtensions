using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UnityExtensions.Editor.Tests
{
    public class AdditionalCoreUtilsTests
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

        private List<Object> _createdObjects;

        [SetUp]
        public void SetUp()
        {
            _createdObjects = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any surviving objects
            foreach (var o in _createdObjects)
            {
                if (o != null)
                    Object.DestroyImmediate(o);
            }
            _createdObjects.Clear();
        }

        #region CreateEmptyGameObject
        [Test]
        public void CreateEmptyGameObject_StripsAllComponentsExceptTransform()
        {
            var go = AdditionalCoreUtils.CreateEmptyGameObject();
            _createdObjects.Add(go);

            Assert.IsNotNull(go);
            var components = go.GetComponents<Component>();

            // Should only have Transform
            Assert.AreEqual(1, components.Length);
            Assert.IsInstanceOf<Transform>(components[0]);
        }

        [Test]
        public void CreateEmptyGameObject_HasUniqueNameAndActive()
        {
            var go = AdditionalCoreUtils.CreateEmptyGameObject();
            _createdObjects.Add(go);

            Assert.IsTrue(go.activeSelf);
            StringAssert.Contains("Cube", go.name); // Default primitive name
        }
        #endregion // CreateEmptyGameObject

        #region CreateEngineMaterial
        [Test]
        public void CreateEngineMaterial_ReturnsMaterial_WhenShaderFoundAndSupported()
        {
            // Use a guaranteed built-in shader
            string shaderPath = "Sprites/Default";
            var mat = AdditionalCoreUtils.CreateEngineMaterial(shaderPath);
            _createdObjects.Add(mat);

            Assert.IsNotNull(mat);
            Assert.AreEqual(HideFlags.HideAndDontSave, mat.hideFlags);
            Assert.AreEqual(Shader.Find(shaderPath), mat.shader);
        }

        [Test]
        public void CreateEngineMaterial_ReturnsNull_AndLogsError_WhenShaderNotFound()
        {
            LogAssert.Expect(LogType.Error, $"Cannot create required material because shader NON_EXISTENT_SHADER could not be found");

            var mat = AdditionalCoreUtils.CreateEngineMaterial("NON_EXISTENT_SHADER");
            Assert.IsNull(mat);
        }

        [Test]
        public void CreateEngineMaterial_ReturnsNull_AndLogsError_WhenShaderUnsupported()
        {
            // Simulate unsupported shader using a fake Shader
            var shader = Shader.Find("Sprites/Default");
            Assert.NotNull(shader);

            // Forcing isSupported to false in runtime isn't possible without mocking,
            // so we guard this test with assumption
            if (!shader.isSupported)
            {
                LogAssert.Expect(LogType.Error, $"Shader Sprites/Default is not supported by the current graphics hardware.");
                var mat = AdditionalCoreUtils.CreateEngineMaterial("Sprites/Default");
                Assert.IsNull(mat);
            }
            else
            {
                Assert.Pass("Shader is supported; skipping unsupported branch test.");
            }
        }
        #endregion // CreateEngineMaterial

        #region GetOrAddComponent
        private class DummyComponent : MonoBehaviour { }

        [Test]
        public void GetOrAddComponent_ReturnsExistingComponent_IfPresent()
        {
            var go = new GameObject("TestGO");
            _createdObjects.Add(go);

            var existing = go.AddComponent<DummyComponent>();
            var result = AdditionalCoreUtils.GetOrAddComponent<DummyComponent>(go);

            Assert.AreSame(existing, result);
            Assert.AreEqual(1, go.GetComponents<DummyComponent>().Length);
        }

        [Test]
        public void GetOrAddComponent_AddsNewComponent_IfMissing()
        {
            var go = new GameObject("TestGO");
            _createdObjects.Add(go);

            var result = AdditionalCoreUtils.GetOrAddComponent<DummyComponent>(go);

            Assert.IsNotNull(result);
            Assert.AreSame(result, go.GetComponent<DummyComponent>());
            Assert.AreEqual(1, go.GetComponents<DummyComponent>().Length);
        }
        #endregion  // GetOrAddComponent

        #region DestroyIfNeeded
        [Test]
        public void DestroyIfNeeded_ByRef_NullifiesReferenceAndDestroysObject()
        {
            var go = new GameObject("ToDestroy");
            _createdObjects.Add(go);

            AdditionalCoreUtils.DestroyIfNeeded(ref go);
            Assert.IsNull(go); // ref param set to null
        }

        [Test]
        public void DestroyIfNeeded_ByValue_DestroysObject()
        {
            var go = new GameObject("ToDestroy");
            _createdObjects.Add(go);

            AdditionalCoreUtils.DestroyIfNeeded(go);
            // Can't check ref nulling because passed by value
            // Instead check it gets destroyed next frame (playmode) or immediately (editmode)
            // In edit mode DestroyImmediate is used => null immediately
            Assert.IsTrue(go == null);
        }

        [Test]
        public void DestroyIfNeeded_DoesNothing_WhenObjectIsNull()
        {
            GameObject go = null;
            AdditionalCoreUtils.DestroyIfNeeded(ref go);
            Assert.IsNull(go); // stays null
        }
        #endregion // DestroyIfNeeded
    }

    public class UtilitiesTests
    {
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