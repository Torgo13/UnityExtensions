using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityExtensions.Editor.Tests
{
    [TestFixture]
    class UnityObjectUtilsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/UnityObjectUtilsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        [UnityTest]
        public IEnumerator Destroy_OneArg_DestroysImmediately_InEditMode()
        {
            Assert.IsFalse(Application.isPlaying);
            var go = new GameObject();
            UnityObjectExtensions.Destroy(go);
            yield return null; // skip frame to allow destruction to run
            Assert.IsTrue(go == null);
        }

        [Test]
        public void RemoveDestroyedObjectsTest()
        {
            var go = new GameObject();
            var list = new List<GameObject> { go };
            UnityObjectExtensions.Destroy(go);
            UnityObjectUtils.RemoveDestroyedObjects(list);
            Assert.Zero(list.Count);
        }

        [Test]
        public void RemoveDestroyedKeysTest()
        {
            var go = new GameObject();
            var dictionary = new Dictionary<GameObject, object> { { go, null } };
            UnityObjectExtensions.Destroy(go);
            UnityObjectUtils.RemoveDestroyedKeys(dictionary);
            Assert.Zero(dictionary.Count);
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        [UnityTest]
        public IEnumerator RemoveDestroyedObjectsWithUndoTest()
        {
            var go = new GameObject();
            UnityObjectExtensions.Destroy(go, withUndo: true);
            yield return null; // skip frame to allow destruction to run
            Assert.IsTrue(go == null);

            Undo.PerformUndo();
            Assert.IsTrue(go != null);

            UnityObjectExtensions.Destroy(go);
            yield return null; // skip frame to allow destruction to run
            Assert.IsTrue(go == null);
        }

        [Test]
        public void ConvertUnityObjectToTypeTest()
        {
            var go = new GameObject();
            var camera = go.AddComponent<Camera>();
            Assert.IsAssignableFrom<Camera>(UnityObjectExtensions.ConvertUnityObjectToType<Camera>(go));
            Assert.IsAssignableFrom<Camera>(UnityObjectExtensions.ConvertUnityObjectToType<Camera>(camera));

            var light = go.AddComponent<Light>();
            Assert.IsAssignableFrom<Light>(UnityObjectExtensions.ConvertUnityObjectToType<Light>(go));
            Assert.IsAssignableFrom<Light>(UnityObjectExtensions.ConvertUnityObjectToType<Light>(light));
            Assert.IsAssignableFrom<Light>(UnityObjectExtensions.ConvertUnityObjectToType<Light>(camera));

            UnityObjectExtensions.Destroy(go);
        }
    }
}
