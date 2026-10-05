namespace FlowBlox.Core.Util
{
    public static class FlowBloxProjectFileNameHelper
    {
        public const string ProjectFileExtension = ".fbprj";

        public static string FromProjectName(string? projectName)
        {
            var safeProjectName = IOUtil.GetValidFileName(projectName ?? string.Empty).Trim('_');
            return EnsureExtension(string.IsNullOrWhiteSpace(safeProjectName) ? "Project" : safeProjectName);
        }

        public static string EnsureExtension(string? fileNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrPath))
                return string.Empty;

            return string.IsNullOrWhiteSpace(Path.GetExtension(fileNameOrPath))
                ? fileNameOrPath + ProjectFileExtension
                : fileNameOrPath;
        }
    }
}
