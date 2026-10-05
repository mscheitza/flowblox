using FlowBlox.Core.Constants;

namespace FlowBlox.AIAssistant.Tools.Resolver
{
    internal static class DistributedDataCatalog
    {
        public const string PythonType = "python";
        public const string AuxiliaryProjectType = "auxiliary_project";

        private static readonly IReadOnlyDictionary<string, string> SupportedTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [PythonType] = ".py",
                [AuxiliaryProjectType] = ".fbprj"
            };

        public static IReadOnlyList<DistributedDataType> GetDataTypes() =>
            SupportedTypes
                .Select(x => new DistributedDataType(x.Key, FindFiles(x.Key, x.Value)))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

        public static string ReadContent(string dataType, string fileName)
        {
            var path = ResolvePath(dataType, fileName);
            if (path == null)
                throw new InvalidOperationException($"Distributed data file '{fileName}' was not found in type '{dataType}'.");

            return File.ReadAllText(path);
        }

        public static string? ResolvePath(string dataType, string fileName)
        {
            ValidateRequest(dataType, fileName);
            var type = SupportedTypes.FirstOrDefault(x =>
                string.Equals(x.Key, dataType, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(type.Key))
                return null;
            if (!string.Equals(Path.GetExtension(fileName), type.Value, StringComparison.OrdinalIgnoreCase))
                return null;

            return GetApplicationDataDirectories()
                .Select(x => Path.Combine(x, type.Key, fileName))
                .FirstOrDefault(File.Exists);
        }

        public static bool IsSupportedType(string dataType) =>
            SupportedTypes.ContainsKey(dataType ?? string.Empty);

        public static bool IsPlainName(string value) =>
            !string.IsNullOrWhiteSpace(value) &&
            string.Equals(value, Path.GetFileName(value), StringComparison.Ordinal) &&
            value.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) < 0 &&
            value is not "." and not "..";

        private static IReadOnlyList<string> FindFiles(string dataType, string extension) =>
            GetApplicationDataDirectories()
                .Select(x => Path.Combine(x, dataType))
                .Where(Directory.Exists)
                .SelectMany(x => Directory.EnumerateFiles(x, $"*{extension}", SearchOption.TopDirectoryOnly))
                .Select(Path.GetFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

        private static IReadOnlyList<string> GetApplicationDataDirectories()
        {
            var candidates = new List<string>
            {
                Path.Combine(GlobalPaths.CurrentDirectory, "data"),
                Path.Combine(AppContext.BaseDirectory, "data")
            };
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                candidates.Add(Path.Combine(directory.FullName, "FlowBlox", "ApplicationDir", "data"));
                directory = directory.Parent;
            }

            return candidates
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(Directory.Exists)
                .ToList();
        }

        private static void ValidateRequest(string dataType, string fileName)
        {
            if (!IsPlainName(dataType))
                throw new InvalidOperationException("dataType must be a plain data directory name.");
            if (!IsSupportedType(dataType))
                throw new InvalidOperationException(
                    $"Distributed data type '{dataType}' is not supported. Supported types: {string.Join(", ", SupportedTypes.Keys)}.");
            if (!IsPlainName(fileName) || string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
                throw new InvalidOperationException("fileName must be a plain filename with an extension and without a path.");
        }
    }

    internal sealed record DistributedDataType(string Name, IReadOnlyList<string> FileNames);
}
