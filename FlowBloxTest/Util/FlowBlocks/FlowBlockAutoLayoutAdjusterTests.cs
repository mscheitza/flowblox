using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.ControlFlow.Recursion;
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

        [TestMethod]
        public void Adjust_PlacesSingleRecursiveSuccessorBelowItsPredecessor()
        {
            var predecessor = CreateFlowBlock<StartFlowBlock>();
            var recursiveCall = CreateFlowBlock<RecursiveCallFlowBlock>(predecessor);
            var blocks = new BaseFlowBlock[] { predecessor, recursiveCall };

            foreach (var block in blocks)
            {
                block.Location = new Point(100, 100);
                block.Size = new Size(328, 235);
            }

            FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.AreEqual(predecessor.Location.X, recursiveCall.Location.X);
            Assert.AreEqual(
                predecessor.Location.Y + predecessor.Size.Height + 40,
                recursiveCall.Location.Y);
        }

        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void Adjust_SeparatesTallDownstreamBranches(bool branchesMerge, bool sharedTailBranches)
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var source = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var firstReader = CreateFlowBlock<ExecutionOrderTestFlowBlock>(source);
            var secondReader = CreateFlowBlock<ExecutionOrderTestFlowBlock>(source);
            var firstWriter = CreateFlowBlock<ExecutionOrderTestFlowBlock>(firstReader);
            var secondWriter = CreateFlowBlock<ExecutionOrderTestFlowBlock>(secondReader);
            var output = CreateFlowBlock<ExecutionOrderTestFlowBlock>(firstWriter);
            if (branchesMerge)
                output.ReferencedFlowBlocks.Add(secondWriter);
            var fileWriter = CreateFlowBlock<ExecutionOrderTestFlowBlock>(output);

            // Preserve the block order, sizes and initial positions from the reported trace.
            var blocks = new BaseFlowBlock[]
            {
                firstWriter, firstReader, fileWriter, secondWriter,
                source, output, start, secondReader
            };
            var locations = new Point[]
            {
                new(1284, 347), new(876, 420), new(2100, 511), new(1284, 530),
                new(468, 541), new(1692, 547), new(60, 553), new(876, 614)
            };
            var heights = new[] { 300, 154, 154, 298, 82, 82, 58, 130 };
            for (var i = 0; i < blocks.Length; i++)
            {
                blocks[i].Location = locations[i];
                blocks[i].Size = new Size(328, heights[i]);
            }

            ExecutionOrderTestFlowBlock otherOutput = null;
            if (sharedTailBranches)
            {
                otherOutput = CreateFlowBlock<ExecutionOrderTestFlowBlock>(output);
                otherOutput.Location = new Point(2100, 700);
                otherOutput.Size = new Size(328, 90);
                blocks = blocks.Append(otherOutput).ToArray();
            }

            FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.IsTrue(secondWriter.Location.Y >= firstWriter.Location.Y + firstWriter.Size.Height + 40);
            Assert.AreEqual(CenterY(firstReader), CenterY(firstWriter), 1.0);
            Assert.AreEqual(CenterY(secondReader), CenterY(secondWriter), 1.0);
            var expectedOutputCenter = branchesMerge
                ? (CenterY(firstWriter) + CenterY(secondWriter)) / 2
                : CenterY(firstWriter);
            Assert.AreEqual(expectedOutputCenter, CenterY(output), 1.0);
            if (sharedTailBranches)
            {
                Assert.IsTrue(otherOutput.Location.Y >= fileWriter.Location.Y + fileWriter.Size.Height + 40);
                Assert.AreEqual(CenterY(output), (fileWriter.Location.Y + otherOutput.Location.Y + otherOutput.Size.Height) / 2.0, 1.0);
            }
            else
            {
                Assert.AreEqual(CenterY(output), CenterY(fileWriter), 1.0);
            }

            var alignedLocations = blocks.Select(x => x.Location).ToArray();
            FlowBlockAutoLayoutAdjuster.Adjust(blocks);
            CollectionAssert.AreEqual(alignedLocations, blocks.Select(x => x.Location).ToArray());
        }

        [TestMethod]
        public void Adjust_WaitsForAllInputsWhenBranchLengthsDiffer()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var first = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var middle = CreateFlowBlock<ExecutionOrderTestFlowBlock>(first);
            var last = CreateFlowBlock<ExecutionOrderTestFlowBlock>(middle);
            var second = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var output = CreateFlowBlock<ExecutionOrderTestFlowBlock>(last);
            output.ReferencedFlowBlocks.Add(second);
            var tail = CreateFlowBlock<ExecutionOrderTestFlowBlock>(output);
            var blocks = new BaseFlowBlock[] { output, second, last, tail, middle, first, start };
            foreach (var block in blocks)
            {
                block.Size = new Size(328, 80);
                block.Location = new Point(60, 900);
            }
            last.Size = new Size(328, 360);
            second.Size = new Size(328, 180);

            FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.IsTrue(second.Location.Y >= last.Location.Y + last.Size.Height + 40);
            Assert.AreEqual(CenterY(first), CenterY(last), 1.0);
            Assert.AreEqual((CenterY(last) + CenterY(second)) / 2, CenterY(output), 1.0);
            Assert.AreEqual(last.Location.X + last.Size.Width + 80, output.Location.X);
            Assert.AreEqual(CenterY(output), CenterY(tail), 1.0);

            var alignedLocations = blocks.Select(block => block.Location).ToArray();
            FlowBlockAutoLayoutAdjuster.Adjust(blocks.Reverse());
            CollectionAssert.AreEqual(alignedLocations, blocks.Select(block => block.Location).ToArray());
        }

        [TestMethod]
        public void Adjust_ReservesHeightForNestedBranchesUntilTheirSharedSuccessor()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var branch = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var sibling = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var upper = CreateFlowBlock<ExecutionOrderTestFlowBlock>(branch);
            var lower = CreateFlowBlock<ExecutionOrderTestFlowBlock>(branch);
            var output = CreateFlowBlock<ExecutionOrderTestFlowBlock>(upper);
            output.ReferencedFlowBlocks.Add(lower);
            output.ReferencedFlowBlocks.Add(sibling);
            var blocks = new BaseFlowBlock[] { output, lower, sibling, upper, branch, start };
            foreach (var block in blocks)
            {
                block.Size = new Size(328, 80);
                block.Location = new Point(60, 900);
            }
            upper.Size = new Size(328, 300);
            lower.Size = new Size(328, 120);
            sibling.Size = new Size(328, 200);

            FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.IsTrue(lower.Location.Y >= upper.Location.Y + upper.Size.Height + 40);
            Assert.IsTrue(sibling.Location.Y >= lower.Location.Y + lower.Size.Height + 40);
            Assert.AreEqual(CenterY(branch), (upper.Location.Y + lower.Location.Y + lower.Size.Height) / 2.0, 1.0);
            Assert.AreEqual((CenterY(upper) + CenterY(lower) + CenterY(sibling)) / 3, CenterY(output), 1.0);
        }

        [TestMethod]
        public void Adjust_AnchorsCycleWhenItsInputsCannotAllBeCompleted()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var first = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var second = CreateFlowBlock<ExecutionOrderTestFlowBlock>(first);
            first.ReferencedFlowBlocks.Add(second);
            var blocks = new BaseFlowBlock[] { start, first, second };
            foreach (var block in blocks)
            {
                block.Size = new Size(328, 80);
                block.Location = new Point(60, 900);
            }

            var result = FlowBlockAutoLayoutAdjuster.Adjust(blocks);

            Assert.AreEqual(3, result.TotalFlowBlocks);
            Assert.AreEqual(1, result.ComponentsProcessed);
            Assert.AreEqual(first.Location.X + first.Size.Width + 80, second.Location.X);
            Assert.AreEqual(CenterY(first), CenterY(second), 1.0);
            var alignedLocations = blocks.Select(block => block.Location).ToArray();
            FlowBlockAutoLayoutAdjuster.Adjust(blocks);
            CollectionAssert.AreEqual(alignedLocations, blocks.Select(block => block.Location).ToArray());
        }

        private static double CenterY(BaseFlowBlock block) => block.Location.Y + block.Size.Height / 2.0;

    }
}
