using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PKGE.Editor
{
    /// <summary>
    /// A formatter that ensures redundant string entries are given a unique name.
    /// </summary>
    public class UniqueNameFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Editor/Core/Utilities/UniqueNameFormatter.cs
        #region Unity.LiveCapture.Editor
        HashSet<string> _names;
        HashSet<string> Names => _names ??= new HashSet<string>(System.StringComparer.Ordinal);

        public string Format(string text)
        {
            var name = ObjectNames.GetUniqueName(Names.ToArray(), text);
            _ = Names.Add(name);
            return name;
        }
        #endregion // Unity.LiveCapture.Editor
    }
}
