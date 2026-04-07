using System.Diagnostics;
using System.Text;

namespace FlowBlox.Core.Util.ShellExecution
{
    public sealed class FlowBloxShellExecutionRequest
    {
        public string Command { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public int? TimeoutMilliseconds { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public Action<string>? OnStandardOutputLine { get; set; }
        public Action<string>? OnStandardErrorLine { get; set; }
    }

    public sealed class FlowBloxShellExecutionResult
    {
        public string Command { get; init; } = string.Empty;
        public string WorkingDirectory { get; init; } = string.Empty;
        public int ExitCode { get; init; }
        public bool TimedOut { get; init; }
        public bool WasCancelled { get; init; }
        public string StandardOutput { get; init; } = string.Empty;
        public string StandardError { get; init; } = string.Empty;
        public string ExceptionMessage { get; init; } = string.Empty;
        public bool Success { get; init; }
    }

    public static class FlowBloxShellExecutor
    {
        public static FlowBloxShellExecutionResult Execute(FlowBloxShellExecutionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = request.Command?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidOperationException("No command was provided.");

            var workingDirectory = string.IsNullOrWhiteSpace(request.WorkingDirectory)
                ? Environment.CurrentDirectory
                : request.WorkingDirectory;

            var (hostFileName, hostArguments) = ResolveShellHost(command);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = hostFileName,
                Arguments = hostArguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();
            var exceptionMessage = string.Empty;
            var exitCode = -1;
            var timedOut = false;
            var wasCancelled = false;

            try
            {
                using var process = new Process { StartInfo = processStartInfo };
                if (!process.Start())
                    throw new InvalidOperationException("The command process could not be started.");

                process.OutputDataReceived += (_, args) =>
                {
                    if (args.Data == null)
                        return;

                    lock (stdoutBuilder)
                    {
                        stdoutBuilder.AppendLine(args.Data);
                    }

                    request.OnStandardOutputLine?.Invoke(args.Data);
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data == null)
                        return;

                    lock (stderrBuilder)
                    {
                        stderrBuilder.AppendLine(args.Data);
                    }

                    request.OnStandardErrorLine?.Invoke(args.Data);
                };

                using var cancellationRegistration = request.CancellationToken.Register(() =>
                {
                    wasCancelled = true;
                    TryKillProcess(process);
                });

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (request.TimeoutMilliseconds.HasValue && request.TimeoutMilliseconds.Value > 0)
                {
                    if (!process.WaitForExit(request.TimeoutMilliseconds.Value))
                    {
                        timedOut = true;
                        TryKillProcess(process);
                        process.WaitForExit();
                    }
                }
                else
                {
                    process.WaitForExit();
                }

                exitCode = process.ExitCode;

                if (request.CancellationToken.IsCancellationRequested)
                    wasCancelled = true;
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            var success = !timedOut
                          && !wasCancelled
                          && string.IsNullOrWhiteSpace(exceptionMessage)
                          && exitCode == 0;

            return new FlowBloxShellExecutionResult
            {
                Command = command,
                WorkingDirectory = workingDirectory,
                ExitCode = exitCode,
                TimedOut = timedOut,
                WasCancelled = wasCancelled,
                StandardOutput = stdoutBuilder.ToString(),
                StandardError = stderrBuilder.ToString(),
                ExceptionMessage = exceptionMessage,
                Success = success
            };
        }

        private static (string FileName, string Arguments) ResolveShellHost(string command)
        {
            if (OperatingSystem.IsWindows())
            {
                return ("cmd.exe", $"/C {command}");
            }

            var escaped = command
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);

            return ("/bin/bash", $"-lc \"{escaped}\"");
        }

        private static void TryKillProcess(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Intentionally ignored: best effort during cancellation/timeout.
            }
        }
    }
}
