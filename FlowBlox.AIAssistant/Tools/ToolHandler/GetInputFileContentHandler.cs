using FlowBlox.AIAssistant.Helper;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools.ToolHandler.Converter;
using FlowBlox.Core.Models.Project;
using Newtonsoft.Json.Linq;
using System.Text;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetInputFileContentHandler : ToolHandlerBase
    {
        public override string Name => "GetInputFileContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns managed input file content for a single key (relative to $Project::InputDirectory). Response fields: mimeType, key, textContent, converter, message.",
            new JObject
            {
                ["key"] = "string (relative path under $Project::InputDirectory)",
                ["usageHint"] = "Returns textContent when possible. XLSX is auto-converted to CSV via Xlsx2CsvConverter."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                var inputFiles = project.InputFiles ?? new List<FlowBloxInputFile>();

                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

                FlowBloxInputFileHelper.ValidateRelativePathOrThrow(key);
                var normalizedKey = FlowBloxInputFileHelper.NormalizeRelativePath(key);

                var inputFile = inputFiles.FirstOrDefault(x =>
                    string.Equals(
                        FlowBloxInputFileHelper.NormalizeRelativePath(x?.RelativePath ?? string.Empty),
                        normalizedKey,
                        StringComparison.OrdinalIgnoreCase));

                if (inputFile == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Input file '{normalizedKey}' was not found."));

                var bytes = inputFile.ContentBytes ?? Array.Empty<byte>();
                var extension = Path.GetExtension(normalizedKey) ?? string.Empty;
                var mimeType = MimeTypeResolver.ResolveFromExtension(extension);

                var converter = string.Empty;
                var textContent = string.Empty;
                var message = string.Empty;

                if (bytes.Length == 0)
                {
                    message = "Input file is empty.";
                }
                else if (IsTextualMimeType(mimeType))
                {
                    textContent = DecodeText(bytes);
                }
                else if (IsXlsxMimeType(mimeType))
                {
                    try
                    {
                        textContent = Xlsx2CsvConverter.Convert(bytes);
                        converter = Xlsx2CsvConverter.Name;
                        message = "Binary spreadsheet was converted to CSV text.";
                    }
                    catch (Exception ex)
                    {
                        converter = Xlsx2CsvConverter.Name;
                        message =
                            $"Spreadsheet conversion to CSV failed. Legacy/binary Excel format may not be supported. Details: {ex.Message}";
                    }
                }
                else
                {
                    message =
                        "Content cannot be returned as text. Format is neither a textual data format nor a supported convertible format.";
                }

                var result = new JObject
                {
                    ["mimeType"] = mimeType,
                    ["key"] = normalizedKey,
                    ["textContent"] = textContent,
                    ["converter"] = converter,
                    ["message"] = message
                };

                return Task.FromResult(ToolHandlerUtilities.Ok(result));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }

        private static bool IsTextualMimeType(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                return false;

            if (mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(mimeType, "application/json", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/xml", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/javascript", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/yaml", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/x-ndjson", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsXlsxMimeType(string mimeType)
        {
            return string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || 
                   string.Equals(mimeType, "application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/vnd.ms-excel.sheet.macroEnabled.12", StringComparison.OrdinalIgnoreCase) || 
                   string.Equals(mimeType, "application/vnd.ms-excel.sheet.binary.macroEnabled.12", StringComparison.OrdinalIgnoreCase);
        }

        private static string DecodeText(byte[] bytes)
        {
            try
            {
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                    .GetString(bytes);
            }
            catch
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}



