using System;
using System.Runtime.Serialization;

namespace FlowBlox.Core.Exceptions
{
    public class GridElementExecutionException : Exception
    {
        public GridElementExecutionException()
        {
        }

        public GridElementExecutionException(string message) : base(message)
        {
        }

        public GridElementExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GridElementExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
