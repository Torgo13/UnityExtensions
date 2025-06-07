using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using UnityEngine.Assertions;

namespace UnityExtensions
{
    public static class EnumUtilities
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/EnumUtility.cs
        #region Unity.Kinematica
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = EnumValues<T>.Values;

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        if (type.GetDescription(val, out var description))
                        {
                            return description;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static void GetAllDescriptions<T>(this T e, List<string> list) where T : IConvertible
        {
            Assert.IsTrue(e is Enum);
            Assert.IsNotNull(list);

            Type type = e.GetType();
            Array values = EnumValues<T>.Values;

            foreach (int val in values)
            {
                if (type.GetDescription(val, out var description))
                {
                    list.Add(description);
                }
            }
        }

        public static bool GetDescription(this Type type, int val, out string description)
        {
            var memInfo = type.GetMember(type.GetEnumName(val));
            var customAttributes = memInfo[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
            var descriptionAttribute = customAttributes.Length > 0
                ? customAttributes[0] as DescriptionAttribute
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
    }

    /// <summary>
    /// Helper class for caching enum values.
    /// </summary>
    /// <typeparam name="T">The enum type whose values should be cached.</typeparam>
    public static class EnumValues<T>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/EnumValues.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Cached result of Enum.GetValues.
        /// </summary>
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
        #endregion // Unity.XR.CoreUtils
    }
}
