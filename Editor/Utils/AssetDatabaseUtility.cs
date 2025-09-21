using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace PKGE.Editor
{
    public static class AssetDatabaseUtility
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/AssetDatabaseUtility.cs
        #region Unity.LiveCapture
        /// <summary>
        /// Returns the list of assets at given directory.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="directory">Path of the assets to load.</param>
        /// <param name="includeSubDirectories">Whether to include child directories in the search or not.</param>
        /// <returns>The list of assets.</returns>
        public static T[] GetAssetsAtPathArray<T>(string directory, bool includeSubDirectories = true) where T : UnityObject
        {
            if (string.IsNullOrEmpty(directory))
            {
                return Array.Empty<T>();
            }

            if (!Directory.Exists(Path.GetDirectoryName($"{directory}/")))
            {
                return Array.Empty<T>();
            }

            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { directory });
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath);

            if (!includeSubDirectories)
            {
                var directoryOS = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                paths = paths.Where(p => Path.GetDirectoryName(p) == directoryOS).ToArray();
            }

            return paths.Select(AssetDatabase.LoadAssetAtPath<T>);
        }

        /// <inheritdoc cref="GetAssetsAtPathArray{T}(string, bool)"/>
        public static List<T> GetAssetsAtPath<T>(string directory, bool includeSubDirectories = true) where T : UnityObject
        {
            return new List<T>(GetAssetsAtPathArray<T>(directory, includeSubDirectories));
        }

        /// <summary>
        /// Returns the GUID of a given asset.
        /// </summary>
        /// <param name="asset">The asset to get the GUID from.</param>
        /// <returns>The string representation of the GUID of the asset.</returns>
        public static string GetAssetGUID(UnityObject asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);

            return AssetDatabase.AssetPathToGUID(path);
        }

        /// <summary>
        /// Retrieves the GUID of an asset given its instanceID.
        /// </summary>
        /// <param name="instanceID">The instanceID of the asset to retrieve the GUID from.</param>
        /// <returns>The string representation of the GUID of the asset.</returns>
        public static string GetAssetGUID(int instanceID)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);

            return AssetDatabase.AssetPathToGUID(path);
        }

        /// <summary>
        /// Returns the asset associated with a given GUID.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="guid">The string representation of a GUID.</param>
        /// <returns>The asset associated with the given GUID.</returns>
        public static T LoadAssetWithGuid<T>(string guid) where T : UnityObject
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// Returns the asset associated with a given GUID.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="guid">The GUID.</param>
        /// <returns>The asset associated with the given GUID.</returns>
        public static T LoadAssetWithGuid<T>(Guid guid) where T : UnityObject
        {
            return LoadAssetWithGuid<T>(guid.ToString("N"));
        }

        /// <summary>
        /// Returns the path of the asset associated with a given GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns>The path of the asset associated with the given GUID.</returns>
        public static string GUIDToAssetPath(Guid guid)
        {
            return AssetDatabase.GUIDToAssetPath(guid.ToString("N"));
        }

        /// <summary>
        /// Returns the asset associated with a given SerializableGuid.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="guid">The SerializableGuid.</param>
        /// <returns>The asset associated with the given GUID.</returns>
        internal static T LoadAssetWithGuid<T>(SerializableGuid guid) where T : UnityObject
        {
            return LoadAssetWithGuid<T>(guid.ToString());
        }

        /// <summary>
        /// Returns the list of sub assets at given asset.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="asset">The main asset reference.</param>
        /// <returns>The list of assets.</returns>
        public static List<T> GetSubAssets<T>(UnityObject asset) where T : UnityObject
        {
            var assets = new List<T>();
            var path = AssetDatabase.GetAssetPath(asset);

            if (!string.IsNullOrEmpty(path))
            {
                foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (a is T subasset)
                        assets.Add(subasset);
                }
            }

            return assets;
        }
        #endregion // Unity.LiveCapture
        
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/AssetDatabaseHelper.cs
        #region UnityEditor.Rendering
        /// <summary>
        /// Finds all assets of type T in the project.
        /// </summary>
        /// <param name="extension">Asset type extension i.e ".mat" for materials. Specifying extension make this faster.</param>
        /// <typeparam name="T">The type of asset you are looking for</typeparam>
        /// <returns>An IEnumerable off all assets found.</returns>
        public static IEnumerable<T> FindAssets<T>(string extension = null)
            where T : UnityObject
        {
            string query = BuildQueryToFindAssets<T>(extension);
            foreach (var guid in AssetDatabase.FindAssets(query))
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                if (asset is T castAsset)
                    yield return castAsset;
            }
        }
        
        /// <summary>
        /// Finds all assets paths of type T in the project.
        /// </summary>
        /// <param name="extension">Asset type extension i.e ".mat" for materials. Specifying extension make this faster.</param>
        /// <typeparam name="T">The type of asset you are looking for</typeparam>
        /// <returns>An IEnumerable off all assets paths found.</returns>
        public static IEnumerable<string> FindAssetPaths<T>(string extension = null)
            where T : UnityObject
        {
            string query = BuildQueryToFindAssets<T>(extension);
            foreach (var guid in AssetDatabase.FindAssets(query))
                yield return AssetDatabase.GUIDToAssetPath(guid);
        }

        static string BuildQueryToFindAssets<T>(string extension = null)
            where T : UnityObject
        {
            string typeName = typeof(T).ToString();
            int i = typeName.LastIndexOf('.');
            if (i != -1)
            {
                typeName = typeName.Substring(i+1, typeName.Length - i-1);
            }

            return string.IsNullOrEmpty(extension) ? $"t:{typeName}" : $"t:{typeName} glob:\"**/*{extension}\"";
        }
        #endregion // UnityEditor.Rendering
    }
}
