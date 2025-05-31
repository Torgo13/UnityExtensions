#if PACKAGE_RENDER_PIPELINES_CORE
#else
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace UnityExtensions.Packages
{
    /// <summary>
    /// ContextContainer is a Dictionary-like storage where the key is a generic parameter and the value is of the same type.
    /// </summary>
    public class ContextContainer : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/ContextContainer.cs
        #region UnityEngine.Rendering
        Item[] _items = new Item[64];
        readonly List<uint> _activeItemIndices = new List<uint>();

        /// <summary>
        /// Retrieves a T of class <c>ContextContainerItem</c> if it was previously created without it being disposed.
        /// </summary>
        /// <typeparam name="T">Is the class which you are trying to fetch.
        /// T has to inherit from <c>ContextContainerItem</c></typeparam>
        /// <returns>The value created previously using <![CDATA[Create<T>]]> .</returns>
        /// <exception cref="InvalidOperationException">This is thrown if the value isn't previously created.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>()
            where T : ContextItem, new()
        {
            var typeId = TypeId<T>.Value;
            if (!Contains(typeId))
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} has not been created yet.");
            }

            return (T) _items[typeId].Storage;
        }

        /// <summary>
        /// Creates the value of type T.
        /// </summary>
        /// <typeparam name="T">Is the class which you are trying to fetch.
        /// T has to inherit from <c>ContextContainerItem</c></typeparam>
         /// <returns>The value of type T created inside the <c>ContextContainer</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if you try to create the value of type T again
        /// after it is already created.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
        public T Create<T>([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
#else
        public T Create<T>()
#endif
            where T : ContextItem, new()
        {
            var typeId = TypeId<T>.Value;
            if (Contains(typeId))
            {
#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
                throw new InvalidOperationException($"Type {typeof(T).FullName} has already been created. It was previously created in member {_items[typeId].memberName} at line {_items[typeId].lineNumber} in {_items[typeId].filePath}.");
#else
                throw new InvalidOperationException($"Type {typeof(T).FullName} has already been created.");
#endif
            }

#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
            return CreateAndGetData<T>(typeId, lineNumber, memberName, filePath);
#else
            return CreateAndGetData<T>(typeId);
#endif
        }

        /// <summary>
        /// Creates the value of type T if the value is not previously created otherwise try to get the value of type T.
        /// </summary>
        /// <typeparam name="T">Is the class which you are trying to fetch.
        /// T has to inherit from <c>ContextContainerItem</c></typeparam>
        /// <returns>Returns the value of type T which is created or retrieved.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
        public T GetOrCreate<T>([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "")
#else
        public T GetOrCreate<T>()
#endif
            where T : ContextItem, new()
        {
            var typeId = TypeId<T>.Value;
            if (Contains(typeId))
            {
                return (T) _items[typeId].Storage;
            }

#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
            return CreateAndGetData<T>(typeId, lineNumber, memberName, filePath);
#else
            return CreateAndGetData<T>(typeId);
#endif
        }

        /// <summary>
        /// Check if the value of type T has previously been created.
        /// </summary>
        /// <typeparam name="T">Is the class which you are trying to fetch.
        /// T has to inherit from <c>ContextContainerItem</c></typeparam>
        /// <returns>Returns true if the value exists and false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<T>()
            where T : ContextItem, new()
        {
            var typeId = TypeId<T>.Value;
            return Contains(typeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Contains(uint typeId) => typeId < _items.Length && _items[typeId].IsSet;

#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
        T CreateAndGetData<T>(uint typeId, int lineNumber, string memberName, string filePath)
#else
        T CreateAndGetData<T>(uint typeId)
#endif
            where T : ContextItem, new()
        {
            if (_items.Length <= typeId)
            {
                var items = new Item[math.max(math.ceilpow2(_typeCount), _items.Length * 2)];
                for (var i = 0; i < _items.Length; i++)
                {
                    items[i] = _items[i];
                }

                _items = items;
            }

            _activeItemIndices.Add(typeId);
            ref var item = ref _items[typeId];
            item.Storage ??= new T();
            item.IsSet = true;
#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
            item.lineNumber = lineNumber;
            item.memberName = memberName;
            item.filePath = filePath;
#endif

            return (T)item.Storage;
        }

        /// <summary>
        /// Call Dispose to remove the created values.
        /// </summary>
        public void Dispose()
        {
            foreach (var index in _activeItemIndices)
            {
                ref var item = ref _items[index];
                item.Storage.Reset();
                item.IsSet = false;
            }

            _activeItemIndices.Clear();
        }

        static uint _typeCount;

        static class TypeId<T>
        {
            public static uint Value = _typeCount++;
        }

        struct Item
        {
            public ContextItem Storage;
            public bool IsSet;
#if CONTEXT_CONTAINER_ALLOCATOR_DEBUG
            public int lineNumber;
            public string memberName;
            public string filePath;
#endif
        }
        #endregion // UnityEngine.Rendering
    }

    /// <summary>
    /// This is needed to add the data to <c>ContextContainer</c> and will control how the data are removed
    /// when calling Dispose on the <c>ContextContainer</c>.
    /// </summary>
    public abstract class ContextItem
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/ContextContainer.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Resets the object so it can be used as a new instance next time it is created.
        /// To avoid memory allocations and generating garbage, the system reuses objects.
        /// This function should clear the object so it can be reused without leaking any
        /// information (e.g. pointers to objects that will no longer be valid to access).
        /// So it is important the implementation carefully clears all relevant members.
        /// Note that this is different from a Dispose or Destructor as the object in not
        /// freed but reset. This can be useful when having large sub-allocated objects like
        /// arrays or lists which can be cleared and reused without re-allocating.
        /// </summary>
        public abstract void Reset();
        #endregion // UnityEngine.Rendering
    }
}
#endif // PACKAGE_RENDER_PIPELINES_CORE
