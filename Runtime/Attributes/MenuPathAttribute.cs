using System;

namespace UnityExtensions.Attributes
{
    /// <summary>
    /// An attribute placed to describe a menu path.
    /// </summary>
    /// <remarks>
    /// This is used similarly to the <see cref="UnityEditor.MenuItem"/> attribute.
    /// </remarks>
    public abstract class MenuPathAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Attributes/MenuPathAttribute.cs
        #region Unity.LiveCapture
        /// <summary>
        /// The menu item represented like a path name. For example, the menu item could be "Sub Menu/Action".
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// The order by which the menu items are displayed. Items in the same sub menu have a separator
        /// placed between them if their priority differs by more than 10.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Creates a new <see cref="MenuPathAttribute"/> instance.
        /// </summary>
        /// <param name="itemName">The menu item represented like a path name. For example, the menu item
        /// could be "Sub Menu/Action".</param>
        /// <param name="priority">The order by which the menu items are displayed. Items in the same sub
        /// menu have a separator placed between them if their priority differs by more than 10.</param>
        protected MenuPathAttribute(string itemName, int priority = 0)
        {
            ItemName = itemName;
            Priority = priority;
        }
        #endregion // Unity.LiveCapture
    }
}
