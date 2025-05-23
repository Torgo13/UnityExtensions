using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for <see cref="Camera"/> components.
    /// </summary>
    public static class CameraExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/CameraExtensions.cs
        #region Unity.XR.CoreUtils
        const float OneOverSqrt2 = 0.707106781f;

        /// <summary>
        /// Calculates the vertical field of view from an aspect neutral (diagonal) field of view and the camera's aspect ratio.
        /// </summary>
        /// <remarks>
        /// The field of view property of a Unity <see cref="Camera"/> stores the vertical field of view.
        /// </remarks>
        /// <param name="camera">The camera to get the aspect ratio from.</param>
        /// <param name="aspectNeutralFieldOfView"> The "aspect neutral" field of view, which is the diagonal field of view.</param>
        /// <returns>The vertical field of view calculated.</returns>
        public static float GetVerticalFieldOfView(this Camera camera, float aspectNeutralFieldOfView)
        {
            var verticalHalfFieldOfViewTangent = Mathf.Tan(aspectNeutralFieldOfView * 0.5f * Mathf.Deg2Rad) *
                OneOverSqrt2 / Mathf.Sqrt(camera.aspect);
            return Mathf.Atan(verticalHalfFieldOfViewTangent) * 2 * Mathf.Rad2Deg;
        }
        
        /// <inheritdoc cref="GetVerticalFieldOfView"/>
        public static double GetVerticalFieldOfViewRad(this Camera camera, double aspectNeutralFieldOfView)
        {
            var verticalHalfFieldOfViewTangent = System.Math.Tan(aspectNeutralFieldOfView * 0.5) *
                OneOverSqrt2 / System.Math.Sqrt(camera.aspect);
            return System.Math.Atan(verticalHalfFieldOfViewTangent) * 2.0;
        }

        /// <summary>
        /// Calculates the horizontal field of view of the <see cref="Camera"/>.
        /// </summary>
        /// <param name="camera">The camera to get the aspect ratio and vertical field of view from.</param>
        /// <returns>The horizontal field of view of the camera.</returns>
        public static float GetHorizontalFieldOfView(this Camera camera)
        {
            var halfFieldOfView = camera.fieldOfView * 0.5f;
            return Mathf.Rad2Deg * Mathf.Atan(Mathf.Tan(halfFieldOfView * Mathf.Deg2Rad) * camera.aspect);
        }
        
        /// <inheritdoc cref="GetHorizontalFieldOfView"/>
        public static double GetHorizontalFieldOfViewRad(this Camera camera)
        {
            var halfFieldOfView = camera.fieldOfView * 0.5 * Mathf.Deg2Rad;
            return System.Math.Atan(System.Math.Tan(halfFieldOfView) * camera.aspect);
        }

        /// <summary>
        /// Calculates the vertical orthographic size for a <see cref="Camera"/> and a given diagonal size.
        /// </summary>
        /// <param name="camera">The camera to get the aspect ratio from.</param>
        /// <param name="size">The diagonal orthographic size.</param>
        /// <returns>The vertical orthographic size calculated.</returns>
        public static float GetVerticalOrthographicSize(this Camera camera, float size)
        {
            return size * OneOverSqrt2 / Mathf.Sqrt(camera.aspect);
        }
        #endregion // Unity.XR.CoreUtils
        
        public static double HorizontalToVerticalFOVRad(double horizontalFOV, double aspect)
        {
            return 2.0 * System.Math.Atan(System.Math.Tan(horizontalFOV * 0.5) / aspect);
        }

        public static bool GetAllCameras(List<Camera> cameras)
        {
            int allCamerasCount = Camera.allCamerasCount;
            if (allCamerasCount == 0)
                return false;

            Camera[] allCameras = ArrayPool<Camera>.Shared.Rent(allCamerasCount);
            _ = Camera.GetAllCameras(allCameras);

            cameras.EnsureCapacity(allCamerasCount);
            for (int i = 0; i < allCamerasCount; i++)
            {
                cameras.Add(allCameras[i]);
            }

            ArrayPool<Camera>.Shared.Return(allCameras);

            return true;
        }
    }
}
