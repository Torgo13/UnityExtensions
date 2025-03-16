using System;
using System.Text.RegularExpressions;

namespace UnityExtensions
{
    public static class PackageUtility
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/PackageUtility.cs
        #region Unity.LiveCapture
        public static Version GetVersion(string version)
        {
            var versionNumbers = Regex.Split(version, @"\D+",
                RegexOptions.None, TimeSpan.FromSeconds(0.1));

            if (versionNumbers.Length >= 4)
            {
                return new Version(
                    int.Parse(versionNumbers[0]),
                    int.Parse(versionNumbers[1]),
                    int.Parse(versionNumbers[2]),
                    int.Parse(versionNumbers[3])
                );
            }

            return versionNumbers.Length switch
            {
                3 => new Version(int.Parse(versionNumbers[0]), int.Parse(versionNumbers[1]), int.Parse(versionNumbers[2])),
                2 => new Version(int.Parse(versionNumbers[0]), int.Parse(versionNumbers[1])),
                _ => default
            };
        }
        #endregion // Unity.LiveCapture
    }
}
