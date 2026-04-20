using System;
using System.IO;
using System.Linq;
using System.Text;
using FlowBlox.Core.Util;

namespace FlowBlox.Core.Models.Project
{
    public static class FlowBloxInputFileHelper
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

        public static string ReplaceInputFilePlaceholders(string input, FlowBloxProject project, FlowBloxInputFile template)
        {
            if (input == null)
                return null;

            if (project == null || template == null)
                return input;

            var absolutePath = string.Empty;
            var relativePath = NormalizeRelativePath(template.RelativePath ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(relativePath) && !string.IsNullOrWhiteSpace(project.ProjectInputDirectory))
            {
                try
                {
                    absolutePath = BuildAbsoluteTargetPath(project.ProjectInputDirectory, relativePath);
                }
                catch
                {
                    absolutePath = string.Empty;
                }
            }

            var replaced = input;
            // Canonical single-colon placeholders.
            replaced = ReplaceOrdinalIgnoreCase(replaced, "$InputFile:Path", absolutePath);
            replaced = ReplaceOrdinalIgnoreCase(replaced, "$InputFile:RelativePath", relativePath);

            // Backward-compatible double-colon placeholders.
            replaced = ReplaceOrdinalIgnoreCase(replaced, "$InputFile::Path", absolutePath);
            replaced = ReplaceOrdinalIgnoreCase(replaced, "$InputFile::RelativePath", relativePath);
            return replaced;
        }

        private static string ReplaceOrdinalIgnoreCase(string input, string search, string replacement)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search))
                return input;

            var source = input;
            var index = source.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return source;

            var builder = new StringBuilder(source.Length + Math.Max(0, replacement?.Length ?? 0));
            var currentIndex = 0;
            var replacementSafe = replacement ?? string.Empty;

            while (index >= 0)
            {
                builder.Append(source, currentIndex, index - currentIndex);
                builder.Append(replacementSafe);
                currentIndex = index + search.Length;
                index = source.IndexOf(search, currentIndex, StringComparison.OrdinalIgnoreCase);
            }

            builder.Append(source, currentIndex, source.Length - currentIndex);
            return builder.ToString();
        }

        /// <summary>
        /// Synchronizes managed input files in the project input directory according to SyncMode.
        /// </summary>
        public static void EnsureInputFilesExist(FlowBloxProject project)
        {
            SynchronizeInputFiles(project);
        }

        /// <summary>
        /// Synchronizes all managed input files into the project input directory according to each file SyncMode.
        /// </summary>
        public static void SynchronizeInputFiles(FlowBloxProject project)
        {
            if (project == null)
                return;

            if (project.InputFiles == null || project.InputFiles.Count == 0)
                return;

            var inputDir = project.ProjectInputDirectory;
            if (string.IsNullOrWhiteSpace(inputDir))
                return;

            Directory.CreateDirectory(inputDir);

            foreach (var tpl in project.InputFiles)
            {
                if (tpl == null)
                    continue;

                if (string.IsNullOrWhiteSpace(tpl.RelativePath))
                    continue;

                var targetPath = BuildAbsoluteTargetPath(inputDir, tpl.RelativePath);
                var syncMode = tpl.SyncMode;

                if (File.Exists(targetPath) && syncMode != FlowBloxInputFileSyncMode.AlwaysOverwrite)
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



