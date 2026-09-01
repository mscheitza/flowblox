using FlowBlox.Core.Logging;
using System.Collections;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    internal abstract class UsageReporterBase
    {
        public AIProviderUsage ReportUsage(object? response)
        {
            var usage = ExtractUsage(response);
            var logValues = GetLogValues(response).ToList();
            if (logValues.Any(x => x.Value.HasValue))
            {
                FlowBloxLogManager.Instance.GetLogger().Info(
                    $"{ProviderDisplayName} usage: " +
                    string.Join(", ", logValues.Select(x => $"{x.Name}={FormatUsageValue(x.Value)}")));
            }

            return usage;
        }

        protected abstract string ProviderDisplayName { get; }

        protected abstract AIProviderUsage ExtractUsage(object? response);

        protected abstract IEnumerable<UsageLogValue> GetLogValues(object? response);

        protected static UsageLogValue LogValue(string name, int? value)
            => new(name, value);

        protected static int? GetIntProperty(object? source, string propertyName)
        {
            var value = GetPropertyValue(source, propertyName);
            if (value == null)
                return null;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }
        }

        protected static object? GetPropertyValue(object? source, string propertyName)
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            if (source is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (string.Equals(Convert.ToString(entry.Key), propertyName, StringComparison.OrdinalIgnoreCase))
                        return entry.Value;
                }
            }

            return source
                .GetType()
                .GetProperty(propertyName)
                ?.GetValue(source);
        }

        private static string FormatUsageValue(int? value)
            => value.HasValue ? value.Value.ToString() : "n/a";

        protected readonly record struct UsageLogValue(string Name, int? Value);
    }
}