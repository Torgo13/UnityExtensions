using UnityEngine;

#if INCLUDE_MATHEMATICS
using static Unity.Mathematics.math;
#else
using static PKGE.Mathematics.math;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Unsafe
{
    public static class SpriteUtilities
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Utilities/SpriteUtilities.cs
        #region UnityEngine.InputSystem.Utilities
        /// <remarks><code>
        /// image.sprite = SpriteUtilities.CreateCircleSprite(16, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
        /// </code></remarks>
        public static Sprite CreateCircleSprite(int radius, Color32 colour,
            bool mipChain = false,
            bool makeNoLongerReadable = false,
            float pixelsPerUnit = 1,
            uint extrude = 0,
            SpriteMeshType spriteMeshType = SpriteMeshType.FullRect)
        {
            // cache the diameter
            var d = radius * 2;
            UnityEngine.Assertions.Assert.IsTrue(d > 0);

            var texture = new Texture2D(d, d, TextureFormat.RGBA32,
                mipChain, linear: false, createUninitialized: true);

            var colours = texture.GetRawTextureData<Color32>();

            Unity.Jobs.IJobParallelForExtensions.Run(new Packages.ClearArrayJob<Color32>
            {
                Data = colours,
            }, colours.Length);

            Unity.Jobs.IJobForExtensions.Run(new CreateCircleSpriteJob
            {
                colours = colours,
                rSquared = radius * radius,
                radius = radius,
                d = d,
                colour = colour,
            }, 2 * radius);

            texture.Apply(updateMipmaps: mipChain, makeNoLongerReadable);

            var sprite = Sprite.Create(texture, new Rect(0, 0, d, d), new Vector2(radius, radius),
                pixelsPerUnit, extrude, spriteMeshType);

            return sprite;
        }
        #endregion // UnityEngine.InputSystem.Utilities

        /// <summary>
        /// loop over the texture memory one column at a time filling in a line between the two x coordinates
        /// of the circle at each column
        /// </summary>
        [Unity.Burst.BurstCompile]
        struct CreateCircleSpriteJob : Unity.Jobs.IJobFor
        {
            [Unity.Collections.WriteOnly]
            [Unity.Collections.LowLevel.Unsafe.NativeDisableContainerSafetyRestriction]
            public Unity.Collections.NativeArray<Color32> colours;
            public float rSquared;
            public int radius;
            public int d;
            public Color32 colour;

            public void Execute(int index)
            {
                var y = index - radius;

                // for the current column, calculate what the x coordinate of the circle would be
                // using x^2 + y^2 = r^2, or x^2 = r^2 - y^2. The square root of the value of the
                // x coordinate will equal half the width of the circle at the current y coordinate
                var halfWidth = (int)sqrt(rSquared - y * y);

                // position the pointer so it points at the memory where we should start filling in
                // the current line
                var pos = (y + radius) * d  // the position of the memory at the start of the row at the current y coordinate
                    + radius - halfWidth;   // the position along the row where we should start inserting colours

                for (var x = 0; x < 2 * halfWidth; x++)
                {
                    colours[pos + x] = colour;
                }
            }
        }
    }
}
