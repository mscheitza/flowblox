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
