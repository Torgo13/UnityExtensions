#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Helpers for drawing shapes for debugging purposes.
    /// </summary>
    public static class DebugDraw
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/DebugDraw.cs
        #region Unity.XR.CoreUtils
        /// <inheritdoc cref="Polygon(System.ReadOnlySpan{Vector3}, Color, float)"/>
        [Conditional("DEBUG")]
        public static void Polygon(List<Vector3> vertices, Color color, float duration = 10f)
        {
            Polygon(vertices.AsSpan(), color, duration);
        }

        /// <summary>
        /// Draws a line following a set of points.
        /// </summary>
        /// <remarks>Connects the points in <paramref name="vertices"/> in order and closes the polygon
        /// by connecting the last point to the first.</remarks>
        /// <param name="vertices">Polygon made of a series of adjacent points in world space.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="duration">How long the line should be visible for.</param>
        [Conditional("DEBUG")]
        public static void Polygon(System.ReadOnlySpan<Vector3> vertices, Color color, float duration = 10f)
        {
            var vertexCount = vertices.Length;
            if (vertexCount < 2)
                return;

            var lengthMinusOne = vertexCount - 1;
            for (var i = 0; i < lengthMinusOne; i++)
            {
                var a = vertices[i];
                var b = vertices[i + 1];
                UnityEngine.Debug.DrawLine(a, b, color, duration);
            }

            var last = vertices[lengthMinusOne];
            var first = vertices[0];
            UnityEngine.Debug.DrawLine(last, first, color, duration);
        }
        #endregion // Unity.XR.CoreUtils
    }
}
