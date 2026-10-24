using Newtonsoft.Json.Linq;
using System.Text;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class FileContentResponseBuilder
    {
        public static JObject Build(
            byte[] bytes,
            string key,
            string fileKind,
            string logPath,
            bool textConversion,
            FieldValueResponseLimiter limiter)
        {
            bytes ??= Array.Empty<byte>();
            ArgumentNullException.ThrowIfNull(limiter);
            var mimeType = MimeTypeResolver.ResolveFromExtension(Path.GetExtension(key) ?? string.Empty);
            var converter = string.Empty;
            var textContent = string.Empty;
            var base64Content = string.Empty;
            var message = string.Empty;

            if (bytes.Length == 0)
            {
                message = $"{fileKind} is empty.";
            }
            else if (!textConversion)
            {
                base64Content = Convert.ToBase64String(bytes);
                message = "Text conversion was disabled; content is returned as Base64.";
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
                    base64Content = Convert.ToBase64String(bytes);
                    message = $"Spreadsheet conversion to CSV failed; content is returned as Base64. Details: {ex.Message}";
                }
            }
            else if (IsDocxMimeType(mimeType))
            {
                try
                {
                    textContent = Docx2TextConverter.Convert(bytes);
                    converter = Docx2TextConverter.Name;
                    message = "DOCX content was converted to page-oriented text including headers, body, and footers.";
                }
                catch (Exception ex)
                {
                    FlowBloxLogManager.Instance.GetLogger().Error(
                        $"Failed to convert {fileKind.ToLowerInvariant()} '{logPath}' from DOCX to text.",
                        ex);
                    converter = Docx2TextConverter.Name;
                    base64Content = Convert.ToBase64String(bytes);
                    message = $"DOCX conversion to text failed; content is returned as Base64. Details: {ex.Message}";
                }
            }
            else
            {
                base64Content = Convert.ToBase64String(bytes);
                message = "No text converter is available for this format; content is returned as Base64.";
            }

            var content = !string.IsNullOrEmpty(textContent) ? textContent : base64Content;
            var limitedContent = limiter.Limit(content);
            if (!limitedContent.IsComplete)
            {
                if (!string.IsNullOrEmpty(textContent))
                    textContent = limitedContent.Value;
                else
                    base64Content = limitedContent.Value;
                message += " Content was truncated to the configured response limit.";
            }

            return new JObject
            {
                ["mimeType"] = mimeType,
                ["key"] = key,
                ["textContent"] = textContent,
                ["base64Content"] = base64Content,
                ["contentEncoding"] = string.IsNullOrEmpty(base64Content)
                    ? "text"
                    : limitedContent.IsComplete ? "base64" : "base64-prefix",
                ["converter"] = converter,
                ["message"] = message,
                ["contentInfo"] = limitedContent.ToMetadata(),
                ["fileContentOutput"] = limiter.CreateMetadata()
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

        private static bool IsDocxMimeType(string mimeType) =>
            string.Equals(
                mimeType,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                StringComparison.OrdinalIgnoreCase);

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
