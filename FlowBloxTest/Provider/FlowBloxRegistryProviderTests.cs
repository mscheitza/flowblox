using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.Provider
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxRegistryProviderTests
    {
        [TestMethod]
        public void BeginScopedRegistry_ReturnsScopedRegistryWhenTransactionIsOpen()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            _ = FlowBloxRegistryProvider.OpenTransaction(detached: true);

            try
            {
                using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);

                Assert.AreSame(project.FlowBloxRegistry, FlowBloxRegistryProvider.GetRegistry());
            }
            finally
            {
                FlowBloxRegistryProvider.CancelTransaction();
            }
        }

        [TestMethod]
        public async Task BeginScopedRegistry_FlowsAcrossAwait()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;

            _ = FlowBloxRegistryProvider.OpenTransaction(detached: true);

            try
            {
                using var scope = FlowBloxRegistryProvider.BeginScopedRegistry(project.FlowBloxRegistry);

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

        [TestMethod]
        public void CancelTransaction_ForNonCurrentTransaction_ThrowsAndPreservesRegistryChain()
        {
            var previousProject = FlowBloxProjectManager.Instance.ActiveProject;
            FlowBloxProjectManager.Instance.ActiveProject = null;

            try
            {
                var outerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
                try
                {
                    var innerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
                    try
                    {
                        Assert.ThrowsExactly<InvalidOperationException>(
                            () => FlowBloxRegistryProvider.CancelTransaction(outerRegistry));
                        Assert.AreSame(innerRegistry, FlowBloxRegistryProvider.GetRegistry());
                        Assert.AreNotSame(outerRegistry, innerRegistry);
                    }
                    finally
                    {
                        if (FlowBloxRegistryProvider.IsCurrentTransaction(innerRegistry))
                            FlowBloxRegistryProvider.CancelTransaction(innerRegistry);
                    }

                    Assert.AreSame(outerRegistry, FlowBloxRegistryProvider.GetRegistry());
                }
                finally
                {
                    if (FlowBloxRegistryProvider.IsCurrentTransaction(outerRegistry))
                        FlowBloxRegistryProvider.CancelTransaction(outerRegistry);
                }

                Assert.IsNull(FlowBloxRegistryProvider.GetRegistry());
            }
            finally
            {
                FlowBloxProjectManager.Instance.ActiveProject = previousProject;
            }
        }

        [TestMethod]
        public void IsCurrentTransactionNested_WithTransactionParent_RemainsFalseWithoutExplicitMetadata()
        {
            var outerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
            try
            {
                Assert.IsFalse(FlowBloxRegistryProvider.IsCurrentTransactionNested());

                var innerRegistry = FlowBloxRegistryProvider.OpenTransaction(detached: true);
                try
                {
                    Assert.IsFalse(FlowBloxRegistryProvider.IsCurrentTransactionNested());

                    var currentSnapshot = FlowBlox.Core.Provider.Registry.FlowBloxRegistryTransactionHistory
                        .GetHistory()
                        .Last(x => x.IsCurrent);
                    Assert.IsFalse(currentSnapshot.IsNested);
                    Assert.AreEqual(2, currentSnapshot.Depth);
                }
                finally
                {
                    if (FlowBloxRegistryProvider.IsCurrentTransaction(innerRegistry))
                        FlowBloxRegistryProvider.CancelTransaction(innerRegistry);
                }

                Assert.IsFalse(FlowBloxRegistryProvider.IsCurrentTransactionNested());
            }
            finally
            {
                if (FlowBloxRegistryProvider.IsCurrentTransaction(outerRegistry))
                    FlowBloxRegistryProvider.CancelTransaction(outerRegistry);
            }
        }

        [TestMethod]
        public void IsCurrentTransactionNested_WithExplicitNestedMetadata_IsTrueWithoutTransactionParent()
        {
            var nestedRegistry = FlowBloxRegistryProvider.OpenTransaction(
                detached: true,
                nested: true);

            try
            {
                Assert.IsTrue(FlowBloxRegistryProvider.IsCurrentTransactionNested());

                var currentSnapshot = FlowBlox.Core.Provider.Registry.FlowBloxRegistryTransactionHistory
                    .GetHistory()
                    .Last(x => x.IsCurrent);
                Assert.IsTrue(currentSnapshot.IsNested);
                Assert.AreEqual(1, currentSnapshot.Depth);
            }
            finally
            {
                if (FlowBloxRegistryProvider.IsCurrentTransaction(nestedRegistry))
                    FlowBloxRegistryProvider.CancelTransaction(nestedRegistry);
            }
        }

        [TestMethod]
        public void OpenTransaction_WhenCurrentTransactionIsNested_ThrowsAndPreservesCurrentTransaction()
        {
            var nestedRegistry = FlowBloxRegistryProvider.OpenTransaction(
                detached: true,
                nested: true);

            try
            {
                Assert.ThrowsExactly<InvalidOperationException>(
                    () => FlowBloxRegistryProvider.OpenTransaction(detached: true));

                Assert.AreSame(nestedRegistry, FlowBloxRegistryProvider.GetRegistry());
                Assert.IsTrue(FlowBloxRegistryProvider.IsCurrentTransactionNested());
            }
            finally
            {
                if (FlowBloxRegistryProvider.IsCurrentTransaction(nestedRegistry))
                    FlowBloxRegistryProvider.CancelTransaction(nestedRegistry);
            }
        }
    }
}
