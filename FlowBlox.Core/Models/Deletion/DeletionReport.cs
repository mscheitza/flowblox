using System.Reflection;

namespace FlowBlox.Core.Models.Deletion
{
    public enum DeletionImpact
    {
        DirectReferenceRemoved,
        CollectionEntryRemoved,
        FieldReferenceInvalidated
    }

    public enum DeletionReviewStatus
    {
        Information,
        ReviewRecommended,
        ManualActionRequired
    }

    public sealed class DeletionDependency
    {
        public required object ReferencingObject { get; init; }
        public required PropertyInfo Property { get; init; }
        public required object ReferencedObject { get; init; }
        public required DeletionImpact Impact { get; init; }
        public required DeletionReviewStatus ReviewStatus { get; init; }
        public bool AutomaticallyRemoved => Impact != DeletionImpact.FieldReferenceInvalidated;
        public object EditTarget { get; init; }
        public int CollectionIndex { get; init; } = -1;
    }

    public sealed class DeletionReport
    {
        public IReadOnlyList<DeletionDependency> Dependencies { get; init; } = Array.Empty<DeletionDependency>();
        public IReadOnlyList<object> Targets { get; init; } = Array.Empty<object>();
        public bool HasDependencies => Dependencies.Count > 0;
    }
}
