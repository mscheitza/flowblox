namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockListViewAttribute : Attribute
    {
        public string[] LVColumnMemberNames { get; set; }

        public Type LVItemFactory { get; set; }

        public bool IsMovable { get; set; }

        public FlowBlockListViewAttribute()
        {
            LVColumnMemberNames = [];
            LVItemFactory = null;
        }
    }
}
