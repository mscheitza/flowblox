using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using System.Threading;

namespace FlowBlox.Core.Provider
{
    public static class FlowBloxRegistryProvider
    {
        private static List<FlowBloxRegistry> _registryChain = new List<FlowBloxRegistry>();
        private static int _currentlyInUseCount;

        public static bool IsCurrentlyDetached => _registryChain.Any(x => x is FlowBloxDetachedRegistry);
        public static bool CurrentlyInUse => Volatile.Read(ref _currentlyInUseCount) > 0;

        public static FlowBloxRegistry GetRegistry()
        {
            if (_registryChain.Any())
                return _registryChain.Last();

            var registry = ThreadBasedGridElementRegistryProvider.GetManagedObject();
            if (registry == null)
            {
                FlowBloxProject project = FlowBloxProjectManager.Instance.ActiveProject;
                if (project != null)
                    registry = project.FlowBloxRegistry;
            }
            return registry;
        }

        public static FlowBloxRegistry OpenTransaction(bool detached = false)
        {
            if (CurrentlyInUse)
                throw new RegistryCurrentlyInUseException();

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

        public static IDisposable MarkRegistryInUse()
        {
            if (_registryChain.Any())
                throw new RegistryCurrentlyInUseException();

            Interlocked.Increment(ref _currentlyInUseCount);
            return new RegistryUseScope();
        }

        private sealed class RegistryUseScope : IDisposable
        {
            private int _disposed;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                Interlocked.Decrement(ref _currentlyInUseCount);
            }
        }
    }
}