using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Models.Project;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation
{
    public class ExtensionDependencyPresenceValidator
    {
        public ValidationResult Validate(
            FbExtensionResult selectedExtension,
            FbVersionResult selectedVersion,
            IEnumerable<FlowBloxProjectExtension> installedExtensions)
        {
            if (selectedVersion?.Dependencies == null || !selectedVersion.Dependencies.Any())
                return ValidationResult.Success;

            var missingDependencies = selectedVersion.Dependencies
                .Where(dep => installedExtensions.All(installed => installed.ExtensionGuid != dep.ExtensionGuid))
                .Select(dep => $"{dep.ExtensionName} ({dep.Version})")
                .ToList();

            if (!missingDependencies.Any())
                return ValidationResult.Success;

            var errorMessage =
                $"Cannot install extension '{selectedExtension?.Name}' because required dependencies are missing: {string.Join(", ", missingDependencies)}.";

            return new ValidationResult(errorMessage);
        }
    }
}
