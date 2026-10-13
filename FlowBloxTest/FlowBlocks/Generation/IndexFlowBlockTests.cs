using FlowBlox.Core.Models.FlowBlocks.Generation;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.FlowBlocks.Generation
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class IndexFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void Initialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void RuntimeStarted_ResetsIndexToInitialIndex()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var index = CreateFlowBlock<IndexFlowBlock>(start);
            index.InitialIndex = 5;

            CreateRuntimeAndExecute(_project);
            Assert.AreEqual("5", index.ResultField.StringValue);

            CreateRuntimeAndExecute(_project);
            Assert.AreEqual("5", index.ResultField.StringValue);
        }
    }
}
