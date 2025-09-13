using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PKGE
{
    public static class UnityObjectExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/UnityObjectUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Returns a component of the specified type that is associated with an object, if possible.
        /// </summary>
        /// <remarks><list>
        /// <item>If the <paramref name="objectIn"/> is the requested type, then this function casts it to
        /// type T and returns it.</item>
        /// <item>If <paramref name="objectIn"/> is a <see cref="GameObject"/>, then this function returns
        /// the first component of the requested type, if one exists.</item>
        /// <item>If <paramref name="objectIn"/> is a different type of component, this function returns
        /// the first component of the requested type on the same GameObject, if one exists.</item>
        /// </list></remarks>
        /// <param name="objectIn">The Unity Object reference to convert.</param>
        /// <typeparam name="T"> The type to convert to.</typeparam>
        /// <returns>A component of type `T`, if found on the object. Otherwise returns `null`.</returns>
        public static T ConvertUnityObjectToType<T>(this UnityObject objectIn) where T : class
        {
            var interfaceOut = objectIn as T;
            if (interfaceOut == null && objectIn != null)
            {
                var go = objectIn as GameObject;
                if (go != null)
                {
                    interfaceOut = go.GetComponent<T>();
                    return interfaceOut;
                }

                var comp = objectIn as Component;
                if (comp != null)
                    interfaceOut = comp.GetComponent<T>();
            }

            return interfaceOut;
        }
        #endregion // Unity.XR.CoreUtils
    }
}
