using UnityEngine;

namespace PKGE
{
    [RequireComponent(typeof(Renderer))]
    public class CustomizeHighlightColor : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/CustomizeHighlightColor.cs
        #region UnityEngine.Rendering
        [ColorUsage(showAlpha: true, hdr: true), SerializeField]
        Color selectionColor = Color.white;

        public Color SelectionColor
        {
            get { return selectionColor; }
            set { selectionColor = value; SetColor(); }
        }

        Renderer _renderer;

#if UNITY_6000_3_OR_NEWER
        MeshRenderer _meshRenderer;
        bool _isMeshRenderer;

        SkinnedMeshRenderer _skinnedMeshRenderer;
        bool _isSkinnedMeshRenderer;
#endif // UNITY_6000_3_OR_NEWER

        MaterialPropertyBlock _propertyBlock;
        static readonly int _SelectionColor = Shader.PropertyToID("_SelectionColor");

        void Start()
        {
            _renderer = GetComponent<Renderer>();

#if UNITY_6000_3_OR_NEWER
            _meshRenderer = _renderer as MeshRenderer;
            _isMeshRenderer = _meshRenderer != null;

            _skinnedMeshRenderer = _renderer as SkinnedMeshRenderer;
            _isSkinnedMeshRenderer = _skinnedMeshRenderer != null;

            if (!_isMeshRenderer && !_isSkinnedMeshRenderer)
#endif // UNITY_6000_3_OR_NEWER
                _propertyBlock = new MaterialPropertyBlock();

            SetColor();
        }

        void OnValidate()
        {
            SetColor();
        }

        void SetColor()
        {
#if UNITY_6000_3_OR_NEWER
            if (_isMeshRenderer)
                _meshRenderer.SetShaderUserValue(new Union4 { Color32 = selectionColor }.UInt);
            else if (_isSkinnedMeshRenderer)
                _skinnedMeshRenderer.SetShaderUserValue(new Union4 { Color32 = selectionColor }.UInt);
            else
#endif // UNITY_6000_3_OR_NEWER
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(_SelectionColor, selectionColor);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
