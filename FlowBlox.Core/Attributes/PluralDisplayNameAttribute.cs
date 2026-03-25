using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class PluralDisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public Type ResourceType { get; }

        public PluralDisplayNameAttribute(string name, Type resourceType)
        {
            Name = name;
            ResourceType = resourceType ?? FlowBloxResourceUtil.DefaultResourceType;
        }
    }
}
