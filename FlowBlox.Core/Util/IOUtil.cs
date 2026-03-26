using FlowBlox.Core.Extensions;

namespace FlowBlox.Core.Util
{
    public static class IOUtil
    {
        /// <summary>
        /// Returns a list of file names (not full paths) matching the given filter in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="fileFilter">The file search pattern (e.g., "*.dll"). Defaults to "*.*".</param>
        /// <returns>List of file names (without path) in the given directory.</returns>
        public static List<string> GetFilesInDirectory(string directory, string fileFilter = "*.*")
        {
            List<string> result = new List<string>();

            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] rgFiles = di.GetFiles(fileFilter);
            foreach (FileInfo fi in rgFiles)
            {
                result.Add(fi.Name);
            }

            return result;
        }

        /// <summary>
        /// Recursively retrieves all file paths (relative to the initial base directory) that match the given filter.
        /// </summary>
        /// <param name="directory">The base directory to search recursively.</param>
        /// <param name="fileFilter">The file search pattern (e.g., "*.dll"). Defaults to "*.*".</param>
        /// <param name="parentDirectory">
        /// The relative path accumulated during recursion. Leave empty when calling externally. 
        /// This is used internally to build the relative paths.
        /// </param>
        /// <returns>
        /// A list of relative file paths (using backslashes), based on the initial <paramref name="directory"/>.
        /// </returns>
        public static List<string> GetFilesInDirectoryRecursive(string directory, string fileFilter = "*.*", string parentDirectory = "")
        {
            var result = new List<string>();

            var filesInDirectory = GetFilesInDirectory(directory, fileFilter);
            result.AddRange(filesInDirectory.Select(x => string.Join("\\", new string[] { parentDirectory, x }.ExceptNullOrEmpty())));

            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                var subDirectoryName = Path.GetFileName(subDirectory);
                var newParentDirectory = string.Join("\\", new string[] { parentDirectory, subDirectoryName }.ExceptNullOrEmpty());
                result.AddRange(GetFilesInDirectoryRecursive(subDirectory, fileFilter, newParentDirectory));
            }

            return result;
        }


        public static string GetValidFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars().Concat([' ', '.']))
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        public static void CopyDirectory(string source, string target)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();

                if (!Directory.Exists(folders.Target))
                    Directory.CreateDirectory(folders.Target);

                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    File.Copy(file, Path.Combine(folders.Target, Path.GetFileName(file)), true);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
        }

        public static string NormalizePath(string path, bool trimTrailingDirectorySeparator = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var replacedSeparators = path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .Trim();

            string normalizedPath;
            try
            {
                normalizedPath = Path.GetFullPath(replacedSeparators);
            }
            catch
            {
                normalizedPath = replacedSeparators;
            }

            return trimTrailingDirectorySeparator
                ? normalizedPath.TrimEnd(Path.DirectorySeparatorChar)
                : normalizedPath;
        }

        public static string EnsureTrailingDirectorySeparator(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return directoryPath;

            return directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? directoryPath
                : directoryPath + Path.DirectorySeparatorChar;
        }
    }
}
