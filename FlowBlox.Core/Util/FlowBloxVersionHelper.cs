using System;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Util
{
    public static class FlowBloxVersionHelper
    {
        private static readonly Regex VersionPrefixRegex = new Regex(@"^\s*(\d+(?:\.\d+){0,3})", RegexOptions.Compiled);

        public static string GetDisplayVersion(string value, string fallback = "0.0.0")
        {
            var normalized = NormalizeVersionString(value);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;

            return fallback;
        }

        public static Version ParseComparableVersion(string value, Version fallback = null)
        {
            fallback ??= new Version(0, 0, 0, 0);

            var normalized = NormalizeVersionString(value);
            if (string.IsNullOrWhiteSpace(normalized))
                return fallback;

            var parts = normalized.Split('.');
            var major = ParsePart(parts, 0);
            var minor = ParsePart(parts, 1);
            var build = ParsePart(parts, 2);
            var revision = ParsePart(parts, 3);

            return new Version(major, minor, build, revision);
        }

        private static string NormalizeVersionString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var raw = value.Trim();
            var semanticCore = raw.Split(['+', '-'], 2)[0];
            var match = VersionPrefixRegex.Match(semanticCore);
            if (!match.Success)
                return null;

            return match.Groups[1].Value;
        }

        private static int ParsePart(string[] parts, int index)
        {
            if (parts == null || index < 0 || index >= parts.Length)
                return 0;

            return int.TryParse(parts[index], out var parsed) ? parsed : 0;
        }
    }
}
