namespace FlowBlox.UICore.Utilities
{
    using System.Windows;

    public static class SynchronizationContextHelper
    {
        public static void PostToUi(SynchronizationContext? uiContext, Action action)
        {
            if (action == null)
                return;

            if (uiContext != null && uiContext != SynchronizationContext.Current)
            {
                uiContext.Post(_ => action(), null);
                return;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(action);
                return;
            }

            action();
        }
    }
}
