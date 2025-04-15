using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Grid.Elements.Util
{
    internal class GridElementHelper
    {
        public static bool CanConnect(BaseResultFlowBlock source, BaseFlowBlock dest)
        {
            // TODO: Die Regeln nach den Kardinalitäten hinterlegen.
            return true;
        }
    }
}
