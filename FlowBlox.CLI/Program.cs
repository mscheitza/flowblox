using CommandLine;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Runner;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.Runner.Serialization;

namespace FlowBlox.CLI
{
    internal partial class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 1;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => exitCode = RunWithOptions(opts))
                .WithNotParsed(_ =>
                {
                    WriteColored("Error parsing arguments.", ConsoleColor.Red);
                    exitCode = 1;
                });

            return exitCode;
        }

        private static int RunWithOptions(Options options)
        {
            var request = new RunnerRequest
            {
                ProjectFile = options.ProjectFile,
                AutoRestart = options.Restart,
                NoDesignerMode = true,

                AbortOnError = options.AbortOnError,
                AbortOnWarning = options.AbortOnWarning,

                UserFields = ParseKeyValueArgs(options.DynamicParameters),
                OptionOverrides = ParseKeyValueArgs(options.OptionOverrides)
            };

            void OnRunnerLog(RunnerLogMessage msg)
            {
                var color = GetColorForLogLevel(msg.LogLevel);
                WriteColored($"[{msg.UtcTimestamp:HH:mm:ss}] {msg.LogLevel}: {msg.Message}", color);
            }

            FlowBloxProjectRunner.LogMessageCreated += OnRunnerLog;

            try
            {
                WriteColored($"Starting execution of project '{Path.GetFileName(options.ProjectFile)}'...", ConsoleColor.Gray);

                var response = FlowBloxProjectRunner.Run(request);

                if (!string.IsNullOrWhiteSpace(options.OutputFile))
                {
                    RunnerJson.WriteFile(options.OutputFile, response);
                    WriteColored($"Runner response written to: {options.OutputFile}", ConsoleColor.DarkGray);
                }

                if (!response.Success)
                {
                    WriteColored($"Execution failed (ExitCode={response.ExitCode}).",
                        ConsoleColor.Red);

                    if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                        WriteColored(response.ErrorMessage, ConsoleColor.Red);

                    if (!options.NonInteractive)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }

                    return response.ExitCode;
                }

                WriteColored($"Execution completed successfully (ExitCode={response.ExitCode}).",
                    ConsoleColor.Green);

                if (!options.NonInteractive)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                }

                return response.ExitCode;
            }
            finally
            {
                FlowBloxProjectRunner.LogMessageCreated -= OnRunnerLog;
            }
        }

        private static ConsoleColor GetColorForLogLevel(FlowBloxLogLevel level)
        {
            return level switch
            {
                FlowBloxLogLevel.Error => ConsoleColor.Red,
                FlowBloxLogLevel.Warning => ConsoleColor.DarkYellow,
                FlowBloxLogLevel.Info => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }

        private static void WriteColored(string message, ConsoleColor color)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }

        private static Dictionary<string, string> ParseKeyValueArgs(IEnumerable<string> args)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (args == null)
                return dict;

            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                var split = arg.Split(new[] { '=' }, 2);
                if (split.Length != 2)
                    continue;

                var key = split[0].Trim().Trim('\"');
                var value = split[1].Trim().Trim('\"');

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                dict[key] = value;
            }

            return dict;
        }
    }
}
