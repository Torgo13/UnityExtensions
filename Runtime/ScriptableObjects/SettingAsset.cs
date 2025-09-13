using System;
using System.IO;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// A class used to define a singleton instance that is stored as an asset.
    /// </summary>
    /// <remarks>
    /// Unlike ScriptableSingleton, this class can be used outside the editor.
    /// </remarks>
    /// <typeparam name="T">The type of the setting asset.</typeparam>
    public abstract class SettingAsset<T> : ScriptableObject where T : ScriptableObject
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/SettingAsset.cs
        #region Unity.LiveCapture
        static T _instance;

        /// <summary>
        /// The asset instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateOrLoad();
                return _instance;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SettingAsset{T}"/> instance.
        /// </summary>
        protected SettingAsset()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        static T CreateOrLoad()
        {
#if UNITY_EDITOR
            var filePath = GetFilePath();

            if (!string.IsNullOrEmpty(filePath))
            {
                var objects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                foreach (var obj in objects)
                {
                    if (obj is T t)
                    {
                        _instance = t;
                        break;
                    }
                }
            }
#else
            _instance = Resources.Load<T>(typeof(T).Name);
#endif
            if (_instance == null)
            {
                _instance = CreateInstance<T>();
                _instance.hideFlags = HideFlags.DontSave;
            }

            return _instance;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Serializes the asset to disk.
        /// </summary>
        public static void Save()
        {
            if (_instance == null)
            {
                Debug.LogError($"Cannot save {nameof(SettingAsset<T>)}: no instance!");
                return;
            }

            var filePath = GetFilePath();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath!);
            }

            UnityEngine.Object obj = _instance;
            if (obj == null)
                return;

            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { obj },
                filePath, true);
        }

        /// <summary>
        /// Gets the file path of the asset relative to the project root folder.
        /// </summary>
        /// <returns>The file path of the asset.</returns>
        protected static string GetFilePath()
        {
            foreach (var customAttribute in typeof(T).GetCustomAttributes(true))
            {
                if (customAttribute is SettingFilePathAttribute attribute)
                {
                    return attribute.FilePath;
                }
            }
            return string.Empty;
        }
#endif
        #endregion // Unity.LiveCapture
    }

    /// <summary>
    /// An attribute that specifies a file location relative to the Project folder or Unity's preferences folder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class SettingFilePathAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/SettingAsset.cs
        #region Unity.LiveCapture
        internal string FilePath { get; }

        public SettingFilePathAttribute(string relativePath, Location location)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("Path is empty.", nameof(relativePath));

            FilePath = CombineFilePath(relativePath, location);
        }

        static string CombineFilePath(string relativePath, Location location)
        {
            if (relativePath[0] == '/')
                relativePath = relativePath.Substring(1);

            switch (location)
            {
#if UNITY_EDITOR
                case Location.PreferencesFolder:
                    return UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder + '/' + relativePath;
#endif
                case Location.ProjectFolder:
                    return relativePath;
                default:
                    Debug.LogError("Unhandled enum: " + location);
                    return relativePath;
            }
        }

        /// <summary>
        /// Specifies the folder location that Unity uses together with the relative path provided in the
        /// <see cref="SettingFilePathAttribute"/> constructor.
        /// </summary>
        public enum Location
        {
            /// <summary>
            /// Use this location to save a file relative to the Preferences folder.
            /// Useful for per-user files that are across all projects.
            /// </summary>
            PreferencesFolder,
            /// <summary>
            /// Use this location to save a file relative to the Project Folder.
            /// Useful for per-project files (not shared between projects).
            /// </summary>
            ProjectFolder,
        }
        #endregion // Unity.LiveCapture
    }
}
