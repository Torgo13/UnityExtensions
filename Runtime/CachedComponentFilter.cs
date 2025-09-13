using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Implement this interface for <see cref="Component"/> classes you want discoverable
    /// by the cached component filter. Make sure the `THostType` matches the `TFilterType` in the
    /// <see cref="CachedComponentFilter{TFilterType, TRootType}"/> filter.
    /// </summary>
    /// <typeparam name="THostType">The type of object the host component contains.</typeparam>
    public interface IComponentHost<THostType> where THostType : class
    {
        /// <summary>
        /// The list of hosted components.
        /// </summary>
        THostType[] HostedComponents { get; }
    }

    /// <summary>
    /// Describes where the initial list of components should be built from.
    /// </summary>
    [Flags]
    public enum CachedSearchType
    {
        /// <summary>
        /// Search in children.
        /// </summary>
        Children = 1,

        /// <summary>
        /// Search on self.
        /// </summary>
        Self = 2,

        /// <summary>
        /// Search in parents.
        /// </summary>
        Parents = 4
    }

    /// <summary>
    /// Provides utility functions to retrieve filtered lists of components. The lists created are automatically cached.
    /// </summary>
    /// <typeparam name="TFilterType">The type of component to find.</typeparam>
    /// <typeparam name="TRootType">The type of component at the root of the hierarchy.</typeparam>
    /// <example>
    /// <para>Proper usage of this class is:</para>
    /// <code>
    /// using (var componentFilter = new CachedComponentFilter&lt;typeToFind,componentTypeThatContains&gt;(instanceOfComponent))
    /// {
    ///
    /// }
    /// </code>
    /// </example>
    public class CachedComponentFilter<TFilterType, TRootType> : IDisposable where TRootType : Component where TFilterType : class
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/CachedComponentFilter.cs
        #region Unity.XR.CoreUtils
        readonly List<TFilterType> _masterComponentStorage;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<TFilterType> TempComponentList = new List<TFilterType>();
        static readonly List<IComponentHost<TFilterType>> TempHostComponentList = new List<IComponentHost<TFilterType>>();

        bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Initializes a new cached component filter.
        /// </summary>
        /// <param name="componentRoot">The component at the root of the hierarchy.</param>
        /// <param name="cachedSearchType">What type of hierarchy traversal to perform.</param>
        /// <param name="includeDisabled">Whether to include components on disabled objects.</param>
        public CachedComponentFilter(TRootType componentRoot, CachedSearchType cachedSearchType = CachedSearchType.Self | CachedSearchType.Children, bool includeDisabled = true)
        {
            _masterComponentStorage = ListPool<TFilterType>.Get();

            TempComponentList.Clear();
            TempHostComponentList.Clear();

            // Components on the root get added first
            if ((cachedSearchType & CachedSearchType.Self) == CachedSearchType.Self)
            {
                componentRoot.GetComponents(TempComponentList);
                componentRoot.GetComponents(TempHostComponentList);
                FilteredCopyToMaster(includeDisabled);
            }

            // Then parents, until/unless we hit an end cap node
            if ((cachedSearchType & CachedSearchType.Parents) == CachedSearchType.Parents)
            {
                var searchRoot = componentRoot.transform.parent;
                while (searchRoot != null)
                {
                    if (searchRoot.GetComponent<TRootType>() != null)
                        break;

                    searchRoot.GetComponents(TempComponentList);
                    searchRoot.GetComponents(TempHostComponentList);
                    FilteredCopyToMaster(includeDisabled);

                    searchRoot = searchRoot.transform.parent;
                }
            }

            // Then children, until/unless we hit an end cap node
            if ((cachedSearchType & CachedSearchType.Children) == CachedSearchType.Children)
            {
                // It's not as graceful going down the hierarchy, so we just use the built-in functions and filter afterwards
                foreach (Transform child in componentRoot.transform)
                {
                    child.GetComponentsInChildren(TempComponentList);
                    child.GetComponentsInChildren(TempHostComponentList);
                    FilteredCopyToMaster(includeDisabled, componentRoot);
                }
            }
        }

        /// <summary>
        /// Initializes a new cached component filter.
        /// </summary>
        /// <param name="componentList">The array of objects to use.</param>
        /// <param name="includeDisabled">Whether to include components on disabled objects.</param>
        public CachedComponentFilter(TFilterType[] componentList, bool includeDisabled = true)
        {
            if (componentList == null)
                return;

            _masterComponentStorage = ListPool<TFilterType>.Get();

            TempComponentList.Clear();
            TempComponentList.AddRange(componentList);
            FilteredCopyToMaster(includeDisabled);
        }

        /// <summary>
        /// Store components that match TChildType.
        /// </summary>
        /// <param name="outputList">The list to which to add matching components.</param>
        /// <typeparam name="TChildType">The type for which to search. Must inherit from or be TFilterType.</typeparam>
        public void StoreMatchingComponents<TChildType>(List<TChildType> outputList) where TChildType : class, TFilterType
        {
            foreach (var currentComponent in _masterComponentStorage)
            {
                if (currentComponent is TChildType asChildType)
                    outputList.Add(asChildType);
            }
        }

        /// <summary>
        /// Get an array of matching components.
        /// </summary>
        /// <typeparam name="TChildType">The type for which to search. Must inherit from or be TFilterType.</typeparam>
        /// <returns>The array of matching components.</returns>
        public TChildType[] GetMatchingComponents<TChildType>() where TChildType : class, TFilterType
        {
            var componentCount = 0;
            foreach (var currentComponent in _masterComponentStorage)
            {
                if (currentComponent is TChildType)
                    componentCount++;
            }

            var outputArray = new TChildType[componentCount];
            componentCount = 0;
            foreach (var currentComponent in _masterComponentStorage)
            {
                var asChildType = currentComponent as TChildType;
                if (asChildType == null)
                    continue;

                outputArray[componentCount] = asChildType;
                componentCount++;
            }

            return outputArray;
        }

        void FilteredCopyToMaster(bool includeDisabled)
        {
            if (includeDisabled)
            {
                _masterComponentStorage.AddRange(TempComponentList);
                foreach (var currentEntry in TempHostComponentList)
                {
                    _masterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
            else
            {
                foreach (var currentEntry in TempComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;
                    if (currentBehaviour != null && !currentBehaviour.enabled)
                        continue;

                    _masterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in TempHostComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;
                    if (currentBehaviour != null && !currentBehaviour.enabled)
                        continue;

                    _masterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
        }

        void FilteredCopyToMaster(bool includeDisabled, TRootType requiredRoot)
        {
            // Here, we want every entry that isn't on the same GameObject as the required root
            // Additionally, any GameObjects that are between this object and the root (children of the root, parent of a component)
            // cannot have a component of the root type, or it is part of a different collection of objects and should be skipped
            if (includeDisabled)
            {
                foreach (var currentEntry in TempComponentList)
                {
                    var currentComponent = currentEntry as Component;

                    if (currentComponent.transform == requiredRoot)
                        continue;

                    if (currentComponent.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    _masterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in TempHostComponentList)
                {
                    var currentComponent = currentEntry as Component;

                    if (currentComponent.transform == requiredRoot)
                        continue;

                    if (currentComponent.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    _masterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
            else
            {
                foreach (var currentEntry in TempComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;

                    if (!currentBehaviour.enabled)
                        continue;

                    if (currentBehaviour.transform == requiredRoot)
                        continue;

                    if (currentBehaviour.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    _masterComponentStorage.Add(currentEntry);
                }

                foreach (var currentEntry in TempHostComponentList)
                {
                    var currentBehaviour = currentEntry as Behaviour;

                    if (!currentBehaviour.enabled)
                        continue;

                    if (currentBehaviour.transform == requiredRoot)
                        continue;

                    if (currentBehaviour.GetComponentInParent<TRootType>() != requiredRoot)
                        continue;

                    _masterComponentStorage.AddRange(currentEntry.HostedComponents);
                }
            }
        }

        /// <summary>
        /// Disposes of the cached component filter.
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">Implement a Dispose method</seealso>
        /// <param name="disposing">Whether to dispose the contents of this object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing && _masterComponentStorage != null)
                ListPool<TFilterType>.Release(_masterComponentStorage);

            _disposedValue = true;
        }

        /// <summary>
        /// Part of the IDisposable pattern.
        /// </summary>
        /// <remarks>
        /// Do not change this code. Put cleanup code in <see cref="Dispose(bool)"/>.
        /// </remarks>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion // Unity.XR.CoreUtils
    }
}
