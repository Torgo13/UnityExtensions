using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityExtensions.Editor
{
    public class SimpleDataCache
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/SimpleDataCache.cs
        #region UnityEditor.ShaderAnalysis.Internal
        [StructLayout(LayoutKind.Explicit)]
        struct Item
        {
            [FieldOffset(0)] internal bool @bool;
            [FieldOffset(0)] internal byte @byte;
            [FieldOffset(0)] internal short @short;
            [FieldOffset(0)] internal ushort @ushort;
            [FieldOffset(0)] internal int @int;
            [FieldOffset(0)] internal uint @uint;
            [FieldOffset(0)] internal string @string;
        }

        readonly Dictionary<int, Item> _items = new Dictionary<int, Item>();

        public void Set(int key, bool v)
        {
            _items[key] = new Item { @bool = v };
        }

        public void Set(int key, byte v)
        {
            _items[key] = new Item { @byte = v };
        }

        public void Set(int key, short v)
        {
            _items[key] = new Item { @short = v };
        }

        public void Set(int key, ushort v)
        {
            _items[key] = new Item { @ushort = v };
        }

        public void Set(int key, int v)
        {
            _items[key] = new Item { @int = v };
        }

        public void Set(int key, uint v)
        {
            _items[key] = new Item { @uint = v };
        }

        public void Set(int key, string v)
        {
            _items[key] = new Item { @string = v };
        }

        public bool GetBool(int key)
        {
            return _items.TryGetValue(key, out Item t) && t.@bool;
        }

        public byte GetByte(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@byte : default;
        }

        public short GetShort(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@short : default;
        }

        public ushort GetUShort(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@ushort : default;
        }

        public int GetInt(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@int : default;
        }

        public uint GetUInt(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@uint : default;
        }

        public string GetString(int key)
        {
            return _items.TryGetValue(key, out Item t) ? t.@string : default;
        }

        public void Clear()
        {
            _items.Clear();
        }
        #endregion // UnityEditor.ShaderAnalysis.Internal
    }
}
