using FlowBlox.Core.Actions;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;

namespace FlowBloxTest.Actions
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxPendingActionStoreTests
    {
        [TestMethod]
        public void Add_StagedManagedObjectDeletion_HidesObjectFromTransientRegistry()
        {
            var parent = new FlowBloxRegistry();
            var field = new FieldElement(FieldTypes.Text);
            parent.RegisterManagedObject(field);
            var transient = new FlowBloxTransientRegistry(parent);

            try
            {
                FlowBloxPendingActionStore.Add(transient, new FlowBloxManagedObjectDeleteAction
                {
                    ManagedObject = field,
                    HistoryManagedObject = field
                });

                Assert.IsFalse(transient.GetManagedObjects().Contains(field));
                Assert.IsFalse(transient.GetManagedObjects<FieldElement>().Contains(field));
                Assert.IsFalse(transient.GetManagedObjects(typeof(FieldElement)).Contains(field));
                Assert.IsFalse(transient.GetFieldElements().Contains(field));
                Assert.IsTrue(parent.GetManagedObjects().Contains(field));

                var pendingAction = FlowBloxPendingActionStore.Take(transient).Single();
                using (FlowBloxRegistryProvider.BeginScopedRegistry(transient))
                    pendingAction.Invoke();
                transient.Commit();

                Assert.IsFalse(parent.GetManagedObjects().Contains(field));
            }
            finally
            {
                FlowBloxPendingActionStore.Take(transient);
            }
        }

        [TestMethod]
        public void Add_StagedFlowBlockDeletion_HidesFlowBlockFromTransientRegistry()
        {
            var parent = new FlowBloxRegistry();
            var flowBlock = new FormatFlowBlock();
            parent.RegisterFlowBlock(flowBlock);
            var transient = new FlowBloxTransientRegistry(parent);

            try
            {
                FlowBloxPendingActionStore.Add(transient, new FlowBloxDeleteAction
                {
                    FlowBlock = flowBlock,
                    HistoryFlowBlock = flowBlock
                });

                Assert.IsFalse(transient.GetFlowBlocks().Contains(flowBlock));
                Assert.IsFalse(transient.GetFlowBlocks(typeof(FormatFlowBlock)).Contains(flowBlock));
                Assert.IsTrue(parent.GetFlowBlocks().Contains(flowBlock));

                var pendingAction = FlowBloxPendingActionStore.Take(transient).Single();
                using (FlowBloxRegistryProvider.BeginScopedRegistry(transient))
                    pendingAction.Invoke();
                transient.Commit();

                Assert.IsFalse(parent.GetFlowBlocks().Contains(flowBlock));
            }
            finally
            {
                FlowBloxPendingActionStore.Take(transient);
            }
        }

    }
}
