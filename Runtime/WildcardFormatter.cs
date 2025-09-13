using System.Collections.Generic;
using System.Text;
using UnityEngine.Pool;

namespace PKGE
{
    public abstract class WildcardFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Runtime/Core/Utilities/WildcardFormatter.cs
        #region Unity.LiveCapture
        protected readonly Dictionary<string, string> Replacements = new Dictionary<string, string>();

        protected string Format(string str)
        {
            using var _0 = StringBuilderPool.Get(out var stringBuilder);
            stringBuilder.Append(str);

            return Format(stringBuilder).ToString();
        }

        protected StringBuilder Format(StringBuilder stringBuilder)
        {
            foreach (var pair in Replacements)
            {
                if (pair.Value != null)
                {
                    stringBuilder.Replace(pair.Key, pair.Value);
                }
            }

            return stringBuilder;
        }
        #endregion // Unity.LiveCapture
    }
}