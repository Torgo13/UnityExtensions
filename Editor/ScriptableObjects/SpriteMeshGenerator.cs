using UnityEditor;
using UnityEngine;

namespace UnityExtensions.Editor
{
    [CreateAssetMenu(fileName = "SpriteMeshGenerator.asset", menuName = "2D/Sprite Mesh Generator", order = 1)]
    public class SpriteMeshGenerator : ScriptableObject
    {
        public Sprite[] sprites;
        [SerializeField] internal string hash;

        internal void OnValidate()
        {
            if (sprites == null || sprites.Length <= 0)
                return;

            var curHash = "";
            foreach (var sprite in sprites)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out var guid, out long _))
                {
                    curHash += guid;
                }
            }

            if (hash == curHash)
                return;
            
            GenerateAndSaveMesh();
            hash = curHash;
        }

        internal void GenerateAndSaveMesh()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                CleanSubAssets();
                foreach (var sprite in sprites)
                {
                    if (sprite == null)
                        continue;
                    
                    var mesh = sprite.GenerateMeshFromSprite();
                    AssetDatabase.AddObjectToAsset(mesh, assetObject: this);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        internal void CleanSubAssets()
        {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));

            foreach (var t in subAssets)
            {
                if (t == this)
                    continue;
                
                Debug.Log(t);
                DestroyImmediate(t, allowDestroyingAssets: true);
            }
        }
    }
}
