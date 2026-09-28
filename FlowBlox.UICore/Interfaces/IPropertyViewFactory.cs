using System.Windows;

namespace FlowBlox.UICore.Interfaces
{
    public interface IPropertyWindowViewFactory
    {
        /// <summary>
        /// Creates a custom property window for the original component instance. Editable windows
        /// own their transaction lifecycle, including Apply, through <see cref="Manager.FlowBloxTransactionEventHandler"/>.
        /// </summary>
        Window Create(object instance, object target, bool readOnly);

        bool CanCreate(object instance, object target, bool readOnly, out string message);

        bool SupportsType(Type type);
    }
}
