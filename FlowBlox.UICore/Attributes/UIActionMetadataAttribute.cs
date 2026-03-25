namespace FlowBlox.UICore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UIActionMetadataAttribute : Attribute
    {
        public bool OnlyShowInPropertyWindow { get; set; } = false;
    }
}
