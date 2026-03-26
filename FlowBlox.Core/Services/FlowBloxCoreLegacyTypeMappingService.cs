using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Serialization;

namespace FlowBlox.Core.Services
{
    public class FlowBloxCoreLegacyTypeMappingService : IFlowBloxLegacyTypeMappingService
    {
        public IEnumerable<FlowBloxLegacyTypeMapping> GetLegacyTypeMappings()
        {
            return
            [
                new FlowBloxLegacyTypeMapping(
                    typeof(ComparisonCondition),
                    [
                        "FlowBlox.Core.Models.FlowBlocks.Additions.Condition, FlowBlox.Core"
                    ]),
                new FlowBloxLegacyTypeMapping(
                    typeof(FieldComparisonCondition),
                    [
                        "FlowBlox.Core.Models.FlowBlocks.Additions.FieldCondition, FlowBlox.Core"
                    ]),
                new FlowBloxLegacyTypeMapping(
                    typeof(LogicalGroupCondition),
                    [
                        "FlowBlox.Core.Models.FlowBlocks.Additions.SummarizationCondition, FlowBlox.Core"
                    ]),
                new FlowBloxLegacyTypeMapping(
                    typeof(RecursiveCallFlowBlock),
                    [
                        "FlowBlox.Core.Models.FlowBlocks.InvokerFlowBlock, FlowBlox.Core"
                    ])
            ];
        }
    }
}
