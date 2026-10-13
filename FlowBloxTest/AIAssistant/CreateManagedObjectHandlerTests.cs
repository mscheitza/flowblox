using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class CreateManagedObjectHandlerTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task CreateManagedObject_RejectsFieldsWithoutRegisteringThem(bool derivedType)
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var type = derivedType ? typeof(DerivedFieldElement) : typeof(FieldElement);
            var previousObjects = project.FlowBloxRegistry.GetManagedObjects().ToArray();

            var response = await new CreateManagedObjectHandler().HandleAsync(
                new JObject { ["typeFullName"] = type.AssemblyQualifiedName, ["name"] = "InvalidField" },
                CancellationToken.None);

            Assert.IsFalse(response.Ok);
            StringAssert.Contains(response.Error, "CreateField");
            StringAssert.Contains(response.Error, "sourceFlowBlockName");
            StringAssert.Contains(response.Error, "CreateUserField");
            CollectionAssert.AreEqual(previousObjects, project.FlowBloxRegistry.GetManagedObjects().ToArray());
        }

        public class DerivedFieldElement : FieldElement
        {
        }
    }
}
