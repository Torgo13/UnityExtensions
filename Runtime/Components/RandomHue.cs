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
        public MeshRenderer[] renderers = System.Array.Empty<MeshRenderer>();
        static readonly int Hue = Shader.PropertyToID("_Hue");

        void OnEnable()
        {
            RandomizeHue(RandomExtensions.SecureRandomUInt);
        }

        void RandomizeHue(uint seed)
        {
            if (renderers.Length <= 0)
                return;

#if STATIC_EVERYTHING
            const float hue = 0f;
#else
            float hue = RandomExtensions.CreateSafe(seed).NextFloat();
#endif

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