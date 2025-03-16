#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class UndoBlockTests
    {
        /// <summary>
        /// Validates that objects registered for creation are properly undone.
        /// </summary>
        [Test]
        public void UndoBlock_RegisterCreatedObject_AddsToUndoStack()
        {
            // Arrange
            var testObject = new GameObject("CreateTestObject");
            
            // Act
            using (var undoBlock = new UndoBlock("Create TestObject"))
            {
                undoBlock.RegisterCreatedObject(testObject);
            }

            // Assert
            Undo.PerformUndo();
            var foundObject = GameObject.Find("CreateTestObject");
            Assert.IsNull(foundObject, "The object should have been undone and no longer exist.");
        }

        /// <summary>
        /// Ensures that changes to an object are tracked and reverted.
        /// </summary>
        [Test]
        public void UndoBlock_RecordObject_TracksChanges()
        {
            // Arrange
            var testObject = new GameObject("MoveTestObject");
            var transform = testObject.transform;
            Vector3 originalPosition = transform.position;

            // Act
            using (var undoBlock = new UndoBlock("Move Object"))
            {
                undoBlock.RecordObject(transform);
                transform.position = new Vector3(10, 0, 0);
            }

            // Assert
            Undo.PerformUndo();
            Assert.AreEqual(originalPosition, transform.position, "The object's position should have been reverted.");
        }

        /// <summary>
        /// Verifies that parenting operations are undone correctly.
        /// </summary>
        [Test]
        public void UndoBlock_SetTransformParent_ChangesParentCorrectly()
        {
            // Arrange
            var childObject = new GameObject("ChildObject");
            var parentObject = new GameObject("ParentObject");

            // Act
            using (var undoBlock = new UndoBlock("Set Parent"))
            {
                undoBlock.SetTransformParent(childObject.transform, parentObject.transform);
            }

            // Assert
            Undo.PerformUndo();
            Assert.IsNull(childObject.transform.parent, "The child's parent should have been reverted to null.");
        }

        /// <summary>
        /// Confirms that adding components is registered and can be undone.
        /// </summary>
        [Test]
        public void UndoBlock_AddComponent_RegistersUndo()
        {
            // Arrange
            var testObject = new GameObject("AddComponentTestObject");

            // Act
            using (var undoBlock = new UndoBlock("Add Component"))
            {
                undoBlock.AddComponent<BoxCollider>(testObject);
            }

            // Assert
            Undo.PerformUndo();
            var component = testObject.GetComponent<BoxCollider>();
            Assert.IsNull(component, "The component addition should have been undone.");
        }
    }
}
#endif
