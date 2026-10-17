namespace FlowBlox.Core.Attributes
{
    /// <summary>
    /// Customizes how collection items are rendered on a FlowBlock card.
    /// Without this attribute, the card renderer keeps its default behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FlowBloxCardListAttribute : Attribute
    {
        /// <summary>
        /// Overrides the item title rendered before the item number.
        /// Resource lookup uses the collection property's DisplayAttribute resource type.
        /// When omitted, the item type's display name is used.
        /// </summary>
        public string ItemDisplayName { get; set; }

        /// <summary>
        /// Specifies the item properties rendered on the card, in display order.
        /// When empty, all eligible public properties are rendered.
        /// </summary>
        public string[] DisplayPropertyNames { get; set; } = [];
    }
}
