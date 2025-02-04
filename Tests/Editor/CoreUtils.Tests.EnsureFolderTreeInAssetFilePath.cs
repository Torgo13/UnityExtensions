using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering;

namespace UnityExtensions.Editor.Tests
{
    partial class CoreUtilsTests
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/CoreUtils.Tests.EnsureFolderTreeInAssetFilePath.cs
        #region UnityEditor.Rendering.Tests
        [Test]
        [TestCase("Assets/TestFolder/")]
        [TestCase("Assets/TestFolder\\")]
        [TestCase("Assets/TestFolder/123/Folder/")]
        [TestCase("Assets/TestFolder\\123\\Folder\\")]
        [TestCase("Assets/TestFolder/something.mat")]
        [TestCase("Assets/TestFolder\\something.mat")]
        public void EnsureFolderTreeInAssetFilePath(string path)
        {
            string folderPath = Path.GetDirectoryName(path);
            CoreUtils.EnsureFolderTreeInAssetFilePath(path);
            Assert.True(AssetDatabase.IsValidFolder(folderPath));
        }

        [Test]
        [TestCase("NotAssetsFolder/TestFolder/", TestName = "EnsureFolderTreeInAssetFilePath throws when filePath does not start with Assets/")]
        public void EnsureFolderTreeInAssetFilePathThrows(string folderPath)
        {
            Assert.False(AssetDatabase.IsValidFolder(folderPath));
            Assert.Throws<ArgumentException>(() => CoreUtils.EnsureFolderTreeInAssetFilePath(folderPath));
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset("Assets/TestFolder");
        }
        #endregion // UnityEditor.Rendering.Tests
    }
}
