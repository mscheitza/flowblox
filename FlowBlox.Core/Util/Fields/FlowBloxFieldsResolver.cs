using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Util.Fields
{
    public static class FlowBloxFieldsResolver
    {
        public static List<FieldElement> GetFieldsOrderedByExecutionFlow()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFieldElements(orderedByExecutionFlow: true)
                .ToList();
        }

        public static List<FieldElement> GetFieldsOrderedByExecutionFlowExcluding(BaseResultFlowBlock resultFlowBlock)
        {
            if (resultFlowBlock == null)
                return GetFieldsOrderedByExecutionFlow();

            return GetFieldsOrderedByExecutionFlow()
                .Where(x => !ReferenceEquals(x.Source, resultFlowBlock))
                .ToList();
        }

        public static List<FieldElement> GetFieldsOfAssociatedFlowBlocks(BaseFlowBlock flowBlock)
        {
            var result = new List<FieldElement>();
            foreach(var referencedFlowBlock in flowBlock?.ReferencedFlowBlocks ?? Enumerable.Empty<BaseFlowBlock>())
            {
                if (referencedFlowBlock is BaseResultFlowBlock resultFlowBlock)
                    result.AddRange(resultFlowBlock.Fields);
            }
            return result;
        }

        public static List<FieldElement> GetFieldsOrderedByReferencedFlowBlocksExcluding(BaseResultFlowBlock resultFlowBlock)
        {
            if (resultFlowBlock == null)
                return new List<FieldElement>();

            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetFieldElements(orderedByExecutionFlow: true)
                .Where(x => !ReferenceEquals(x.Source, resultFlowBlock))
                .OrderByDescending(x => resultFlowBlock.ReferencedFlowBlocks.Contains(x.Source))
                .ToList();
        }

        public static List<FieldElement> GetFieldsOrderedByReferencedFlowBlocks(BaseFlowBlock? referencedFlowBlock)
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFieldElements(orderedByExecutionFlow: true)
                .OrderByDescending(x => referencedFlowBlock?.ReferencedFlowBlocks.Contains(x.Source) == true)
                .ToList();
        }
    }
}
