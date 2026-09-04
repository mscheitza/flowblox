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
    }
}