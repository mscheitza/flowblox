using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    public class UpdateFlowBlockSelectionFilterTests
    {
        [TestMethod]
        public async Task UpdateFlowBlock_RejectsFieldReferenceOutsideSelectionFilter()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var registry = project.FlowBloxRegistry;
            var directPredecessor = CreateFlowBlock<ConcatUriFlowBlock>(registry);
            var unrelatedSource = CreateFlowBlock<ConcatUriFlowBlock>(registry);
            var pipe = CreateFlowBlock<SplitFlowBlock>(registry);
            pipe.ReferencedFlowBlocks.Add(directPredecessor);

            var handler = new UpdateFlowBlockHandler();
            var response = await handler.HandleAsync(
                new JObject
                {
                    ["name"] = pipe.Name,
                    ["path"] = "/InputField",
                    ["value"] = new JObject
                    {
                        ["resolveFieldElementByFQName"] = unrelatedSource.ResultField.FullyQualifiedName
                    }
                },
                CancellationToken.None);

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "not selectable");
            StringAssert.Contains(response.Error, "BasePipeFlowBlock");
            StringAssert.Contains(response.Error, directPredecessor.ResultField.FullyQualifiedName);
        }

        [TestMethod]
        public async Task UpdateFlowBlock_AllowsFieldReferenceInsideSelectionFilter()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var registry = project.FlowBloxRegistry;
            var directPredecessor = CreateFlowBlock<ConcatUriFlowBlock>(registry);
            var pipe = CreateFlowBlock<SplitFlowBlock>(registry);
            pipe.ReferencedFlowBlocks.Add(directPredecessor);

            var handler = new UpdateFlowBlockHandler();
            var response = await handler.HandleAsync(
                new JObject
                {
                    ["name"] = pipe.Name,
                    ["path"] = "/InputField",
                    ["value"] = new JObject
                    {
                        ["resolveFieldElementByFQName"] = directPredecessor.ResultField.FullyQualifiedName
                    }
                },
                CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreSame(directPredecessor.ResultField, pipe.InputField);
        }

        private static T CreateFlowBlock<T>(FlowBlox.Core.Provider.Registry.FlowBloxRegistry registry)
            where T : FlowBlox.Core.Models.FlowBlocks.Base.BaseFlowBlock
        {
            var flowBlock = registry.CreateFlowBlockUnregistered<T>();
            registry.PostProcessFlowBlockCreated(flowBlock);
            registry.Register(flowBlock);
            return flowBlock;
        }
    }
}