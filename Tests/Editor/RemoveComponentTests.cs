using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions.Editor.Tests
{
    class RemoveComponent
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/RemoveComponentTests.cs
        #region UnityEngine.Rendering.Tests
        #region Components
        interface ITest { }
        protected class Apple : MonoBehaviour, ITest { }
        protected class Banana : MonoBehaviour, ITest { }
        [RequireComponent(typeof(Apple))]
        protected class AdditionalApple : MonoBehaviour, IAdditionalData, ITest { }
        [RequireComponent(typeof(Banana))]
        protected class AdditionalBanana : MonoBehaviour, IAdditionalData, ITest { }
        [RequireComponent(typeof(Banana))]
        protected class AdditionalBananaColor : MonoBehaviour, IAdditionalData, ITest { }
        protected class WaterMelon : MonoBehaviour, IAdditionalData, ITest{ }
        [RequireComponent(typeof(Apple), typeof(Banana))]
        protected class FruitBasket : MonoBehaviour, IAdditionalData, ITest { }
        #endregion

        #region SetUp & TearDown

        protected GameObject m_GameObject;
        [SetUp]
        public void SetUp()
        {
            m_GameObject = new("RemoveComponentTestsRoot");
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_GameObject);
        }
        #endregion

        protected Type[] GenericRemoveComponent(
                int selectionAmount,
                [DisallowNull] Type componentToRemove,
                [DisallowNull] Type[] componentsToAdd,
                [DisallowNull] Action<Component> removeMethod)
        {
            Assert.IsTrue(selectionAmount > 0, "GenericRemoveComponent should only be called with a non null selection");

            //Create object to select
            GameObject[] selectedGameObjects = new GameObject[selectionAmount];
            for (int i = 0; i < selectionAmount; ++i)
            {
                (selectedGameObjects[i] = new GameObject($"Selected_{i}", componentsToAdd)).transform.parent = m_GameObject.transform;
            }

            //update selected objects
            Selection.objects = selectedGameObjects;

            //Call method to test
            removeMethod(Selection.activeGameObject.GetComponent(componentToRemove));

            //Assert all selection have same component
            var typesOfFirstSelected = selectedGameObjects[0].GetComponents<ITest>().Select(c => c.GetType());
            Assert.IsTrue(DoesAllGameObjectHaveSameComponents(selectedGameObjects.Skip(1), typesOfFirstSelected), "Not all the GameObject of the selection have same Components");

            //Will assert that remaining components are the desired ones
            return typesOfFirstSelected.ToArray();
        }

        bool DoesAllGameObjectHaveSameComponents([NotNull] System.Collections.Generic.IEnumerable<GameObject> objectsToCheck, System.Collections.Generic.IEnumerable<Type> typesToCheck)
        {
            var typeAmount = typesToCheck.Count();
            foreach(var objectToCheck in objectsToCheck)
            {
                if (objectToCheck.GetComponents<ITest>().Length != typeAmount)
                    return false;

                foreach (var typeToCheck in typesToCheck)
                    if (!objectToCheck.TryGetComponent(typeToCheck, out _))
                        return false;
            }
            return true;
        }
        #endregion // UnityEngine.Rendering.Tests
    }

    [TestOf(typeof(RemoveComponentUtils))]
    class RemoveComponentUtilsTests : RemoveComponent
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/RemoveComponentTests.cs
        #region UnityEngine.Rendering.Tests
        //No multi-edition test needed as this is not Selection dependant
        static TestCaseData[] s_RemoveComponentTestCaseData =
        {
            new TestCaseData(typeof(Banana), new Type[] {typeof(Banana), typeof(AdditionalBanana)})
                .SetName("Removal of target component removes it's additional data")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(typeof(Banana), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(AdditionalBananaColor)})
                .SetName("Removal of target component removes all it's additional data")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(typeof(Apple), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(Apple), typeof(AdditionalApple)})
                .SetName("Given multiple components, each with additional data, the removal of the component only removes its additional data")
                .Returns(new Type[] {typeof(Banana), typeof(AdditionalBanana) }),
        };

        [Test, TestCaseSource(nameof(s_RemoveComponentTestCaseData))]
        public Type[] RemoveComponentAndPropagateTheDeleteToAdditionalData([DisallowNull] Type componentToRemove, [DisallowNull] Type[] componentsToAdd)
        {
            return GenericRemoveComponent(1, componentToRemove, componentsToAdd, RemoveComponentUtils.RemoveComponent);
        }
        #endregion // UnityEngine.Rendering.Tests
    }

    [TestOf(typeof(RemoveAdditionalDataUtils))]
    class RemoveAdditionalDataUtilsTests : RemoveComponent
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/RemoveComponentTests.cs
        #region UnityEngine.Rendering.Tests
        //No multi-edition test needed as this is not Selection dependant
        static TestCaseData[] s_TryGetComponentsToRemoveTestCaseData =
        {
            new TestCaseData(typeof(AdditionalBanana))
                .Returns(new string[] {"Banana"})
                .SetName("For additional data targeting one component, return the targeted component (most common case)"),
            new TestCaseData(typeof(FruitBasket))
                .Returns(new string[] { "Apple", "Banana" })
                .SetName("For additional data targeting multiple components, return all the targeted components"),
            new TestCaseData(typeof(WaterMelon))
                .Returns(Array.Empty<string>())
                .SetName("For additional data targeting no component, return empty collection."),
        };

        [Test, TestCaseSource(nameof(s_TryGetComponentsToRemoveTestCaseData))]
        public string[] TryGetComponentsToRemove([DisallowNull] Type type)
        {
            string[] result = Array.Empty<string>();
            var additionalData = m_GameObject.AddComponent(type) as IAdditionalData;

            using (ListPool<Type>.Get(out var componentsToRemove))
            {
                if (RemoveAdditionalDataUtils.TryGetComponentsToRemove(additionalData, componentsToRemove, out var error))
                    result = componentsToRemove.Select(t => t.Name).ToArray();
            }
            return result;
        }

        //No multi-edition REQUIRED: This is Selection dependent to handle correctly the popup
        static TestCaseData[] s_RemoveAdditionalDataComponentTestCaseData =
        {
            new TestCaseData(1, typeof(AdditionalBanana), new Type[] {typeof(Banana), typeof(AdditionalBanana) })
                .SetName("For single additional data, when removing it, the target component is deleted")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(1, typeof(AdditionalBananaColor), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(AdditionalBananaColor)})
                .SetName("For multiple additional data, when removing one of them, target component is deleted, and the other additional data")
                .Returns(Array.Empty<Type>()),
           new TestCaseData(1, typeof(AdditionalBananaColor), new Type[] {typeof(Banana), typeof(Banana), typeof(AdditionalBanana), typeof(AdditionalBananaColor)})
                .SetName("For multiple additional component and data, when removing one of them everything is removed")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(1, typeof(AdditionalApple), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(Apple), typeof(AdditionalApple)})
                .SetName("For multiple types of target component, when deleting an additional data, only the target component is being removed")
                .Returns(new Type[] {typeof(Banana), typeof(AdditionalBanana)}),
            new TestCaseData(3, typeof(AdditionalBanana), new Type[] {typeof(Banana), typeof(AdditionalBanana) })
                .SetName("For single additional data, when removing it, the target component is deleted (multi-edition case)")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(3, typeof(AdditionalBananaColor), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(AdditionalBananaColor)})
                .SetName("For multiple additional data, when removing one of them, target component is deleted, and the other additional data (multi-edition case)")
                .Returns(Array.Empty<Type>()),
           new TestCaseData(3, typeof(AdditionalBananaColor), new Type[] {typeof(Banana), typeof(Banana), typeof(AdditionalBanana), typeof(AdditionalBananaColor)})
                .SetName("For multiple additional component and data, when removing one of them everything is removed (multi-edition case)")
                .Returns(Array.Empty<Type>()),
            new TestCaseData(3, typeof(AdditionalApple), new Type[] {typeof(Banana), typeof(AdditionalBanana), typeof(Apple), typeof(AdditionalApple)})
                .SetName("For multiple types of target component, when deleting an additional data, only the target component is being removed (multi-edition case)")
                .Returns(new Type[] {typeof(Banana), typeof(AdditionalBanana)})
        };

        [Test, TestCaseSource(nameof(s_RemoveAdditionalDataComponentTestCaseData))]
        [NUnit.Framework.Property("FogBugz", "1396805")]
        [NUnit.Framework.Property("Jira", "UUM-5452")]
        public Type[] RemoveAdditionalDataComponentAndPropagateToComponent(int selectionAmount, [DisallowNull] Type componentToRemove, [DisallowNull] Type[] componentsToAdd)
        {
            return GenericRemoveComponent(selectionAmount, componentToRemove, componentsToAdd, c => RemoveAdditionalDataUtils.RemoveAdditionalData(new UnityEditor.MenuCommand(c), false));
        }
        #endregion // UnityEngine.Rendering.Tests
    }
}
