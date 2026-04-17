using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks.Additions;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestDefinitionDependencyFilter
    {
        internal static void FilterDependencies(List<IFlowBloxComponent> dependencies)
        {
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            if (!dependencies.Any())
                return;

            var containsNonTestDefinitions = dependencies.Any(x => x is not FlowBloxTestDefinition);
            if (!containsNonTestDefinitions)
                return;

            dependencies.RemoveAll(x => x is FlowBloxTestDefinition);
        }
    }
}