using FlowBlox.Core.Models.Project;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class ContainedFileResolver
    {
        public static string ResolveExistingFile(string baseDirectory, string relativePath)
        {
            FlowBloxInputFileHelper.ValidateRelativePathOrThrow(relativePath);
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new InvalidOperationException("Project output directory is not configured.");

            var fullBaseDirectory = Path.GetFullPath(baseDirectory);
            var normalizedRelativePath = FlowBloxInputFileHelper
                .NormalizeRelativePath(relativePath)
                .Replace('/', Path.DirectorySeparatorChar);
            var candidate = Path.GetFullPath(Path.Combine(fullBaseDirectory, normalizedRelativePath));

            EnsureContained(fullBaseDirectory, candidate);
            if (!File.Exists(candidate))
                throw new FileNotFoundException($"Output file '{FlowBloxInputFileHelper.NormalizeRelativePath(relativePath)}' was not found.");

            var resolvedBaseDirectory = ResolveLinks(fullBaseDirectory);
            var resolvedCandidate = ResolveLinks(candidate);
            EnsureContained(resolvedBaseDirectory, resolvedCandidate);
            return resolvedCandidate;
        }

        private static void EnsureContained(string baseDirectory, string candidate)
        {
            var normalizedBase = Path.TrimEndingDirectorySeparator(Path.GetFullPath(baseDirectory));
            var normalizedCandidate = Path.GetFullPath(candidate);
            var basePrefix = normalizedBase + Path.DirectorySeparatorChar;

            if (!normalizedCandidate.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Relative path resolves outside the project output directory.");
        }

        private static string ResolveLinks(string path) => ResolveLinks(path, depth: 0);

        private static string ResolveLinks(string path, int depth)
        {
            if (depth > 64)
                throw new IOException("Too many symbolic-link levels while resolving the output file path.");

            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath)
                ?? throw new InvalidOperationException($"Path '{path}' has no filesystem root.");
            var current = root;
            var relative = Path.GetRelativePath(root, fullPath);

            foreach (var segment in relative.Split(
                         [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                         StringSplitOptions.RemoveEmptyEntries))
            {
                var next = Path.Combine(current, segment);
                FileSystemInfo info = Directory.Exists(next)
                    ? new DirectoryInfo(next)
                    : new FileInfo(next);
                var target = info.Exists ? info.ResolveLinkTarget(returnFinalTarget: true) : null;
                current = target == null
                    ? next
                    : ResolveLinks(target.FullName, depth + 1);
            }

            return Path.GetFullPath(current);
        }
    }
}
