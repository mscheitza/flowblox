using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Repository;
using FlowBlox.Core.Models.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation
{
    public class ExtensionCompatibilityValidator
    {
        private readonly ExtensionRepository _repository;

        public ExtensionCompatibilityValidator(ExtensionRepository repository)
        {
            _repository = repository;
        }

        public async Task<ValidationResult> ValidateAsync(
            FbExtensionResult selectedExtension,
            FbVersionResult selectedVersion,
            IEnumerable<FlowBloxProjectExtension> installedExtensions)
        {
            // Checking the compatibility of the version to be installed with the existing dependencies
            foreach (var installedExtension in installedExtensions)
            {
                foreach (var dependency in installedExtension.Dependencies)
                {
                    if (dependency.ExtensionGuid == selectedExtension.Guid)
                    {
                        var result = await ValidateVersionCompatibilityAsync(
                            dependency.Version, selectedVersion.Version, selectedExtension.Name, installedExtension.Name);
                        if (result != ValidationResult.Success)
                        {
                            return result;
                        }
                    }
                }
            }

            // Checking the compatibility of the dependencies of the version to be installed with the installed extensions
            foreach (var dependency in selectedVersion.Dependencies)
            {
                var installedDependency = installedExtensions.FirstOrDefault(e => e.ExtensionGuid == dependency.ExtensionGuid);
                if (installedDependency != null)
                {
                    var result = await ValidateVersionCompatibilityAsync(
                        dependency.Version, installedDependency.Version, dependency.ExtensionName, selectedExtension.Name);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
            }

            // Checking the compatibility of dependencies between the selected version and installed versions
            foreach (var installedExtension in installedExtensions)
            {
                foreach (var installedDependency in installedExtension.Dependencies)
                {
                    foreach (var selectedDependency in selectedVersion.Dependencies)
                    {
                        if (installedDependency.ExtensionGuid == selectedDependency.ExtensionGuid)
                        {
                            // Überprüfen der Versionen der beiden Abhängigkeiten
                            var result = await ValidateVersionCompatibilityAsync(
                                selectedDependency.Version, installedDependency.Version, installedDependency.ExtensionName, selectedExtension.Name);
                            if (result != ValidationResult.Success)
                            {
                                return result;
                            }

                            result = await ValidateVersionCompatibilityAsync(
                                installedDependency.Version, selectedDependency.Version, selectedDependency.ExtensionName, selectedExtension.Name);
                            if (result != ValidationResult.Success)
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            return ValidationResult.Success;
        }

        private async Task<ValidationResult> ValidateVersionCompatibilityAsync(
            string requiredVersion, string actualVersion, string dependencyName, string extensionName)
        {
            if (Version.Parse(actualVersion) < Version.Parse(requiredVersion))
            {
                string errorMessage = $"Cannot use version {actualVersion} of {dependencyName} because it is less than the required version {requiredVersion} for the dependency {extensionName}.";
                return new ValidationResult(errorMessage);
            }
            else if (Version.Parse(actualVersion) > Version.Parse(requiredVersion))
            {
                var extension = await _repository.GetExtensionByNameAsync(dependencyName);
                if (extension == null || !CheckBackwardCompatibilityAsync(extension, requiredVersion, actualVersion))
                {
                    string errorMessage = $"Cannot use version {actualVersion} of {dependencyName} because it is not fully backward compatible with version {requiredVersion} required by the dependency {extensionName}.";
                    return new ValidationResult(errorMessage);
                }
            }

            return ValidationResult.Success;
        }

        private bool CheckBackwardCompatibilityAsync(FbExtensionResult extension, string startVersion, string endVersion)
        {
            var versionsToCheck = extension.Versions
                .Where(v => Version.Parse(v.Version) > Version.Parse(startVersion) && Version.Parse(v.Version) <= Version.Parse(endVersion))
                .OrderBy(v => Version.Parse(v.Version));

            foreach (var version in versionsToCheck)
            {
                if (!version.BackwardsCompatible)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
