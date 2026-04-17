using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.Core.Util
{
    public class FlowBloxTestDeletionEnsurer
    {
        public bool EnsureFieldElementDeletable(FieldElement fieldElement, List<IFlowBloxComponent> dependencies)
        {
            if (fieldElement == null)
                throw new ArgumentNullException(nameof(fieldElement));

            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            if (!dependencies.Any())
                return true;

            if (dependencies.Any(x => x is not FlowBloxTestDefinition))
                return false;

            var testDefinitions = dependencies
                .OfType<FlowBloxTestDefinition>()
                .ToList();

            foreach (var testDefinition in testDefinitions)
            {
                if (TryRemoveFieldConfigurationsFromTestDefinition(testDefinition, fieldElement))
                {
                    dependencies.Remove(testDefinition);
                }
            }

            return !dependencies.Any();
        }

        public bool EnsureFlowBlockDeletable(BaseFlowBlock flowBlock, List<IFlowBloxComponent> dependencies)
        {
            if (flowBlock == null)
                throw new ArgumentNullException(nameof(flowBlock));

            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            if (!dependencies.Any())
                return true;

            if (dependencies.Any(x => x is not FlowBloxTestDefinition))
                return false;

            var testDefinitions = dependencies
                .OfType<FlowBloxTestDefinition>()
                .ToList();

            foreach (var testDefinition in testDefinitions)
            {
                if (TryRemoveFlowBlockDatasetsFromTestDefinition(testDefinition, flowBlock))
                {
                    dependencies.Remove(testDefinition);
                }
            }

            return !dependencies.Any();
        }

        private static bool TryRemoveFieldConfigurationsFromTestDefinition(
            FlowBloxTestDefinition testDefinition,
            FieldElement fieldElement)
        {
            if (testDefinition == null)
                throw new ArgumentNullException(nameof(testDefinition));

            if (fieldElement == null)
                throw new ArgumentNullException(nameof(fieldElement));

            var fullyQualifiedName = fieldElement.FullyQualifiedName;

            foreach (var entry in testDefinition.Entries.ToList())
            {
                var configurationsToRemove = entry.FlowBloxTestConfigurations
                    .Where(x =>
                        x.FieldElement != null &&
                        string.Equals(x.FieldElement.FullyQualifiedName, fullyQualifiedName, StringComparison.Ordinal))
                    .ToList();

                foreach (var configuration in configurationsToRemove)
                {
                    entry.FlowBloxTestConfigurations.Remove(configuration);
                }
            }

            return !testDefinition.Entries
                .SelectMany(x => x.FlowBloxTestConfigurations)
                .Any(x =>
                    x.FieldElement != null &&
                    string.Equals(x.FieldElement.FullyQualifiedName, fullyQualifiedName, StringComparison.Ordinal));
        }

        private static bool TryRemoveFlowBlockDatasetsFromTestDefinition(
            FlowBloxTestDefinition testDefinition,
            BaseFlowBlock flowBlock)
        {
            if (testDefinition == null)
                throw new ArgumentNullException(nameof(testDefinition));

            if (flowBlock == null)
                throw new ArgumentNullException(nameof(flowBlock));

            var entriesToRemove = testDefinition.Entries
                .Where(x => x.FlowBlock == flowBlock)
                .ToList();

            foreach (var entry in entriesToRemove)
            {
                testDefinition.Entries.Remove(entry);
            }

            return !testDefinition.Entries.Any(x => x.FlowBlock == flowBlock);
        }
    }
}