using System.Security.Cryptography;
using System.Diagnostics;

namespace FlowBloxTest.FlowBlocks.AI
{
    internal static class OnnxRuntimeTestFileHelper
    {
        private static readonly TimeSpan CopyTimeout = TimeSpan.FromSeconds(10);

        public static void EnsureDirectory(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);
            foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory))
            {
                var targetFile = Path.Combine(targetDirectory, Path.GetFileName(sourceFile));
                if (FilesAreEqual(sourceFile, targetFile))
                    continue;

                CopyWithRetry(sourceFile, targetFile);
            }
        }

        private static bool FilesAreEqual(string sourceFile, string targetFile)
        {
            if (!File.Exists(targetFile))
                return false;

            if (new FileInfo(sourceFile).Length != new FileInfo(targetFile).Length)
                return false;

            using var sourceStream = File.OpenRead(sourceFile);
            using var targetStream = File.OpenRead(targetFile);
            return SHA256.HashData(sourceStream).SequenceEqual(SHA256.HashData(targetStream));
        }

        private static void CopyWithRetry(string sourceFile, string targetFile)
        {
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    File.Copy(sourceFile, targetFile, overwrite: true);
                    return;
                }
                catch (IOException) when (stopwatch.Elapsed < CopyTimeout)
                {
                    Thread.Sleep(250);
                }
                catch (IOException exception)
                {
                    throw new IOException(
                        $"Runtime file '{targetFile}' differs from the synchronized source and remained locked for " +
                        $"{CopyTimeout.TotalSeconds:0} seconds. Stop the process using the DLL or restart the test host, then retry.",
                        exception);
                }
            }
        }
    }
}
