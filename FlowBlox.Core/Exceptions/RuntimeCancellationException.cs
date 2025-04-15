using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Exceptions
{
    public class RuntimeCancellationException : Exception
    {
        public RuntimeCancellationException()
        {
        }

        public RuntimeCancellationException(string message) : base(message)
        {
        }
    }
}
