using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowBlox.UICore.Utilities
{
    public sealed class LibraryMetadataResult
    {
        public string AssemblyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    public static class LibraryMetadataExtractor
    {
        public static LibraryMetadataResult Extract(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The file path must not be empty.", nameof(filePath));

            var assemblyName = AssemblyName.GetAssemblyName(filePath);
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            var result = new LibraryMetadataResult
            {
                AssemblyName = assemblyName.Name ?? string.Empty,
                Description = ResolveDescription(versionInfo, assemblyName.Name),
                Version = NormalizeToMajorMinorPatch(
                    assemblyName.Version?.ToString() ??
                    versionInfo.FileVersion ??
                    versionInfo.ProductVersion)
            };

            return result;
        }

        public static string NormalizeToMajorMinorPatch(string rawVersion)
        {
            if (string.IsNullOrWhiteSpace(rawVersion))
                return "1.0.0";

            if (Version.TryParse(rawVersion, out var parsedVersion))
            {
                var patch = parsedVersion.Build >= 0 ? parsedVersion.Build : 0;
                return $"{parsedVersion.Major}.{parsedVersion.Minor}.{patch}";
            }

            var numberMatches = Regex.Matches(rawVersion, @"\d+");
            if (numberMatches.Count == 0)
                return "1.0.0";

            var major = numberMatches.Count > 0 ? numberMatches[0].Value : "1";
            var minor = numberMatches.Count > 1 ? numberMatches[1].Value : "0";
            var patchFromRaw = numberMatches.Count > 2 ? numberMatches[2].Value : "0";

            return $"{major}.{minor}.{patchFromRaw}";
        }

        private static string ResolveDescription(FileVersionInfo fileVersionInfo, string fallbackName)
        {
            if (!string.IsNullOrWhiteSpace(fileVersionInfo.FileDescription))
                return fileVersionInfo.FileDescription;

            if (!string.IsNullOrWhiteSpace(fileVersionInfo.ProductName))
                return fileVersionInfo.ProductName;

            return fallbackName ?? string.Empty;
        }
    }
}
