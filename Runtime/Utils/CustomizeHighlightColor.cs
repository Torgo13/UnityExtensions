using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
{
    [RequireComponent(typeof(Renderer))]
    public class CustomizeHighlightColor : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/CustomizeHighlightColor.cs
        #region UnityEngine.Rendering
        public Color selectionColor = Color.white;
        Renderer rndr;
        MaterialPropertyBlock propertyBlock;
        private static readonly int SelectionColor = Shader.PropertyToID("_SelectionColor");

        void Start()
        {
            rndr = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            SetColor();
        }

        void OnValidate()
        {
            SetColor();
        }

        void SetColor()
        {
            rndr.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(SelectionColor, selectionColor);

            rndr.SetPropertyBlock(propertyBlock);
        }
        #endregion // UnityEngine.Rendering
    }
}
