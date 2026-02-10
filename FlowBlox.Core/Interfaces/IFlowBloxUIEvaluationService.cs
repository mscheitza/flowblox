namespace FlowBlox.Core.Interfaces
{
    /// <summary>
    /// Provides a runtime evaluation indicating whether the current FlowBlox application
    /// instance supports user interface (UI) features such as WPF components.
    /// This service is only registered in UI-capable environments.
    /// </summary>
    public interface IFlowBloxUIEvaluationService
    {
        /// <summary>
        /// Returns <c>true</c> to indicate that the current environment supports UI features.
        /// </summary>
        /// <returns><c>true</c> if UI is supported; otherwise, <c>false</c>.</returns>
        bool IsUISupported();
    }
}
