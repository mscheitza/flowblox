using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.FlowBlocks.Execution
{
    [TestClass]
    public class ExecutionOrderTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void ExecutionOrder_Start_Then_Siblings_UsesExecutionIndex()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var node1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node2 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node3 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);

            node1.Name = "Node1";
            node2.Name = "Node2";
            node3.Name = "Node3";

            node1.ExecutionIndex = 0;
            node2.ExecutionIndex = 1;
            node3.ExecutionIndex = -1;

            var executionResult = CreateRuntimeExecuteAndCaptureInvocationOrder(_project);

            CollectionAssert.AreEqual(
                new List<string> { "Start", "Node1", "Node2", "Node3" },
                executionResult.InvocationOrder);
        }

        [TestMethod]
        public void ExecutionOrder_Start_Siblings_Then_InputReferenceNode_WithStartIterationContext()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var node1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node2 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node3 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var nodeX1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(node3);

            node1.Name = "Node1";
            node2.Name = "Node2";
            node3.Name = "Node3";
            nodeX1.Name = "NodeX1";

            node1.ExecutionIndex = 0;
            node2.ExecutionIndex = 1;
            node3.ExecutionIndex = -1;
            nodeX1.AssociatedIterationContext = start;

            var executionResult = CreateRuntimeExecuteAndCaptureInvocationOrder(_project);

            Assert.AreSame(start, nodeX1.IterationContext, "NodeX1 must resolve IterationContext to Start.");

            CollectionAssert.AreEqual(
                new List<string> { "Start", "Node1", "Node2", "Node3", "NodeX1" },
                executionResult.InvocationOrder);
        }

        [TestMethod]
        public void ExecutionOrder_Start_Siblings_Then_ChainedNodes()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var node1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node2 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var node3 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(start);
            var nodeX1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(node3);
            var nodeY1 = CreateFlowBlock<ExecutionOrderTestFlowBlock>(nodeX1);

            node1.Name = "Node1";
            node2.Name = "Node2";
            node3.Name = "Node3";
            nodeX1.Name = "NodeX1";
            nodeY1.Name = "NodeY1";

            node1.ExecutionIndex = 0;
            node2.ExecutionIndex = 1;
            node3.ExecutionIndex = -1;
            nodeX1.AssociatedIterationContext = start;
            nodeY1.AssociatedIterationContext = start;

            var executionResult = CreateRuntimeExecuteAndCaptureInvocationOrder(_project);

            CollectionAssert.AreEqual(
                new List<string> { "Start", "Node1", "Node2", "Node3", "NodeX1", "NodeY1" },
                executionResult.InvocationOrder);
        }
    }
}
