using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json;
using FlowBlox.Core.Util.Parsing;
using FlowBlox.Test.Runtime;
using FlowBloxTest.FlowBlocks.IO;
using System.Globalization;

namespace FlowBloxTest.FlowBlocks.Conditions
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class ParsingStrategyTests : FlowBloxTestsBase
    {
        [TestMethod]
        [DataRow(FieldTypes.Float)]
        [DataRow(FieldTypes.Double)]
        public void FieldComparisons_UseFieldSeparatorsForBothOperands(FieldTypes type)
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            var field = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, type);
            field.FieldType.DecimalSeparator = ",";
            field.FieldType.GroupSeparator = ".";
            field.StringValue = "1.234,5";
            var previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                var comparison = new FieldComparisonCondition { FieldElement = field, Operator = ComparisonOperator.Equals, Value = "1.234,5" };
                Assert.IsTrue(comparison.Compare());
                var logical = new FieldLogicalComparisonCondition { FieldElement = field, Operator = ComparisonOperator.GreaterThan, Value = "1.000,0" };
                Assert.IsTrue(logical.Check());
                Assert.AreEqual(field.Value, field.FieldType.GetSpecialParsingStrategy().Parse(comparison.Value));
                var right = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, fieldName: "Right");
                right.StringValue = "1.234,5";
                comparison.Value = right.FullyQualifiedName;
                Assert.IsTrue(comparison.Compare());
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [TestMethod]
        public void FieldDateComparison_UsesExactFieldFormatAndReportsInvalidRightValue()
        {
            var project = new FlowBloxProject();
            using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);
            var field = project.FlowBloxRegistry.CreateUserField(UserFieldTypes.Memory, FieldTypes.DateTime);
            field.FieldType.DateFormat = "dd.MM.yyyy HH:mm:ss";
            field.StringValue = "09.10.2026 18:15:00";
            var condition = new FieldComparisonCondition { FieldElement = field, Operator = ComparisonOperator.Equals, Value = field.StringValue };
            Assert.IsTrue(condition.Compare());
            var logical = new FieldLogicalComparisonCondition { FieldElement = field, Operator = ComparisonOperator.LowerThan, Value = "10.10.2026 18:15:00" };
            Assert.IsTrue(logical.Check());
            condition.Value = "2026-10-09";
            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => condition.Compare());
            Assert.IsInstanceOfType<FormatException>(exception.InnerException);
        }

        [TestMethod]
        public void TypedComparisons_RequireSpecialStrategy_WhileTextNeedsNone()
        {
            var number = new ComparisonCondition { Operator = ComparisonOperator.Equals, Value = "1.234,5" };
            Assert.ThrowsExactly<InvalidOperationException>(() => number.Compare(1234.5f));
            Assert.ThrowsExactly<InvalidOperationException>(() => number.Compare(1234.5d));
            var date = new ComparisonCondition { Operator = ComparisonOperator.Equals, Value = "09.10.2026" };
            Assert.ThrowsExactly<InvalidOperationException>(() => date.Compare(new DateTime(2026, 10, 9)));

            number.Value = "1,234.5";
            Assert.IsTrue(number.Compare(1234.5d, new DoubleParsingStrategy(CultureInfo.InvariantCulture.NumberFormat)));
            date.Value = "2026-10-09";
            Assert.IsTrue(date.Compare(new DateTime(2026, 10, 9), new DateTimeParsingStrategy("yyyy-MM-dd")));

            var text = new ComparisonCondition { Operator = ComparisonOperator.Equals, Value = "literal" };
            Assert.IsTrue(text.Compare("literal", new DateTimeParsingStrategy("invalid")));
            Assert.ThrowsExactly<InvalidOperationException>(() => number.Compare(1234.5d, new FloatParsingStrategy(CultureInfo.InvariantCulture.NumberFormat)));
        }

        [TestMethod]
        public void TableReader_TypedColumnConditionsUseTheirOwnTypeDefinitions()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var table = new RecordingTable();
            table.DataToRead.Columns.Add("Amount", typeof(double));
            table.DataToRead.Columns.Add("Date", typeof(DateTime));
            table.DataToRead.Rows.Add(1234.5, new DateTime(2026, 10, 9));
            table.DataToRead.Rows.Add(10.0, new DateTime(2026, 10, 9));
            var start = CreateFlowBlock<StartFlowBlock>();
            var reader = CreateFlowBlock<TableReaderFlowBlock>(start);
            reader.ReferencedTable = table;
            var field = project.FlowBloxRegistry.CreateField(reader, fieldType: FieldTypes.Double);
            reader.MappingEntries.Add(new TableSelectorMappingEntry { ColumnName = "Amount", Field = field });
            reader.ColumnConditions.Add(new TableSelectorColumnCondition
            {
                ColumnName = "Amount", Operator = ComparisonOperator.GreaterThan, Value = "1.000,0",
                ColumnType = new TypeElement { FieldType = FieldTypes.Double, DecimalSeparator = ",", GroupSeparator = "." }
            });
            reader.ColumnConditions.Add(new TableSelectorColumnCondition
            {
                ColumnName = "Date", Operator = ComparisonOperator.Equals, Value = "09.10.2026",
                ColumnType = new TypeElement { FieldType = FieldTypes.DateTime, DateFormat = "dd.MM.yyyy" }
            });

            reader.Execute(new FlowBloxUnitTestRuntime(project), null);

            Assert.AreEqual(1, table.ReadCount);
            Assert.AreEqual(1, reader.GridElementResult.Results.Count);
        }

        [TestMethod]
        public void ColumnCondition_LegacyJsonKeepsOptionalType_AndCustomFormatRoundTrips()
        {
            var legacy = JsonConvert.DeserializeObject<TableSelectorColumnCondition>("{\"ColumnName\":\"Name\",\"Value\":\"Ada\"}");
            Assert.IsNull(legacy.ColumnType);
            Assert.IsTrue(legacy.Compare("Ada"));
            Assert.ThrowsExactly<InvalidOperationException>(() => legacy.Compare(1.0d));
            var condition = new TableSelectorColumnCondition
            {
                ColumnName = "Amount", Operator = ComparisonOperator.Equals, Value = "1.234,5",
                ColumnType = new TypeElement { FieldType = FieldTypes.Double, DecimalSeparator = ",", GroupSeparator = "." }
            };
            var restored = JsonConvert.DeserializeObject<TableSelectorColumnCondition>(JsonConvert.SerializeObject(condition));
            Assert.IsTrue(restored.Compare(1234.5d));
            Assert.AreNotSame(condition.ColumnType, restored.ColumnType);
            Assert.AreNotSame(legacy.ColumnType, restored.ColumnType);
        }
    }
}
