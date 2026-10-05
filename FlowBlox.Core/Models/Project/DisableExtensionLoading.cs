namespace FlowBlox.Core.Models.Project
{
    /// <summary>
    /// Temporarily disables loading project extensions for the current logical execution context.
    /// </summary>
    public sealed class DisableExtensionLoading : IDisposable
    {
        private static readonly AsyncLocal<int> ScopeDepth = new();
        private bool _disposed;

        internal static bool IsActive => ScopeDepth.Value > 0;

        public DisableExtensionLoading()
        {
            ScopeDepth.Value++;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ScopeDepth.Value = Math.Max(0, ScopeDepth.Value - 1);
        }
    }
}
