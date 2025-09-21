using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PKGE.Editor
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
                foreach (var rc in (RequireComponent[])c.GetType().GetCustomAttributes(typeof(RequireComponent), inherit: true))
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
            using var _0 = UnityEngine.Pool.ListPool<Component>.Get(out var additionalData);
            foreach (var c in dependencies)
            {
                if (c != component && c is IAdditionalData)
                    additionalData.Add(c);
            }

            using var _1 = UnityEngine.Pool.ListPool<Component>.Get(out var remove);
            foreach (var c in dependencies)
            {
                if (!additionalData.Contains(c))
                    remove.Add(c);
            }

            if (!CanRemoveComponent(component, remove))
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
