using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.Fields;

namespace FlowBloxTest.Models.Project
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxProjectPropertyTests
    {
        private FlowBloxProject? _previousActiveProject;

        [TestInitialize]
        public void Initialize()
        {
            _previousActiveProject = FlowBloxProjectManager.Instance.ActiveProject;
        }

        [TestCleanup]
        public void Cleanup()
        {
            FlowBloxProjectManager.Instance.ActiveProject = _previousActiveProject;
        }

        [TestMethod]
        public void GetProjectPropertyElements_IncludesLocalProjectPaths()
        {
            var project = new FlowBloxProject();
            var projectFilePath = Path.Combine(Path.GetTempPath(), "FlowBlox", "Project.fbprj");
            project.SetProjectFilePath(projectFilePath);

            var properties = project.GetProjectPropertyElements().ToDictionary(x => x.Key, x => x.Value);
            var expectedFilePath = Path.GetFullPath(projectFilePath);

            Assert.AreEqual(expectedFilePath, properties["FilePath"]);
            Assert.AreEqual(Path.GetDirectoryName(expectedFilePath), properties["Directory"]);
        }

        [TestMethod]
        public void LocalProjectPathPlaceholders_AreResolved()
        {
            var project = new FlowBloxProject();
            var projectFilePath = Path.Combine(Path.GetTempPath(), "FlowBlox", "Project.fbprj");
            project.SetProjectFilePath(projectFilePath);
            FlowBloxProjectManager.Instance.ActiveProject = project;

            var resolved = FlowBloxFieldHelper.ReplaceFieldsInString(
                "$Project::Directory|$Project::FilePath");
            var expectedFilePath = Path.GetFullPath(projectFilePath);
            var expectedDirectory = Path.GetDirectoryName(expectedFilePath);

            Assert.AreEqual(
                $"{expectedDirectory}|{expectedFilePath}",
                resolved);
        }

        [TestMethod]
        public void UnsavedProjectPathProperties_AreEmpty()
        {
            var project = new FlowBloxProject();

            Assert.AreEqual(string.Empty, project.ProjectDirectory);
            Assert.AreEqual(string.Empty, project.ProjectFilePath);
        }
    }
}
