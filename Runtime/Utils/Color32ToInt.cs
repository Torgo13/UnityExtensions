using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Conversion struct which takes advantage of Color32 struct layout for fast conversion to and from Int32.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Color32ToInt
    {
        //https://github.com/Unity-Technologies/ProjectAuditor/blob/0.11.0/Editor/Utils/Color32ToInt.cs
        #region Unity.ProjectAuditor.Editor.Utils
        /// <summary>
        /// Int field which shares an offset with the color field.
        /// Set m_Color to read a converted value from this field.
        /// </summary>
        [FieldOffset(0)] readonly int m_Int;

        /// <summary>
        /// Color32 field which shares an offset with the int field.
        /// Set m_Int to read a converted value from this field.
        /// </summary>
        [FieldOffset(0)] readonly Color32 m_Color;

        /// <summary>
        /// The int value.
        /// </summary>
        public int Int => m_Int;

        /// <summary>
        /// The color value.
        /// </summary>
        public Color32 Color => m_Color;

        /// <summary>
        /// Constructor for Color32 to Int32 conversion.
        /// </summary>
        /// <param name="color">The color which will be converted to an int.</param>
        Color32ToInt(Color32 color)
        {
            m_Int = 0;
            m_Color = color;
        }

        /// <summary>
        /// Constructor for Int32 to Color32 conversion.
        /// </summary>
        /// <param name="value">The int which will be converted to an Color32.</param>
        Color32ToInt(int value)
        {
            m_Color = default;
            m_Int = value;
        }

        /// <summary>
        /// Convert a Color32 to an Int32.
        /// </summary>
        /// <param name="color">The Color32 which will be converted to an int.</param>
        /// <returns>The int value for the given color.</returns>
        public static int Convert(Color32 color)
        {
            var convert = new Color32ToInt(color);
            return convert.m_Int;
        }

        /// <summary>
        /// Convert a Color32 to an Int32.
        /// </summary>
        /// <param name="value">The int which will be converted to an Color32.</param>
        /// <returns>The Color32 value for the given int.</returns>
        public static Color32 Convert(int value)
        {
            var convert = new Color32ToInt(value);
            return convert.m_Color;
        }
        #endregion // Unity.ProjectAuditor.Editor.Utils
    }
}