using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Actions
{
    public static class FlowBloxPendingActionStore
    {
        private static readonly Dictionary<FlowBloxRegistry, List<FlowBloxBaseAction>> Actions = new(ReferenceEqualityComparer.Instance);

        public static void Add(FlowBloxRegistry registry, FlowBloxBaseAction action)
        {
            if (registry == null || action == null)
                return;
            if (!Actions.TryGetValue(registry, out var actions))
            {
                actions = new List<FlowBloxBaseAction>();
                Actions.Add(registry, actions);
            }
            actions.Add(action);

            if (registry.SupportsDeletionStaging)
                StageDeletedTargets(registry, action);
        }

        private static void StageDeletedTargets(
            FlowBloxRegistry registry,
            FlowBloxBaseAction action)
        {
            switch (action)
            {
                case FlowBloxDeleteAction flowBlockDelete:
                    registry.StageDeletion(flowBlockDelete.FlowBlock);
                    break;
                case FlowBloxManagedObjectDeleteAction managedObjectDelete:
                    registry.StageDeletion(managedObjectDelete.ManagedObject);
                    break;
                case FlowBloxReactiveObjectDeleteAction reactiveObjectDelete:
                    foreach (var managedObject in reactiveObjectDelete.ManagedObjects)
                        registry.StageDeletion(managedObject);
                    break;
            }

            foreach (var associatedAction in action.AssociatedActions)
                StageDeletedTargets(registry, associatedAction);
        }

        public static IReadOnlyList<FlowBloxBaseAction> Take(FlowBloxRegistry registry)
        {
            if (registry == null || !Actions.Remove(registry, out var actions))
                return Array.Empty<FlowBloxBaseAction>();
            return actions;
        }
    }
}
