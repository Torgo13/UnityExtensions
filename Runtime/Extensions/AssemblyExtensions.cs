using System;
using System.Reflection;

namespace PKGE
{
    public static class AssemblyExtensions
    {
        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Runtime/Extensions/AssemblyExtensions.cs
        #region UnityEngine.GraphToolsFoundation.Overdrive
        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                UnityEngine.Debug.LogException(e);
                UnityEngine.Debug.LogWarning("Can't load assembly '" + assembly.GetName() + "'. Problematic types follow.");
                foreach (TypeLoadException tle in (TypeLoadException[])e.LoaderExceptions)
                {
                    UnityEngine.Debug.LogWarning("Can't load type '" + tle.TypeName + "': " + tle.Message);
                }

                var types = e.Types;
                var temp = UnityEngine.Pool.ListPool<Type>.Get();
                temp.EnsureCapacity(types.Length);

                foreach (var type in types)
                {
                    if (type != null)
                    {
                        temp.Add(type);
                    }
                }

                types = temp.Count > 0 ? temp.ToArray() : Type.EmptyTypes;
                UnityEngine.Pool.ListPool<Type>.Release(temp);
                return types;
            }
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive
    }
}
