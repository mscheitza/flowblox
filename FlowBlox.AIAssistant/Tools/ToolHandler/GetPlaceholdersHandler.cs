using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetPlaceholdersHandler : ToolHandlerBase
    {
        public override string Name => "GetPlaceholders";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns available placeholders for EnableFieldSelection-capable string properties. Includes field placeholders ($FlowBlock::FieldName / $User::FieldName) and additional $Project:: / $Options:: placeholders. Values are not resolved.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var registry = ToolHandlerUtilities.GetRegistry();
            var project = ToolHandlerUtilities.GetProject();
            var options = FlowBloxOptions.GetOptionInstance();

            var fieldPlaceholders = registry.GetFieldElements()
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.FullyQualifiedName))
                .GroupBy(x => x.FullyQualifiedName, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.UserField ? 0 : 1)
                .ThenBy(x => x.FullyQualifiedName, StringComparer.OrdinalIgnoreCase)
                .Select(ToFieldPlaceholderInfo)
                .ToList();

            var projectPlaceholders = project.GetProjectPropertyElements()
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Placeholder))
                .OrderBy(x => x.Placeholder, StringComparer.OrdinalIgnoreCase)
                .Select(ToProjectPlaceholderInfo)
                .ToList();

            var optionPlaceholders = options.GetOptions()
                .Where(x => x != null && x.IsPlaceholderEnabled)
                .Select(ToOptionPlaceholderInfo)
                .OrderBy(x => x.Value<string>("placeholder"), StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["fieldPlaceholders"] = new JArray(fieldPlaceholders),
                ["projectPlaceholders"] = new JArray(projectPlaceholders),
                ["optionPlaceholders"] = new JArray(optionPlaceholders),
                ["summary"] = new JObject
                {
                    ["fieldPlaceholderCount"] = fieldPlaceholders.Count,
                    ["userFieldCount"] = fieldPlaceholders.Count(x => x.Value<bool?>("isUserField") == true),
                    ["runtimeFieldCount"] = fieldPlaceholders.Count(x => x.Value<bool?>("isUserField") != true),
                    ["projectPlaceholderCount"] = projectPlaceholders.Count,
                    ["optionPlaceholderCount"] = optionPlaceholders.Count
                },
                ["hint"] = "Use these placeholders only in properties whose type metadata indicates EnableFieldSelection."
            }));
        }

        private static JObject ToFieldPlaceholderInfo(FieldElement field)
        {
            var isUserField = field.UserField;

            return new JObject
            {
                ["placeholder"] = field.FullyQualifiedName,
                ["fullyQualifiedFieldName"] = field.FullyQualifiedName,
                ["fieldName"] = field.Name ?? string.Empty,
                ["isUserField"] = isUserField,
                ["userFieldType"] = isUserField ? field.UserFieldType.ToString() : string.Empty,
                ["originFlowBlockName"] = isUserField ? string.Empty : field.FlowBlockName ?? string.Empty,
                ["origin"] = isUserField ? "UserField" : "FlowBlockResultField",
                ["fieldType"] = field.FieldType?.FieldType.ToString() ?? string.Empty
            };
        }

        private static JObject ToProjectPlaceholderInfo(FlowBloxProjectPropertyElement propertyElement)
        {
            return new JObject
            {
                ["placeholder"] = propertyElement.Placeholder,
                ["key"] = propertyElement.Key ?? string.Empty,
                ["displayName"] = propertyElement.DisplayName ?? string.Empty,
                ["description"] = propertyElement.Description ?? string.Empty,
                ["origin"] = "ProjectProperty"
            };
        }

        private static JObject ToOptionPlaceholderInfo(OptionElement optionElement)
        {
            return new JObject
            {
                ["placeholder"] = $"$Options::{optionElement.Name}",
                ["name"] = optionElement.Name ?? string.Empty,
                ["displayName"] = optionElement.DisplayName ?? optionElement.Name ?? string.Empty,
                ["description"] = optionElement.Description ?? string.Empty,
                ["optionType"] = optionElement.Type.ToString(),
                ["origin"] = "Option"
            };
        }
    }
}
