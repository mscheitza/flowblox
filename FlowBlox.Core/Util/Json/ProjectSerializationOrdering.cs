using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Util.Json
{
    internal static class ProjectSerializationOrdering
    {
        public static (List<FieldElement> UserFields, List<IManagedObject> ManagedObjects, List<BaseFlowBlock> FlowBlocks) CreateOrderedTopLevelCollections(FlowBloxRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var userFields = registry.GetUserFields().ToList();
            var managedObjects = OrderByReferenceDependencies(
                registry.GetManagedObjects(),
                managedObject => managedObject.GetAssociatedManagedObjects());
            var flowBlocks = OrderByReferenceDependencies(
                registry.GetFlowBlocks(),
                flowBlock => flowBlock.ReferencedFlowBlocks.ExceptNull());

            return (userFields, managedObjects, flowBlocks);
        }

        private static List<T> OrderByReferenceDependencies<T>(
            IEnumerable<T> items,
            Func<T, IEnumerable<T>> referencesResolver)
            where T : class
        {
            var orderedItems = items?.Where(x => x != null).ToList() ?? new List<T>();
            if (orderedItems.Count <= 1)
                return orderedItems;

            var itemSet = new HashSet<T>(orderedItems);
            var originalIndex = orderedItems
                .Select((item, index) => new { item, index })
                .ToDictionary(x => x.item, x => x.index);

            var dependencyGraph = orderedItems.ToDictionary(
                keySelector: item => item,
                elementSelector: item => new HashSet<T>(
                    referencesResolver(item)?
                        .Where(reference => reference != null && itemSet.Contains(reference))
                        ?? Enumerable.Empty<T>()));

            var dependents = orderedItems.ToDictionary(item => item, _ => new List<T>());
            foreach (var (item, dependencies) in dependencyGraph)
            {
                foreach (var dependency in dependencies)
                {
                    dependents[dependency].Add(item);
                }
            }

            foreach (var key in dependents.Keys.ToList())
            {
                dependents[key] = dependents[key]
                    .OrderBy(x => originalIndex[x])
                    .ToList();
            }

            var result = new List<T>(orderedItems.Count);
            var visited = new HashSet<T>();

            void Traverse(T current)
            {
                if (!visited.Add(current))
                    return;

                result.Add(current);

                foreach (var dependent in dependents[current])
                {
                    var dependencies = dependencyGraph[dependent];
                    if (dependencies.Count == 1 || dependencies.All(visited.Contains))
                        Traverse(dependent);
                }
            }

            var roots = orderedItems
                .Where(item => dependencyGraph[item].Count == 0)
                .OrderBy(item => originalIndex[item]);

            foreach (var root in roots)
                Traverse(root);

            foreach (var item in orderedItems.Where(item => !visited.Contains(item)))
            {
                result.Add(item);
            }

            return result;
        }
    }
}
