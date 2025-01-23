#if INCLUDE_UGUI
using UnityEngine;

using UnityEngine.UI;

namespace Unity.XR.CoreUtils.Tests
{
    /// <summary>
    /// This class exists to allow testing of the overload for MaterialUtils.GetMaterialClone that takes a Graphic-derived class
    /// </summary>
    [AddComponentMenu("")]
    class TestImage : Graphic
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Runtime/Scripts/TestImage.cs
        #region Unity.XR.CoreUtils.Tests
        protected override void OnPopulateMesh(VertexHelper vh) {}
        #endregion // Unity.XR.CoreUtils.Tests
    }
}
#endif
