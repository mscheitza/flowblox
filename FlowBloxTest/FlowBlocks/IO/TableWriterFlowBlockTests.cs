using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.FlowBlocks.IO
{
    [TestClass]
    public sealed class TableWriterFlowBlockTests
    {
        [TestMethod]
        public void RuntimeFinished_RemovesReadableTableDataSourceChangedListener()
        {
            var table = new RecordingTable();
            var writer = new TableWriterFlowBlock
            {
                ReferencedTable = table
            };

            writer.RuntimeStarted(null);
            writer.RuntimeFinished(null);
            writer.RuntimeStarted(null);
            writer.RuntimeFinished(null);

            var readsBeforeChange = table.ReadCount;
            table.RaiseDataSourceChanged();

            Assert.AreEqual(readsBeforeChange, table.ReadCount);
            Assert.AreEqual(0, table.ListenerCount);
        }

        [TestMethod]
        public void RuntimeStarted_ReplacesExistingReadableTableDataSourceChangedListener()
        {
            var table = new RecordingTable();
            var writer = new TableWriterFlowBlock
            {
                ReferencedTable = table
            };

            writer.RuntimeStarted(null);
            writer.RuntimeStarted(null);

            var readsBeforeChange = table.ReadCount;
            table.RaiseDataSourceChanged();

            Assert.AreEqual(readsBeforeChange + 1, table.ReadCount);
            Assert.AreEqual(1, table.ListenerCount);
        }

        [TestMethod]
        public void OnAfterSave_SynchronizesRequiredFieldsFromColumnDefinitions()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var registry = project.FlowBloxRegistry;
            var source = registry.CreateFlowBlockUnregistered<ConcatUriFlowBlock>();
            registry.PostProcessFlowBlockCreated(source);
            registry.Register(source);

            var writer = registry.CreateFlowBlockUnregistered<TableWriterFlowBlock>();
            registry.PostProcessFlowBlockCreated(writer);
            registry.Register(writer);

            writer.TableColumnDefinitions.Add(new TableColumnDefinition
            {
                Field = source.ResultField,
                IsRequired = true,
                ColumnName = "InvoiceNumber"
            });

            writer.OnAfterSave();

            Assert.IsTrue(writer.RequiredFields.Contains(source.ResultField));
        }
    }
}
