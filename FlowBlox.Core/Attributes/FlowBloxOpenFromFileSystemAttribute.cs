namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FlowBloxOpenFromFileSystemAttribute : Attribute
    {
        public string InitialDirectoryMethod { get; set; } = string.Empty;
    }
}
