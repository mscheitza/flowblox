using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models
{
    public class NotificationEventArgs
    {
        public NotificationEventArgs(string message) 
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}
