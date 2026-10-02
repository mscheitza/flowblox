using FlowBlox.Core.Models.FlowBlocks.AI;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace FlowBloxTest.FlowBlocks.AI
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public class FlowBloxRuntimeModelCacheTests
    {
        private int _createdResources = 0;

        TestModelResource CreateResource()
        {
            _createdResources++;
            return new TestModelResource();
        }

        [TestMethod]
        public void CacheReusesModelPerRuntimeAndFirstCloseDisposesIt()
        {
            var runtime = CreateRuntime();
            var cache = new FlowBloxRuntimeModelCache<TestModelResource>();

            var first = cache.Open(runtime, @"C:\models\qa", CreateResource, out var firstAlreadyOpen);
            var second = cache.Open(runtime, @"C:\models\qa\", CreateResource, out var secondAlreadyOpen);

            Assert.AreEqual(1, _createdResources);
            Assert.IsFalse(firstAlreadyOpen);
            Assert.IsTrue(secondAlreadyOpen);
            Assert.AreSame(first, second);

            Assert.IsTrue(cache.Close(runtime, @"C:\models\qa"));
            Assert.IsTrue(first.IsDisposed);
            Assert.IsFalse(cache.Close(runtime, @"C:\models\qa"));
        }

        [TestMethod]
        public void CacheDoesNotShareModelsBetweenRuntimes()
        {
            var firstRuntime = CreateRuntime();
            var secondRuntime = CreateRuntime();

            var cache = new FlowBloxRuntimeModelCache<TestModelResource>();
            var first = cache.Open(firstRuntime, @"C:\models\qa", () => new TestModelResource(), out var firstAlreadyOpen);
            var second = cache.Open(secondRuntime, @"C:\models\qa", () => new TestModelResource(), out var secondAlreadyOpen);

            Assert.IsFalse(firstAlreadyOpen);
            Assert.IsFalse(secondAlreadyOpen);
            Assert.AreNotSame(first, second);

            cache.Close(firstRuntime, @"C:\models\qa");
            cache.Close(secondRuntime, @"C:\models\qa");
        }

        private static FlowBloxUnitTestRuntime CreateRuntime()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var startFlowBlock = project.FlowBloxRegistry.CreateFlowBlockUnregistered<StartFlowBlock>();
            project.FlowBloxRegistry.PostProcessFlowBlockCreated(startFlowBlock);
            project.FlowBloxRegistry.Register(startFlowBlock);
            return new FlowBloxUnitTestRuntime(project);
        }

        private sealed class TestModelResource : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }
}
