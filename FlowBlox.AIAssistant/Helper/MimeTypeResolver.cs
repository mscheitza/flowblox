namespace FlowBlox.AIAssistant.Helper
{
    internal static class MimeTypeResolver
    {
        private static readonly IReadOnlyDictionary<string, string> MimeByExtension =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".txt"] = "text/plain",
                [".log"] = "text/plain",
                [".csv"] = "text/csv",
                [".tsv"] = "text/tab-separated-values",
                [".json"] = "application/json",
                [".jsonl"] = "application/x-ndjson",
                [".ndjson"] = "application/x-ndjson",
                [".xml"] = "application/xml",
                [".xsd"] = "application/xml",
                [".xsl"] = "application/xml",
                [".xslt"] = "application/xml",
                [".html"] = "text/html",
                [".htm"] = "text/html",
                [".css"] = "text/css",
                [".js"] = "application/javascript",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                [".xls"] = "application/vnd.ms-excel",
                [".xlsm"] = "application/vnd.ms-excel.sheet.macroEnabled.12",
                [".xlsb"] = "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                [".pdf"] = "application/pdf",
                [".zip"] = "application/zip",
                [".gz"] = "application/gzip",
                [".7z"] = "application/x-7z-compressed",
                [".py"] = "text/x-python",
                [".md"] = "text/markdown",
                [".yaml"] = "application/yaml",
                [".yml"] = "application/yaml",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".gif"] = "image/gif",
                [".bmp"] = "image/bmp",
                [".webp"] = "image/webp",
                [".svg"] = "image/svg+xml"
            };

        public static string ResolveFromExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream";

            return MimeByExtension.TryGetValue(extension, out var mimeType)
                ? mimeType
                : "application/octet-stream";
        }
    }
}
