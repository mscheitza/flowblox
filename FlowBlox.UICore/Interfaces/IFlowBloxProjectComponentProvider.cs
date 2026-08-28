using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Models;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxProjectComponentProvider
    {
        event EventHandler SelectedFlowBlocksChanged;

        ProjectChangelist GetCurrentChangelist();

        FlowBloxRegistry GetCurrentRegistry();

        IReadOnlyCollection<BaseFlowBlock> GetSelectedFlowBlocks();

        void SetSelectedFlowBlocks(IEnumerable<BaseFlowBlock> flowBlocks);
    }
}
