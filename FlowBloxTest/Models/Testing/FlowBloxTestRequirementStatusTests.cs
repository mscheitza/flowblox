using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Testing;

namespace FlowBloxTest.Models.Testing
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxTestRequirementStatusTests
    {
        [TestMethod]
        public void FieldRequirement_IsSatisfiedOnlyByUsableSelectionConfiguration()
        {
            var configuration = new FlowBloxFieldTestConfiguration
            {
                UIRequiredForExecution = true
            };

            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.Keep;
            Assert.IsFalse(configuration.UIRequirementSatisfied);

            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.UserInput;
            configuration.UserInput = " ";
            Assert.IsFalse(configuration.UIRequirementSatisfied);
            configuration.UserInput = "manual value";
            Assert.IsTrue(configuration.UIRequirementSatisfied);

            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue;
            Assert.IsTrue(configuration.UIRequirementSatisfied);

            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.First;
            Assert.IsTrue(configuration.UIRequirementSatisfied);
            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.Last;
            Assert.IsTrue(configuration.UIRequirementSatisfied);

            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.Index;
            configuration.Index = null;
            Assert.IsFalse(configuration.UIRequirementSatisfied);
            configuration.Index = -1;
            Assert.IsFalse(configuration.UIRequirementSatisfied);
            configuration.Index = 0;
            Assert.IsTrue(configuration.UIRequirementSatisfied);
        }

        [TestMethod]
        public void FieldRequirement_RaisesStatusChangeForRelevantProperties()
        {
            var configuration = new FlowBloxFieldTestConfiguration();
            var statusChanges = 0;
            configuration.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(FlowBloxFieldTestConfiguration.UIRequirementSatisfied))
                    statusChanges++;
            };

            configuration.UIRequiredForExecution = true;
            configuration.SelectionMode = FlowBloxTestConfigurationSelectionMode.UserInput;
            configuration.UserInput = "value";
            configuration.Index = 0;

            Assert.AreEqual(4, statusChanges);
        }

        [TestMethod]
        public void AssociatedFlowBlockRequirement_IsSatisfiedWhenDatasetExecutes()
        {
            var dataset = new FlowBlockTestDataset
            {
                UIAssociatedFlowBlockRequiredForExecution = true,
                Execute = false
            };

            Assert.IsFalse(dataset.UIAssociatedFlowBlockRequirementSatisfied);
            dataset.Execute = true;
            Assert.IsTrue(dataset.UIAssociatedFlowBlockRequirementSatisfied);
        }
    }
}
