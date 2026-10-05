using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Logic;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using System.Collections.ObjectModel;

namespace FlowBloxTest.FlowBlocks.Logic
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class DecisionFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void Decision_SelectsFieldBasedOutputFromFirstMatchingRule()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var databaseType = CreateUserField("DatabaseType");
            var mysqlConnectionString = CreateUserField("MySqlConnectionString");
            databaseType.StringValue = "MySQL";
            mysqlConnectionString.StringValue = "Server=localhost;Database=flowbloxdb";

            var decision = CreateFlowBlock<DecisionFlowBlock>(start);
            decision.Decisions = new ObservableCollection<FieldComparisonCondition>
            {
                new()
                {
                    FieldElement = databaseType,
                    Operator = ComparisonOperator.Equals,
                    Value = "PostgreSQL",
                    OutputValue = "Host=localhost;Database=flowbloxdb"
                },
                new()
                {
                    FieldElement = databaseType,
                    Operator = ComparisonOperator.Equals,
                    Value = "MySQL",
                    OutputValue = mysqlConnectionString.FullyQualifiedName
                }
            };

            CreateRuntimeAndExecute(_project);

            Assert.AreEqual(mysqlConnectionString.StringValue, decision.ResultField.StringValue);
        }

        [TestMethod]
        public void Decision_UsesFallbackAndPreservesLegacyMatchedFieldOutput()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var comparedField = CreateUserField("ComparedValue");
            var fallbackField = CreateUserField("FallbackValue");
            comparedField.StringValue = "actual";
            fallbackField.StringValue = "fallback";

            var decision = CreateFlowBlock<DecisionFlowBlock>(start);
            decision.Decisions.Add(new FieldComparisonCondition
            {
                FieldElement = comparedField,
                Operator = ComparisonOperator.Equals,
                Value = "missing"
            });
            decision.FallbackValue = fallbackField.FullyQualifiedName;

            CreateRuntimeAndExecute(_project);
            Assert.AreEqual("fallback", decision.ResultField.StringValue);

            decision.Decisions[0].Value = "actual";
            CreateRuntimeAndExecute(_project);
            Assert.AreEqual("actual", decision.ResultField.StringValue);
        }

        [TestMethod]
        public void Decision_ProducesEmptyResultWhenNoRuleMatchesAndNoFallbackExists()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var comparedField = CreateUserField("ComparedValue");
            comparedField.StringValue = "actual";

            var decision = CreateFlowBlock<DecisionFlowBlock>(start);
            decision.Decisions.Add(new FieldComparisonCondition
            {
                FieldElement = comparedField,
                Operator = ComparisonOperator.Equals,
                Value = "missing",
                OutputValue = "unreachable"
            });

            CreateRuntimeAndExecute(_project);

            Assert.IsNull(decision.ResultField.StringValue);
        }
    }
}
