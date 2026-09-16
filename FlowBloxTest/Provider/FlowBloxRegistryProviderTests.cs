using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.Provider
{
    [TestClass]
    public class FlowBloxRegistryProviderTests
    {
        [TestMethod]
        public void BeginProjectRegistryScope_ReturnsProjectRegistryWhenTransactionIsOpen()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            _ = FlowBloxRegistryProvider.OpenTransaction(detached: true);

            try
            {
                using var scope = FlowBloxRegistryProvider.BeginProjectRegistryScope();

                Assert.AreSame(project.FlowBloxRegistry, FlowBloxRegistryProvider.GetRegistry());
            }
            finally
            {
                FlowBloxRegistryProvider.CancelTransaction();
            }
        }

        [TestMethod]
        public async Task BeginProjectRegistryScope_FlowsAcrossAwait()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            _ = FlowBloxRegistryProvider.OpenTransaction(detached: true);

            try
            {
                using var scope = FlowBloxRegistryProvider.BeginProjectRegistryScope();

                await Task.Delay(1);

                Assert.AreSame(project.FlowBloxRegistry, FlowBloxRegistryProvider.GetRegistry());
            }
            finally
            {
                FlowBloxRegistryProvider.CancelTransaction();
            }
        }

        [TestMethod]
        public void OpenDetachedTransaction_WithoutAvailableRegistry_UsesEmptyRegistry()
        {
            var previousProject = FlowBloxProjectManager.Instance.ActiveProject;
            FlowBloxProjectManager.Instance.ActiveProject = null;

            try
            {
                var outerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
                Assert.IsNotNull(outerRegistry);
                Assert.IsFalse(outerRegistry.GetFlowBlocks().Any());
                Assert.IsFalse(outerRegistry.GetManagedObjects().Any());

                var innerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
                FlowBloxRegistryProvider.CancelTransaction();

                Assert.AreSame(outerRegistry, FlowBloxRegistryProvider.GetRegistry());
                Assert.AreNotSame(outerRegistry, innerRegistry);

                FlowBloxRegistryProvider.CancelTransaction();
                Assert.IsNull(FlowBloxRegistryProvider.GetRegistry());
            }
            finally
            {
                FlowBloxProjectManager.Instance.ActiveProject = previousProject;
            }
        }
    }
}