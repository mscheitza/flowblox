using System;
using System.IO;
using System.Linq;

namespace FlowBlox.Core.Models.Project
{
    public static class FlowBloxInputTemplateHelper
    {
        /// <summary>
        /// Validates that the given relative path stays inside the project input directory.
        /// Prevents rooted paths and path traversal ("..").
        /// </summary>
        public static void ValidateRelativePathOrThrow(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new InvalidOperationException("RelativePath is empty.");

            if (Path.IsPathRooted(relativePath))
                throw new InvalidOperationException("RelativePath must not be rooted.");

            var normalized = relativePath.Replace('\\', '/').Trim('/');
            if (normalized.Length == 0)
                throw new InvalidOperationException("RelativePath is invalid.");

            var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(s => s == "." || s == ".."))
                throw new InvalidOperationException("RelativePath must not contain '.' or '..' segments.");
        }

        public static string BuildAbsoluteTargetPath(string projectInputDirectory, string relativePath)
        {
            ValidateRelativePathOrThrow(relativePath);

            if (string.IsNullOrWhiteSpace(projectInputDirectory))
                throw new InvalidOperationException("Project input directory is not configured.");

            var combined = Path.Combine(projectInputDirectory, relativePath);

            // Ensure the result is still under projectInputDirectory.
            var fullBase = Path.GetFullPath(projectInputDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var fullTarget = Path.GetFullPath(combined);

            if (!fullTarget.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("RelativePath escapes the project input directory.");

            return fullTarget;
        }

        /// <summary>
        /// Creates all input template files in the project input directory if they do not exist yet.
        /// </summary>
        public static void EnsureInputFilesExist(FlowBloxProject project)
        {
            if (project == null)
                return;

            if (project.InputTemplates == null || project.InputTemplates.Count == 0)
                return;

            var inputDir = project.ProjectInputDirectory;
            if (string.IsNullOrWhiteSpace(inputDir))
                return;

            Directory.CreateDirectory(inputDir);

            foreach (var tpl in project.InputTemplates)
            {
                if (tpl == null)
                    continue;

                if (string.IsNullOrWhiteSpace(tpl.RelativePath))
                    continue;

                var targetPath = BuildAbsoluteTargetPath(inputDir, tpl.RelativePath);

                if (File.Exists(targetPath))
                    continue;

                var parentDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(parentDir))
                    Directory.CreateDirectory(parentDir);

                var bytes = tpl.ContentBytes ?? Array.Empty<byte>();
                File.WriteAllBytes(targetPath, bytes);
            }
        }
    }
}
