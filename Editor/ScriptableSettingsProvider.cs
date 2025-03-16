using UnityEditor;
using UnityEngine.UIElements;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Expose a <see cref="ScriptableSettings{T}"/> object as a settings provider.
    /// </summary>
    /// <typeparam name="T">The ScriptableSettings type to be exposed.</typeparam>
    public abstract class ScriptableSettingsProvider<T> : SettingsProvider where T : ScriptableSettingsBase<T>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/ScriptableSettingsProvider.cs
        #region Unity.XR.CoreUtils.Editor
        T _target;
        SerializedObject _serializedObject;

        /// <summary>
        /// The ScriptableSettings being provided.
        /// </summary>
        protected T Target
        {
            get
            {
                if (_target == null || _serializedObject == null)
                    GetSerializedSettings();

                return _target;
            }
        }

        /// <summary>
        /// A SerializedObject representing the ScriptableSettings being provided.
        /// </summary>
        protected SerializedObject SerializedObject
        {
            get
            {
                if (_serializedObject == null)
                    _serializedObject = GetSerializedSettings();

                return _serializedObject;
            }
        }

        /// <summary>
        /// Initialize a new ScriptableSettingsProvider.
        /// </summary>
        /// <param name="path">The path to this settings view within the Preferences or Project Settings window.</param>
        /// <param name="scope">The scope of these settings.</param>
        protected ScriptableSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        /// <summary>
        /// Use this function to implement a handler for when the user clicks on the Settings in the Settings window.
        /// You can fetch a settings Asset or set up UIElements UI from this function.
        /// </summary>
        /// <param name="searchContext">Search context in the search box on the Settings window.</param>
        /// <param name="rootElement">Root of the UIElements tree. If you add to this root, the SettingsProvider uses
        /// UIElements instead of calling SettingsProvider.OnGUI to build the UI. If you do not add to this
        /// VisualElement, then you must use the IMGUI to build the UI.</param>
        public abstract override void OnActivate(string searchContext, VisualElement rootElement);

        /// <summary>
        /// Use this function to draw the UI based on IMGUI. This assumes you haven't added any children to the
        /// rootElement passed to the OnActivate function.
        /// </summary>
        /// <param name="searchContext">Search context for the Settings window. Used to show or hide relevant properties.</param>
        public abstract override void OnGUI(string searchContext);

        /// <summary>
        /// Initialize this settings object and return a SerializedObject wrapping it.
        /// </summary>
        /// <returns>The SerializedObject wrapper.</returns>
        SerializedObject GetSerializedSettings()
        {
            if (typeof(EditorScriptableSettings<T>).IsAssignableFrom(typeof(T)))
            {
                _target = EditorScriptableSettings<T>.Instance;
                return new SerializedObject(_target);
            }

            _target = ScriptableSettings<T>.Instance;
            return new SerializedObject(_target);
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
