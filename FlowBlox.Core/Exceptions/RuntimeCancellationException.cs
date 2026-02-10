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
