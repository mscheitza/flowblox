using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FlowBlox.Core.Provider
{
    public static class FlowBloxRegistryProvider
    {
        private static List<FlowBloxRegistry> _registryChain = new List<FlowBloxRegistry>();

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

        public static FlowBloxRegistry OpenTransaction()
        {
            if (!_registryChain.Any())
                _registryChain.Add(GetRegistry());

            _registryChain.Add(new FlowBloxTransientRegistry(_registryChain.Last()));
            return _registryChain.Last();
        }

        public static void CommitTransaction()
        {
            var currentRegistry = _registryChain.Last();
            if (!(currentRegistry is FlowBloxTransientRegistry))
                throw new InvalidOperationException($"The current registry is not from type \"{nameof(FlowBloxTransientRegistry)}\".");

            ((FlowBloxTransientRegistry)currentRegistry).Commit();
            RemoveFromChain(currentRegistry);
        }

        public static void CancelTransaction() => RemoveFromChain(_registryChain.Last());

        public static void RemoveFromChain(FlowBloxRegistry registry)
        {
            _registryChain.Remove(registry);
            if (_registryChain.Count == 1)
                _registryChain.RemoveAt(0);
        }
    }
}
