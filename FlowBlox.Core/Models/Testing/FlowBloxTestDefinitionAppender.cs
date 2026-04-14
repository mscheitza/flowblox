using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestDefinitionAppender
    {
        private readonly Lazy<FlowBloxRegistry> _registry;

        public FlowBloxTestDefinitionAppender()
        {
            _registry = new Lazy<FlowBloxRegistry>(FlowBloxRegistryProvider.GetRegistry);
        }

        public void Append(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock)
        {
            FlowBloxTestCapture flowBloxCapture = new FlowBloxTestCapture();
            flowBloxCapture.CreateCapture(_registry.Value.GetStartFlowBlock(), currentFlowBlock);
            var capturedFlowBlocks = flowBloxCapture.GetCapturedFlowBlocks();

            AppendUserFields(testDefinition);
            AppendConfigurations(testDefinition, capturedFlowBlocks);
        }

        private void AppendUserFields(FlowBloxTestDefinition testDefinition)
        {
            foreach(var userField in _registry.Value.GetUserFields())
            {
                var entry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == null);
                if (entry == null)
                {
                    entry = new FlowBlockTestDataset()
                    {
                        ParentTestDefinition = testDefinition,
                        FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>()
                    };

                    testDefinition.Entries.Add(entry);
                }

                if (!entry.FlowBloxTestConfigurations.Any(x => x.FieldElement == userField))
                {
                    entry.FlowBloxTestConfigurations.Add(new FlowBloxFieldTestConfiguration()
                    {
                        FieldElement = userField
                    });
                }
            }
        }

        private void AppendConfigurations(FlowBloxTestDefinition testDefinition, List<BaseFlowBlock> capturedFlowBlocks)
        {
            int capturedFlowBlockIndex = 0;
            foreach (var flowBlock in capturedFlowBlocks)
            {
                var entry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == flowBlock);
                if (entry == null)
                {
                    entry = new FlowBlockTestDataset()
                    {
                        ParentTestDefinition = testDefinition,
                        FlowBlock = flowBlock,
                        Execute = capturedFlowBlockIndex == capturedFlowBlocks.Count - 1,
                        FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>()
                    };

                    testDefinition.Entries.Add(entry);
                }

                if (flowBlock is BaseResultFlowBlock)
                {
                    var resultFlowBlock = (BaseResultFlowBlock)flowBlock;
                    foreach (var field in resultFlowBlock.Fields)
                    {
                        if (!entry.FlowBloxTestConfigurations.Any(x => x.FieldElement == field))
                        {
                            entry.FlowBloxTestConfigurations.Add(new FlowBloxFieldTestConfiguration()
                            {
                                FieldElement = field
                            });
                        }
                    }
                }

                capturedFlowBlockIndex++;
            }
        }
    }
}

