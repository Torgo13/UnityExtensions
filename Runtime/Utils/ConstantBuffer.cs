using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PKGE
{
    /// <summary>
    /// Constant Buffer management class.
    /// </summary>
    public class ConstantBuffer
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/ConstantBuffer.cs
        #region UnityEngine.Rendering
        static readonly List<ConstantBufferBase> RegisteredConstantBuffers = new List<ConstantBufferBase>();

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it globally via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void PushGlobal<CBType>(CommandBuffer cmd, in CBType data, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(cmd, data);
            cb.SetGlobal(cmd, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it globally.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void PushGlobal<CBType>(in CBType data, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(data);
            cb.SetGlobal(shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it to a compute shader via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Push<CBType>(CommandBuffer cmd, in CBType data, ComputeShader cs, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(cmd, data);
            cb.Set(cmd, cs, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it to a compute shader.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Push<CBType>(in CBType data, ComputeShader cs, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(data);
            cb.Set(cs, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it to a material via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="mat">Material to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Push<CBType>(CommandBuffer cmd, in CBType data, Material mat, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(cmd, data);
            cb.Set(mat, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it to a material.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="mat">Material to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Push<CBType>(in CBType data, Material mat, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(data);
            cb.Set(mat, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        public static void UpdateData<CBType>(CommandBuffer cmd, in CBType data) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(cmd, data);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="data">Input data of the constant buffer.</param>
        public static void UpdateData<CBType>(in CBType data) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.UpdateData(data);
        }

        /// <summary>
        /// Bind the constant buffer globally via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void SetGlobal<CBType>(CommandBuffer cmd, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.SetGlobal(cmd, shaderId);
        }

        /// <summary>
        /// Bind the constant buffer globally.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void SetGlobal<CBType>(int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.SetGlobal(shaderId);
        }

        /// <summary>
        /// Bind the constant buffer to a compute shader via a command buffer.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Set<CBType>(CommandBuffer cmd, ComputeShader cs, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.Set(cmd, cs, shaderId);
        }

        /// <summary>
        /// Bind the constant buffer to a compute shader.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Set<CBType>(ComputeShader cs, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.Set(cs, shaderId);
        }

        /// <summary>
        /// Bind the constant buffer to a material.
        /// </summary>
        /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
        /// <param name="mat">Material to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public static void Set<CBType>(Material mat, int shaderId) where CBType : struct
        {
            var cb = ConstantBufferSingleton<CBType>.instance;

            cb.Set(mat, shaderId);
        }

        /// <summary>
        /// Release all currently allocated singleton constant buffers.
        /// This needs to be called before shutting down the application.
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var cb in RegisteredConstantBuffers)
                cb.Release();

            RegisteredConstantBuffers.Clear();
        }

        internal static void Register(ConstantBufferBase cb)
        {
            RegisteredConstantBuffers.Add(cb);
        }
    }

    /// <summary>
    /// The base class of Constant Buffer.
    /// </summary>
    public abstract class ConstantBufferBase
    {
        /// <summary>
        /// Release the constant buffer.
        /// </summary>
        public abstract void Release();
    }

    /// <summary>
    /// An instance of a constant buffer.
    /// </summary>
    /// <typeparam name="CBType">The type of structure representing the constant buffer data.</typeparam>
    public class ConstantBuffer<CBType> : ConstantBufferBase where CBType : struct
    {
        // Used to track all global bindings used by this CB type.
        readonly HashSet<int> _globalBindings = new HashSet<int>();
        // Array is required by the ComputeBuffer SetData API
        readonly CBType[] _data = new CBType[1];

        readonly ComputeBuffer _gpuConstantBuffer;

        /// <summary>
        /// Constant Buffer constructor.
        /// </summary>
        public ConstantBuffer()
        {
            _gpuConstantBuffer = new ComputeBuffer(1,
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<CBType>(), ComputeBufferType.Constant);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer via a command buffer.
        /// </summary>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        public void UpdateData(CommandBuffer cmd, in CBType data)
        {
            _data[0] = data;
#if UNITY_2021_1_OR_NEWER
            cmd.SetBufferData(_gpuConstantBuffer, _data);
#else
            cmd.SetComputeBufferData(_gpuConstantBuffer, _data);
#endif
        }

        /// <summary>
        /// Update the GPU data of the constant buffer.
        /// </summary>
        /// <param name="data">Input data of the constant buffer.</param>
        public void UpdateData(in CBType data)
        {
            _data[0] = data;
            _gpuConstantBuffer.SetData(_data);
        }

        /// <summary>
        /// Bind the constant buffer globally via a command buffer.
        /// </summary>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void SetGlobal(CommandBuffer cmd, int shaderId)
        {
            _globalBindings.Add(shaderId);
            cmd.SetGlobalConstantBuffer(_gpuConstantBuffer, shaderId, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Bind the constant buffer globally.
        /// </summary>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void SetGlobal(int shaderId)
        {
            _globalBindings.Add(shaderId);
            Shader.SetGlobalConstantBuffer(shaderId, _gpuConstantBuffer, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Bind the constant buffer to a compute shader via a command buffer.
        /// </summary>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void Set(CommandBuffer cmd, ComputeShader cs, int shaderId)
        {
            cmd.SetComputeConstantBufferParam(cs, shaderId, _gpuConstantBuffer, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Bind the constant buffer to a compute shader.
        /// </summary>
        /// <param name="cs">Compute shader to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void Set(ComputeShader cs, int shaderId)
        {
            cs.SetConstantBuffer(shaderId, _gpuConstantBuffer, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Bind the constant buffer to a material.
        /// </summary>
        /// <param name="mat">Material to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void Set(Material mat, int shaderId)
        {
            // This isn't done via command buffer because as long as the buffer itself is not destroyed,
            // the binding stays valid. Only the commit of data needs to go through the command buffer.
            // We do it here anyway for now to simplify user API.
            mat.SetConstantBuffer(shaderId, _gpuConstantBuffer, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Bind the constant buffer to a material property block.
        /// </summary>
        /// <param name="mpb">Material property block to which the constant buffer should be bound.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void Set(MaterialPropertyBlock mpb, int shaderId)
        {
            mpb.SetConstantBuffer(shaderId, _gpuConstantBuffer, 0, _gpuConstantBuffer.stride);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it globally via a command buffer.
        /// </summary>
        /// <param name="cmd">Command Buffer used to execute the graphic commands.</param>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void PushGlobal(CommandBuffer cmd, in CBType data, int shaderId)
        {
            UpdateData(cmd, data);
            SetGlobal(cmd, shaderId);
        }

        /// <summary>
        /// Update the GPU data of the constant buffer and bind it globally.
        /// </summary>
        /// <param name="data">Input data of the constant buffer.</param>
        /// <param name="shaderId">Shader property id to bind the constant buffer to.</param>
        public void PushGlobal(in CBType data, int shaderId)
        {
            UpdateData(data);
            SetGlobal(shaderId);
        }

        /// <summary>
        /// Release the constant buffers.
        /// </summary>
        public override void Release()
        {
            // Depending on the device, globally bound buffers can leave stale "valid" shader ids pointing to a destroyed buffer.
            // In DX11 it does not cause issues but on Vulkan this will result in skipped drawcalls (even if the buffer is not actually accessed in the shader).
            // To avoid this kind of issues, it's good practice to "unbind" all globally bound buffers upon destruction.
            foreach (int shaderId in _globalBindings)
                Shader.SetGlobalConstantBuffer(shaderId, (ComputeBuffer)null, 0, 0);
            _globalBindings.Clear();

            CoreUtils.SafeRelease(_gpuConstantBuffer);
        }
    }

    class ConstantBufferSingleton<CBType> : ConstantBuffer<CBType> where CBType : struct
    {
        static ConstantBufferSingleton<CBType> _instance;
        internal static ConstantBufferSingleton<CBType> instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConstantBufferSingleton<CBType>();
                    ConstantBuffer.Register(_instance);
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public override void Release()
        {
            base.Release();
            _instance = null;
        }
        #endregion // UnityEngine.Rendering
    }
}
