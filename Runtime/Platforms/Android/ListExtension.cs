using System.Collections.Generic;
using UnityEngine;

namespace PKGE.Platforms.Android
{
    static class ListExtension
    {
        //https://github.com/needle-mirror/com.unity.purchasing/blob/5.0.0-pre.3/Runtime/Purchasing/Stores/Android/AAR/ListExtension.cs
        #region UnityEngine.Purchasing
        internal static AndroidJavaObject ToJava<T>(this List<T> values)
        {
            return ToJavaArray(values);
        }

        static AndroidJavaObject ToJavaArray<T>(List<T> values)
        {
            var list = new AndroidJavaObject("java.util.ArrayList");
            foreach (var value in values)
            {
                list.Call<bool>("add", value);
            }
            return list;
        }
        #endregion // UnityEngine.Purchasing
    }
}
