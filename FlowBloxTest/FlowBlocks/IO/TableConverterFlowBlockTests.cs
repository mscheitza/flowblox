using System.Data;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;

namespace FlowBloxTest.FlowBlocks.IO
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class TableConverterFlowBlockTests : FlowBloxTestsBase
    {
        [TestMethod]
        public void Execute_ReadsSourceOnceAndWritesCompleteDataTableWithoutMapping()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var source = new RecordingTable();
            source.DataToRead.Columns.Add("Id", typeof(int));
            source.DataToRead.Columns.Add("Name", typeof(string));
            source.DataToRead.Rows.Add(7, "Ada");
            var target = new RecordingTable();
            var start = CreateFlowBlock<StartFlowBlock>();
            var converter = CreateFlowBlock<TableConverterFlowBlock>(start);
            converter.SourceTable = source;
            converter.TargetTable = target;

            converter.Execute(new FlowBloxUnitTestRuntime(project), null);

            Assert.AreEqual(1, source.ReadCount);
            Assert.AreEqual(1, target.WriteCount);
            Assert.AreSame(source.DataToRead, target.LastWrittenData);
            Assert.AreEqual("Ada", target.LastWrittenData.Rows[0]["Name"]);
        }
    }
}
