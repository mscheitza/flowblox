using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.Json;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace FlowBloxTest.FlowBlocks.Json
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class JsonGenerationTests : FlowBloxTestsBase
    {
        private const string ParticipantsCsv = """
            FirstName;LastName;DateOfBirth
            Anna;Becker;1990-01-15
            Paul;Smith;1985-07-03
            Leyla;Yilmaz;2001-12-24
            """;

        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void JsonGeneration_FromCsv_CreatesParticipantsArray()
        {
            var start = CreateFlowBlock<StartFlowBlock>();

            var jsonObject = CreateFlowBlock<JsonObjectFlowBlock>(start);
            jsonObject.JsonContent = "{}";

            var tableReader = CreateFlowBlock<TableReaderFlowBlock>(jsonObject);
            tableReader.ReferencedTable = CreateCsvTable(ParticipantsCsv);

            var firstName = CreateTableField(tableReader, "FirstName");
            var lastName = CreateTableField(tableReader, "LastName");
            var dateOfBirth = CreateTableField(tableReader, "DateOfBirth");

            var nodeWriter = CreateFlowBlock<JsonObjectWriterFlowBlock>(tableReader);
            nodeWriter.AssociatedJsonObject = jsonObject;
            nodeWriter.Path = "$.participants";
            nodeWriter.IsArray = true;
            nodeWriter.Assignments = new ObservableCollection<JsonPropertyValueAssignment>
            {
                new() { PropertyName = "firstName", FieldValue = firstName },
                new() { PropertyName = "lastName", FieldValue = lastName },
                new() { PropertyName = "dateOfBirth", FieldValue = dateOfBirth }
            };

            var jsonOutput = CreateFlowBlock<JsonObjectOutputFlowBlock>(nodeWriter);
            jsonOutput.AssociatedJsonObject = jsonObject;
            jsonOutput.Indented = false;

            CreateRuntimeAndExecute(_project);

            var actual = JToken.Parse(jsonOutput.ResultField.StringValue);
            var expected = JToken.Parse("""
            {
              "participants": [
                { "firstName": "Anna", "lastName": "Becker", "dateOfBirth": "1990-01-15" },
                { "firstName": "Paul", "lastName": "Smith", "dateOfBirth": "1985-07-03" },
                { "firstName": "Leyla", "lastName": "Yilmaz", "dateOfBirth": "2001-12-24" }
              ]
            }
            """);

            Assert.IsTrue(JToken.DeepEquals(expected, actual),
                $"Expected:{Environment.NewLine}{expected}{Environment.NewLine}Actual:{Environment.NewLine}{actual}");
        }

        private CsvTable CreateCsvTable(string content)
        {
            var sourceField = CreateUserField("ParticipantsCsv");
            sourceField.StringValue = content;

            var dataSource = CreateManagedObject<MemoryObject>();
            dataSource.Field = sourceField;
            dataSource.FileName = "participants.csv";

            var table = CreateManagedObject<CsvTable>();
            table.DataSource = dataSource;
            table.FirstRowHeader = true;
            table.Separator = ";";
            return table;
        }

        private FlowBlox.Core.Models.Components.FieldElement CreateTableField(
            TableReaderFlowBlock tableReader,
            string columnName)
        {
            var field = Registry.CreateField(tableReader, FieldNameGenerationMode.UseFallbackIndexOnly);
            tableReader.MappingEntries.Add(new TableSelectorMappingEntry
            {
                ColumnName = columnName,
                Field = field
            });
            return field;
        }
    }
}
