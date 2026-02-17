using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Runner.Serialization
{
    public static class RunnerPathTemplateResolver
    {
        // Supports:
        // - %ENV% variables via Environment.ExpandEnvironmentVariables
        // - $Options::Key (legacy)
        // - %Options::Key% (path-friendly)
        // - %timestamp%, %date%, %time%, %projectName%, %hash%, %guid%
        public static string Resolve(string template, RunnerPathTemplateContext ctx = null)
        {
            if (string.IsNullOrWhiteSpace(template))
                return template;

            ctx ??= new RunnerPathTemplateContext();

            // 1) Expand %ENV% variables first.
            var value = Environment.ExpandEnvironmentVariables(template);

            // 2) Replace %Options::X% (new syntax) using enabled placeholder options.
            value = ReplaceOptionPercentSyntax(value);

            // 3) Replace specials.
            value = ReplaceSpecials(value, ctx);

            return value;
        }

        private static string ReplaceOptionPercentSyntax(string value)
        {
            // Pattern: %Options::Some.Key%
            // Only options with IsPlaceholderEnabled are allowed.
            var options = FlowBloxOptions.GetOptionInstance()
                .GetOptions()
                .Where(x => x.IsPlaceholderEnabled)
                .ToDictionary(x => x.Name, x => x.Value?.ToString() ?? "", StringComparer.OrdinalIgnoreCase);

            return Regex.Replace(value, "%Options::(?<key>[^%]+)%", m =>
            {
                var key = m.Groups["key"].Value?.Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return "";

                return options.TryGetValue(key, out var optVal) ? optVal : "";
            }, RegexOptions.IgnoreCase);
        }

        private static string ReplaceSpecials(string value, RunnerPathTemplateContext ctx)
        {
            var ts = ctx.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var date = ctx.UtcNow.ToString("yyyy-MM-dd");
            var time = ctx.UtcNow.ToString("HH-mm-ss");
            var guid = Guid.NewGuid().ToString("N")[..8];

            var projectNameSafe = MakeFileNameSafe(ctx.ProjectName);

            var hash = "";
            if (!string.IsNullOrWhiteSpace(ctx.ContentForHash))
                hash = ComputeShortSha256(ctx.ContentForHash);

            value = ReplaceToken(value, "%timestamp%", ts);
            value = ReplaceToken(value, "%date%", date);
            value = ReplaceToken(value, "%time%", time);
            value = ReplaceToken(value, "%guid%", guid);

            // Optional tokens
            value = ReplaceToken(value, "%projectName%", projectNameSafe);
            value = ReplaceToken(value, "%hash%", hash);

            return value;
        }

        private static string ReplaceToken(string value, string token, string replacement)
        {
            if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
                return value;

            replacement ??= "";
            return Regex.Replace(value, Regex.Escape(token), replacement, RegexOptions.IgnoreCase);
        }

        private static string ComputeShortSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant()[..8];
        }

        private static string MakeFileNameSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);

            foreach (var ch in input)
            {
                if (invalid.Contains(ch))
                    sb.Append('_');
                else
                    sb.Append(ch);
            }

            // Avoid trailing dots/spaces
            return sb.ToString().Trim().TrimEnd('.');
        }
    }
}