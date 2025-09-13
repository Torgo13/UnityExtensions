using System;
using UnityEngine;

namespace PKGE.Editor.Tests
{
    class PrefabScope : IDisposable
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture.tests/Tests/Editor/Core/PrefabScope.cs
        #region Unity.LiveCapture.Tests.Editor
        bool m_Disposed;

        public GameObject Root { get; private set; }

        public PrefabScope(string path)
        {
            var prefab = Resources.Load<GameObject>(path);

            Root = GameObject.Instantiate(prefab);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(PrefabScope));

            if (Root != null)
                GameObject.DestroyImmediate(Root);

            m_Disposed = true;
        }
        #endregion // Unity.LiveCapture.Tests.Editor
    }
}
