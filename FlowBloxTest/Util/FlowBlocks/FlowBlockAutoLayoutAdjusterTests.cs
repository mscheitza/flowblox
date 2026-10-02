using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBloxTest.FlowBlocks;
using FlowBloxTest.FlowBlocks.Execution;
using System.Drawing;

namespace FlowBloxTest.Util.FlowBlocks
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBlockAutoLayoutAdjusterTests : FlowBloxTestsBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
        }

        [TestMethod]
        public void Adjust_ShiftsCompleteLayoutWhenCalculatedPositionIsNegative()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var children = Enumerable.Range(0, 3)
                .Select(_ => CreateFlowBlock<ExecutionOrderTestFlowBlock>(start))
                .ToList();
            var blocks = children.Cast<BaseFlowBlock>().Prepend(start).ToList();

            foreach (var block in blocks)
            {
                block.Location = new Point(60, 10);
                block.Size = new Size(328, 235);
            }

            FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.AreEqual(40, blocks.Min(x => x.Location.Y));
            Assert.IsTrue(blocks.All(x => x.Location.Y >= 0));
        }
    }
}
