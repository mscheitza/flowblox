using System;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlowBlockDataGridAttribute : Attribute
    {
        public FlowBlockDataGridAttribute()
        {
            GridColumnMemberNames = Array.Empty<string>();
        }

        public bool IsMovable { get; set; }

        /// <summary>
        /// If set, only these item properties are rendered as columns (whitelist).
        /// If empty, all eligible public instance properties are considered.
        /// </summary>
        public string[] GridColumnMemberNames { get; set; }
    }
}
