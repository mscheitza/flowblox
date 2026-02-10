using System.Windows;

namespace FlowBlox.UICore.Interfaces
{
    public interface IPropertyWindowViewFactory
    {
        Window Create(object instance, object target, bool readOnly);

        bool SupportsType(Type type);
    }
}
