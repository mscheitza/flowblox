using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class CreateUserFieldHandler : ToolHandlerBase
    {
        public override string Name => "CreateUserField";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Creates a user field (Input or Memory) with default FieldType=Text, ensures Input_/Memory_ prefix naming, and returns resolver metadata.",
            new JObject
            {
                ["userFieldType"] = "UserFieldTypes (Input|Memory)",
                ["name"] = "string? (optional custom suffix/name; prefix is auto-applied if missing)",
                ["fieldType"] = "FieldTypes? (default Text)",
                ["storeValueLocally"] = "bool? (optional; default true for input fields, false for storage fields)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var userFieldTypeValue = args.Value<string>("userFieldType");
            if (!Enum.TryParse<UserFieldTypes>(userFieldTypeValue, true, out var userFieldType)
                || userFieldType == UserFieldTypes.None)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    "userFieldType is required and must be one of: Input, Memory."));
            }

            var fieldTypeValue = args.Value<string>("fieldType");
            var fieldType = ParseEnum(fieldTypeValue, FieldTypes.Text);

            var registry = ToolHandlerUtilities.GetRegistry();
            var requestedName = args.Value<string>("name");
            var fieldPrefix = userFieldType == UserFieldTypes.Input ? "Input_" : "Memory_";
            var resolvedName = ResolveUserFieldName(
                registry,
                fieldPrefix,
                requestedName);

            var field = registry.CreateUserField(userFieldType, fieldType, resolvedName);

            var storeValueLocally = args.Value<bool?>("storeValueLocally");
            if (storeValueLocally.HasValue)
                field.StoreValueLocally = storeValueLocally.Value;

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["created"] = true,
                ["name"] = field.Name,
                ["fullyQualifiedFieldName"] = field.FullyQualifiedName,
                ["userFieldType"] = field.UserFieldType.ToString(),
                ["fieldType"] = field.FieldType?.FieldType.ToString() ?? string.Empty,
                ["typeFullName"] = typeof(FieldElement).FullName ?? string.Empty,
                ["resolver"] = new JObject
                {
                    ["resolveFieldElementByFQName"] = field.FullyQualifiedName
                }
            }));
        }

        private static string ResolveUserFieldName(FlowBlox.Core.Provider.Registry.FlowBloxRegistry registry, string requiredPrefix, string? requestedName)
        {
            var normalizedRequested = (requestedName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedRequested))
            {
                var prefixed = normalizedRequested.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase)
                    ? normalizedRequested
                    : requiredPrefix + normalizedRequested;

                return EnsureUniqueUserFieldName(registry, prefixed, "_");
            }

            return GetNextNumberedUserFieldName(registry, requiredPrefix);
        }

        private static string EnsureUniqueUserFieldName(FlowBlox.Core.Provider.Registry.FlowBloxRegistry registry, string candidate, string separator)
        {
            if (!registry.HasUserField(candidate))
                return candidate;

            var suffix = 1;
            while (registry.HasUserField(candidate + separator + suffix))
                suffix++;

            return candidate + separator + suffix;
        }

        private static string GetNextNumberedUserFieldName(FlowBlox.Core.Provider.Registry.FlowBloxRegistry registry, string prefix)
        {
            var index = 0;
            while (registry.HasUserField(prefix + index))
                index++;

            return prefix + index;
        }

        private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct
        {
            if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, true, out var parsed))
                return parsed;

            return fallback;
        }
    }
}
