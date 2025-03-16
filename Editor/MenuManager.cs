using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Contains a set of method to be able to manage Menu Items for the editor
    /// </summary>
    public static class MenuManager
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/MenuManager.cs
        #region UnityEditor.Rendering
        #region Add Menu Item
        static Action<string, string, bool, int, Action, Func<bool>> _addMenuItem = GetAddMenuItemMethod();
        static Action<string, string, bool, int, Action, Func<bool>> GetAddMenuItemMethod()
        {
            MethodInfo addMenuItemMethodInfo = typeof(Menu).GetMethod("AddMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
            var nameParam = Expression.Parameter(typeof(string), "name");
            var shortcutParam = Expression.Parameter(typeof(string), "shortcut");
            var checkedParam = Expression.Parameter(typeof(bool), "checked");
            var priorityParam = Expression.Parameter(typeof(int), "priority");
            var executeParam = Expression.Parameter(typeof(Action), "execute");
            var validateParam = Expression.Parameter(typeof(Func<bool>), "validate");

            var addMenuItemExpressionCall = Expression.Call(null, addMenuItemMethodInfo,
                nameParam,
                shortcutParam,
                checkedParam,
                priorityParam,
                executeParam,
                validateParam);

            return Expression.Lambda<Action<string, string, bool, int, Action, Func<bool>>>(
                addMenuItemExpressionCall,
                nameParam,
                shortcutParam,
                checkedParam,
                priorityParam,
                executeParam,
                validateParam).Compile();
        }

        /// <summary>
        /// Adds a menu Item to the editor
        /// </summary>
        /// <param name="path">The path to the menu item</param>
        /// <param name="shortcut">The shortcut of the menu item</param>
        /// <param name="checked">If the item can have a state, pressed or not</param>
        /// <param name="priority">The priority of the menu item</param>
        /// <param name="execute">The action that will be called once the menu item is pressed</param>
        /// <param name="validate">The action that will be called to know if the menu item is enabled</param>
        public static void AddMenuItem(string path, string shortcut, bool @checked, int priority, System.Action execute, System.Func<bool> validate)
        {
            _addMenuItem(path, shortcut, @checked, priority, execute, validate);
        }

        #endregion

        #region Remove Menu Item
        static Action<string> _removeMenuItem = GetRemoveMenuItemMethod();
        static Action<string> GetRemoveMenuItemMethod()
        {
            MethodInfo removeMenuItemMethodInfo = typeof(Menu).GetMethod("RemoveMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
            var nameParam = Expression.Parameter(typeof(string), "name");
            return Expression.Lambda<Action<string>>(
                Expression.Call(null, removeMenuItemMethodInfo, nameParam),
                nameParam).Compile();
        }
        #endregion

        /// <summary>
        /// Removes a Menu item from the editor, if the path is not found it does nothing
        /// </summary>
        /// <param name="path">The path of the menu item to be removed</param>
        public static void RemoveMenuItem(string path)
        {
            _removeMenuItem(path);
        }
        #endregion // UnityEditor.Rendering
    }
}
