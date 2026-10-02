using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Constants;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetPublishedScriptContentHandler : ToolHandlerBase
    {
        private const string PythonScriptType = "python";

        public override string Name => "GetPublishedScriptContent";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns a published FlowBlox setup script from data. Currently only Python scripts are supported.",
            new JObject
            {
                ["scriptType"] = "string (allowed: python)",
                ["scriptName"] = "string (published .py filename)"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var scriptType = (args.Value<string>("scriptType") ?? string.Empty).Trim();
            if (!string.Equals(scriptType, PythonScriptType, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    "scriptType must be 'python'.",
                    new JObject { ["allowedScriptTypes"] = new JArray(PythonScriptType) }));
            }

            var scriptName = (args.Value<string>("scriptName") ?? string.Empty).Trim();
            if (!IsSafePythonFileName(scriptName))
                return Task.FromResult(ToolHandlerUtilities.Fail("scriptName must be a plain .py filename without a path."));

            var scriptDirectory = FindScriptDirectory(PythonScriptType);
            if (scriptDirectory == null)
                return Task.FromResult(ToolHandlerUtilities.Fail("The published Python script directory could not be found."));

            var availableScripts = Directory
                .EnumerateFiles(scriptDirectory, "*.py", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var resolvedName = availableScripts.FirstOrDefault(x =>
                string.Equals(x, scriptName, StringComparison.OrdinalIgnoreCase));
            if (resolvedName == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Published Python script '{scriptName}' was not found.",
                    new JObject { ["availableScriptNames"] = new JArray(availableScripts) }));
            }

            var content = File.ReadAllText(Path.Combine(scriptDirectory, resolvedName));
            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["scriptType"] = PythonScriptType,
                ["scriptName"] = resolvedName,
                ["content"] = content
            }));
        }

        private static bool IsSafePythonFileName(string scriptName) =>
            !string.IsNullOrWhiteSpace(scriptName) &&
            string.Equals(scriptName, Path.GetFileName(scriptName), StringComparison.Ordinal) &&
            string.Equals(Path.GetExtension(scriptName), ".py", StringComparison.OrdinalIgnoreCase) &&
            scriptName.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) < 0;

        private static string? FindScriptDirectory(string scriptType)
        {
            var candidates = new List<string>
            {
                Path.Combine(GlobalPaths.CurrentDirectory, "data", scriptType),
                Path.Combine(AppContext.BaseDirectory, "data", scriptType)
            };

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                candidates.Add(Path.Combine(
                    directory.FullName,
                    "FlowBlox",
                    "ApplicationDir",
                    "data",
                    scriptType));
                directory = directory.Parent;
            }

            return candidates
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(Directory.Exists);
        }
    }
}
