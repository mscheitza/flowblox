using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Views.PropertyView
{
    public interface IValidatableFactory<T> where T : Control
    {
        bool Validate(T control);
    }
}
