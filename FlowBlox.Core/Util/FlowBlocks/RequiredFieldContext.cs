using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public class RequiredFieldContext
    {
        public FieldElement FieldElement { get; set; }

        public FlowBloxComponent FlowBloxComponent { get; set; }
    }
}
