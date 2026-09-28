using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Interfaces
{
    public interface IPropertyViewNestedTransaction
    {
        bool HasPendingChanges { get; }

        Task<bool> PrepareHostCommitAsync(MetroWindow window, bool withoutVerification);

        void Cancel();
    }
}
