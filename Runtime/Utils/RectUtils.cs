using System;
using UnityEngine;

namespace UnityExtensions
{
    public static class RectUtils
    {
        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Editor/GraphElements/Utils/RectUtils.cs
        #region UnityEditor.GraphToolsFoundation.Overdrive
        public static bool IntersectsSegment(Rect rect, Vector2 p1, Vector2 p2)
        {
            float minX = Math.Min(p1.x, p2.x);
            float maxX = Math.Max(p1.x, p2.x);

            if (maxX > rect.xMax)
            {
                maxX = rect.xMax;
            }

            if (minX < rect.xMin)
            {
                minX = rect.xMin;
            }

            if (minX > maxX)
            {
                return false;
            }

            float minY = Math.Min(p1.y, p2.y);
            float maxY = Math.Max(p1.y, p2.y);

            float dx = p2.x - p1.x;

            if (Math.Abs(dx) > float.Epsilon)
            {
                float a = (p2.y - p1.y) / dx;
                float b = p1.y - a * p1.x;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                (minY, maxY) = (maxY, minY);
            }

            if (maxY > rect.yMax)
            {
                maxY = rect.yMax;
            }

            if (minY < rect.yMin)
            {
                minY = rect.yMin;
            }

            if (minY > maxY)
            {
                return false;
            }

            return true;
        }

        public static Rect Encompass(Rect a, Rect b)
        {
            return new Rect
            {
                xMin = Math.Min(a.xMin, b.xMin),
                yMin = Math.Min(a.yMin, b.yMin),
                xMax = Math.Max(a.xMax, b.xMax),
                yMax = Math.Max(a.yMax, b.yMax)
            };
        }

        public static Rect Inflate(Rect a, float left, float top, float right, float bottom)
        {
            return new Rect
            {
                xMin = a.xMin - left,
                yMin = a.yMin - top,
                xMax = a.xMax + right,
                yMax = a.yMax + bottom
            };
        }
        #endregion // UnityEditor.GraphToolsFoundation.Overdrive
    }
}
