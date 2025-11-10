using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Singleton of a Component class.
    /// </summary>
    /// <remarks>
    /// Use this class to get a static instance of a component.
    /// Mainly used to have a default instance.
    /// </remarks>
    /// <typeparam name="TType">Component type.</typeparam>
    public static class ComponentSingleton<TType>
        where TType : Component
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/ComponentSingleton.cs
        #region UnityEngine.Rendering
        static TType _instance;
        /// <summary>
        /// Instance of the required component type.
        /// </summary>
        public static TType instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("Default " + typeof(TType).Name)
                        { hideFlags = HideFlags.HideAndDontSave };

#if !UNITY_EDITOR
                    GameObject.DontDestroyOnLoad(go);
#endif

                    go.SetActive(false);
                    _instance = go.AddComponent<TType>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Release the component singleton.
        /// </summary>
        public static void Release()
        {
            if (_instance != null)
            {
                var go = _instance.gameObject;
                CoreUtils.Destroy(ref go);
                _instance = null;
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
