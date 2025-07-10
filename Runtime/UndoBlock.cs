using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace UnityExtensions
{
    /// <summary>
    /// Represents a series of object actions as a single undo-operation.
    /// </summary>
    /// <remarks>
    /// UndoBlock methods work in both Edit mode and Play mode. In Play mode undo-operations are disabled.
    /// This class mirrors the normal functions you find in the <see cref="UnityEditor.Undo"/> class
    /// and collapses them into one operation when the block is complete.
    /// </remarks>
    /// <example>
    /// <para>Proper usage of this class is:</para>
    /// <code>
    /// using (var undoBlock = new UndoBlock("Desired Undo Message"))
    /// {
    ///     undoBlock.yourCodeToUndo();
    /// }
    /// </code>
    /// </example>
    public class UndoBlock : IDisposable
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/UndoBlock.cs
        #region Unity.XR.CoreUtils

        readonly int _undoGroup;
        bool _disposedValue; // To detect redundant calls of Dispose

#if UNITY_EDITOR
        readonly string _undoLabel;
        bool _dirty;
        readonly bool _testMode;
#endif

        /// <summary>
        /// Initialize a new UndoBlock.
        /// </summary>
        /// <param name="undoLabel">The label to apply to the undo group created within this undo block.</param>
        /// <param name="testMode">Whether this is part of a test run.</param>
        public UndoBlock(string undoLabel, bool testMode = false)
        {
#if UNITY_EDITOR
            _dirty = false;
            _testMode = testMode;
            if (!Application.isPlaying && !_testMode)
            {
                UnityEditor.Undo.IncrementCurrentGroup();
                _undoGroup = UnityEditor.Undo.GetCurrentGroup();
                UnityEditor.Undo.SetCurrentGroupName(undoLabel);
                _undoLabel = undoLabel;
            }
            else
            {
                _undoGroup = -1;
            }
#else
            _undoGroup = -1;
#endif
        }

        /// <summary>
        /// Register undo operations for a newly created object.
        /// </summary>
        /// <param name="objectToUndo">The object that was created.</param>
        public void RegisterCreatedObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if (!_testMode && !Application.isPlaying)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(objectToUndo, _undoLabel);
                _dirty = true;
            }
#endif
        }

        /// <summary>
        /// Records any changes done on the object after the RecordObject function.
        /// </summary>
        /// <param name="objectToUndo">The reference to the object that you will be modifying.</param>
        public void RecordObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if (!_testMode && !Application.isPlaying)
                UnityEditor.Undo.RecordObject(objectToUndo, _undoLabel);
#endif
        }

        /// <summary>
        /// Sets the parent transform of an object and records an undo operation.
        /// </summary>
        /// <param name="transform">The Transform component whose parent is to be changed.</param>
        /// <param name="newParent">The parent Transform to be assigned.</param>
        public void SetTransformParent(Transform transform, Transform newParent)
        {
#if UNITY_EDITOR
            if (!_testMode && !Application.isPlaying)
                UnityEditor.Undo.SetTransformParent(transform, newParent, _undoLabel);
            else
                transform.parent = newParent;
#else
            transform.parent = newParent;
#endif
        }

        /// <summary>
        /// Adds a component to the game object and registers an undo operation for this action.
        /// </summary>
        /// <param name="gameObject">The game object you want to add the component to.</param>
        /// <typeparam name="T">The type of component you want to add.</typeparam>
        /// <returns>The new component.</returns>
        public T AddComponent<T>(GameObject gameObject) where T : Component
        {
#if UNITY_EDITOR
            if (!_testMode && !Application.isPlaying)
            {
                _dirty = true;
                return UnityEditor.Undo.AddComponent<T>(gameObject);
            }
#endif

            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Dispose of this object.
        /// </summary>
        /// <param name="disposing">Whether to clean up this object's state.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _undoGroup > -1)
                {
#if UNITY_EDITOR
                    if (!_testMode && !Application.isPlaying)
                    {
                        UnityEditor.Undo.CollapseUndoOperations(_undoGroup);
                        if (_dirty)
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                                SceneManager.GetActiveScene());
                        }
                    }

                    _dirty = false;
#endif
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
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