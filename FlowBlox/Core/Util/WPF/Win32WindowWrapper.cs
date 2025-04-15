using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.WPF
{
    public class Win32WindowWrapper : IWin32Window
    {
        private readonly IntPtr _handle;

        public Win32WindowWrapper(IntPtr handle)
        {
            _handle = handle;
        }

        public IntPtr Handle => _handle;
    }
}
