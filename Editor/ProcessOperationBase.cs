using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnityExtensions.Editor
{
    public class ProcessOperationBase
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/ProcessOperationBase.cs
        #region UnityEditor.ShaderAnalysis.PSSLInternal
        readonly List<string> _errors = new List<string> { string.Empty };
        readonly StringBuilder _errorBuilder = new StringBuilder();

        readonly List<string> _lines = new List<string>();
        readonly StringBuilder _outputBuilder = new StringBuilder();

        readonly Process _process;

        public List<string> errors
        {
            get
            {
                Flush();
                return _errors;
            }
        }

        public List<string> lines
        {
            get
            {
                Flush();
                return _lines;
            }
        }

        public string output
        {
            get
            {
                Flush();
                return _outputBuilder.ToString();
            }
        }

        public ProcessOperationBase(Process process)
        {
            _process = process;
        }

        public bool isComplete
        {
            get
            {
                return _process.HasExited;
            }
        }

        public void Cancel()
        {
            if (!_process.HasExited)
                _process.Kill();
        }

        void Flush()
        {
            while (_process.StandardError.Peek() > -1)
            {
                var line = _process.StandardError.ReadLine();
                _errorBuilder.AppendLine(line);
            }
            _errors[0] = _errorBuilder.ToString();

            while (_process.StandardOutput.Peek() > -1)
            {
                var line = _process.StandardOutput.ReadLine();
                _lines.Add(line);
                _outputBuilder.AppendLine(line);
            }
        }
        #endregion // UnityEditor.ShaderAnalysis.PSSLInternal
    }
}
