using System;
using System.Text.RegularExpressions;

namespace UnityExtensions
{
    public class FileNameFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/FilenameFormatter.cs
        #region Unity.LiveCapture
        public static FileNameFormatter Instance { get; } = new FileNameFormatter();

        static readonly string InvalidFilenameChars = Regex.Escape("/?<>\\:*|\"");
        static readonly string InvalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", InvalidFilenameChars);

        public string Format(string name)
        {
            return Regex.Replace(name, InvalidRegStr, "_",
                RegexOptions.None, TimeSpan.FromSeconds(0.1));
        }
        #endregion // Unity.LiveCapture
    }
}
