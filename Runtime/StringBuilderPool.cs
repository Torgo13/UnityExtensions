// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System.Text;
using UnityEngine.Pool;

namespace UnityExtensions
{
    public static class StringBuilderPool
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/6000.1/Modules/UIElementsEditor/StringBuilderPool.cs
        #region UnityEditor.UIElements
        private static readonly ObjectPool<StringBuilder> Pool =
            new ObjectPool<StringBuilder>(() => new StringBuilder(), null, sb => sb.Clear());

        public static StringBuilder Get() => Pool.Get();
        public static PooledObject<StringBuilder> Get(out StringBuilder value) => Pool.Get(out value);
        public static void Release(StringBuilder toRelease) => Pool.Release(toRelease);
        #endregion // UnityEditor.UIElements
    }
}
