using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Xml;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using System.Collections.ObjectModel;
using FlowBlox.Core.Enums;

namespace FlowBloxTest.FlowBlocks.Xml
{
    [TestClass]
    public class XmlGenerationTests : FlowBloxTestsBase
    {
        [TestMethod]
        public void XmlGenerationTest()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var registry = project.FlowBloxRegistry;

            var startFlowBlock = CreateFlowBlock<StartFlowBlock>(registry);

            var xmlDocumentFlowBlock = CreateFlowBlock<XmlDocumentFlowBlock>(registry, startFlowBlock);
            xmlDocumentFlowBlock.XmlContent = "<root><teilnehmer-liste/></root>";

            var tableReader = CreateFlowBlock<TableReaderFlowBlock>(registry, xmlDocumentFlowBlock);

            var userField = CreateUserField(registry, "TeilnehmerListe-CsvContent");
            userField.StringValue = "Vorname;Nachname;Sprache\nAnna;Becker;DE\nPaul;Smith;EN";

            // TODO: Aktuell kann dem CsvTable kein Separator übergeben werden
            //       Weiterhin können aktuell die FlowBloxOptions nicht manuell per Unit-Test Ausführung gesetzt werden.
            // Bitte beides umsetzen.

            var dataSource = CreateManagedObject<MemoryObject>(registry);
            dataSource.Field = userField;
            dataSource.FileName = Path.GetRandomFileName();

            var csvTable = CreateManagedObject<CsvTable>(registry);
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

            var nodeAppenderFlowBlock = CreateFlowBlock<XmlDocumentNodeWriterFlowBlock>(registry, tableReader);
            nodeAppenderFlowBlock.XPath = "/root/teilnehmer-liste";
            nodeAppenderFlowBlock.NodeName = "teilnehmer";
            nodeAppenderFlowBlock.AssociatedXmlDocument = xmlDocumentFlowBlock;
    
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

            var xmlWriterFlowBlock = CreateFlowBlock<XmlDocumentOutputFlowBlock>(registry, nodeAppenderFlowBlock);
            xmlWriterFlowBlock.AssociatedXmlDocument = xmlDocumentFlowBlock;

            CreateRuntimeAndExecute(project);

            var createdXml = xmlWriterFlowBlock.ResultField.StringValue;
            Assert.IsNotNull(createdXml);
            Assert.IsTrue(createdXml.Contains("<vorname>Anna</vorname>"));
            Assert.IsTrue(createdXml.Contains("<sprache>EN</sprache>"));
            Console.WriteLine("Resulting XML:\n" + createdXml);
        }
    }
}
