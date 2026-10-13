using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.Components
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class FieldElementTests
    {
        [TestMethod]
        public void RenameMemoryField_UpdatesFullyQualifiedNameInFieldSelectionProperty()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var field = project.FlowBloxRegistry.CreateUserField(
                UserFieldTypes.Memory,
                FieldTypes.Text,
                "PreviousName");
            var formatFlowBlock = new FormatFlowBlock
            {
                Name = "Format",
                FormatExpression = $"Value: {field.FullyQualifiedName}"
            };
            formatFlowBlock.OnAfterLoad();

            field.Name = "CurrentName";

            Assert.AreEqual("Value: $User::CurrentName", formatFlowBlock.FormatExpression);
        }
    }
}
