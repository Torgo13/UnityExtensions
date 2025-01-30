using System.Collections.Generic;
using System.Text;

namespace UnityExtensions
{
    public abstract class WildcardFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Runtime/Core/Utilities/WildcardFormatter.cs
        #region Unity.LiveCapture
        protected readonly Dictionary<string, string> m_Replacements = new Dictionary<string, string>();

        protected string Format(string str)
        {
            using var _0 = StringBuilderPool.Get(out var m_StringBuilder);
            m_StringBuilder.Append(str);

            foreach (var pair in m_Replacements)
            {
                if (pair.Value != null)
                {
                    m_StringBuilder.Replace(pair.Key, pair.Value);
                }
            }

            return m_StringBuilder.ToString();
        }
        #endregion // Unity.LiveCapture
    }
}