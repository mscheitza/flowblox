using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.Xml;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using System.Collections.ObjectModel;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.Testing;
using FlowBloxTest.Constants;
using FlowBloxTest.FlowBlocks.Execution;

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

        [TestMethod]
        public void TestExecutor_FailsWhenExecutedFlowBlockAssociatedFlowBlockIsNotExecuted()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var xmlDocumentFlowBlock = CreateFlowBlock<XmlDocumentFlowBlock>(startFlowBlock);
            var xmlWriterFlowBlock = CreateFlowBlock<XmlDocumentOutputFlowBlock>(xmlDocumentFlowBlock);
            xmlWriterFlowBlock.AssociatedXmlDocument = xmlDocumentFlowBlock;

            var testDefinition = new FlowBloxTestDefinition
            {
                Name = "Associated flow block validation",
                Entries = new ObservableCollection<FlowBlockTestDataset>
                {
                    new()
                    {
                        FlowBlock = xmlDocumentFlowBlock,
                        Execute = false
                    },
                    new()
                    {
                        FlowBlock = xmlWriterFlowBlock,
                        Execute = true
                    }
                }
            };

            var logMessages = new List<string>();
            var executor = new FlowBloxTestExecutor();
            executor.Initialize(testDefinition, xmlWriterFlowBlock, [xmlWriterFlowBlock]);
            executor.GetRuntime().LogMessageCreated += (_, message, _) => logMessages.Add(message);

            var result = executor.ExecuteTest();

            Assert.IsFalse(result.Success);
            Assert.IsTrue(logMessages.Any(x =>
                x.Contains($"FlowBlock \"{xmlWriterFlowBlock.Name}\" is executed") &&
                x.Contains($"associated FlowBlock \"{xmlDocumentFlowBlock.Name}\" is not executed")));
        }

        [TestMethod]
        public void TestExecutor_ReportsAllFailedExpectationConditionsIncludingExpectedValue()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var resultBlock = CreateFlowBlock<ExecutionOrderTestFlowBlock>(startFlowBlock);

            var testDefinition = new FlowBloxTestDefinition
            {
                Name = "Expectation validation",
                Entries = new ObservableCollection<FlowBlockTestDataset>
                {
                    new()
                    {
                        FlowBlock = resultBlock,
                        Execute = true,
                        FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>
                        {
                            new()
                            {
                                FieldElement = resultBlock.ResultField,
                                SelectionMode = FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue,
                                UserInput = "missing expected value",
                                ExpectationConditions = new ObservableCollection<ExpectationCondition>
                                {
                                    new()
                                    {
                                        ExpectationConditionTarget = ExpectationConditionTarget.NumberOfDatasets,
                                        Operator = ComparisonOperator.Equals,
                                        Value = "2"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var logMessages = new List<(string Message, FlowBloxLogLevel LogLevel)>();
            var executor = new FlowBloxTestExecutor();
            executor.Initialize(testDefinition, resultBlock, [resultBlock]);
            executor.GetRuntime().LogMessageCreated += (_, message, logLevel) => logMessages.Add((message, logLevel));

            var result = executor.ExecuteTest();

            Assert.IsFalse(result.Success);
            var failedConditionMessages = logMessages
                .Where(x =>
                    x.LogLevel == FlowBloxLogLevel.Error &&
                    x.Message.Contains("Expectation condition") &&
                    x.Message.Contains("failed for the field"))
                .ToList();

            Assert.AreEqual(2, failedConditionMessages.Count);
            Assert.IsTrue(failedConditionMessages.Any(x =>
                x.Message.Contains("\"2\"")));
            Assert.IsTrue(failedConditionMessages.Any(x =>
                x.Message.Contains("\"missing expected value\"")));
            Assert.IsTrue(logMessages.Any(x =>
                x.LogLevel == FlowBloxLogLevel.Error &&
                x.Message.Contains("2 expectation condition(s) were not met")));
            Assert.IsFalse(logMessages.Any(x =>
                x.LogLevel == FlowBloxLogLevel.Success &&
                x.Message.Contains("All test expectations met")));
        }

        [TestMethod]
        public void TestExecutor_LogsDifferentMessageWhenNoExpectationsAreDefined()
        {
            var startFlowBlock = CreateFlowBlock<StartFlowBlock>();
            var resultBlock = CreateFlowBlock<ExecutionOrderTestFlowBlock>(startFlowBlock);

            var testDefinition = new FlowBloxTestDefinition
            {
                Name = "No expectations",
                Entries = new ObservableCollection<FlowBlockTestDataset>
                {
                    new()
                    {
                        FlowBlock = resultBlock,
                        Execute = true,
                        FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>
                        {
                            new()
                            {
                                FieldElement = resultBlock.ResultField,
                                SelectionMode = FlowBloxTestConfigurationSelectionMode.First
                            }
                        }
                    }
                }
            };

            var logMessages = new List<(string Message, FlowBloxLogLevel LogLevel)>();
            var executor = new FlowBloxTestExecutor();
            executor.Initialize(testDefinition, resultBlock, [resultBlock]);
            executor.GetRuntime().LogMessageCreated += (_, message, logLevel) => logMessages.Add((message, logLevel));

            var result = executor.ExecuteTest();

            Assert.IsTrue(result.Success);
            Assert.IsTrue(logMessages.Any(x =>
                x.LogLevel == FlowBloxLogLevel.Info &&
                x.Message.Contains("No test expectations defined")));
            Assert.IsFalse(logMessages.Any(x =>
                x.LogLevel == FlowBloxLogLevel.Success &&
                x.Message.Contains("All test expectations met")));
        }

    }
}
