using UnityEngine;

namespace PKGE
{
    //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/Effects/RandomHue.cs
    #region BoatAttack
    /// <summary>
    /// Simple script to set a random hue on a shader with the property '_Hue'
    /// </summary>
    public class RandomHue : MonoBehaviour
    {
        public MeshRenderer[] renderers;
        static readonly int Hue = Shader.PropertyToID("_Hue");

        void OnEnable()
        {
            RandomizeHue();
        }

        void RandomizeHue()
        {
#if STATIC_EVERYTHING
            var hue = 0f;
#else
            var hue = Random.Range(0f, 1f);
#endif

            if (renderers == null || renderers.Length <= 0)
                return;
            
            foreach (var t in renderers)
            {
                if (t == null)
                    continue;
                
                t.material.SetFloat(Hue, hue);
            }
        }
    }
    #endregion // BoatAttack
}