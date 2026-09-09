using FlowBlox.Core.Models.FlowBlocks.IO;

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
    }
}
