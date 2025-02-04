using System;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Class to serizalize Enum as string and recover it's state
    /// </summary>
    [Serializable]
    public class SerializableEnum
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/SerializableEnum.cs
        #region UnityEngine.Rendering
        [SerializeField] private string m_EnumValueAsString;
        [SerializeField] private string m_EnumTypeAsString;

        /// <summary> Value as enum </summary>
        public Enum value
        {
            get => !string.IsNullOrEmpty(m_EnumTypeAsString) && Enum.TryParse(Type.GetType(m_EnumTypeAsString), m_EnumValueAsString, out object result) ? (Enum)result : default;
            set => m_EnumValueAsString = value.ToString();
        }

        /// <summary>
        /// Construct an enum to be serialized with a type
        /// </summary>
        /// <param name="enumType">The underliying type of the enum</param>
        public SerializableEnum(Type enumType)
        {
            m_EnumTypeAsString = enumType.AssemblyQualifiedName;
            m_EnumValueAsString = Enum.GetNames(enumType)[0];
        }
        #endregion // UnityEngine.Rendering
    }
}
