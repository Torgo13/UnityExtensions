using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// A formatter that ensures redundant string entries are given a unique name.
    /// </summary>
    public class UniqueNameFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Editor/Core/Utilities/UniqueNameFormatter.cs
        #region Unity.LiveCapture.Editor
        readonly HashSet<string> m_Names = new HashSet<string>();

        public string Format(string text)
        {
            var name = ObjectNames.GetUniqueName(m_Names.ToArray(), text);
            m_Names.Add(name);
            return name;
        }
        #endregion // Unity.LiveCapture.Editor
    }
}
