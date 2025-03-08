using NUnit.Framework;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityExtensions.Editor.Tests
{
    class GameObjectUtilsTests
    {
        const int k_ChildCount = 2;
        static HideFlags[] s_HideFlagsValues = EnumValues<HideFlags>.Values;

        GameObject m_GameObject;
        GameObject m_GameObjectCopy;

        [TearDown]
        public void AfterEach()
        {
            UnityObject.DestroyImmediate(m_GameObject);
            UnityObject.DestroyImmediate(m_GameObjectCopy);
        }

        [TestCaseSource(typeof(GameObjectUtilsTests), nameof(s_HideFlagsValues))]
        public void CloneWithHideFlags_NoChildren(HideFlags hideFlags)
        {
            m_GameObject = new GameObject { hideFlags = hideFlags };
            m_GameObjectCopy = GameObjectUtils.CloneWithHideFlags(m_GameObject);
            Assert.AreEqual(m_GameObject.hideFlags, m_GameObjectCopy.hideFlags);
        }

        [TestCaseSource(typeof(GameObjectUtilsTests), nameof(s_HideFlagsValues))]
        public void CloneWithHideFlags_Children_SameFlags(HideFlags hideFlags)
        {
            m_GameObject = new GameObject { hideFlags = hideFlags };
            for (var i = 0; i < k_ChildCount; ++i)
            {
                var child = new GameObject { hideFlags = hideFlags };
                child.transform.parent = m_GameObject.transform;
                for (var j = 0; j < k_ChildCount; ++j)
                {
                    var childChild = new GameObject { hideFlags = hideFlags };
                    childChild.transform.parent = child.transform;
                }
            }

            m_GameObjectCopy = GameObjectUtils.CloneWithHideFlags(m_GameObject);
            CompareHideFlagsRecursively(m_GameObject, m_GameObjectCopy);
        }

        [Test]
        public void CloneWithHideFlags_Children_DifferentFlags()
        {
            var hideFlagsCount = s_HideFlagsValues.Length;
            m_GameObject = new GameObject { hideFlags = s_HideFlagsValues[0] };
            var originals = new GameObject[hideFlagsCount];
            originals[0] = m_GameObject;
            for (var i = 1; i < hideFlagsCount; ++i)
            {
                var child = new GameObject { hideFlags = s_HideFlagsValues[i] };
                child.transform.parent = originals[(i - 1) / k_ChildCount].transform;
                originals[i] = child;
            }

            m_GameObjectCopy = GameObjectUtils.CloneWithHideFlags(m_GameObject);
            CompareHideFlagsRecursively(m_GameObject, m_GameObjectCopy);
        }

        static void CompareHideFlagsRecursively(GameObject obj1, GameObject obj2)
        {
            Assert.AreEqual(obj1.hideFlags, obj2.hideFlags);
            var obj1Transform = obj1.transform;
            var obj2Transform = obj2.transform;
            for (var i = 0; i < obj1Transform.childCount; ++i)
            {
                CompareHideFlagsRecursively(obj1Transform.GetChild(i).gameObject, obj2Transform.GetChild(i).gameObject);
            }
        }

        private GameObject testGameObject;
        private GameObject childGameObject;
        private const string testTag = "Player";

        private class TestComponent : MonoBehaviour { }

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("TestObject");
            childGameObject = new GameObject("ChildObject");
            childGameObject.transform.parent = testGameObject.transform;
            childGameObject.tag = testTag;
            childGameObject.AddComponent<TestComponent>();
            testGameObject.tag = testTag;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testGameObject);
        }

        /// <summary>
        /// Validates that the function correctly finds a component in the children of the provided GameObject.
        /// </summary>
        [Test]
        public void ExhaustiveComponentSearch_ComponentInChildren_ReturnsComponent()
        {
            var component = GameObjectUtils.ExhaustiveComponentSearch<TestComponent>(testGameObject);
            Assert.IsNotNull(component);
            Assert.IsInstanceOf<TestComponent>(component);
        }

        /// <summary>
        /// Tests that the function returns null when the specified component is not found in the provided GameObject.
        /// </summary>
        [Test]
        public void ExhaustiveComponentSearch_NoComponent_ReturnsNull()
        {
            Object.DestroyImmediate(childGameObject.GetComponent<TestComponent>());
            var component = GameObjectUtils.ExhaustiveComponentSearch<TestComponent>(testGameObject);
            Assert.IsNull(component);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validates that the function finds disabled components in editor mode.
        /// </summary>
        [Test]
        public void ExhaustiveComponentSearch_EditorMode_FindsDisabledComponents()
        {
            var disabledGameObject = new GameObject("DisabledObject");
            disabledGameObject.AddComponent<TestComponent>();
            disabledGameObject.SetActive(false);

            var component = GameObjectUtils.ExhaustiveComponentSearch<TestComponent>(null);
            Assert.IsNotNull(component);
            Assert.IsInstanceOf<TestComponent>(component);

            Object.DestroyImmediate(disabledGameObject);
        }
#endif

        /// <summary>
        /// Validates that the function correctly finds a component in the children of the provided GameObject with the specified tag.
        /// </summary>
        [Test]
        public void ExhaustiveTaggedComponentSearch_ComponentInChildrenWithTag_ReturnsComponent()
        {
            var component = GameObjectUtils.ExhaustiveTaggedComponentSearch<TestComponent>(testGameObject, testTag);
            Assert.IsNotNull(component);
            Assert.IsInstanceOf<TestComponent>(component);
        }

        /// <summary>
        /// Tests that the function returns null when no component with the specified tag is found.
        /// </summary>
        [Test]
        public void ExhaustiveTaggedComponentSearch_ComponentWithTagNotFound_ReturnsNull()
        {
            const string nonExistentTag = "NonExistentTag";
            TestComponent component = null;
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Tag: {nonExistentTag} is not defined.");
            Assert.Throws<UnityException>(() => component = GameObjectUtils.ExhaustiveTaggedComponentSearch<TestComponent>(testGameObject, nonExistentTag));
            Assert.IsNull(component);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validates that the function finds disabled components with the specified tag in editor mode.
        /// </summary>
        [Test]
        public void ExhaustiveTaggedComponentSearch_EditorMode_FindsDisabledComponentsWithTag()
        {
            var disabledGameObject = new GameObject("DisabledObject");
            disabledGameObject.AddComponent<TestComponent>();
            disabledGameObject.SetActive(false);
            disabledGameObject.tag = testTag;

            var component = GameObjectUtils.ExhaustiveTaggedComponentSearch<TestComponent>(null, testTag);
            Assert.IsNotNull(component);
            Assert.IsInstanceOf<TestComponent>(component);

            Object.DestroyImmediate(disabledGameObject);
        }
#endif
    }
}
