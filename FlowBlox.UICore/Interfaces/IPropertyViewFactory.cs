using System.Windows;

namespace FlowBlox.UICore.Interfaces
{
    public interface IPropertyWindowViewFactory
    {
        Window Create(object instance, object target, bool readOnly);

        bool CanCreate(object instance, object target, bool readOnly, out string message);

        bool SupportsType(Type type);
    }
}
