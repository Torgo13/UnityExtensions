using UnityEditor;

namespace UnityExtensions.Editor
{
    public static class PrefabUtilities
    {
        //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Utils/Editor/PrefabUtilities.cs
        #region Unity.Megacity.EditorTools
        /// <summary>
        /// Helper function to allow reverting prefab changes from selection
        /// </summary>
        [MenuItem("Tools/Prefabs/Revert Selection")]
        static void RevertSelection()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                }
            }
        }

        [MenuItem("Prefabs/Revert Selection (preserve scale)")]
        static void RevertSelectionPreserveScale()
        {
            var gameObjectSelection = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(gameObjectSelection, "revert selection");
            foreach (var go in gameObjectSelection)
            {
                if (go != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(go) &&
                    PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                {
                    var t = EditorJsonUtility.ToJson(go.transform);
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                    EditorJsonUtility.FromJsonOverwrite(t, go.transform);
                }
            }
        }
        #endregion // Unity.Megacity.EditorTools
    }
}
