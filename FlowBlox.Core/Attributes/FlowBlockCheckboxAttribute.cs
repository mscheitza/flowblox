using System;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockCheckboxAttribute : Attribute
    {
        /// <summary>
        /// Optional header label shown above the checkbox.
        /// This can be either:
        /// <list type="bullet">
        /// <item>A resource key that will be resolved via FlowBloxResourceUtil, or</item>
        /// <item>A literal text used as-is if no resource is found.</item>
        /// </list>
        /// </summary>
        public string HeaderLabel { get; set; }

        /// <summary>
        /// Optional resource type used for resolving the header label.
        /// </summary>
        public Type ResourceType { get; set; }

        public FlowBlockCheckboxAttribute()
        {
        }

        public FlowBlockCheckboxAttribute(string headerLabel)
        {
            HeaderLabel = headerLabel;
            ResourceType = null;
        }

        public FlowBlockCheckboxAttribute(string headerLabel, Type resourceType)
        {
            HeaderLabel = headerLabel;
            ResourceType = resourceType;
        }
    }
}
