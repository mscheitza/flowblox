using System.Collections;
using System.Reflection;
using FlowBlox.Core.Actions;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Deletion;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Validations;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Util
{
    public static class FlowBloxDeletionHelper
    {
        public static DeletionReport Analyze(FlowBloxRegistry registry, params object[] instances)
            => Analyze(registry, (IEnumerable<object>)instances);

        public static DeletionReport Analyze(FlowBloxRegistry registry, IEnumerable<object> instances)
            => Analyze(registry, instances, dependencyContextsToExclude: null);

        public static DeletionReport Analyze(
            FlowBloxRegistry registry,
            IEnumerable<object> instances,
            IEnumerable<object> dependencyContextsToExclude)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var targets = (instances ?? Enumerable.Empty<object>())
                .Where(x => x != null)
                .Distinct(ReferenceEqualityComparer.Instance)
                .ToList();

            var deletionScope = BuildDeletionScope(registry, targets);
            var dependencyContexts = BuildReferenceSet(registry, dependencyContextsToExclude);
            var directTargets = BuildReferenceSet(registry, targets);
            var deletedFieldNames = deletionScope
                .OfType<FieldElement>()
                .Select(x => x.FullyQualifiedName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.Ordinal);

            var dependencies = new List<DeletionDependency>();
            var roots = registry.GetFlowBlocks().Cast<object>()
                .Concat(registry.GetManagedObjects().Cast<object>())
                .Where(x => !deletionScope.Contains(x))
                .ToList();

            FlowBloxObjectGraphVisitor.VisitPropertiesRecursive(roots, (root, owner, property, value) =>
            {
                if (deletionScope.Contains(owner))
                    return;

                if (deletionScope.Contains(value))
                {
                    if (!IsPropertyContextTargetReference(
                            root, owner, value, directTargets, dependencyContexts))
                    {
                        dependencies.Add(CreateDependency(
                            owner, property, value, DeletionImpact.DirectReferenceRemoved, editTarget: root));
                    }
                    return;
                }

                if (value is IList collection)
                {
                    for (var i = 0; i < collection.Count; i++)
                    {
                        var item = collection[i];
                        if (item != null && deletionScope.Contains(item) &&
                            !IsPropertyContextTargetReference(
                                root, owner, item, directTargets, dependencyContexts))
                        {
                            dependencies.Add(CreateDependency(owner, property, item, DeletionImpact.CollectionEntryRemoved, i, root));
                        }
                    }
                }

                if (value is string text && deletedFieldNames.Count > 0 &&
                    property.GetCustomAttribute<FlowBloxUIAttribute>()?.UiOptions
                        .HasFlag(UIOptions.EnableFieldSelection) == true)
                {
                    foreach (Match match in Regex.Matches(text, BaseFlowBlock.Regex_FullyQualifiedFieldNames))
                    {
                        if (!deletedFieldNames.Contains(match.Value))
                            continue;

                        var referencedField = deletionScope
                            .OfType<FieldElement>()
                            .First(x => string.Equals(x.FullyQualifiedName, match.Value, StringComparison.Ordinal));

                        dependencies.Add(CreateDependency(
                            owner, property, referencedField, DeletionImpact.FieldReferenceInvalidated,
                            editTarget: root));
                    }
                }
            });

            var measureProviders = registry.GetFlowBlocks().Cast<object>()
                .Concat(registry.GetManagedObjects().Cast<object>())
                .OfType<IDeletionMeasureProvider>();

            foreach (var provider in measureProviders)
                provider.ContributeDeletionMeasures(dependencies);

            dependencies = dependencies
                .GroupBy(x => new DependencyKey(
                    x.ReferencingObject, x.Property, x.ReferencedObject, x.Impact, x.CollectionIndex),
                    DependencyKeyComparer.Instance)
                .Select(x => x.First())
                .ToList();

            return new DeletionReport
            {
                Targets = targets,
                Dependencies = dependencies
            };
        }

        public static FlowBloxBaseAction CreateAction(FlowBloxRegistry registry, DeletionReport report)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            var targetActions = report.Targets.Select(x => CreateTargetAction(registry, x)).ToList();
            if (targetActions.Count == 0)
                throw new InvalidOperationException("A deletion action requires at least one target.");

            var rootAction = targetActions[0];
            rootAction.AssociatedActions.AddRange(CreateEditActions(report.Dependencies));
            rootAction.AssociatedActions.AddRange(CreateDisconnectActions(report.Dependencies));
            rootAction.AssociatedActions.AddRange(targetActions.Skip(1));
            return rootAction;
        }

        private static IEnumerable<FlowBloxEditAction> CreateEditActions(
            IEnumerable<DeletionDependency> dependencies)
        {
            foreach (var group in dependencies
                         .Where(x => x.AutomaticallyRemoved && !IsGraphConnection(x))
                         .GroupBy(x => x.EditTarget ?? x.ReferencingObject, ReferenceEqualityComparer.Instance))
            {
                var changes = group.ToList();
                var action = new FlowBloxEditAction { Target = group.Key };
                action.CaptureRestoreState();
                action.CaptureRepetitionState((_, copier) => ApplyChanges(copier, changes));
                yield return action;
            }
        }

        private static IEnumerable<FlowBloxDisconnectAction> CreateDisconnectActions(
            IEnumerable<DeletionDependency> dependencies)
        {
            return dependencies
                .Where(IsGraphConnection)
                .Select(x => new FlowBloxDisconnectAction
                {
                    From = (BaseFlowBlock)x.ReferencedObject,
                    To = (BaseFlowBlock)x.ReferencingObject
                });
        }

        private static bool IsGraphConnection(DeletionDependency dependency)
            => dependency.Impact == DeletionImpact.CollectionEntryRemoved &&
               dependency.ReferencingObject is BaseFlowBlock &&
               dependency.ReferencedObject is BaseFlowBlock &&
               dependency.Property.Name == nameof(BaseFlowBlock.ReferencedFlowBlocks);

        private static void ApplyChanges(
            DynamicDeepCopier copier,
            IReadOnlyCollection<DeletionDependency> changes)
        {
            foreach (var change in changes.Where(x => x.Impact == DeletionImpact.DirectReferenceRemoved))
                change.Property.SetValue(copier.ResolveCopy(change.ReferencingObject), null);

            foreach (var propertyChanges in changes
                         .Where(x => x.Impact == DeletionImpact.CollectionEntryRemoved)
                         .GroupBy(x => x.Property))
            {
                var ownerCopy = copier.ResolveCopy(propertyChanges.First().ReferencingObject);
                if (propertyChanges.Key.GetValue(ownerCopy) is not IList collection)
                    continue;

                foreach (var change in propertyChanges.OrderByDescending(x => x.CollectionIndex))
                {
                    if (change.CollectionIndex >= 0 && change.CollectionIndex < collection.Count)
                        collection.RemoveAt(change.CollectionIndex);
                }
            }
        }

        private static FlowBloxBaseAction CreateTargetAction(FlowBloxRegistry registry, object target)
        {
            if (target is BaseFlowBlock flowBlock)
            {
                return new FlowBloxDeleteAction
                {
                    FlowBlock = flowBlock,
                    HistoryFlowBlock = (BaseFlowBlock)registry.GetOriginalRef(flowBlock)
                };
            }

            if (target is IManagedObject managedObject)
            {
                return new FlowBloxManagedObjectDeleteAction
                {
                    ManagedObject = managedObject,
                    HistoryManagedObject = (IManagedObject)registry.GetOriginalRef(managedObject)
                };
            }

            if (target is FlowBloxReactiveObject reactiveObject &&
                FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(reactiveObject))
            {
                var managedObjects = GetNestedManagedObjects(target)
                    .Distinct()
                    .ToList();
                return new FlowBloxReactiveObjectDeleteAction
                {
                    ManagedObjects = managedObjects,
                    HistoryManagedObjects = managedObjects
                        .Select(x => (IManagedObject)registry.GetOriginalRef(x))
                        .ToList()
                };
            }

            throw new NotSupportedException($"No deletion action is available for {target.GetType().FullName}.");
        }

        public static DeletionReport Delete(FlowBloxRegistry registry, params object[] instances)
        {
            var report = Analyze(registry, instances);
            CreateAction(registry, report).Invoke();
            return report;
        }

        public static void DeleteInstance(FlowBloxRegistry registry, object instance)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (instance == null)
                return;

            var deletedInstances = new HashSet<object>(ReferenceEqualityComparer.Instance);
            DeleteInstanceRecursive(registry, instance, deletedInstances);
        }

        private static HashSet<object> BuildDeletionScope(FlowBloxRegistry registry, IEnumerable<object> targets)
        {
            var result = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var target in targets)
            {
                AddWithOriginalReference(registry, result, target);
                if (target is BaseFlowBlock flowBlock)
                {
                    foreach (var managedObject in flowBlock.DefinedManagedObjects)
                        AddWithOriginalReference(registry, result, managedObject);
                }

                if (IsNestedRowDeleteScope(target))
                {
                    foreach (var managedObject in GetNestedManagedObjects(target))
                        AddWithOriginalReference(registry, result, managedObject);
                }
            }
            return result;
        }

        private static HashSet<object> BuildReferenceSet(
            FlowBloxRegistry registry,
            IEnumerable<object> values)
        {
            var result = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var value in values ?? Enumerable.Empty<object>())
            {
                if (value == null)
                    continue;

                AddWithOriginalReference(registry, result, value);
            }

            return result;
        }

        private static bool IsPropertyContextTargetReference(
            object root,
            object owner,
            object referencedObject,
            IReadOnlySet<object> directTargets,
            IReadOnlySet<object> dependencyContexts)
        {
            return directTargets.Contains(referencedObject) &&
                   (dependencyContexts.Contains(root) || dependencyContexts.Contains(owner));
        }

        private static void AddWithOriginalReference(
            FlowBloxRegistry registry,
            ISet<object> result,
            object value)
        {
            result.Add(value);
            if (value is IFlowBloxComponent component)
                result.Add(registry.GetOriginalRef(component));
        }

        private static DeletionDependency CreateDependency(
            object owner,
            PropertyInfo property,
            object referencedObject,
            DeletionImpact impact,
            int collectionIndex = -1,
            object editTarget = null)
        {
            var reviewStatus = impact switch
            {
                DeletionImpact.CollectionEntryRemoved => DeletionReviewStatus.Information,
                DeletionImpact.FieldReferenceInvalidated => DeletionReviewStatus.ManualActionRequired,
                _ => ReactiveObjectRequiredStateChecker.IsRequired(owner, property)
                    ? DeletionReviewStatus.ManualActionRequired
                    : DeletionReviewStatus.ReviewRecommended
            };
            return new DeletionDependency
            {
                ReferencingObject = owner,
                Property = property,
                ReferencedObject = referencedObject,
                Impact = impact,
                ReviewStatus = reviewStatus,
                EditTarget = editTarget ?? owner,
                CollectionIndex = collectionIndex
            };
        }

        private readonly record struct DependencyKey(
            object Owner,
            PropertyInfo Property,
            object ReferencedObject,
            DeletionImpact Impact,
            int CollectionIndex);

        private sealed class DependencyKeyComparer : IEqualityComparer<DependencyKey>
        {
            public static DependencyKeyComparer Instance { get; } = new();

            public bool Equals(DependencyKey x, DependencyKey y) =>
                ReferenceEquals(x.Owner, y.Owner) &&
                Equals(x.Property, y.Property) &&
                ReferenceEquals(x.ReferencedObject, y.ReferencedObject) &&
                x.Impact == y.Impact &&
                x.CollectionIndex == y.CollectionIndex;

            public int GetHashCode(DependencyKey obj) => HashCode.Combine(
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Owner),
                obj.Property,
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.ReferencedObject),
                obj.Impact,
                obj.CollectionIndex);
        }

        private static void DeleteInstanceRecursive(
            FlowBloxRegistry registry,
            object instance,
            ISet<object> deletedInstances)
        {
            if (instance == null || !deletedInstances.Add(instance))
                return;

            if (IsNestedRowDeleteScope(instance))
            {
                foreach (var nestedManagedObject in GetNestedManagedObjects(instance))
                {
                    DeleteInstanceRecursive(registry, nestedManagedObject, deletedInstances);
                }
            }

            registry.Unregister(instance);
        }

        private static bool IsNestedRowDeleteScope(object instance)
        {
            return FlowBloxReactiveObjectHelper.IsReactiveObjectOnly(instance);
        }

        private static IEnumerable<IManagedObject> GetNestedManagedObjects(object instance)
        {
            var instanceType = instance.GetType();
            var properties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                var uiAttribute = property.GetCustomAttribute<FlowBloxUIAttribute>();
                if (!IsRowManagedAssociation(uiAttribute))
                    continue;

                var propertyValue = property.GetValue(instance);
                if (propertyValue is IManagedObject managedObject)
                {
                    yield return managedObject;
                    continue;
                }

                if (propertyValue is IEnumerable enumerable && property.PropertyType != typeof(string))
                {
                    foreach (var childManagedObject in enumerable.OfType<IManagedObject>())
                    {
                        yield return childManagedObject;
                    }
                }
            }
        }

        private static bool IsRowManagedAssociation(FlowBloxUIAttribute? uiAttribute)
        {
            if (uiAttribute?.Factory != UIFactory.Association)
                return false;

            var operations = uiAttribute.Operations;
            var supportsCreate = operations.HasFlag(UIOperations.Create);
            var supportsDelete = operations.HasFlag(UIOperations.Delete);
            var supportsLink = operations.HasFlag(UIOperations.Link);
            var supportsUnlink = operations.HasFlag(UIOperations.Unlink);

            return supportsCreate && supportsDelete && !supportsLink && !supportsUnlink;
        }
    }
}
