using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Xml;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using System.Collections.ObjectModel;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;

namespace FlowBloxTest.FlowBlocks.Xml
{
    [TestClass]
    public class XmlGenerationTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void XmlGenerationTest()
        {
            var registry = _project.FlowBloxRegistry;

            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();

            var xmlDocumentFlowBlock = CreateFlowBlock<XmlDocumentFlowBlock>(startFlowBlock);
            xmlDocumentFlowBlock.XmlContent = "<root><teilnehmer-liste/></root>";

            var tableReader = CreateFlowBlock<TableReaderFlowBlock>(xmlDocumentFlowBlock);

            var userField = CreateUserField("TeilnehmerListe-CsvContent");
            userField.StringValue = "Vorname;Nachname;Sprache\nAnna;Becker;DE\nPaul;Smith;EN";

            var dataSource = CreateManagedObject<MemoryObject>();
            dataSource.Field = userField;
            dataSource.FileName = Path.GetRandomFileName();

            var csvTable = CreateManagedObject<CsvTable>();
            csvTable.FirstRowHeader = true;
            csvTable.DataSource = dataSource;

            tableReader.ReferencedTable = csvTable;

            var field0_Vorname = registry.CreateField(tableReader, FieldNameGenerationMode.UseFallbackIndexOnly);
            var field1_Nachname = registry.CreateField(tableReader, FieldNameGenerationMode.UseFallbackIndexOnly);
            var field2_Sprache = registry.CreateField(tableReader, FieldNameGenerationMode.UseFallbackIndexOnly);

            tableReader.MappingEntries = new ObservableCollection<TableSelectorMappingEntry>()
            {
                new TableSelectorMappingEntry()
                {
                    ColumnName = "Vorname",
                    Field = field0_Vorname
                },
                new TableSelectorMappingEntry()
                {
                    ColumnName = "Nachname",
                    Field = field1_Nachname
                },
                new TableSelectorMappingEntry()
                {
                    ColumnName = "Sprache",
                    Field = field2_Sprache
                }
            };

            var nodeAppenderFlowBlock = CreateFlowBlock<XmlDocumentNodeWriterFlowBlock>(tableReader);
            nodeAppenderFlowBlock.XPath = "/root/teilnehmer-liste";
            nodeAppenderFlowBlock.NodeName = "teilnehmer";
            nodeAppenderFlowBlock.AssociatedXmlDocument = xmlDocumentFlowBlock;
            nodeAppenderFlowBlock.UpdateExistingNode = false;
    
            nodeAppenderFlowBlock.Assignments = new ObservableCollection<XmlAssignment>()
            {
                new XmlAssignment()
                {
                    XPath = "teilnehmer/vorname",
                    FieldValue = field0_Vorname
                },
                new XmlAssignment()
                {
                    XPath = "teilnehmer/nachname",
                    FieldValue = field1_Nachname
                },
                new XmlAssignment()
                {
                    XPath = "teilnehmer/sprache",
                    FieldValue = field2_Sprache
                }
            };

            var xmlWriterFlowBlock = CreateFlowBlock<XmlDocumentOutputFlowBlock>(nodeAppenderFlowBlock);
            xmlWriterFlowBlock.AssociatedXmlDocument = xmlDocumentFlowBlock;

            CreateRuntimeAndExecute(_project);

            var createdXml = xmlWriterFlowBlock.ResultField.StringValue;
            Assert.IsNotNull(createdXml);
            Assert.IsTrue(createdXml.Contains("<vorname>Anna</vorname>"));
            Assert.IsTrue(createdXml.Contains("<sprache>EN</sprache>"));
            Console.WriteLine("Resulting XML:\n" + createdXml);
        }
    }
}
