using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FlowBlockUIGroupAttribute : Attribute
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public ControlAlignment ControlAlignment { get; set; }

        public FlowBlockUIGroupAttribute()
        {

        }

        public FlowBlockUIGroupAttribute(string name, int order, ControlAlignment controlAlignment = ControlAlignment.Fill)
        {
            Name = name;
            Order = order;
            ControlAlignment = controlAlignment;
        }
    }

    public enum ControlAlignment
    {
        Fill,
        Top
    }
}
