using UnityEngine;
using UnityEngine.Assertions;

namespace UnityExtensions
{
    /// <summary>
    /// A helper class for accessing settings stored in <see cref="ScriptableObject"/> instances.
    /// </summary>
    /// <typeparam name="T">A class derived from <see cref="ScriptableObject"/>.</typeparam>
    public abstract class ScriptableSettings<T> : ScriptableSettingsBase<T> where T : ScriptableObject
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/ScriptableSettings.cs
        #region Unity.XR.CoreUtils
        const string k_CustomSavePathFormat = "{0}Resources/{1}.asset";
        const string k_SavePathFormat = "{0}Resources/ScriptableSettings/{1}.asset";
        const string k_LoadPathFormat = "ScriptableSettings/{0}";

        /// <summary>
        /// Retrieves a reference to the given settings class. Loads and initializes the class once, and caches the reference for all future access.
        /// </summary>
        /// <value>A settings class derived from <see cref="ScriptableObject"/>.</value>
        public static T Instance
        {
            get
            {
                if (BaseInstance == null)
                    CreateAndLoad();

                return BaseInstance;
            }
        }

        internal static T CreateAndLoad()
        {
            Assert.IsNull(BaseInstance);

            // Try to load the singleton
            var path = HasCustomPath ? GetFilePath() : string.Format(k_LoadPathFormat, GetFilePath());
            BaseInstance = Resources.Load(path) as T;

            // Create it if it doesn't exist
            if (BaseInstance == null)
            {
                BaseInstance = CreateInstance<T>();

                // And save it back out if appropriate
                Save(HasCustomPath ? k_CustomSavePathFormat : k_SavePathFormat);
            }

            Assert.IsNotNull(BaseInstance);

            return BaseInstance;
        }
        #endregion // Unity.XR.CoreUtils
    }
}