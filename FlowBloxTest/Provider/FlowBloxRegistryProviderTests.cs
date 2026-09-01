using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.Provider
{
    [TestClass]
    public class FlowBloxRegistryProviderTests
    {
        [TestMethod]
        public void OpenTransaction_ThrowsWhenRegistryIsMarkedInUse()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();

            using var registryUseScope = FlowBloxRegistryProvider.MarkRegistryInUse();

            AssertThrowsRegistryCurrentlyInUse(
                () => FlowBloxRegistryProvider.OpenTransaction(detached: true));
        }

        [TestMethod]
        public void MarkRegistryInUse_ThrowsWhenTransactionIsOpen()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
            _ = FlowBloxRegistryProvider.OpenTransaction(detached: true);

            try
            {
                AssertThrowsRegistryCurrentlyInUse(
                    () => FlowBloxRegistryProvider.MarkRegistryInUse());
            }
            finally
            {
                FlowBloxRegistryProvider.CancelTransaction();
            }
        }

        private static void AssertThrowsRegistryCurrentlyInUse(Action action)
        {
            try
            {
                action();
            }
            catch (RegistryCurrentlyInUseException)
            {
                return;
            }

            Assert.Fail($"Expected {nameof(RegistryCurrentlyInUseException)}.");
        }
    }
}
