using UnityEngine;

namespace UnityExtensions
{
    public static class SampleUtility
    {
        //https://github.com/needle-mirror/com.unity.animation.cs-jobs-samples/blob/0.6.1-preview/Samples/Scripts/SampleUtility.cs
        #region SampleUtility
        public static AnimationClip LoadAnimationClipFromFbx(string fbxName, string clipName)
        {
            var clips = Resources.LoadAll<AnimationClip>(fbxName);
            foreach (var clip in clips)
            {
                if (clip.name == clipName)
                    return clip;
            }
            
            return null;
        }

        public static GameObject CreateEffector(string name, Vector3 position, Quaternion rotation)
        {
            var effector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effector.name = name;
            effector.transform.SetPositionAndRotation(position, rotation);
            effector.transform.localScale = Vector3.one * 0.15f;
            var meshRenderer = effector.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.color = Color.magenta;
            return effector;
        }
        #endregion // SampleUtility
    }
}
