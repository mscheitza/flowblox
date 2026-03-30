using System;
using System.IO;
using System.Linq;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.Project
{
    public static class FlowBloxInputTemplateHelper
    {
        public static string NormalizeRelativePath(string relativePath)
        {
            if (relativePath == null)
                return string.Empty;

            return relativePath.Replace('\\', '/').Trim('/');
        }

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

            var normalized = NormalizeRelativePath(relativePath);
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

            var normalizedRelativePath = NormalizeRelativePath(relativePath);
            var combined = Path.Combine(projectInputDirectory, normalizedRelativePath);

            // Ensure the result is still under projectInputDirectory.
            var fullBase = IOUtil.EnsureTrailingDirectorySeparator(IOUtil.NormalizePath(projectInputDirectory, trimTrailingDirectorySeparator: true));
            var fullTarget = IOUtil.NormalizePath(combined);

            if (!fullTarget.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("RelativePath escapes the project input directory.");

            return fullTarget;
        }

        /// <summary>
        /// Synchronizes input template files in the project input directory according to SyncMode.
        /// </summary>
        public static void EnsureInputFilesExist(FlowBloxProject project)
        {
            SynchronizeInputFiles(project);
        }

        /// <summary>
        /// Synchronizes all input templates into the project input directory according to each template SyncMode.
        /// </summary>
        public static void SynchronizeInputFiles(FlowBloxProject project)
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
                var syncMode = tpl.SyncMode;

                if (File.Exists(targetPath) && syncMode != FlowBloxInputTemplateSyncMode.AlwaysOverwrite)
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
