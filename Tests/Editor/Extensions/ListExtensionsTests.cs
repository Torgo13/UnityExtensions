using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UnityExtensions.Editor.Tests
{
    class ListExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Runtime/Scripts/ListExtensionTests.cs
        #region Unity.XR.CoreUtils

        const int k_DefaultTestCapacity = 35;

        class TestElement
        {
            public int Value;
        }

        void EnsureCapacityHelper<T>(List<T> list, int desiredCapacity = k_DefaultTestCapacity)
        {
            Assert.AreNotEqual(desiredCapacity, list.Capacity);

            list.EnsureCapacity(desiredCapacity);

            Assert.AreEqual(desiredCapacity, list.Capacity);
        }

        void SwapAtIndicesHelper<T>(List<T> list, System.Func<int, T> create, System.Action<T, T> assertEqual, int addCount = k_DefaultTestCapacity)
        {
            Assert.NotNull(create);
            for (var i = 0; i < addCount; i++)
            {
                list.Add(create(i));
            }

            Assert.AreEqual(list.Count, addCount);

            var lastIndex = addCount - 1;
            var firstValue = create(0);
            var lastValue = create(lastIndex);

            assertEqual(list[0], firstValue);
            assertEqual(list[lastIndex], lastValue);

            list.SwapAtIndices(0, lastIndex);

            assertEqual(list[0], lastValue);
            assertEqual(list[lastIndex], firstValue);
        }

        void FillHelper<T>(List<T> list, System.Action<T, T> assertEqual, T defaultNewValue, int fillNum = 5)
            where T : new()
        {
            Assert.NotNull(assertEqual);
            Assert.AreNotEqual(fillNum, 0);

            var beforeCount = list.Count;

            list.Fill(fillNum);

            var fillCount = beforeCount + fillNum;

            Assert.AreEqual(beforeCount + fillNum, list.Count);

            for (var i = beforeCount; i < fillCount; i++)
            {
                assertEqual(list[i], defaultNewValue);
            }
        }

        static void AssertEqualInt(int x, int y)
        {
            Assert.AreEqual(x, y);
        }

        static void AssertEqualRef(TestElement x, TestElement y)
        {
            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.AreNotEqual(x, y);
            Assert.AreEqual(x.Value, y.Value);
        }

        static int CreateInt(int i) => i;

        static TestElement CreateRef(int i) => new TestElement() { Value = i };

        [Test]
        public void Test_EnsureCapacity()
        {
            var intList = new List<int>(4);
            var refList = new List<TestElement>(2);

            EnsureCapacityHelper(intList);
            EnsureCapacityHelper(refList);
        }

        [Test]
        public void Test_SwapAtIndices()
        {
            var intList = new List<int>();
            var refList = new List<TestElement>();

            SwapAtIndicesHelper(intList, CreateInt, AssertEqualInt);
            SwapAtIndicesHelper(refList, CreateRef, AssertEqualRef);
        }

        [Test]
        public void Test_Fill()
        {
            var intList = new List<int>();
            var refList = new List<TestElement>();

            FillHelper(intList, AssertEqualInt, new int());
            FillHelper(refList, AssertEqualRef, new TestElement());
        }

        #endregion // Unity.XR.CoreUtils

        void EnsureRoomHelper<T>(List<T> list, int desiredRoom = k_DefaultTestCapacity)
        {
            Assert.AreNotEqual(desiredRoom, list.Capacity);

            list.EnsureRoom(desiredRoom);

            Assert.AreEqual(list.Count + desiredRoom, list.Capacity);
        }

        [Test]
        public void Test_EnsureRoom()
        {
            var intList = new List<int>(4);
            var refList = new List<TestElement>(2);

            EnsureRoomHelper(intList);
            EnsureRoomHelper(refList);
        }

        //https://github.com/Unity-Technologies/Graphics/blob/274b2c01bdceac862ed35742dcfa90e48e5f3248/Packages/com.unity.render-pipelines.core/Tests/Editor/RemoveRange.Extensions.Tests.cs
        #region UnityEditor.Rendering.Tests
        static TestCaseData[] s_ListTestsCaseDatas =
        {
            new TestCaseData(new int[] {1,2,3,4,5,6}, 1, 2).SetName("Remove middle"),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 0, 2).SetName("Remove front"),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 0, 6).SetName("Remove all"),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 5, 1).SetName("Remove back"),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 0, 0).SetName("Index 0"),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 5, 0).SetName("Count 0")
        };

        bool ItemInRangeAreRemovedAfterRemoveRange<TList>(TList list, int startIndex, int count)
            where TList : IList<int>
        {
            using (UnityEngine.Pool.ListPool<int>.Get(out var copy))
            {
                foreach (int integer in list)
                    copy.Add(integer);

                if (list.TryRemoveElementsInRange(startIndex, count, out var exception))
                {
                    copy.RemoveRange(startIndex, count);
                    return copy.SequenceEqual(list as List<int>);
                }

                return false;
            }
        }

        [Test, TestCaseSource(nameof(s_ListTestsCaseDatas))]
        public void ItemInRangeAreRemovedAfterRemoveRangeForList(int[] ints, int startIndex, int count)
        {
            using (UnityEngine.Pool.ListPool<int>.Get(out var copy))
            {
                copy.AddRange(ints);
                Assert.IsTrue(ItemInRangeAreRemovedAfterRemoveRange(copy, startIndex, count));
            }
        }

        static TestCaseData[] s_ListTestsCaseDatasExceptions =
        {
            new TestCaseData(new int[] {1,2,3,4,5,6}, 5, -1).SetName("Count negative").Returns(typeof(System.ArgumentOutOfRangeException)),
            new TestCaseData(new int[] {1,2,3,4,5,6}, -1, 2).SetName("Index negative").Returns(typeof(System.ArgumentOutOfRangeException)),
            new TestCaseData(new int[] {1,2,3,4,5,6}, 5, 5).SetName("Count exceeds list size").Returns(typeof(System.ArgumentException)),
        };

        System.Exception ExceptionsAreCorrect<TList>(TList list, int startIndex, int count)
            where TList : IList<int>
        {
            list.TryRemoveElementsInRange(startIndex, count, out var error);
            return error;
        }

        [Test, TestCaseSource(nameof(s_ListTestsCaseDatasExceptions))]
        public System.Type ExceptionsAreCorrectForList(int[] ints, int startIndex, int count)
        {
            using (UnityEngine.Pool.ListPool<int>.Get(out var copy))
            {
                copy.AddRange(ints);
                return ExceptionsAreCorrect(copy, startIndex, count).GetType();
            }
        }
        #endregion // UnityEditor.Rendering.Tests
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections.Tests/ListExtensionsTests.cs
        #region ListExtensionsTests
        [Test]
        public void ListExtensions_RemoveSwapBack_Item()
        {
            var list = new List<char>(new[] { 'a', 'b', 'c', 'd' });

            Assert.True(list.RemoveSwapBack('b'));
            CollectionAssert.AreEqual(new[] { 'a', 'd', 'c', }, list);

            Assert.True(list.RemoveSwapBack('c'));
            CollectionAssert.AreEqual(new[] { 'a', 'd' }, list);

            Assert.False(list.RemoveSwapBack('z'));
            CollectionAssert.AreEqual(new[] { 'a', 'd' }, list);

            Assert.True(list.RemoveSwapBack('a'));
            CollectionAssert.AreEqual(new[] { 'd' }, list);

            Assert.True(list.RemoveSwapBack('d'));
            CollectionAssert.IsEmpty(list);

            Assert.False(list.RemoveSwapBack('d'));
            CollectionAssert.IsEmpty(list);
        }

        [Test]
        public void ListExtensions_RemoveSwapBack_Predicate()
        {
            var list = new List<char>(new[] { 'a', 'b', 'c', 'd' });

            Assert.True(list.RemoveSwapBack(c => c == 'b'));
            CollectionAssert.AreEqual(new[] { 'a', 'd', 'c', }, list);

            Assert.True(list.RemoveSwapBack(c => c == 'c'));
            CollectionAssert.AreEqual(new[] { 'a', 'd' }, list);

            Assert.False(list.RemoveSwapBack(c => c == 'z'));
            CollectionAssert.AreEqual(new[] { 'a', 'd' }, list);

            Assert.True(list.RemoveSwapBack(c => c == 'a'));
            CollectionAssert.AreEqual(new[] { 'd' }, list);

            Assert.True(list.RemoveSwapBack(c => c == 'd'));
            CollectionAssert.IsEmpty(list);

            Assert.False(list.RemoveSwapBack(c => c == 'd'));
            CollectionAssert.IsEmpty(list);
        }

        // https://unity3d.atlassian.net/browse/DOTSR-1432
        [Test]
        public void ListExtensions_RemoveAtSwapBack()
        {
            var list = new List<char>(new[] { 'a', 'b', 'c', 'd' });

            list.RemoveAtSwapBack(1);
            CollectionAssert.AreEqual(new[] { 'a', 'd', 'c', }, list);

            list.RemoveAtSwapBack(2);
            CollectionAssert.AreEqual(new[] { 'a', 'd' }, list);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => list.RemoveAtSwapBack(12));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => list.RemoveAtSwapBack(-5));

            list.RemoveAtSwapBack(0);
            CollectionAssert.AreEqual(new[] { 'd' }, list);

            list.RemoveAtSwapBack(0);
            CollectionAssert.IsEmpty(list);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => list.RemoveAtSwapBack(0));
        }
        #endregion // ListExtensionsTests
    }
}