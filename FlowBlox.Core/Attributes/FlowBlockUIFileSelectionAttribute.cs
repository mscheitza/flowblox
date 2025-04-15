using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Attributes
{
    namespace FlowBlox.Core.Attributes
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class FlowBlockUIFileSelectionAttribute : Attribute
        {
            public string Filter { get; set; }

            public FlowBlockUIFileSelectionAttribute(string filter)
            {
                Filter = filter;
            }
        }
    }
}
