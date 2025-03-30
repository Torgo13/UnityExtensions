using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    class SerializationUtilitiesTests
    {
        //https://github.com/needle-mirror/com.unity.addressables/blob/b9b97fefbdf24fe7f86d2f50efae7f0fd5a1bba7/Tests/Editor/ContentCatalogTests.cs
        #region UnityEditor.AddressableAssets.Tests
        [Test]
        public void SerializationUtility_ReadWrite_Int32()
        {
            var data = new byte[100];
            for (int i = 0; i < 1000; i++)
            {
                var val = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                var off = UnityEngine.Random.Range(0, data.Length - sizeof(int));
                Assert.AreEqual(off + sizeof(int), ArrayExtensions.WriteInt32ToByteArray(data, val, off));
                Assert.AreEqual(val, ArrayExtensions.ReadInt32FromByteArray(data, off));
            }
        }
        #endregion // UnityEditor.AddressableAssets.Tests
    }
}
