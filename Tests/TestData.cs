using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    static class TestData
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Runtime/Scripts/TestData.cs
        #region Unity.XR.CoreUtils.Tests
        public static Vector2[] RandomVector2Array(int length, float range = 0.0001f)
        {
            var array = new Vector2[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector2(Random.Range(-range, range), Random.Range(-range, range));
            }

            return array;
        }

        public static Vector3[] RandomXZVector3Array(int length, float range = 0.0001f)
        {
            var array = new Vector3[length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
            }

            return array;
        }
        #endregion // Unity.XR.CoreUtils.Tests
    }
}
