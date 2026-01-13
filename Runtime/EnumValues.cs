using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace PKGE
{
    public static class EnumUtilities
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/EnumUtility.cs
        #region Unity.Kinematica
        public static string GetDescription<T>(this T val) where T : Enum
        {
            return GetDescription<T>(EnumValues<T>.Name(val));
        }

        public static string GetDescription<T>(string name) where T : Enum
        {
            if (TryGetDescription<T>(name, out var description))
                return description;

            return string.Empty;
        }

        public static bool TryGetDescription<T>(string name, out string description) where T : Enum
        {
            System.Reflection.MemberInfo[] memberInfo = typeof(T).GetMember(name);
            foreach (var member in memberInfo)
            {
                object[] customAttributes = member.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false);
                foreach (var attribute in customAttributes)
                {
                    if (attribute is System.ComponentModel.DescriptionAttribute descriptionAttribute)
                    {
                        description = descriptionAttribute.Description;
                        return true;
                    }
                }
            }

            description = string.Empty;
            return false;
        }

        public static void GetAllDescriptions<T>(List<string> list) where T : Enum
        {
            Assert.IsNotNull(list);

            string[] names = EnumValues<T>.Names;
            list.EnsureCapacity(names.Length);

            foreach (var name in names)
            {
                if (TryGetDescription<T>(name, out var description))
                {
                    list.Add(description);
                }
            }
        }

        public static string[] GetDescriptions<T>() where T : Enum
        {
            using var _0 = UnityEngine.Pool.ListPool<string>.Get(out var list);
            GetAllDescriptions<T>(list);
            return list.ToArray();
        }

        public static bool GetDescription(this Type type, int val, out string description)
        {
            var memInfo = type.GetMember(type.GetEnumName(val));
            var customAttributes = memInfo[0]
                .GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), inherit: false);
            var descriptionAttribute = customAttributes.Length > 0
                ? customAttributes[0] as System.ComponentModel.DescriptionAttribute
                : null;

            if (descriptionAttribute != null)
            {
                description = descriptionAttribute.Description;
                return true;
            }

            description = string.Empty;
            return false;
        }

        public static T TypeFromName<T>(string name) where T : Enum
        {
            var values = EnumValues<T>.Values;
            foreach (var value in values)
            {
                if (value.GetDescription().Equals(name, StringComparison.Ordinal))
                    return value;
            }

            return default;
        }
        #endregion // Unity.Kinematica

#if UNITY_EDITOR
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Editor/Core/Utilities/EnumExtensions.cs
        #region Unity.LiveCapture.Editor
        /// <summary>
        /// Gets the display name of the enum value.
        /// </summary>
        /// <param name="value">The enum value to get the display name of.</param>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <returns>The display name of the enum.</returns>
        /// <remarks><see cref="UnityEngine.InspectorNameAttribute"/> is stripped from the build
        /// unless manually preserved with <see cref="UnityEngine.Scripting.PreserveAttribute"/> or link.xml.</remarks>
        public static string GetDisplayName<T>(this T value) where T : Enum
        {
            var memberInfo = typeof(T).GetMember(EnumValues<T>.Name(value));

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(UnityEngine.InspectorNameAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((UnityEngine.InspectorNameAttribute)attrs[0]).displayName;
                }
            }

            return value.ToString();
        }
        #endregion // Unity.LiveCapture.Editor

        /// <inheritdoc cref="GetDisplayName{T}(T)"/>
        public static string[] GetDisplayNames<T>() where T : Enum
        {
            var displayNames = new string[EnumValues<T>.Values.Length];
            for (int i = 0; i < displayNames.Length; i++)
            {
                System.Reflection.MemberInfo[] memberInfo = typeof(T).GetMember(EnumValues<T>.Names[i]);

                if (memberInfo.Length > 0)
                {
                    object[] attrs = memberInfo[0].GetCustomAttributes(typeof(UnityEngine.InspectorNameAttribute), inherit: false);

                    if (attrs.Length > 0)
                    {
                        displayNames[i] = ((UnityEngine.InspectorNameAttribute)attrs[0]).displayName;
                        continue;
                    }
                }
            }
            
            return displayNames;
        }
#endif // UNITY_EDITOR
    }

    /// <summary>
    /// Helper class for caching enum values.
    /// </summary>
    /// <typeparam name="T">The enum type whose values should be cached.</typeparam>
    public static class EnumValues<T>
        where T : Enum
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/EnumValues.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Cached result of Enum.GetValues.
        /// </summary>
        /// <remarks>
        /// The elements of the array are sorted by the binary values of the enumeration constants (that is, by their unsigned magnitude).
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.enum.getvalues?view=netstandard-2.1"/>
        /// </remarks>
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Cached result of Enum.GetNames.
        /// </summary>
        public static readonly string[] Names = Enum.GetNames(typeof(T));

        /// <summary>
        /// Cached result of EnumUtilities.GetDescriptions.
        /// </summary>
        public static readonly string[] Descriptions = EnumUtilities.GetDescriptions<T>();

#if UNITY_EDITOR
        /// <summary>
        /// Cached result of EnumUtilities.GetDisplayNames.
        /// </summary>
        public static readonly string[] DisplayNames = EnumUtilities.GetDisplayNames<T>();
#endif // UNITY_EDITOR

        /// <summary>
        /// If negative values are present, <see cref="Values"/> will be sorted as if they are unsigned.
        /// This creates a copy of the array and sorts it so that negative values are before the positive ones.
        /// </summary>
        /// <remarks>Cannot be used to index other arrays if sorting was performed.</remarks>
        public static readonly T[] SortedValues = SortValues();

        public static int Length => Values.Length;

        private static T[] SortValues()
        {
            var sortedValues = new T[Length];
            Array.Copy(Values, sortedValues, Length);
            Array.Sort(sortedValues);
            return sortedValues;
        }

        public static bool TryGetIndex(T value, out int index)
        {
            index = Array.IndexOf(Values, value);
            return index != -1;
        }

        public static bool TryGetIndex(string name, out int index)
        {
            index = Array.IndexOf(Names, name);
            return index != -1;
        }

        public static string Name(T value)
        {
            _ = TryGetName(value, out var name);
            return name;
        }

        public static bool TryGetName(T value, out string name)
        {
            if (TryGetIndex(value, out var index))
            {
                name = Names[index];
                return true;
            }

            name = string.Empty;
            return false;
        }

        public static bool TryParse(string name, out T result)
        {
            if (TryGetIndex(name, out var index))
            {
                result = Values[index];
                return true;
            }

            result = default;
            return false;
        }

        public static string Description(T value)
        {
            _ = TryGetDescription(value, out var description);
            return description;
        }

        public static bool TryGetDescription(T value, out string description)
        {
            if (TryGetIndex(value, out var index))
            {
                description = Descriptions[index];
                return true;
            }

            description = string.Empty;
            return false;
        }

#if UNITY_EDITOR
        public static string DisplayName(T value)
        {
            _ = TryGetDisplayName(value, out var displayName);
            return displayName;
        }

        public static bool TryGetDisplayName(T value, out string displayName)
        {
            if (TryGetIndex(value, out var index))
            {
                displayName = DisplayNames[index];
                return true;
            }

            displayName = string.Empty;
            return false;
        }
#endif // UNITY_EDITOR
    }

    public static class EnumExtensions
    {
        public static string ToStringFast<T>(this T e) where T : Enum
        {
            return EnumValues<T>.Name(e);
        }

        public static string ToDisplayStringFast<T>(this T e) where T : Enum
        {
            return EnumValues<T>.Description(e);
        }
    }
}
