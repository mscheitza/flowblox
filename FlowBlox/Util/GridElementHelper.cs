using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Util
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