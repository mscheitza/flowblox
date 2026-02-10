using System.Windows.Forms;

namespace FlowBlox.Views.PropertyView
{
    public interface IValidatableFactory<T> where T : Control
    {
        bool Validate(T control);
    }
}
