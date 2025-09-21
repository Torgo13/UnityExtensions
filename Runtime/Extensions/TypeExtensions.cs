using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/TypeExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Adds all types assignable to this one to a list, using an optional predicate test.
        /// </summary>
        /// <param name="type">The type against which assignable types are matched.</param>
        /// <param name="list">The list to which assignable types are appended.</param>
        /// <param name="predicate">Custom delegate to filter the type list.
        /// Return false to ignore given type.</param>
        public static void GetAssignableTypes(this Type type, List<Type> list, Func<Type, bool> predicate = null)
        {
            foreach (var types in ReflectionUtils.GetCachedTypesPerAssembly())
            {
                foreach (var t in types)
                {
                    if (type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && (predicate == null || predicate(t)))
                        list.Add(t);
                }
            }
        }

        /// <summary>
        /// Finds all types that implement the given interface type and appends them to a list.
        /// If the input type is not an interface type, no action is taken.
        /// </summary>
        /// <param name="type">The interface type whose implementors are to be found.</param>
        /// <param name="list">The list to which implementors are to be appended.</param>
        public static void GetImplementationsOfInterface(this Type type, List<Type> list)
        {
            if (type.IsInterface)
                GetAssignableTypes(type, list);
        }

        /// <summary>
        /// Finds all types that extend the given class type and appends them to a list
        /// If the input type is not a class type, no action is taken.
        /// </summary>
        /// <param name="type">The class type of whom list will be found.</param>
        /// <param name="list">The list to which extension types will be appended.</param>
        public static void GetExtensionsOfClass(this Type type, List<Type> list)
        {
            if (type.IsClass)
                GetAssignableTypes(type, list);
        }

        /// <summary>
        /// Searches through all interfaces implemented by this type and, if any of them match the given generic interface,
        /// appends them to a list.
        /// </summary>
        /// <param name="type">The type whose interfaces will be searched.</param>
        /// <param name="genericInterface">The generic interface used to match implemented interfaces.</param>
        /// <param name="interfaces">The list to which generic interfaces will be appended.</param>
        public static void GetGenericInterfaces(this Type type, Type genericInterface, List<Type> interfaces)
        {
            foreach (var typeInterface in type.GetInterfaces())
            {
                if (typeInterface.IsGenericType
                    && typeInterface.GetGenericTypeDefinition() == genericInterface)
                {
                    interfaces.Add(typeInterface);
                }
            }
        }

        /// <summary>
        /// Gets a specific property of the Type or any of its base Types.
        /// </summary>
        /// <param name="type">The type which will be searched for fields.</param>
        /// <param name="name">Name of the property to get.</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted.</param>
        /// <returns>An object representing the field that matches the specified requirements, if found;
        /// otherwise, `null`.</returns>
        public static PropertyInfo GetPropertyRecursively(this Type type, string name,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.DeclaredOnly)
        {
            PropertyInfo property = default;

            while (property == null && type != null)
            {
                property = type.GetProperty(name, bindingAttr);
                type = type.BaseType;
            }

            return property;
        }

        /// <summary>
        /// Gets a specific field of the Type or any of its base Types.
        /// </summary>
        /// <param name="type">The type which will be searched for fields.</param>
        /// <param name="name">Name of the field to get.</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted.</param>
        /// <returns>An object representing the field that matches the specified requirements, if found;
        /// otherwise, `null`.</returns>
        public static FieldInfo GetFieldRecursively(this Type type, string name,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.DeclaredOnly)
        {
            FieldInfo field = default;

            while (field == null && type != null)
            {
                field = type.GetField(name, bindingAttr);
                type = type.BaseType;
            }

            return field;
        }

        /// <summary>
        /// Gets all fields of the Type or any of its base Types.
        /// </summary>
        /// <param name="type">Type we are going to get fields on.</param>
        /// <param name="fields">A list to which all fields of this type will be added.</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted.</param>
        public static void GetFieldsRecursively(this Type type, List<FieldInfo> fields,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.DeclaredOnly)
        {
            while (type != null)
            {
                fields.AddRange(type.GetFields(bindingAttr));
                type = type.BaseType;
            }
        }

        /// <summary>
        /// Gets all properties of the Type or any of its base Types.
        /// </summary>
        /// <param name="type">Type we are going to get properties on.</param>
        /// <param name="fields">A list to which all properties of this type will be added.</param>
        /// <param name="bindingAttr">A bitmask specifying how the search is conducted.</param>
        public static void GetPropertiesRecursively(this Type type, List<PropertyInfo> fields,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.DeclaredOnly)
        {
            while (type != null)
            {
                fields.AddRange(type.GetProperties(bindingAttr));
                type = type.BaseType;
            }
        }

        /// <summary>
        /// Gets the field info on a collection of classes that are from a collection of interfaces.
        /// </summary>
        /// <param name="classes">Collection of classes to get fields from.</param>
        /// <param name="fields">A list to which matching fields will be added.</param>
        /// <param name="interfaceTypes">Collection of interfaceTypes to check if field type
        /// implements any interface type.</param>
        /// <param name="bindingAttr">Binding flags of fields.</param>
        /// <exception cref="ArgumentException"><paramref name="interfaceTypes"/> contains a <see cref="Type"/> that is not an interface.</exception>
        /// <exception cref="ArgumentException"><paramref name="classes"/> contains a <see cref="Type"/> that is not a class.</exception>
        public static void GetInterfaceFieldsFromClasses(this IEnumerable<Type> classes, List<FieldInfo> fields,
            List<Type> interfaceTypes, BindingFlags bindingAttr)
        {
            foreach (var type in interfaceTypes)
            {
                if (!type.IsInterface)
                    throw new ArgumentException($"Type {type} in interfaceTypes is not an interface!");
            }

            using var _0 = ListPool<FieldInfo>.Get(out var tempFields);
            foreach (var type in classes)
            {
                if (!type.IsClass)
                    throw new ArgumentException($"Type {type} in classes is not a class!");

                tempFields.Clear();
                type.GetFieldsRecursively(tempFields, bindingAttr);
                foreach (var field in tempFields)
                {
                    foreach (var @interface in field.FieldType.GetInterfaces())
                    {
                        if (interfaceTypes.Contains(@interface))
                        {
                            fields.Add(field);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first attribute of a given type.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to return.</typeparam>
        /// <param name="type">The type whose attribute will be returned.</param>
        /// <param name="inherit">Whether to search this type's inheritance chain to find the attribute.</param>
        /// <returns>The first <typeparamref name="TAttribute"/> found.</returns>
        public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit = false)
            where TAttribute : Attribute
        {
            Assert.IsTrue(type.IsDefined(typeof(TAttribute), inherit), "Attribute not found");
            return (TAttribute)type.GetCustomAttributes(typeof(TAttribute), inherit)[0];
        }

        /// <summary>
        /// Returns an array of types from the current type back to the declaring type that includes an inherited attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute we are checking if is defined.</typeparam>
        /// <param name="type">Type that has the attribute or inherits the attribute.</param>
        /// <param name="types">A list to which matching types will be added.</param>
        public static void IsDefinedGetInheritedTypes<TAttribute>(this Type type, List<Type> types)
            where TAttribute : Attribute
        {
            while (type != null)
            {
                if (type.IsDefined(typeof(TAttribute), inherit: true))
                {
                    types.Add(type);
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Search by name through a fields of a type and its base types and return the field if one is found.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <param name="fieldName">The name of the field to search for.</param>
        /// <returns>The field, if found.</returns>
        public static FieldInfo GetFieldInTypeOrBaseType(this Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.NonPublic
                                                   | BindingFlags.Public
                                                   | BindingFlags.FlattenHierarchy
                                                   | BindingFlags.Instance);

                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Returns a human-readable name for a class with its generic arguments filled in.
        /// </summary>
        /// <param name="type">The type to get a name for.</param>
        /// <returns>The human-readable name.</returns>
        public static string GetNameWithGenericArguments(this Type type)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            return type.GetNameWithGenericArguments(sb).ToString();
        }

        /// <inheritdoc cref="GetNameWithGenericArguments(Type)"/>
        public static StringBuilder GetNameWithGenericArguments(this Type type, StringBuilder sb)
        {
            var name = type.Name;

            // Replace + with . for subclasses
            if (!type.IsGenericType)
                return sb.Append(name).Replace('+', '.');

            // Trim off `1
            int index = name.IndexOf('`');
            _ = sb.Append(index == -1 ? name : name.AsSpan(0, index))
                .Replace('+', '.')
                .Append('<');

            var arguments = type.GetGenericArguments();
            _ = arguments[0].GetNameWithGenericArguments(sb);

            for (var i = 1; i < arguments.Length; i++)
            {
                _ = arguments[i].GetNameWithGenericArguments(sb.Append(',').Append(' '));
            }

            return sb.Append('>');
        }

        /// <summary>
        /// Returns a human-readable name for a class with its assembly qualified generic arguments filled in.
        /// </summary>
        /// <param name="type">The type to get a name for.</param>
        /// <returns>The human-readable name.</returns>
        public static string GetNameWithFullGenericArguments(this Type type)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            return type.GetNameWithFullGenericArguments(sb).ToString();
        }

        /// <inheritdoc cref="GetNameWithFullGenericArguments(Type)"/>
        public static StringBuilder GetNameWithFullGenericArguments(this Type type, StringBuilder sb)
        {
            var name = type.Name;

            // Replace + with . for subclasses
            if (!type.IsGenericType)
                return sb.Append(name).Replace('+', '.');

            // Trim off `1
            int index = name.IndexOf('`');
            _ = sb.Append(index == -1 ? name : name.AsSpan(0, index))
                .Replace('+', '.')
                .Append('<');

            var arguments = type.GetGenericArguments();
            _ = arguments[0].GetNameWithFullGenericArguments(sb);

            for (var i = 1; i < arguments.Length; i++)
            {
                _ = arguments[i].GetNameWithFullGenericArguments(sb.Append(',').Append(' '));
            }

            return sb.Append('>');
        }

        /// <summary>
        /// Returns a human-readable, assembly qualified name for a class with its assembly qualified generic arguments
        /// filled in.
        /// </summary>
        /// <param name="type">The type to get a name for.</param>
        /// <returns>The human-readable name.</returns>
        public static string GetFullNameWithGenericArguments(this Type type)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            return type.GetFullNameWithGenericArguments(sb).ToString();
        }

        /// <inheritdoc cref="GetFullNameWithGenericArguments(Type)"/>
        public static StringBuilder GetFullNameWithGenericArguments(this Type type, StringBuilder sb)
        {
            if (type.IsGenericParameter)
                return type.GetFullNameWithGenericArgumentsInternal(sb);

            // Handle sub-classes
            var declaringType = type.DeclaringType;
            if (declaringType != null)
            {
                var typeNames = ListPool<StringBuilder>.Get();
                typeNames.Add(type.GetNameWithFullGenericArguments(StringBuilderPool.Get()));

                while (true)
                {
                    var parentDeclaringType = declaringType.DeclaringType;
                    if (parentDeclaringType == null)
                    {
                        typeNames.Add(declaringType.GetFullNameWithGenericArguments(StringBuilderPool.Get()));
                        break;
                    }

                    typeNames.Add(declaringType.GetNameWithFullGenericArguments(StringBuilderPool.Get()));
                    declaringType = parentDeclaringType;
                }

                _ = sb.Append(typeNames[^1]);
                StringBuilderPool.Release(typeNames[^1]);

                for (int i = typeNames.Count - 2; i >= 0; i--)
                {
                    _ = sb.Append('.').Append(typeNames[i]);
                    StringBuilderPool.Release(typeNames[i]);
                }

                ListPool<StringBuilder>.Release(typeNames);

                return sb;
            }

            return type.GetFullNameWithGenericArgumentsInternal(sb);
        }

        static string GetFullNameWithGenericArgumentsInternal(this Type type)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            return type.GetFullNameWithGenericArgumentsInternal(sb).ToString();
        }

        static StringBuilder GetFullNameWithGenericArgumentsInternal(this Type type, StringBuilder sb)
        {
            var name = type.FullName;

            if (string.IsNullOrEmpty(name))
                return sb;

            if (!type.IsGenericType)
                return sb.Append(name);


            // Trim off `1
            int index = name.IndexOf('`');

            _ = sb.Append(index == -1 ? name : name.AsSpan(0, index))
                .Append('<');

            var arguments = type.GetGenericArguments();
            _ = arguments[0].GetFullNameWithGenericArgumentsInternal(sb);

            for (var i = 1; i < arguments.Length; i++)
            {
                _ = arguments[i].GetFullNameWithGenericArgumentsInternal(sb.Append(',').Append(' '));
            }

            return sb.Append('>');
        }

        /// <summary>
        /// Tests if class type IsAssignableFrom or IsSubclassOf another type.
        /// </summary>
        /// <param name="checkType">type wanting to check.</param>
        /// <param name="baseType">type wanting to check against.</param>
        /// <returns>True if IsAssignableFrom or IsSubclassOf.</returns>
        public static bool IsAssignableFromOrSubclassOf(this Type checkType, Type baseType)
        {
            return checkType.IsAssignableFrom(baseType) || checkType.IsSubclassOf(baseType);
        }

        /// <summary>
        /// Searches this type and all base types for a method by name.
        /// </summary>
        /// <param name="type">The type being searched.</param>
        /// <param name="name">The name of the method for which to search.</param>
        /// <param name="bindingAttr">BindingFlags passed to Type.GetMethod.</param>
        /// <returns>MethodInfo for the first matching method found. Null if no method is found.</returns>
        public static MethodInfo GetMethodRecursively(this Type type, string name, BindingFlags bindingAttr)
        {
            MethodInfo method = null;

            while (method == null && type != null)
            {
                method = type.GetMethod(name, bindingAttr);
                type = type.BaseType;
            }

            return method;
        }
        #endregion // Unity.XR.CoreUtils
    }
}
