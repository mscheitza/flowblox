using System;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockDataGridAttribute : Attribute
    {
        public FlowBlockDataGridAttribute()
        {
            
        }

        public bool IsMovable { get; set; }
    }
}
