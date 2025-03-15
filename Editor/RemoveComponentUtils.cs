using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityExtensions.Editor
{
    public static class RemoveComponentUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/RemoveComponentUtils.cs
        #region UnityEditor.Rendering
        public static IEnumerable<Component> ComponentDependencies([DisallowNull] Component component)
        {
            if (component == null)
                yield break;

            var componentType = component.GetType();
            foreach (var c in component.gameObject.GetComponents<Component>())
            {
                foreach (var rc in c.GetType().GetCustomAttributes(typeof(RequireComponent), true).Cast<RequireComponent>())
                {
                    if (rc.m_Type0 == componentType || rc.m_Type1 == componentType || rc.m_Type2 == componentType)
                    {
                        yield return c;
                        break;
                    }
                }
            }
        }

        public static bool CanRemoveComponent([DisallowNull] Component component, IEnumerable<Component> dependencies)
        {
            if (!dependencies.Any())
                return true;

            Component firstDependency = dependencies.First();
            string error = $"Can't remove {component.GetType().Name} because {firstDependency.GetType().Name} depends on it.";
            EditorUtility.DisplayDialog("Can't remove component", error, "OK");
            return false;
        }

        public static bool RemoveComponent([DisallowNull] Component component, IEnumerable<Component> dependencies)
        {
            var additionalData = dependencies
                    .Where(c => c != component && c is IAdditionalData)
                    .ToList();

            if (!CanRemoveComponent(component, dependencies.Where(c => !additionalData.Contains(c))))
                return false;

            bool removed = true;
            var isAssetEditing = EditorUtility.IsPersistent(component);
            try
            {
                if (isAssetEditing)
                {
                    AssetDatabase.StartAssetEditing();
                }
                Undo.SetCurrentGroupName($"Remove {component.GetType()} and additional data components");

                // The components with RequireComponent(typeof(T)) also contain the AdditionalData attribute, proceed with removal
                foreach (var additionalDataComponent in additionalData)
                {
                    if (additionalDataComponent != null)
                    {
                        Undo.DestroyObjectImmediate(additionalDataComponent);
                    }
                }
                Undo.DestroyObjectImmediate(component);
            }
            catch
            {
                removed = false;
            }
            finally
            {
                if (isAssetEditing)
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            return removed;
        }

        public static void RemoveComponent([DisallowNull] Component comp)
        {
            var dependencies = ComponentDependencies(comp);
            if (!RemoveComponent(comp, dependencies))
            {
                //preserve built-in behavior
                if (CanRemoveComponent(comp, dependencies))
                    Undo.DestroyObjectImmediate(comp);
            }
        }
        #endregion // UnityEditor.Rendering
    }
}
