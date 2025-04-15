using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlowBlox.UICore.Interfaces
{
    public interface IPropertyWindowViewFactory
    {
        Window Create(object instance, object target, bool readOnly);

        bool SupportsType(Type type);
    }
}
