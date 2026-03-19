using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxUIElement
    {
        BaseFlowBlock InternalFlowBlock { get; }

        bool ElementSelected { get; }

        event EventHandler ElementSelectedChangedByUser;
    }
}