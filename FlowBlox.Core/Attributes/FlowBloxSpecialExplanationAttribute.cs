using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class FlowBloxSpecialExplanationAttribute : Attribute
    {
        public string SpecialExplanation { get; }

        public SpecialExplanationIcon Icon { get; set; } = SpecialExplanationIcon.Information;

        public string Color { get; set; }

        public Type ResourceType { get; set; } = typeof(FlowBloxTexts);

        public FlowBloxSpecialExplanationAttribute(string specialExplanation)
        {
            SpecialExplanation = specialExplanation;
        }

        public string GetResolvedSpecialExplanation() => Resolve(SpecialExplanation);

        private string Resolve(string valueOrResourceKey)
        {
            if (string.IsNullOrWhiteSpace(valueOrResourceKey))
            {
                return string.Empty;
            }

            try
            {
                var localized = FlowBloxResourceUtil.GetLocalizedString(valueOrResourceKey, ResourceType);
                if (!string.IsNullOrWhiteSpace(localized) && !string.Equals(localized, valueOrResourceKey, StringComparison.Ordinal))
                    return localized;
            }
            catch
            {
                // Fall back to raw value/key to keep metadata resolution resilient.
            }

            return valueOrResourceKey;
        }
    }
}

