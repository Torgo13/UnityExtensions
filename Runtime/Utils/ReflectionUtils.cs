using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace UnityExtensions
{
    /// <summary>
    /// Utility methods for common reflection-based operations.
    /// </summary>
    public static class ReflectionUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/ReflectionUtils.cs
        #region Unity.XR.CoreUtils
        static Assembly[] _assemblies;
        static List<Type[]> _typesPerAssembly;
        static List<Dictionary<string, Type>> _assemblyTypeMaps;

        public static Assembly[] GetCachedAssemblies() { return _assemblies ??= AppDomain.CurrentDomain.GetAssemblies(); }

        public static List<Type[]> GetCachedTypesPerAssembly()
        {
            if (_typesPerAssembly == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                _typesPerAssembly = new List<Type[]>(assemblies.Length);
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        _typesPerAssembly.Add(assembly.GetTypes());
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Skip any assemblies that don't load properly -- suppress errors
                    }
                }
            }

            return _typesPerAssembly;
        }

        public static List<Dictionary<string, Type>> GetCachedAssemblyTypeMaps()
        {
            if (_assemblyTypeMaps == null)
            {
                var typesPerAssembly = GetCachedTypesPerAssembly();
                _assemblyTypeMaps = new List<Dictionary<string, Type>>(typesPerAssembly.Count);
                foreach (var types in typesPerAssembly)
                {
                    try
                    {
                        var typeMap = new Dictionary<string, Type>(types.Length);
                        foreach (var type in types)
                        {
                            typeMap[type.FullName!] = type;
                        }

                        _assemblyTypeMaps.Add(typeMap);
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Skip any assemblies that don't load properly -- suppress errors
                    }
                }
            }

            return _assemblyTypeMaps;
        }

        /// <summary>
        /// Caches type information from all currently loaded assemblies.
        /// </summary>
        public static void PreWarmTypeCache() { GetCachedAssemblyTypeMaps(); }

        /// <summary>
        /// Executes a delegate function for every assembly that can be loaded.
        /// </summary>
        /// <remarks>
        /// `ForEachAssembly` iterates through all assemblies and executes a method on each one.
        /// If an <see cref="ReflectionTypeLoadException"/> is thrown, it is caught and ignored.
        /// </remarks>
        /// <param name="callback">The callback method to execute for each assembly.</param>
        public static void ForEachAssembly(Action<Assembly> callback)
        {
            var assemblies = GetCachedAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    callback(assembly);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip any assemblies that don't load properly -- suppress errors
                }
            }
        }

        /// <summary>
        /// Executes a delegate function for each type in every assembly.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        public static void ForEachType(Action<Type> callback)
        {
            var typesPerAssembly = GetCachedTypesPerAssembly();
            foreach (var types in typesPerAssembly)
            {
                foreach (var type in types)
                {
                    callback(type);
                }
            }
        }

        /// <summary>
        /// Search all assemblies for a type that matches a given predicate delegate.
        /// </summary>
        /// <param name="predicate">The predicate function.
        /// Must return <see langword="true"/> for the type that matches the search.</param>
        /// <returns>The first type for which <paramref name="predicate"/> returns <see langword="true"/>,
        /// or `null` if no matching type exists.</returns>
        public static Type FindType(Func<Type, bool> predicate)
        {
            var typesPerAssembly = GetCachedTypesPerAssembly();
            foreach (var types in typesPerAssembly)
            {
                foreach (var type in types)
                {
                    if (predicate(type))
                        return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Find a type in any assembly by its full name.
        /// </summary>
        /// <param name="fullName">The name of the type as returned by <see cref="Type.FullName"/>.</param>
        /// <returns>The type found, or null if no matching type exists.</returns>
        public static Type FindTypeByFullName(string fullName)
        {
            var typesPerAssembly = GetCachedAssemblyTypeMaps();
            foreach (var assemblyTypes in typesPerAssembly)
            {
                if (assemblyTypes.TryGetValue(fullName, out var type))
                    return type;
            }

            return null;
        }

        /// <summary>
        /// Search all assemblies for a set of types that matches any one of a set of predicates.
        /// </summary>
        /// <remarks>
        /// This function tests each predicate against each type in each assembly. If the predicate returns
        /// <see langword="true"/> for a type, then that <see cref="Type"/> object is assigned to the corresponding index of
        /// the <paramref name="resultList"/>. If a predicate returns <see langword="true"/> for more than one type, then the
        /// last matching result is used. If no type matches the predicate, then that index of <paramref name="resultList"/>
        /// is left unchanged.
        /// </remarks>
        /// <param name="predicates">The predicate functions. A predicate function must return <see langword="true"/>
        /// for the type that matches the search and should only match one type.</param>
        /// <param name="resultList">The list to which found types will be added. The list must have
        /// the same number of elements as the <paramref name="predicates"/> list.</param>
        public static void FindTypesBatch(List<Func<Type, bool>> predicates, List<Type> resultList)
        {
            var typesPerAssembly = GetCachedTypesPerAssembly();
            for (var i = 0; i < predicates.Count; i++)
            {
                var predicate = predicates[i];
                foreach (var assemblyTypes in typesPerAssembly)
                {
                    foreach (var type in assemblyTypes)
                    {
                        if (predicate(type))
                            resultList[i] = type;
                    }
                }
            }
        }

        /// <summary>
        /// Searches all assemblies for a set of types by their <see cref="Type.FullName"/> strings.
        /// </summary>
        /// <remarks>
        /// If a type name in <paramref name="typeNames"/> is not found,
        /// then the corresponding index of <paramref name="resultList"/> is set to `null`.
        /// </remarks>
        /// <param name="typeNames">A list containing the <see cref="Type.FullName"/> strings of the types to find.</param>
        /// <param name="resultList">An empty list to which any matching <see cref="Type"/> objects are added. A
        /// result in <paramref name="resultList"/> has the same index as corresponding name
        /// in <paramref name="typeNames"/>.</param>
        public static void FindTypesByFullNameBatch(List<string> typeNames, List<Type> resultList)
        {
            var assemblyTypeMap = GetCachedAssemblyTypeMaps();
            foreach (var typeName in typeNames)
            {
                var found = false;
                foreach (var typeMap in assemblyTypeMap)
                {
                    if (typeMap.TryGetValue(typeName, out var type))
                    {
                        resultList.Add(type);
                        found = true;
                        break;
                    }
                }

                // If a type can't be found, add a null entry to the list to ensure indexes match
                if (!found)
                    resultList.Add(null);
            }
        }

        /// <summary>
        /// Searches for a type by assembly simple name and its <see cref="Type.FullName"/>.
        /// an assembly with the given simple name and returns the type with the given full name in that assembly
        /// </summary>
        /// <param name="assemblyName">Simple name of the assembly (<see cref="Assembly.GetName()"/>).</param>
        /// <param name="typeName">Full name of the type to find (<see cref="Type.FullName"/>).</param>
        /// <returns>The type if found, otherwise null</returns>
        public static Type FindTypeInAssemblyByFullName(string assemblyName, string typeName)
        {
            var assemblies = GetCachedAssemblies();
            var assemblyTypeMaps = GetCachedAssemblyTypeMaps();
            for (var i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name != assemblyName)
                    continue;

                return assemblyTypeMaps[i].GetValueOrDefault(typeName);
            }

            return null;
        }

        /// <summary>
        /// Cleans up a variable name for display in UI.
        /// </summary>
        /// <param name="name">The variable name to clean up.</param>
        /// <returns>The display name for the variable.</returns>
        public static string NicifyVariableName(string name)
        {
            if (name.StartsWith("m_"))
                name = name.Substring(2, name.Length - 2);
            else if (name.StartsWith('_'))
                name = name.Substring(1, name.Length - 1);

            if (name[0] == 'k' && name[1] >= 'A' && name[1] <= 'Z')
                name = name.Substring(1, name.Length - 1);

            // Insert a space before any capital letter unless it is the beginning or end of a word
            name = Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1",
                RegexOptions.None, TimeSpan.FromSeconds(0.1));
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            return name;
        }
        #endregion // Unity.XR.CoreUtils

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/ReflectionUtils.cs
        #region UnityEngine.Rendering.Tests
        /// <summary>
        /// Calls a private method from a class
        /// </summary>
        /// <param name="targetType">The Type on which to invoke the static method.</param>
        /// <param name="methodName">The method name</param>
        /// <param name="args">The arguments to pass to the method</param>
        /// <returns>The return value from the static method invoked, or null for methods returning void.</returns>
        public static object InvokeStatic(this Type targetType, string methodName, params object[] args)
        {
            Assert.IsTrue(targetType != null, "Invalid Type");
            Assert.IsFalse(string.IsNullOrEmpty(methodName), "The methodName to set cannot be null");

            var mi = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsTrue(mi != null, $"Could not find method `{methodName}` on type `{targetType}`");
            return mi.Invoke(null, args);
        }

        /// <summary>
        /// Calls a private method from a class
        /// </summary>
        /// <param name="target">The object instance on which to invoke the method.</param>
        /// <param name="methodName">The method name</param>
        /// <param name="args">The arguments to pass to the method</param>
        /// <returns>The return value from the invoked method, or null if the method does not return a value.</returns>
        public static object Invoke(this object target, string methodName, params object[] args)
        {
            Assert.IsTrue(target != null, "The target cannot be null");
            Assert.IsFalse(string.IsNullOrEmpty(methodName), "The method name to set cannot be null");

            var mi = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(mi != null, $"Could not find method `{methodName}` on object `{target}`");
            return mi.Invoke(target, args);
        }

        private static FieldInfo FindField(this Type type, string fieldName)
        {
            FieldInfo fi = null;

            while (type != null)
            {
                fi = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (fi != null)
                    break;

                type = type.BaseType;
            }

            Assert.IsTrue(fi != null, $"Could not find method `{fieldName}` on object `{type}`");

            return fi;
        }

        /// <summary>
        /// Sets a private field from a class
        /// </summary>
        /// <param name="target">The object instance that contains the field to be set.</param>
        /// <param name="fieldName">The field to change</param>
        /// <param name="value">The new value</param>
        public static void SetField(this object target, string fieldName, object value)
        {
            Assert.IsTrue(target != null, "The target cannot be null");
            Assert.IsFalse(string.IsNullOrEmpty(fieldName), "The field to set cannot be null");
            
            target.GetType().FindField(fieldName).SetValue(target, value);
        }

        /// <summary>
        /// Gets the value of a private field from a class
        /// </summary>
        /// <param name="target">The object instance that contains the field to be retrieved.</param>
        /// <param name="fieldName">The name of the private field to get the value from.</param>
        /// <returns>The value of the specified field from the target object.</returns>
        public static object GetField(this object target, string fieldName)
        {
            Assert.IsTrue(target != null, "The target cannot be null");
            Assert.IsFalse(string.IsNullOrEmpty(fieldName), "The field to set cannot be null");
            
            return target.GetType().FindField(fieldName).GetValue(target);
        }

        /// <summary>
        /// Gets all the fields from a class
        /// </summary>
        /// <param name="target">The object instance from which to get the fields.</param>
        /// <returns>An ordered enumeration of FieldInfo objects representing each field defined within
        /// the typeof the target object.</returns>
        public static IEnumerable<FieldInfo> GetFields(this object target)
        {
            Assert.IsTrue(target != null, "The target cannot be null");

            return target.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .OrderBy(t => t.MetadataToken);
        }
        #endregion // UnityEngine.Rendering.Tests

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Utility/ReflectionUtils.cs#L8
        #region UnityEngine.Rendering.HighDefinition
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null.</exception>
        public static void ForEachFieldOfType<T>(this object instance, Action<T> callback,
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var fields = instance.GetType().GetFields(flags);
            if (fields.Length == 0)
                return;

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.GetValue(instance) is T fieldValue)
                {
                    callback(fieldValue);
                }
            }
        }
        #endregion // UnityEngine.Rendering.HighDefinition
    }
}