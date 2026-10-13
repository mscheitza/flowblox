using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Validations;

namespace FlowBloxTest.Util
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FlowBloxComponentValidatorTests
    {
        [TestMethod]
        public void Validate_ValidatesManagedObjectWhenItIsTheRoot()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();

            var messages = FlowBloxComponentValidator.Validate(new SQLTable());

            Assert.IsTrue(messages.Count > 0);
        }

        [TestMethod]
        public void Validate_ValidatesOwnedReactiveObjectsWithoutFollowingComponentReferences()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            var reader = new TableReaderFlowBlock
            {
                Name = "Reader",
                ReferencedTable = new SQLTable()
            };
            reader.MappingEntries.Add(new TableSelectorMappingEntry
            {
                ColumnName = "Id"
            });
            reader.ReferencedFlowBlocks.Add(new StartFlowBlock());

            var messages = FlowBloxComponentValidator.Validate(reader);

            Assert.IsTrue(messages.Any(x => x.StartsWith("[MappingEntries[0]]", StringComparison.Ordinal)));
            Assert.IsFalse(messages.Any(x => x.StartsWith("[ReferencedTable]", StringComparison.Ordinal)));
            Assert.IsFalse(messages.Any(x => x.StartsWith("[ReferencedFlowBlocks", StringComparison.Ordinal)));
        }

        [TestMethod]
        public void Validate_DoesNotResolveOnDemandByteArrayValue()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var field = new FieldElement(FieldTypes.Text)
            {
                Name = "Response",
                StringValue = "{\"jobs\":[]}"
            };
            project.FlowBloxRegistry.Register(field);

            var messages = FlowBloxComponentValidator.Validate(field.FieldType);

            Assert.AreEqual(0, messages.Count);
        }
    }
}
