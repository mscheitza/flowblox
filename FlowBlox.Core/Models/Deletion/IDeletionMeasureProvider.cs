namespace FlowBlox.Core.Models.Deletion
{
    /// <summary>
    /// Allows a model to replace low-level reference changes with the actual
    /// domain-level measure shown in and executed from a deletion report.
    /// </summary>
    public interface IDeletionMeasureProvider
    {
        void ContributeDeletionMeasures(IList<DeletionDependency> dependencies);
    }
}
