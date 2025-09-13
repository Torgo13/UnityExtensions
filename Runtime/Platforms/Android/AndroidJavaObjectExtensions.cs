using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PKGE.Platforms.Android
{
    static class AndroidJavaObjectExtensions
    {
        //https://github.com/needle-mirror/com.unity.purchasing/blob/5.0.0-pre.3/Runtime/Purchasing/Stores/Android/AAR/AndroidJavaObjectExtensions.cs
        #region UnityEngine.Purchasing.Models
        internal static IEnumerable<T> Enumerate<T>(this AndroidJavaObject androidJavaList)
        {
            var size = androidJavaList?.Call<int>("size") ?? 0;
            return Enumerable.Range(0, size).Select(i => androidJavaList.Call<T>("get", i)).ToList();
        }

        internal static IEnumerable<AndroidJavaObject> Enumerate(this AndroidJavaObject androidJavaList)
        {
            return androidJavaList.Enumerate<AndroidJavaObject>();
        }
        #endregion // UnityEngine.Purchasing.Models
    }
}