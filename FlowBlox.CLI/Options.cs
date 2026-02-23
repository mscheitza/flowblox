using CommandLine;

namespace FlowBlox.CLI
{
    public class Options
    {
        [Option('p', "project", Required = true, HelpText = "Path to the FlowBlox project file.")]
        public string ProjectFile { get; set; }

        [Option("project-space-guid", Required = false,
            HelpText = "Optional: Load project from Project Space by GUID instead of using the local project file content.")]
        public string ProjectSpaceGuid { get; set; }

        [Option("project-space-version", Required = false,
            HelpText = "Optional: Stable Project Space version number. Requires --project-space-guid.")]
        public int? ProjectSpaceVersion { get; set; }

        [Option('r', "restart", Required = false, HelpText = "Enable AutoRestart of the runtime.")]
        public bool Restart { get; set; }

        [Option('u', Separator = ' ', HelpText = "User field parameters. Format: FieldName=Value")]
        public IEnumerable<string> DynamicParameters { get; set; }

        [Option('o', "option", Separator = ' ', HelpText = "Override FlowBlox options. Format: OptionKey=Value")]
        public IEnumerable<string> OptionOverrides { get; set; }

        [Option('f', "outputfile", Required = false, HelpText = "Optional: Write runner response JSON to this file.")]
        public string OutputFile { get; set; }

        [Option("abort-on-error", Required = false, Default = true,
            HelpText = "Abort execution when a runtime error log message occurs. Default: true")]
        public bool AbortOnError { get; set; }

        [Option("abort-on-warning", Required = false, Default = false,
            HelpText = "Abort execution when a runtime warning log message occurs. Default: false")]
        public bool AbortOnWarning { get; set; }

        [Option("non-interactive", Default = false)]
        public bool NonInteractive { get; set; }
    }
}