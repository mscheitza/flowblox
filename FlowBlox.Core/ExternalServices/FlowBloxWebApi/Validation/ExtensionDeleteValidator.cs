using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Validation
{
    public class ExtensionDeleteValidator
    {
        private readonly FlowBloxRegistry _registry;
        private readonly ILogger _logger;

        public ExtensionDeleteValidator(FlowBloxRegistry registry, ILogger logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public ValidationResult Validate(string extensionName)
        {
            try
            {
                // 1. Lade die Assembly, die der Extension entspricht
                Assembly extensionAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals(extensionName, StringComparison.OrdinalIgnoreCase));

                if (extensionAssembly == null)
                {
                    return ValidationResult.Success; // Assembly wurde nicht gefunden, daher keine Referenzen
                }

                // 2. Sammle alle Typen aus der Assembly, die IFlowBloxComponent implementieren
                var extensionTypes = extensionAssembly.GetTypes()
                    .Where(t => typeof(IFlowBloxComponent).IsAssignableFrom(t))
                    .ToList();

                if (!extensionTypes.Any())
                {
                    return ValidationResult.Success; // Keine relevanten Typen, daher sicher zu löschen
                }

                // 3. Ermittle alle Flow-Blocks und ManagedObjects im Projekt-Repository
                var allFlowBlocks = _registry.GetFlowBlocks();
                var allManagedObjects = _registry.GetManagedObjects();

                // 4. Überprüfe auf Referenzen
                var conflicts = new List<string>();
                foreach (var obj in allFlowBlocks.Cast<IFlowBloxComponent>().Concat(allManagedObjects))
                {
                    var objectType = obj.GetType();
                    if (extensionTypes.Contains(objectType))
                    {
                        conflicts.Add($"Object '{obj.Name}' of type '{objectType.FullName}' is still in use.");
                    }
                }

                if (conflicts.Any())
                {
                    // Rückgabe eines ValidationResult mit den Konflikten als Fehler
                    return new ValidationResult(
                        $"Cannot delete extension because the following objects are still in use:\n{string.Join("\n", conflicts)}"
                    );
                }

                return ValidationResult.Success; // Alle Checks bestanden, Extension kann gelöscht werden
            }
            catch (Exception ex)
            {
                _logger.Error("Error validating extension delete operation.", ex);
                return new ValidationResult("An error occurred during validation. Check logs for more details.");
            }
        }
    }
}
