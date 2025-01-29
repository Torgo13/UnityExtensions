using System.Text.RegularExpressions;

namespace UnityExtensions
{
    public class FileNameFormatter
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/FilenameFormatter.cs
        #region Unity.LiveCapture
        public static FileNameFormatter Instance { get; } = new FileNameFormatter();

        static readonly string s_InvalidFilenameChars = Regex.Escape("/?<>\\:*|\"");
        static readonly string s_InvalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", s_InvalidFilenameChars);

        public string Format(string name)
        {
            return Regex.Replace(name, s_InvalidRegStr, "_");
        }
        #endregion // Unity.LiveCapture
    }
}
