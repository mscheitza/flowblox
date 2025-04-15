using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.Toolbox
{
    [Serializable()]
    public class ToolboxElementSummary
    {
        public List<ToolboxElement> ToolboxElements { get; set; }
    }
}
