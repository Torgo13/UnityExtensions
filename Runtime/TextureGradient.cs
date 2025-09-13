using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace PKGE
{
    /// <summary>
    /// A wrapper around <see cref="Gradient"/> to automatically bake it into a texture.
    /// </summary>
    [Serializable]
    public class TextureGradient : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/TextureGradient.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Texture Size computed.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public int textureSize { get; private set; }

        /// <summary>
        /// Internal Gradient used to generate the Texture
        /// </summary>
        [SerializeField]
        Gradient gradient;

        Texture2D _texture;

        int _requestedTextureSize = -1;

        bool _isTextureDirty;
        bool _precise;

        /// <summary>All color keys defined in the gradient.</summary>
        public GradientColorKey[] colorKeys => gradient?.colorKeys;

        /// <summary>All alpha keys defined in the gradient.</summary>
        public GradientAlphaKey[] alphaKeys => gradient?.alphaKeys;

        /// <summary>Controls how the gradient colors are interpolated.</summary>
        [SerializeField, HideInInspector]
        public GradientMode mode = GradientMode.PerceptualBlend;

        /// <summary>Indicates the color space that the gradient color keys are using.</summary>
        [SerializeField, HideInInspector]
        public ColorSpace colorSpace = ColorSpace.Uninitialized;

        /// <summary>
        /// Creates a new <see cref="TextureGradient"/> from an existing <c>Gradient</c>.
        /// </summary>
        /// <param name="baseCurve">The source <c>Gradient</c>.</param>
        public TextureGradient(Gradient baseCurve)
            : this(baseCurve.colorKeys, baseCurve.alphaKeys)
        {
            mode = baseCurve.mode;
            colorSpace = baseCurve.colorSpace;
            gradient.mode = baseCurve.mode;
            gradient.colorSpace = baseCurve.colorSpace;
        }

        /// <summary>
        /// Creates a new <see cref="TextureCurve"/> from an arbitrary number of keyframes.
        /// </summary>
        /// <param name="colorKeys">An array of keyframes used to define the color of gradient.</param>
        /// <param name="alphaKeys">An array of keyframes used to define the alpha of gradient.</param>
        /// <param name="mode">Indicates the color space that the gradient color keys are using.</param>
        /// <param name="colorSpace">Controls how the gradient colors are interpolated.</param>
        /// <param name="requestedTextureSize">Texture Size used internally, if '-1' using Nyquist-Shannon limits.</param>
        /// <param name="precise">if precise uses 4*Nyquist-Shannon limits, 2* otherwise.</param>
        public TextureGradient(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys,
            GradientMode mode = GradientMode.PerceptualBlend, ColorSpace colorSpace = ColorSpace.Uninitialized,
            int requestedTextureSize = -1, bool precise = false)
        {
            Rebuild(colorKeys, alphaKeys, mode, colorSpace, requestedTextureSize, precise);
        }

        void Rebuild(GradientColorKey[] cKeys, GradientAlphaKey[] aKeys, GradientMode gradientMode,
            ColorSpace cSpace, int requestedTextureSize, bool precise)
        {
            gradient = new Gradient();
            gradient.mode = gradientMode;
            gradient.colorSpace = cSpace;
            gradient.SetKeys(cKeys, aKeys);
            _precise = precise;
            _requestedTextureSize = requestedTextureSize;
            if (requestedTextureSize > 0)
            {
                textureSize = requestedTextureSize;
            }
            else
            {
                float smallestDelta = 1.0f;
                using var _0 = UnityEngine.Pool.ListPool<float>.Get(out var times);
                times.EnsureCapacity(cKeys.Length + aKeys.Length);
                for (int i = 0; i < cKeys.Length; ++i)
                {
                    times.Add(cKeys[i].time);
                }

                for (int i = 0; i < aKeys.Length; ++i)
                {
                    times.Add(aKeys[i].time);
                }

                times.Sort();

                // Found the smallest increment between 2 keys
                for (int i = 1, timesCount = times.Count; i < timesCount; ++i)
                {
                    int k0 = Math.Max(i - 1, 0);
                    int k1 = Math.Min(i, timesCount - 1);
                    float delta = Math.Abs(times[k0] - times[k1]);

                    // Do not compare if time is duplicated
                    if (delta > 0 && delta < smallestDelta)
                        smallestDelta = delta;
                }

                // Nyquist-Shannon
                // smallestDelta: 1.00f => Sampling => 2
                // smallestDelta: 0.50f => Sampling => 3
                // smallestDelta: 0.33f => Sampling => 4
                // smallestDelta: 0.25f => Sampling => 5

                // 2x: Theoretical limits
                // 4x: Preserve original frequency

                // Round to the closest 4 * Nyquist-Shannon limits
                // 4x for Fixed to capture sharp discontinuity
                float scale;
                if (precise || gradientMode == GradientMode.Fixed)
                    scale = 4.0f;
                else
                    scale = 2.0f;

                float size = scale * (float)Math.Ceiling(1.0 / smallestDelta + 1.0);
                textureSize = (int)Math.Round(size);
                // Arbitrary max (1024)
                textureSize = Math.Min(textureSize, 1024);
            }

            SetDirty();
        }

        /// <summary>
        /// Cleans up the internal texture resource.
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// Releases the internal texture resource.
        /// </summary>
        public void Release()
        {
            CoreUtils.Destroy(_texture);
            _texture = null;
        }

        /// <summary>
        /// Marks the curve as dirty to trigger a redraw of the texture the next time <see cref="GetTexture"/>
        /// is called.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDirty()
        {
            _isTextureDirty = true;
        }

        static GraphicsFormat GetTextureFormat()
        {
            return GraphicsFormat.R8G8B8A8_UNorm;
        }

        /// <summary>
        /// Gets the texture representation of this Gradient.
        /// </summary>
        /// <returns>A texture.</returns>
        public Texture2D GetTexture()
        {
            float step = 1.0f / (textureSize - 1);

            if (_texture != null && _texture.width != textureSize)
            {
                UnityEngine.Object.DestroyImmediate(_texture);
                _texture = null;
            }

            if (_texture == null)
            {
                _texture = new Texture2D(textureSize, 1, GetTextureFormat(),
                    TextureCreationFlags.DontInitializePixels);
                _texture.name = "GradientTexture";
                _texture.hideFlags = HideFlags.HideAndDontSave;
                _texture.filterMode = FilterMode.Bilinear;
                _texture.wrapMode = TextureWrapMode.Clamp;
                _texture.anisoLevel = 0;
                _isTextureDirty = true;
            }

            if (_isTextureDirty)
            {
                var pixels = _texture.GetPixelData<Color>(mipLevel: 0);

                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Evaluate(i * step);

                _texture.Apply(false, false);
                _isTextureDirty = false;
                _texture.IncrementUpdateCount();
            }

            return _texture;
        }

        /// <summary>
        /// Evaluate a time value on the Gradient.
        /// </summary>
        /// <param name="time">The time within the Gradient you want to evaluate.</param>
        /// <returns>The value of the Gradient, at the point in time specified.</returns>
        public Color Evaluate(float time)
        {
            if (textureSize <= 0)
                return Color.black;

            return gradient.Evaluate(time);
        }

        /// <summary>
        /// Setup Gradient with an array of color keys and alpha keys.
        /// </summary>
        /// <param name="cKeys">Color keys of the gradient (maximum 8 color keys).</param>
        /// <param name="aKeys">Alpha keys of the gradient (maximum 8 alpha keys).</param>
        /// <param name="gradientMode">Indicates the color space that the gradient color keys are using.</param>
        /// <param name="cSpace">Controls how the gradient colors are interpolated.</param>
        public void SetKeys(GradientColorKey[] cKeys, GradientAlphaKey[] aKeys,
            GradientMode gradientMode, ColorSpace cSpace)
        {
            gradient.SetKeys(cKeys, aKeys);
            gradient.mode = gradientMode;
            gradient.colorSpace = cSpace;
            // Rebuild will make the TextureGradient Dirty.
            Rebuild(cKeys, aKeys, gradientMode, cSpace, _requestedTextureSize, _precise);
        }
        #endregion // UnityEngine.Rendering
    }
}
