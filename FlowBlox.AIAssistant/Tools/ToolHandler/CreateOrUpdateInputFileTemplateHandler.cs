using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools.ToolHandler.Converter;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;
using System.Text;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class CreateOrUpdateInputFileTemplateHandler : ToolHandlerBase
    {
        private static readonly string[] SupportedConverters =
        [
            Csv2XlsxConverter.Name,
            Csv2XlsConverter.Name
        ];

        public override string Name => "CreateOrUpdateInputFileTemplate";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Creates or updates an input file template by key (relative to $Project::InputDirectory). If key exists, it is overwritten.",
            new JObject
            {
                ["key"] = "string (relative path under $Project::InputDirectory)",
                ["generatedTemplate"] = "object (required): { textContent: string, converter?: string }",
                ["supportedConverters"] = new JArray(SupportedConverters),
                ["syncMode"] = "string? (CreateIfNotExists|AlwaysOverwrite, default: CreateIfNotExists)",
                ["usageHint"] =
                    "Use generatedTemplate.textContent for template content. " +
                    "Set generatedTemplate.converter='Csv2XlsxConverter' or 'Csv2XlsConverter' to convert CSV text to Excel. " +
                    "No attachments."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                project.InputTemplates ??= new List<FlowBloxInputFileTemplate>();

                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

                FlowBloxInputTemplateHelper.ValidateRelativePathOrThrow(key);
                var normalizedKey = FlowBloxInputTemplateHelper.NormalizeRelativePath(key);

                var generatedTemplate = args["generatedTemplate"] as JObject;
                if (generatedTemplate == null)
                {
                    return Task.FromResult(ToolHandlerUtilities.Fail(
                        "generatedTemplate is required and must contain at least textContent."));
                }

                var textContent = generatedTemplate.Value<string>("textContent") ?? string.Empty;
                var converter = (generatedTemplate.Value<string>("converter") ?? string.Empty).Trim();

                var contentBytes = ConvertContent(textContent, converter);
                var contentBase64 = Convert.ToBase64String(contentBytes);

                var syncMode = ParseSyncMode(args.Value<string>("syncMode"));
                var existing = project.InputTemplates.FirstOrDefault(x =>
                    string.Equals(
                        FlowBloxInputTemplateHelper.NormalizeRelativePath(x?.RelativePath ?? string.Empty),
                        normalizedKey,
                        StringComparison.OrdinalIgnoreCase));

                var created = existing == null;
                var template = existing ?? new FlowBloxInputFileTemplate();
                template.RelativePath = normalizedKey;
                template.ContentBase64 = contentBase64;
                template.SyncMode = syncMode;

                if (created)
                    project.InputTemplates.Add(template);

                FlowBloxInputTemplateHelper.EnsureInputFilesExist(project);

                var materializedPath = FlowBloxInputTemplateHelper.BuildAbsoluteTargetPath(
                    project.ProjectInputDirectory,
                    normalizedKey);

                var payload = new JObject
                {
                    ["created"] = created,
                    ["updated"] = !created,
                    ["key"] = normalizedKey,
                    ["syncMode"] = syncMode.ToString(),
                    ["contentSource"] = "generatedTemplate",
                    ["converterUsed"] = string.IsNullOrWhiteSpace(converter) ? "None" : converter,
                    ["supportedConverters"] = new JArray(SupportedConverters),
                    ["sizeBytes"] = template.ContentBytes?.LongLength ?? 0,
                    ["materializedPath"] = materializedPath,
                    ["projectInputDirectory"] = project.ProjectInputDirectory ?? string.Empty
                };

                return Task.FromResult(ToolHandlerUtilities.Ok(payload));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }

        private static byte[] ConvertContent(string textContent, string converter)
        {
            if (string.IsNullOrWhiteSpace(converter))
                return Encoding.UTF8.GetBytes(textContent ?? string.Empty);

            if (string.Equals(converter, Csv2XlsxConverter.Name, StringComparison.OrdinalIgnoreCase))
                return Csv2XlsxConverter.Convert(textContent ?? string.Empty);

            if (string.Equals(converter, Csv2XlsConverter.Name, StringComparison.OrdinalIgnoreCase))
                return Csv2XlsConverter.Convert(textContent ?? string.Empty);

            throw new InvalidOperationException(
                $"Unsupported converter '{converter}'. Supported converters: {string.Join(", ", SupportedConverters)}.");
        }

        private static FlowBloxInputTemplateSyncMode ParseSyncMode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return FlowBloxInputTemplateSyncMode.CreateIfNotExists;

            if (Enum.TryParse<FlowBloxInputTemplateSyncMode>(value.Trim(), true, out var parsed))
                return parsed;

            return FlowBloxInputTemplateSyncMode.CreateIfNotExists;
        }
    }
}
