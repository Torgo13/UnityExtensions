using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Ensures that all scriptable settings have backing data that can be inspected and edited at compile-time.
    /// </summary>
    [InitializeOnLoad]
    internal static class ScriptableSettingsInitializer
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/ScriptableSettingsInitializer.cs
        #region Unity.XR.CoreUtils.Editor
        static ScriptableSettingsInitializer()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                LoadAllSettingsClasses();
        }

        static void Update()
        {
            if (EditorApplication.isCompiling)
                return;

            EditorApplication.update -= Update;
            LoadAllSettingsClasses();
        }

        static void LoadAllSettingsClasses()
        {
            var instances = new List<ScriptableSettingsBase>();
            ReflectionUtils.ForEachAssembly(assembly =>
            {
                foreach (var type in GetSettingsClasses(assembly))
                {
                    instances.Add(ScriptableSettingsBase.GetInstanceByType(type));
                }
            });

            foreach (var instance in instances)
            {
                instance.LoadInEditor();
            }
        }

        static IEnumerable<Type> GetSettingsClasses(Assembly assembly)
        {
            Func<Type, bool> filter = t => t.IsSubclassOf(typeof(ScriptableSettings<>));
            Func<Type, bool> editorFilter = t => t.IsSubclassOf(typeof(ScriptableSettingsBase)) && !t.IsAbstract;
            return assembly.GetTypes().Where(EditorApplication.isPlayingOrWillChangePlaymode ? filter : editorFilter);
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
