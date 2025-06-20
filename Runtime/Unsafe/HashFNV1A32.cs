using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;

namespace UnityExtensions.Unsafe
{
    public ref struct HashFNV1A32
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/HashFNV1A32.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// FNV prime.
        /// </summary>
        const uint Prime = 16777619;

        /// <summary>
        /// FNV offset basis.
        /// </summary>
        const uint OffsetBasis = 2166136261;

        uint _hash;

        public static HashFNV1A32 Create()
        {
            return new HashFNV1A32 { _hash = OffsetBasis };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in int input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in uint input)
        {
            unchecked
            {
                _hash = (_hash ^ input) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in bool input)
        {
            _hash = (_hash ^ (input ? 1u : 0u)) * Prime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in float input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in double input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in Vector2 input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in Vector3 input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in Vector4 input)
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append<T>(T input) where T : struct
        {
            unchecked
            {
                _hash = (_hash ^ (uint)input.GetHashCode()) * Prime;
            }
        }

        readonly
        public int value => (int)_hash;

        readonly
        public override int GetHashCode()
        {
            return value;
        }
        #endregion // UnityEngine.Rendering
    }

    public static class DelegateHashCodeUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/HashFNV1A32.cs
        #region UnityEngine.Rendering
        //Cache to prevent CompilerGeneratedAttribute extraction for known delegate
        static NativeHashMap<int, bool> _methodHashCodeToSkipTargetHashMap
            = new NativeHashMap<int, bool>(30, Allocator.Persistent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFuncHashCode(Delegate del)
        {
            //Get MethodInfo hash code as the main one to be used
            var methodHashCode = del.Method.GetHashCode();

            //Check if we are dealing with lambda or static delegates and skip target if we are.
            //Static methods have a null Target.
            //Lambdas have a CompilerGeneratedAttribute as they are generated by a compiler.
            //If Lambda have any captured variable Target hashcode will be different each time we re-create lambda.
            if (!_methodHashCodeToSkipTargetHashMap.TryGetValue(methodHashCode, out var skipTarget))
            {
                skipTarget = del.Target == null || (
                    del.Method.DeclaringType?.IsNestedPrivate == true &&
                    Attribute.IsDefined(del.Method.DeclaringType, typeof(CompilerGeneratedAttribute), false)
                );

                _methodHashCodeToSkipTargetHashMap.Add(methodHashCode, skipTarget);
            }

            //Combine method info hashcode and target hashcode if needed
            return skipTarget ? methodHashCode : methodHashCode ^ RuntimeHelpers.GetHashCode(del.Target);
        }

        //used for testing
#if UNITY_COLLECTIONS_2_1_4_OR_NEWER
        internal static int GetTotalCacheCount() => _methodHashCodeToSkipTargetHashMap.Count;
#else
        internal static int GetTotalCacheCount() => _methodHashCodeToSkipTargetHashMap.Count();
#endif // UNITY_COLLECTIONS_2_1_4_OR_NEWER
            
        internal static void ClearCache() => _methodHashCodeToSkipTargetHashMap.Clear();
        #endregion // UnityEngine.Rendering
    }
}
