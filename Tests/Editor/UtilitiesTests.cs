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
    }
}
