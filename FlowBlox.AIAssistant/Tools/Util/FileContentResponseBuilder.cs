using Newtonsoft.Json.Linq;
using System.Text;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class FileContentResponseBuilder
    {
        public static JObject Build(byte[] bytes, string key, string fileKind, string logPath)
        {
            bytes ??= Array.Empty<byte>();
            var mimeType = MimeTypeResolver.ResolveFromExtension(Path.GetExtension(key) ?? string.Empty);
            var converter = string.Empty;
            var textContent = string.Empty;
            var message = string.Empty;

            if (bytes.Length == 0)
            {
                message = $"{fileKind} is empty.";
            }
            else if (IsTextualMimeType(mimeType))
            {
                textContent = DecodeText(bytes, fileKind, logPath);
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
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Failed to convert {fileKind.ToLowerInvariant()} '{logPath}' from Excel to CSV.",
                        ex);
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

            return new JObject
            {
                ["mimeType"] = mimeType,
                ["key"] = key,
                ["textContent"] = textContent,
                ["converter"] = converter,
                ["message"] = message
            };
        }

        private static bool IsTextualMimeType(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                return false;

            return mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/json", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/xml", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/javascript", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/yaml", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mimeType, "application/x-ndjson", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsXlsxMimeType(string mimeType) =>
            string.Equals(mimeType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mimeType, "application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mimeType, "application/vnd.ms-excel.sheet.macroEnabled.12", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mimeType, "application/vnd.ms-excel.sheet.binary.macroEnabled.12", StringComparison.OrdinalIgnoreCase);

        private static string DecodeText(byte[] bytes, string fileKind, string logPath)
        {
            try
            {
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                    .GetString(bytes);
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error(
                    $"{fileKind} '{logPath}' is not valid UTF-8; decoding with replacement characters.",
                    ex);
                return Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
