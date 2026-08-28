using System;
using System.Windows.Forms;

namespace FlowBlox.Util.WPF
{
    public class Win32WindowWrapper : IWin32Window
    {
        private readonly nint _handle;

        public Win32WindowWrapper(nint handle)
        {
            _handle = handle;
        }

        public nint Handle => _handle;
    }
}