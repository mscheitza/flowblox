using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.PropertyView
{
    public class AssociationBeforeLinkResult
    {
        public required bool Cancel { get; init; }

        public object LinkedObject { get; init; }
    }
}
