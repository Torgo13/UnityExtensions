using System;
using System.Text.RegularExpressions;

namespace PKGE.Editor
{
    /// <summary>
    /// Stores a package version in a logically comparable format.
    /// </summary>
    /// <remarks>
    /// The version information stored in the <see cref="PackageVersion"/> follows the Semantic Versioning Specification
    /// (SemVer) https://semver.org/. The version consists of a <see cref="MajorVersion"/>.<see cref="MinorVersion"/>.<see cref="PatchVersion"/>
    /// numerical value followed by optional -<see cref="Prerelease"/>+<see cref="BuildMetaData"/> version information.
    ///
    /// <see cref="PackageVersion"/> follows all the standard for valid and invalid formatting of a version
    /// except for limiting the <see cref="MajorVersion"/>, <see cref="MinorVersion"/>, and <see cref="PatchVersion"/>
    /// to the <c>ulong.MaxValue</c>. There is no such restriction for the values of <see cref="Prerelease"/> or <see cref="BuildMetaData"/>.
    /// </remarks>
    public readonly struct PackageVersion : IEquatable<PackageVersion>, IComparable<PackageVersion>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/ProjectValidation/PackageVersion.cs
        #region Unity.XR.CoreUtils.Editor
        const string Major = "major";
        const string Minor = "minor";
        const string Patch = "patch";
        const string k_Prerelease = "prerelease";
        const string k_BuildMetaData = "buildmetadata";

        // From https://semver.org/ standard https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
        // ^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$
        static readonly Regex Regex = new Regex($@"^(?<{Major}>0|[1-9]\d*)\.(?<{Minor}>0|[1-9]\d*)\." +
            $@"(?<{Patch}>0|[1-9]\d*)(?:-(?<{k_Prerelease}>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)" +
            $@"(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<{k_BuildMetaData}>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");

        readonly string _version;

        readonly ulong _majorVersion;
        readonly ulong _minorVersion;
        readonly ulong _patchVersion;
        readonly string _prerelease;
        readonly string _buildMetaData;

        /// <summary>
        /// Major version number.
        /// </summary>
        public ulong MajorVersion => _majorVersion;

        /// <summary>
        /// Minor version number.
        /// </summary>
        public ulong MinorVersion => _minorVersion;

        /// <summary>
        /// Patch version number.
        /// </summary>
        public ulong PatchVersion => _patchVersion;

        /// <summary>
        /// Reports whether the package is a prerelease.
        /// </summary>
        public bool IsPrerelease => !string.IsNullOrEmpty(_prerelease);

        /// <summary>
        /// The prerelease version information.
        /// </summary>
        public string Prerelease => _prerelease;

        /// <summary>
        /// The build metadata version information.
        /// </summary>
        public string BuildMetaData => _buildMetaData;

        /// <summary>
        /// Creates a <see cref="PackageVersion"/> structure from a valid <paramref name="version"/> string.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <exception cref="FormatException">Thrown when the <paramref name="version"/> is not valid.</exception>
        public PackageVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                _majorVersion = default;
                _minorVersion = default;
                _patchVersion = default;
                _prerelease = default;
                _buildMetaData = default;
                _version = default;
                return;
            }

            _majorVersion = 0;
            _minorVersion = 0;
            _patchVersion = 0;
            _prerelease = string.Empty;
            _buildMetaData = string.Empty;
            _version = version;

            var match = Regex.Match(version);
            if (match.Success)
            {
                var groups = match.Groups;
                _majorVersion = ulong.Parse(groups[Major].Value);
                _minorVersion = ulong.Parse(groups[Minor].Value);
                _patchVersion = ulong.Parse(groups[Patch].Value);

                if (groups[k_Prerelease].Success)
                    _prerelease = groups[k_Prerelease].Value;

                if (groups[k_BuildMetaData].Success)
                    _buildMetaData = groups[k_BuildMetaData].Value;
            }
            else
                throw new FormatException($"Malformed package version string: {version}");
        }

        /// <summary>
        /// Creates a <see cref="PackageVersion"/> structure from the individual version components.
        /// </summary>
        /// <param name="major">The major version value.</param>
        /// <param name="minor">The minor version value.</param>
        /// <param name="patch">The patch version value.</param>
        /// <param name="prerelease">The prerelease version information.</param>
        /// <param name="buildMetaData">The build metadata version information.</param>
        public PackageVersion(ulong major, ulong minor, ulong patch, string prerelease, string buildMetaData)
            : this(GetValueString(major, minor, patch, prerelease, buildMetaData))
        {
        }

        internal static string GetValueString(ulong major, ulong minor, ulong patch, string prerelease, string buildMetaData)
        {
            var prereleaseString = string.Empty;
            if (!string.IsNullOrEmpty(prerelease))
                prereleaseString += $"-{prerelease}";

            var buildMetaDataString = string.Empty;
            if (!string.IsNullOrEmpty(buildMetaData))
                buildMetaDataString += $"+{buildMetaData}";

            return $"{major}.{minor}.{patch}{prereleaseString}{buildMetaDataString}";
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(PackageVersion other)
        {
            return _majorVersion == other.MajorVersion
                && _minorVersion == other.MinorVersion
                && _patchVersion == other.PatchVersion
                && _prerelease == other.Prerelease
                && _buildMetaData == other.BuildMetaData;
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public override bool Equals(object obj)
        {
            return obj is PackageVersion other && Equals(other);
        }

        /// <inheritdoc cref="IEquatable{T}"/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_majorVersion;
                hashCode = (hashCode * 397) ^ (int)_minorVersion;
                hashCode = (hashCode * 397) ^ (int)_patchVersion;
                hashCode = (hashCode * 397) ^ _prerelease.GetHashCode();
                hashCode = (hashCode * 397) ^ _buildMetaData.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/>s are equal
        /// <returns><c>true</c> if operands are equal, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator ==(PackageVersion left, PackageVersion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/>s are not equal
        /// <returns><c>true</c> if operands are not equal, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator !=(PackageVersion left, PackageVersion right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is greater than <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is less than <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is greater than or equals <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is greater than or equals <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than and equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >=(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Operand to check if <see cref="PackageVersion"/> <paramref name="left"/> is less than or equals <paramref name="right"/>.
        /// <returns><c>true</c> if <paramref name="left"/> is less than or equals <paramref name="right"/>, <c>false</c> otherwise.</returns>
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than and equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <=(PackageVersion left, PackageVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc cref="IComparable"/>
        public int CompareTo(PackageVersion other)
        {
            var compare = _majorVersion.CompareTo(other.MajorVersion);
            if (compare != 0)
                return compare;

            compare = _minorVersion.CompareTo(other.MinorVersion);
            if (compare != 0)
                return compare;

            compare = _patchVersion.CompareTo(other.PatchVersion);
            if (compare != 0)
                return compare;

            compare = PackageVersionUtility.EmptyOrNullSubVersionCompare(_prerelease, other.Prerelease);
            if (compare != 0)
                return compare;

            compare = PackageVersionUtility.EmptyOrNullSubVersionCompare(_buildMetaData, other.BuildMetaData);

            return compare;
        }

        /// <summary>
        /// Implicitly creates a new package version from a string.
        /// </summary>
        /// <param name="version">The package version string value.</param>
        /// <returns>A new <see cref="PackageVersion"/> structure.</returns>
        public static implicit operator PackageVersion(string version) => new PackageVersion(version);

        /// <summary>
        /// Returns a properly formatted version string for the <see cref="PackageVersion"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _version;
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
