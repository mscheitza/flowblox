using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Provider
{
    public static class FlowBloxRegistryProvider
    {
        private static readonly List<FlowBloxRegistry> _registryChain = new();
        private static readonly AsyncLocal<FlowBloxRegistry> _scopedProjectRegistry = new();

        public static bool IsCurrentlyDetached => _registryChain.Any(x => x is FlowBloxDetachedRegistry);

        public static FlowBloxRegistry GetRegistry()
        {
            var scopedProjectRegistry = _scopedProjectRegistry.Value;
            if (scopedProjectRegistry != null)
                return scopedProjectRegistry;

            if (_registryChain.Any())
                return _registryChain.Last();

            var registry = ThreadBasedGridElementRegistryProvider.GetManagedObject();
            if (registry == null)
                registry = ResolveProjectRegistry();

            return registry;
        }

        [Obsolete("This is only for FlowBlox internal use. Use GetRegistry instead.", false)]
        public static FlowBloxRegistry GetProjectRegistry() => ResolveProjectRegistry();

        public static IDisposable BeginProjectRegistryScope()
        {
            var registry = ResolveProjectRegistry();
            var previousRegistry = _scopedProjectRegistry.Value;
            _scopedProjectRegistry.Value = registry;
            return new ProjectRegistryScope(previousRegistry);
        }

        private static FlowBloxRegistry ResolveProjectRegistry()
        {
            FlowBloxProject project = FlowBloxProjectManager.Instance.ActiveProject;
            return project?.FlowBloxRegistry;
        }

        public static FlowBloxRegistry OpenTransaction(bool detached = false)
        {
            if (!_registryChain.Any())
                _registryChain.Add(GetRegistry());

            _registryChain.Add(detached
                ? new FlowBloxDetachedRegistry(_registryChain.Last())
                : new FlowBloxTransientRegistry(_registryChain.Last()));
            return _registryChain.Last();
        }

        public static void CommitTransaction()
        {
            var currentRegistry = _registryChain.Last();
            if (currentRegistry is FlowBloxTransientRegistry transientRegistry)
                transientRegistry.Commit();

            RemoveFromChain(currentRegistry);
        }

        public static void CancelTransaction() => RemoveFromChain(_registryChain.Last());

        public static void RemoveFromChain(FlowBloxRegistry registry)
        {
            _registryChain.Remove(registry);
            if (_registryChain.Count == 1)
                _registryChain.RemoveAt(0);
        }

        private sealed class ProjectRegistryScope : IDisposable
        {
            private readonly FlowBloxRegistry _previousRegistry;
            private bool _disposed;

            public ProjectRegistryScope(FlowBloxRegistry previousRegistry)
            {
                _previousRegistry = previousRegistry;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _scopedProjectRegistry.Value = _previousRegistry;
            }
        }
    }
}
