using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.ShellExecution;

namespace FlowBlox.Core.Models.Project
{
    /// <summary>
    /// Materializes managed input files, resolves command placeholders and executes an input-file command.
    /// </summary>
    public static class FlowBloxInputFileCommandExecutor
    {
        public static FlowBloxShellExecutionResult Execute(
            FlowBloxProject project,
            FlowBloxInputFile inputFile,
            CancellationToken cancellationToken = default,
            bool inputFileAlreadyEnsured = false,
            Action<string>? onCommandResolved = null)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(inputFile);

            var relativePath = FlowBloxInputFileHelper.NormalizeRelativePath(inputFile.RelativePath ?? string.Empty);
            var rawCommand = inputFile.Command ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawCommand))
                throw new InvalidOperationException($"Input file '{relativePath}' has no command configured.");

            if (!inputFileAlreadyEnsured)
                FlowBloxInputFileHelper.EnsureInputFileExists(project, inputFile);

            var resolvedCommand = FlowBloxInputFileHelper.ReplaceInputFilePlaceholders(rawCommand, project, inputFile);
            resolvedCommand = FlowBloxFieldHelper.ReplaceFieldsInString(resolvedCommand ?? string.Empty);
            if (string.IsNullOrWhiteSpace(resolvedCommand))
                throw new InvalidOperationException($"Resolved command is empty for input file '{relativePath}'.");

            onCommandResolved?.Invoke(resolvedCommand);

            return FlowBloxShellExecutor.Execute(new FlowBloxShellExecutionRequest
            {
                Command = resolvedCommand,
                WorkingDirectory = project.ProjectInputDirectory,
                CancellationToken = cancellationToken
            });
        }
    }
}
