namespace FlowBlox.Core.Attributes
{
    /// <summary>
    /// Indicates that a property referencing a flow block can be resolved via a custom member (property or parameterless method) on the same object, and optionally a display condition.
    /// <example>
    /// <code>
    /// [CustomFlowBlockResolvable(nameof(InputReference), nameof(CanDisplayAssociatedInputReferenceHint))]
    /// public BaseFlowBlock AssociatedInputReference { get; set; }
    ///
    /// public BaseFlowBlock InputReference => ... // returns a BaseFlowBlock
    /// public bool CanDisplayAssociatedInputReferenceHint => ReferencedFlowBlocks.Count() > 1;
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AssociatedFlowBlockResolvableCustomAttribute : Attribute
    {
        /// <summary>
        /// Name of the member (property or parameterless method) that should return a BaseFlowBlock used as the resolved flow block.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Optional name of a member (property or parameterless method) that returns a bool.
        /// <para>If provided and returns false, no hint text should be displayed.</para>
        /// </summary>
        public string DisplayCondition { get; set; }

        public AssociatedFlowBlockResolvableCustomAttribute(string memberName)
        {
            MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
        }

        public AssociatedFlowBlockResolvableCustomAttribute(string memberName, string displayCondition)
            : this(memberName)
        {
            DisplayCondition = displayCondition;
        }
    }
}
