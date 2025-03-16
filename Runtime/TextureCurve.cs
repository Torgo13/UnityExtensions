using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityExtensions
{
    /// <summary>
    /// A wrapper around <c>AnimationCurve</c> to automatically bake it into a texture.
    /// </summary>
    /// <remarks><list type="bullet">
    /// <item>Dirty state handling so we know when a curve has changed or not.</item>
    /// <item>Looping support (infinite curve).</item>
    /// <item>Zero-value curve.</item>
    /// <item>Cheaper length property.</item>
    /// </list></remarks>
    [Serializable]
    public class TextureCurve : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/TextureCurve.cs
        #region UnityEngine.Rendering
        const int Precision = 128; // Edit LutBuilder3D if you change this value
        const float Step = 1f / Precision;

        /// <summary>
        /// The number of keys in the curve.
        /// </summary>
        [field: SerializeField]
        int length { get; set; } // Calling AnimationCurve.length is very slow, so it is cached

        public int Length { get { if (_isCurveDirty) { length = curve.length; } return length; } set { length = value; } }
        
        [SerializeField]
        bool loop;

        [SerializeField]
        float zeroValue;

        [SerializeField]
        float range;

        /// <summary>
        /// Internal curve used to generate the Texture
        /// </summary>
        [SerializeField]
        AnimationCurve curve;

        AnimationCurve _loopingCurve;
        Texture2D _texture;

        bool _isCurveDirty;
        bool _isTextureDirty;

        /// <summary>
        /// Retrieves the key at index.
        /// </summary>
        /// <param name="index">The index to look for.</param>
        /// <value>A key.</value>
        public Keyframe this[int index] => curve[index];

        /// <summary>
        /// Creates a new <see cref="TextureCurve"/> from an existing <c>AnimationCurve</c>.
        /// </summary>
        /// <param name="baseCurve">The source <c>AnimationCurve</c>.</param>
        /// <param name="zeroValue">The default value to use when the curve doesn't have any key.</param>
        /// <param name="loop">Should the curve automatically loop in the given <paramref name="bounds"/>?</param>
        /// <param name="bounds">The boundaries of the curve.</param>
        public TextureCurve(AnimationCurve baseCurve, float zeroValue, bool loop, in Vector2 bounds)
            : this(baseCurve.keys, zeroValue, loop, bounds) { }

        /// <summary>
        /// Creates a new <see cref="TextureCurve"/> from an arbitrary number of keyframes.
        /// </summary>
        /// <param name="keys">An array of Keyframes used to define the curve.</param>
        /// <param name="zeroValue">The default value to use when the curve doesn't have any key.</param>
        /// <param name="loop">Should the curve automatically loop in the given <paramref name="bounds"/>?</param>
        /// <param name="bounds">The boundaries of the curve.</param>
        public TextureCurve(Keyframe[] keys, float zeroValue, bool loop, in Vector2 bounds)
        {
            curve = new AnimationCurve(keys);
            this.zeroValue = zeroValue;
            this.loop = loop;
            range = bounds.magnitude;
            length = keys.Length;
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
            if (_texture != null)
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
            _isCurveDirty = true;
            _isTextureDirty = true;
        }

        static GraphicsFormat GetTextureFormat()
        {
            // UUM-41070: We require `Sample | SetPixels` but with the deprecated FormatUsage this was checking `SetPixels`
            // For now, we keep checking for `SetPixels` until the performance hit of doing the correct checks is evaluated
            if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.SetPixels))
                return GraphicsFormat.R16_SFloat;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.R8_UNorm, FormatUsage.SetPixels))
                return GraphicsFormat.R8_UNorm;

            return GraphicsFormat.R8G8B8A8_UNorm;
        }

        /// <summary>
        /// Gets the texture representation of this curve.
        /// </summary>
        /// <returns>A 128x1 texture.</returns>
        public Texture2D GetTexture()
        {
            if (_texture == null)
            {
                _texture = new Texture2D(Precision, 1, GetTextureFormat(), TextureCreationFlags.None);
                _texture.name = "CurveTexture";
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
                    pixels[i] = new Color(Evaluate(i * Step), pixels[i].g, pixels[i].b, pixels[i].a);

                _texture.Apply(false, false);
                _isTextureDirty = false;
            }

            return _texture;
        }

        /// <summary>
        /// Evaluate a time value on the curve.
        /// </summary>
        /// <param name="time">The time within the curve you want to evaluate.</param>
        /// <returns>The value of the curve, at the point in time specified.</returns>
        public float Evaluate(float time)
        {
            if (_isCurveDirty)
                length = curve.length;

            if (length == 0)
                return zeroValue;

            if (!loop || length == 1)
                return curve.Evaluate(time);

            if (_isCurveDirty)
            {
                if (_loopingCurve == null)
                    _loopingCurve = new AnimationCurve();

                var prev = curve[length - 1];
                prev.time -= range;
                var next = curve[0];
                next.time += range;
                _loopingCurve.keys = curve.keys; // GC pressure
                _loopingCurve.AddKey(prev);
                _loopingCurve.AddKey(next);
                _isCurveDirty = false;
            }

            return _loopingCurve.Evaluate(time);
        }

        /// <summary>
        /// Adds a new key to the curve.
        /// </summary>
        /// <param name="time">The time at which to add the key.</param>
        /// <param name="value">The value for the key.</param>
        /// <returns>The index of the added key, or -1 if the key could not be added.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddKey(float time, float value)
        {
            int r = curve.AddKey(time, value);

            if (r > -1)
                SetDirty();

            return r;
        }

        /// <summary>
        /// Removes the keyframe at <paramref name="index"/> and inserts <paramref name="key"/>.
        /// </summary>
        /// <param name="index">The index of the keyframe to replace.</param>
        /// <param name="key">The new keyframe to insert at the specified index.</param>
        /// <returns>The index of the keyframe after moving it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int MoveKey(int index, in Keyframe key)
        {
            int r = curve.MoveKey(index, key);
            SetDirty();
            return r;
        }

        /// <summary>
        /// Removes a key.
        /// </summary>
        /// <param name="index">The index of the key to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveKey(int index)
        {
            curve.RemoveKey(index);
            SetDirty();
        }

        /// <summary>
        /// Smoothes the in and out tangents of the keyframe at <paramref name="index"/>.
        /// A <paramref name="weight"/> of 0 evens out tangents.
        /// </summary>
        /// <param name="index">The index of the keyframe to be smoothed.</param>
        /// <param name="weight">The smoothing weight to apply to the keyframe's tangents.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SmoothTangents(int index, float weight)
        {
            curve.SmoothTangents(index, weight);
            SetDirty();
        }
        #endregion // UnityEngine.Rendering
    }
}
