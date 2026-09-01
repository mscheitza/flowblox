namespace FlowBlox.Core.Exceptions
{
    public sealed class RegistryCurrentlyInUseException : InvalidOperationException
    {
        public RegistryCurrentlyInUseException()
            : base("The FlowBlox registry is currently in use.")
        {
        }
    }
}
