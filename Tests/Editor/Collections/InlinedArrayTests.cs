using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PKGE.Collections;

namespace PKGE.Tests
{
    public class InlinedArrayTests
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Assets/Tests/InputSystem/Utilities/InlinedArrayTests.cs
        #region UnityEngine.InputSystem.Utilities
        [Test]
        [Category("Utilities")]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        [TestCase(10, 0)]
        [TestCase(10, 5)]
        [TestCase(10, 9)]
        public void Utilities_InlinedArray_CanRemoveElementAtIndex(int count, int removeAt)
        {
            var comparisonList = new List<string>();
            var array = new InlinedArray<string>();

            for (var i = 0; i < count; ++i)
            {
                comparisonList.Add(i.ToString());
                array.Append(i.ToString());
            }

            Assert.That(array.length, Is.EqualTo(comparisonList.Count));

            array.RemoveAt(removeAt);
            comparisonList.RemoveAt(removeAt);

            Assert.That(array, Is.EquivalentTo(comparisonList));
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
