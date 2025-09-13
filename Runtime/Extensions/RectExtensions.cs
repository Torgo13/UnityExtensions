using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PKGE
{
    public static class RectExtensions
    {
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        /// <summary>Inflate a rect</summary>
        /// <param name="r">The rect to inflate.</param>
        /// <param name="delta">x and y are added/subtracted to/from the edges of
        /// the rect, inflating it in all directions</param>
        /// <returns>The inflated rect</returns>
        public static Rect Inflated(this Rect r, Vector2 delta)
        {
            if (r.width + delta.x * 2 < 0)
                delta.x = -r.width / 2;

            if (r.height + delta.y * 2 < 0)
                delta.y = -r.height / 2;

            return new Rect(
                r.xMin - delta.x, r.yMin - delta.y,
                r.width + delta.x * 2, r.height + delta.y * 2);
        }
        #endregion // Unity.Cinemachine
        
        //https://github.com/Unity-Technologies/com.unity.demoteam.hair/blob/75a7f446209896bc1bce0da2682cfdbdf30ce447/Runtime/Utility/Extensions.cs
        #region Unity.DemoTeam.Hair
        public static Rect ClipLeft(this Rect position, float width)
        {
            return new Rect(position.x + width, position.y, position.width - width, position.height);
        }

        public static Rect ClipLeft(this Rect position, float width, out Rect clipped)
        {
            clipped = new Rect(position.x, position.y, width, position.height);
            return position.ClipLeft(width);
        }
        #endregion // Unity.DemoTeam.Hair
    }
}
