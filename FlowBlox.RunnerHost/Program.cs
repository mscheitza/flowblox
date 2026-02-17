using CommandLine;
using FlowBlox.Core.Runner;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.Runner.Serialization;

namespace FlowBlox.Core.RunnerHost
{
    internal class Program
    {
        private class Options
        {
            [Option("request", Required = true, HelpText = "Path to RunnerRequest JSON file.")]
            public string RequestFile { get; set; }

            [Option("response", Required = true, HelpText = "Path to RunnerResponse JSON file.")]
            public string ResponseFile { get; set; }
        }

        static int Main(string[] args)
        {
            int exitCode = RunnerExitCodes.UnknownError;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => exitCode = Run(opts))
                .WithNotParsed(_ =>
                {
                    Console.Error.WriteLine("Invalid arguments.");
                    exitCode = RunnerExitCodes.UnknownError;
                });

            return exitCode;
        }

        private static int Run(Options options)
        {
            try
            {
                if (!File.Exists(options.RequestFile))
                    throw new FileNotFoundException("Request file not found.", options.RequestFile);

                // Read request
                var request = RunnerJson.ReadFile<RunnerRequest>(options.RequestFile);

                if (request == null)
                    throw new InvalidOperationException("RunnerRequest could not be deserialized.");

                // Execute project
                var response = FlowBloxProjectRunner.Run(request);

                // Ensure response directory exists
                var responseDir = Path.GetDirectoryName(options.ResponseFile);
                if (!string.IsNullOrWhiteSpace(responseDir) && !Directory.Exists(responseDir))
                    Directory.CreateDirectory(responseDir);

                // Write response
                RunnerJson.WriteFile(options.ResponseFile, response);

                return response.ExitCode;
            }
            catch (Exception ex)
            {
                try
                {
                    // Try to write minimal failure response
                    var fallbackResponse = new RunnerResponse
                    {
                        Success = false,
                        ExitCode = RunnerExitCodes.RuntimeError,
                        ErrorMessage = ex.Message,
                        Exception = ex.ToString(),
                        StartedUtc = DateTime.UtcNow,
                        FinishedUtc = DateTime.UtcNow
                    };

                    RunnerJson.WriteFile(options.ResponseFile, fallbackResponse);
                }
                catch
                {
                    // ignore secondary failures
                }

                return RunnerExitCodes.RuntimeError;
            }
        }
    }
}
