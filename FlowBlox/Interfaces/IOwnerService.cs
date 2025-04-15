using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Interfaces
{
    public interface IOwnerService
    {
        /// <summary>
        /// Returns the current owner window, preferably the last modal window.
        /// If no modal window is found, returns the last non-modal window.
        /// </summary>
        /// <returns>The current owner window</returns>
        IWin32Window GetCurrentOwner();
    }
}