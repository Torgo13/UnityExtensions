using NUnit.Framework;
using UnityExtensions.Unsafe;

namespace UnityExtensions.Editor.Unsafe.Tests
{
    unsafe class FixedBufferStringQueueTests
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/FixedBufferStringQueueTests.cs
        #region UnityEngine.Rendering.Tests
        [Test]
        public void PushAndPopInBufferRange()
        {
            const int size = 512;
            var bufferStart = stackalloc byte[size];
            var buffer = new CoreUnsafeUtils.FixedBufferStringQueue(bufferStart, size);

            Assert.True(buffer.TryPush("Lorem ipsum dolor sit"));
            Assert.True(buffer.TryPush("amet, consectetur adipiscing"));
            Assert.True(buffer.TryPush("elit, sed do eiusmod"));
            Assert.True(buffer.TryPush("tempor incididunt ut labore"));

            Assert.AreEqual(4, buffer.Count);

            Assert.True(buffer.TryPop(out string v) && v == "Lorem ipsum dolor sit");
            Assert.True(buffer.TryPop(out v) && v == "amet, consectetur adipiscing");
            Assert.True(buffer.TryPop(out v) && v == "elit, sed do eiusmod");
            Assert.True(buffer.TryPop(out v) && v == "tempor incididunt ut labore");
        }

        [Test]
        public void PushAndPopOutOfBufferRange()
        {
            const int size = 64;
            var bufferStart = stackalloc byte[size];
            var buffer = new CoreUnsafeUtils.FixedBufferStringQueue(bufferStart, size);

            Assert.True(buffer.TryPush("Lorem ipsum dolor sit"));
            Assert.False(buffer.TryPush("amet, consectetur adipiscing"));

            Assert.AreEqual(1, buffer.Count);
        }

        [Test]
        public void PushAndPopAndClear()
        {
            const int size = 128;
            var bufferStart = stackalloc byte[size];
            var buffer = new CoreUnsafeUtils.FixedBufferStringQueue(bufferStart, size);

            Assert.True(buffer.TryPush("Lorem ipsum dolor sit"));
            Assert.True(buffer.TryPush("amet, consectetur adipiscing"));
            Assert.False(buffer.TryPush("elit, sed do eiusmod"));

            Assert.AreEqual(2, buffer.Count);
            buffer.Clear();
            Assert.AreEqual(0, buffer.Count);

            Assert.True(buffer.TryPush("elit, sed do eiusmod"));
            Assert.True(buffer.TryPush("tempor incididunt ut labore"));
            Assert.True(buffer.TryPop(out string v) && v == "elit, sed do eiusmod");
            Assert.True(buffer.TryPop(out v) && v == "tempor incididunt ut labore");

            Assert.False(buffer.TryPop(out v));
        }
        #endregion // UnityEngine.Rendering.Tests
    }
}
