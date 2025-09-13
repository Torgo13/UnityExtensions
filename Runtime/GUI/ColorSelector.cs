using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PKGE
{
    //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/UI/ColorSelector.cs
    #region BoatAttack.UI
    public class ColorSelector : MonoBehaviour
    {
        public Color value;
        public bool loop;
        public int startOption;
        private int _currentOption;

        public delegate void UpdateValue(int index);

        public UpdateValue updateVal;

        private void ValueUpdate(int i)
        {
            updateVal?.Invoke(i);
        }

        private void Awake()
        {
            _currentOption = startOption;
            UpdateColor();
        }

        public void NextOption()
        {
            _currentOption = ValidateIndex(_currentOption + 1);
            UpdateColor();
            ValueUpdate(_currentOption);
        }

        public void PreviousOption()
        {
            _currentOption = ValidateIndex(_currentOption - 1);
            UpdateColor();
            ValueUpdate(_currentOption);
        }

        public int CurrentOption
        {
            get => _currentOption;
            set
            {
                _currentOption = ValidateIndex(value);
                UpdateColor();
                ValueUpdate(_currentOption);
            }
        }

        private void UpdateColor()
        {
            value = GetPaletteColor(_currentOption);
        }

        private int ValidateIndex(int index)
        {
            if (loop)
            {
                return (int)Mathf.Repeat(index, ColorPalette.Length);
            }

            return Mathf.Clamp(index, 0, ColorPalette.Length);
        }
        
        //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/GameSystem/AppSettings.cs#L295
        #region BoatAttack
        public static int SeedNow
        {
            get
            {
                DateTime dt = DateTime.UtcNow;
                return (int)(dt.Ticks % int.MaxValue);
            }
        }
        
        public static Color[] ColorPalette;
        private static Texture2D _colorPaletteRaw;

        public static Color GetPaletteColor(int index)
        {
            GenerateColors();
            return ColorPalette[index];
        }

        public static Color GetRandomPaletteColor
        {
            get
            {
                GenerateColors();
                Random.InitState(SeedNow + Random.Range(0, 1000));
                return ColorPalette[Random.Range(0, ColorPalette.Length)];
            }
        }

        private static void GenerateColors()
        {
            if (ColorPalette != null && ColorPalette.Length != 0)
                return;

            if (_colorPaletteRaw == null)
                _colorPaletteRaw = Resources.Load<Texture2D>("textures/colorSwatch");

            if (_colorPaletteRaw == null)
            {
                const int colourDepth = 4;
                const int colourDepthSq = colourDepth * colourDepth;
                const float scale = 1f / colourDepth;
                int length = (int)System.Math.Pow(colourDepth, 3);
                
                ColorPalette = new Color[length];
                for (int i = 0; i < ColorPalette.Length; i++)
                {
                    int index = i;
                    int b = index / colourDepthSq;
                    index -= b * colourDepthSq;
                    int g = index / colourDepth;
                    int r = index % colourDepth;

                    ColorPalette[i] = new Color(r * scale, g * scale, b * scale);
                }
            }

            ColorPalette = _colorPaletteRaw.GetPixels();
#if DEBUG
            Debug.Log($"Found {ColorPalette.Length} colors.");
#endif // DEBUG
        }
        #endregion // BoatAttack
    }
    #endregion // BoatAttack.UI
}