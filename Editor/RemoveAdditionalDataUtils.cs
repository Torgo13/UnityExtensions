using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Utilities to remove <see cref="MonoBehaviour"/> implementing <see cref="IAdditionalData"/>
    /// </summary>
    public static class RemoveAdditionalDataUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/RemoveAdditionalDataUtils.cs
        #region UnityEditor.Rendering
        static int s_DialogToSkip;

        /// <summary>
        /// Removes a <see cref="IAdditionalData"/> and it's components defined by <see cref="RequireComponent"/>
        /// </summary>
        /// <param name="command">The command that is executing the removal</param>
        /// <param name="promptDisplay">If the command must prompt a display to get user confirmation</param>
        /// <exception cref="Exception">If the given <see cref="MonoBehaviour"/> is not an <see cref="IAdditionalData"/></exception>
        public static void RemoveAdditionalData([DisallowNull] MenuCommand command, bool promptDisplay = true)
        {
            if (command.context is not Component component)
                return;

            //If the user agrees to remove the component, everything is removed in the current selection.
            //So other components will not trigger this (contextual menu implementation check component existence)
            //But if the user chose to cancel, we need to skip the prompt for a certain number of components given by the selection size.
            if (ShouldPrompt())
                RemoveAdditionalData(component, promptDisplay);
        }

        static void RemoveAdditionalData([DisallowNull] Component additionalDataComponent, bool promptDisplay = true)
        {
            using (ListPool<Type>.Get(out var componentTypesToRemove))
            {
                if (!TryGetComponentsToRemove((IAdditionalData)additionalDataComponent, componentTypesToRemove, out var error))
                    throw error;

                if (!promptDisplay || EditorUtility.DisplayDialog(
                    title: "Are you sure you want to proceed?",
                    message: $"This operation will also remove {string.Join($"{Environment.NewLine} - ", componentTypesToRemove)}.",
                    ok: "Remove everything",
                    cancel: "Cancel"))
                {
                    RemoveAdditionalDataComponentOnSelection(additionalDataComponent.GetType(), componentTypesToRemove);
                }
                else
                {
                    IgnoreNextPromptsForThisSelection();
                }
            }
        }

        static void IgnoreNextPromptsForThisSelection()
            => s_DialogToSkip = Selection.count - 1;

        static bool ShouldPrompt()
        {
            if (s_DialogToSkip > 0)
            {
                --s_DialogToSkip;
                return false;
            }

            return true;
        }

        static void RemoveAdditionalDataComponentOnSelection([DisallowNull] Type additionalDataType, [DisallowNull] List<Type> componentsTypeToRemove)
        {
            foreach (var selectedGameObject in Selection.gameObjects)
            {
                RemoveAdditionalDataComponent(selectedGameObject.GetComponent(additionalDataType), componentsTypeToRemove);
            }
        }

        static void RemoveAdditionalDataComponent([DisallowNull] Component additionalDataComponent, [DisallowNull] List<Type> componentsTypeToRemove)
        {
            using (ListPool<Component>.Get(out var components))
            {
                // Fetch all components
                foreach (var type in componentsTypeToRemove)
                {
                    components.AddRange(additionalDataComponent.GetComponents(type));
                }

                // Remove all of them
                foreach (var mono in components)
                {
                    RemoveComponentUtils.RemoveComponent(mono);
                }
            }
        }

        //internal for tests
        [MustUseReturnValue]
        internal static bool TryGetComponentsToRemove([DisallowNull] IAdditionalData additionalData, [DisallowNull] List<Type> componentsToRemove, [NotNullWhen(false)] out Exception error)
        {
            var type = additionalData.GetType();
            var requiredComponents = type.GetCustomAttributes(typeof(RequireComponent), true).Cast<RequireComponent>();

            if (!requiredComponents.Any())
            {
                error = new Exception($"Missing attribute {typeof(RequireComponent).FullName} on type {type.FullName}");
                return false;
            }

            foreach (var rc in requiredComponents)
            {
                componentsToRemove.Add(rc.m_Type0);
                if (rc.m_Type1 != null)
                    componentsToRemove.Add(rc.m_Type1);
                if (rc.m_Type2 != null)
                    componentsToRemove.Add(rc.m_Type2);
            }

            error = null;
            return true;
        }
        #endregion // UnityEditor.Rendering
    }
}
