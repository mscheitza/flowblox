namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBloxListViewAttribute : Attribute
    {
        public string[] LVColumnMemberNames { get; set; }

        public Type ItemFactory { get; set; }

        public bool IsMovable { get; set; }

        public FlowBloxListViewAttribute()
        {
            LVColumnMemberNames = [];
            ItemFactory = null;
        }
    }
}
