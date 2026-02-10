using CommandLine;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;

namespace FlowBlox.CLI
{
    internal class Program
    {
        public class Options
        {
            [Option('p', "project", Required = true, HelpText = "Set path to project file.")]
            public string ProjectFile { get; set; }

            [Option('r', "restart", Required = false, HelpText = "Should the runtime restart automatically?")]
            public bool Restart { get; set; }

            // Dynamische Parameter werden als Dictionary erfasst
            [Option('u', Separator = ' ', HelpText = "User field parameters defined in project.")]
            public IEnumerable<string> DynamicParameters { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunWithOptions(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Error parsing arguments.");
            Environment.Exit(1);
        }

        private static void RunWithOptions(Options options)
        {
            Console.WriteLine($"Loading project from '{options.ProjectFile}'...");
            var project = FlowBloxProject.FromFile(options.ProjectFile);
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var parameterToValue = ParseDynamicParameters(options.DynamicParameters);

            List<string> warnings = new List<string>();
            foreach (var fieldElement in FlowBloxRegistryProvider.GetRegistry().GetUserFields(UserFieldTypes.Input))
            {
                if (parameterToValue.TryGetValue(fieldElement.Name, out string parameterStringValue))
                {
                    Console.WriteLine($"User field '{fieldElement.Name}' set to value '{parameterStringValue}'.");
                    fieldElement.StringValue = parameterStringValue;
                }
                else if (string.IsNullOrEmpty(fieldElement.StringValue))
                {
                    if (!string.IsNullOrEmpty(fieldElement.StringValue))
                        warnings.Add($"Warning: No value provided for '{fieldElement.Name}'.");
                    else
                        warnings.Add($"Warning: No value provided for '{fieldElement.Name}', current value '{fieldElement.StringValue}' will be used.");
                }
            }

            if (warnings.Count > 0)
            {
                Console.WriteLine("The following warnings were generated:");
                warnings.ForEach(Console.WriteLine);
                Console.WriteLine("Continue anyway? (y/n)");
                if (Console.ReadKey().KeyChar != 'y')
                    return;
            }

            if (options.Restart)
                Console.WriteLine("AutoRestart is enabled.");

            var runtime = new FlowBloxRuntime(project)
            {
                IsNoDesignerMode = true,
                AutoRestart = options.Restart
            };

            Console.WriteLine("Logfile path: " + runtime.GetLogfilePath());

            Task.Run(async () => await StartExecutionWithDynamicDisplay(runtime, project.ProjectName))
                .GetAwaiter()
                .GetResult();
        }

        private static async Task StartExecutionWithDynamicDisplay(FlowBloxRuntime runtime, string projectName)
        {
            Console.Write($"Execution of project '{projectName}' has started...");
            var symbols = new string[] { "/", "-", "\\", "|" };
            int index = 0;

            var runTask = Task.Run(() => runtime.Execute());

            while (!runTask.IsCompleted)
            {
                Console.Write($"\rExecution of project {projectName} has started... {symbols[index++ % symbols.Length]}");
                await Task.Delay(500);
            }

            Console.WriteLine($"\rExecution of project {projectName} has completed. Press any key to exit.");
            Console.ReadKey();
        }

        private static Dictionary<string, string> ParseDynamicParameters(IEnumerable<string> dynamicParameters)
        {
            var dict = new Dictionary<string, string>();
            foreach (var param in dynamicParameters)
            {
                var splitParam = param.Split(new char[] { '=' }, 2);
                if (splitParam.Length == 2)
                {
                    dict.Add(splitParam[0].Trim('\"'), splitParam[1].Trim('\"'));
                }
            }
            return dict;
        }
    }
}
