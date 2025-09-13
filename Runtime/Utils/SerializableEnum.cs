using System;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Class to serialize Enum as string and recover its state
    /// </summary>
    [Serializable]
    public class SerializableEnum
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/SerializableEnum.cs
        #region UnityEngine.Rendering
        [SerializeField] private string enumValueAsString;
        [SerializeField] private string enumTypeAsString;

        /// <summary> Value as enum </summary>
        public Enum value
        {
            get => !string.IsNullOrEmpty(enumTypeAsString)
                   && Enum.TryParse(Type.GetType(enumTypeAsString), enumValueAsString, out object result)
                ? (Enum)result
                : default;
            set => enumValueAsString = value.ToString();
        }

        /// <summary>
        /// Construct an enum to be serialized with a type
        /// </summary>
        /// <param name="enumType">The underlying type of the enum</param>
        public SerializableEnum(Type enumType)
        {
            enumTypeAsString = enumType.AssemblyQualifiedName;
            enumValueAsString = Enum.GetNames(enumType)[0];
        }
        #endregion // UnityEngine.Rendering
    }
}
