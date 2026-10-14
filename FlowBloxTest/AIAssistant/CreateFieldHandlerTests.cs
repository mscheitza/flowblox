using FlowBlox.AIAssistant.Tools.Handler;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.IO;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.AIAssistant
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class CreateFieldHandlerTests
    {
        private FlowBloxProject _previousProject;
        private FlowBloxProject _project;
        private FileWriterFlowBlock _source;

        [TestInitialize]
        public void Initialize()
        {
            _previousProject = FlowBloxProjectManager.Instance.ActiveProject;
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
            _source = _project.FlowBloxRegistry.CreateFlowBlockUnregistered<FileWriterFlowBlock>();
            _project.FlowBloxRegistry.PostProcessFlowBlockCreated(_source);
            _project.FlowBloxRegistry.Register(_source);
        }

        [TestCleanup]
        public void Cleanup()
        {
            FlowBloxProjectManager.Instance.ActiveProject = _previousProject;
        }

        [TestMethod]
        public async Task CreateField_RepairsMissingSingleResultWithOwnedRegisteredDefaultField()
        {
            _source.ResultField = null;

            var response = await new CreateFieldHandler().HandleAsync(
                new JObject { ["sourceFlowBlockName"] = _source.Name }, CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            Assert.IsNotNull(_source.ResultField);
            Assert.AreSame(_source, _source.ResultField.Source);
            Assert.AreEqual(_source.DefaultResultFieldName, _source.ResultField.Name);
            Assert.AreEqual(_source.DefaultResultFieldType, _source.ResultField.FieldType.FieldType);
            Assert.IsTrue(_project.FlowBloxRegistry.GetManagedObjects().Contains(_source.ResultField));
            Assert.AreEqual(_source.ResultField.FullyQualifiedName,
                response.Result.Value<string>("fullyQualifiedFieldName"));
        }

        [TestMethod]
        public async Task CreateField_RejectsExistingSingleResultWithoutCreatingOrReplacingFields()
        {
            var existingField = _source.ResultField;
            var previousObjects = _project.FlowBloxRegistry.GetManagedObjects().ToArray();

            var response = await new CreateFieldHandler().HandleAsync(
                new JObject { ["sourceFlowBlockName"] = _source.Name }, CancellationToken.None);

            Assert.IsFalse(response.Ok);
            Assert.AreSame(existingField, _source.ResultField);
            CollectionAssert.AreEqual(previousObjects, _project.FlowBloxRegistry.GetManagedObjects().ToArray());
        }

        [TestMethod]
        public async Task CreateField_RepairsMissingSingleResultWithExplicitNameAndType()
        {
            _source.ResultField = null;

            var response = await new CreateFieldHandler().HandleAsync(new JObject
            {
                ["sourceFlowBlockName"] = _source.Name,
                ["fieldType"] = nameof(FieldTypes.Integer),
                ["overrideName"] = "CustomResult"
            }, CancellationToken.None);

            Assert.IsTrue(response.Ok, response.Error);
            Assert.AreEqual("CustomResult", _source.ResultField.Name);
            Assert.AreEqual(FieldTypes.Integer, _source.ResultField.FieldType.FieldType);
        }
    }
}
