using System.Reflection;

namespace FlowBlox.Core.Util
{
    /// <summary>
    /// Resolves FlowBlox.Core.RunnerHost from the directory of the executing assembly.
    /// Supports:
    /// - Self-contained exe (Windows)
    /// - Self-contained binary (Linux/macOS)
    /// - Framework-dependent dll (started via dotnet)
    /// </summary>
    public static class RunnerHostResolver
    {
        public sealed class RunnerHostCommand
        {
            public string FileName { get; }
            public string PrefixArguments { get; }

            public RunnerHostCommand(string fileName, string prefixArguments = "")
            {
                FileName = fileName;
                PrefixArguments = prefixArguments;
            }

            public string BuildArguments(string requestFile, string responseFile)
            {
                var args = $"--request \"{requestFile}\" --response \"{responseFile}\"";
                return string.IsNullOrWhiteSpace(PrefixArguments)
                    ? args
                    : $"{PrefixArguments} {args}";
            }
        }

        public static RunnerHostCommand Resolve()
        {
            var assemblyDir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrWhiteSpace(assemblyDir))
                throw new InvalidOperationException("Unable to determine executing assembly directory.");

            var exeWin = Path.Combine(assemblyDir, "FlowBlox.RunnerHost.exe");
            var exeUnix = Path.Combine(assemblyDir, "FlowBlox.RunnerHost");
            var dll = Path.Combine(assemblyDir, "FlowBlox.RunnerHost.dll");

            if (File.Exists(exeWin))
                return new RunnerHostCommand(exeWin);

            if (File.Exists(exeUnix))
                return new RunnerHostCommand(exeUnix);

            if (File.Exists(dll))
                return new RunnerHostCommand("dotnet", $"\"{dll}\"");

            throw new InvalidOperationException(
                $"FlowBlox.Core.RunnerHost not found in '{assemblyDir}'. " +
                "Ensure RunnerHost is deployed next to the Core assembly.");
        }
    }
}