using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class FlowBloxSupportedTypesAttribute : Attribute
    {
        public FlowBloxSupportedTypesAttribute(params Type[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
        }

        public Type[] SupportedTypes { get; }
    }
}
