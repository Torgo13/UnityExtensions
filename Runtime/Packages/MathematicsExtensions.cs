using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace UnityExtensions.Runtime.Packages
{
    public static class MathematicsExtensions
    {

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/GPUDriven/GPUDrivenRenderingUtils.cs
        #region UnityEngine.Rendering.Tests
        public static uint4 UnpackUintTo4x8Bit(uint val)
        {
            return new uint4(val & 0xFF, (val >> 8) & 0xFF, (val >> 16) & 0xFF, (val >> 24) & 0xFF);
        }
        #endregion // UnityEngine.Rendering.Tests
    }
}
