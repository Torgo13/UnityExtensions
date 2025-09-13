using UnityEngine;

namespace PKGE
{
    [RequireComponent(typeof(Renderer))]
    public class CustomizeHighlightColor : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/CustomizeHighlightColor.cs
        #region UnityEngine.Rendering
        public Color selectionColor = Color.white;
        Renderer _renderer;
        MaterialPropertyBlock _propertyBlock;
        static readonly int SelectionColor = Shader.PropertyToID("_SelectionColor");

        void Start()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            SetColor();
        }

        void OnValidate()
        {
            SetColor();
        }

        void SetColor()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(SelectionColor, selectionColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
        #endregion // UnityEngine.Rendering
    }
}
