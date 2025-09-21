using System;
using System.Collections.Generic;
using System.Reflection;

namespace PKGE
{
    public static class AssemblyExtensions
    {
        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Runtime/Extensions/AssemblyExtensions.cs
        #region UnityEngine.GraphToolsFoundation.Overdrive
        public static IEnumerable<Type> GetTypesSafe(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.LogWarning("Can't load assembly '" + assembly.GetName() + "'. Problematic types follow.");
                foreach (TypeLoadException tle in (TypeLoadException[])e.LoaderExceptions)
                {
                    Debug.LogWarning("Can't load type '" + tle.TypeName + "': " + tle.Message);
                }

                return Type.EmptyTypes;
            }
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive
    }
}
