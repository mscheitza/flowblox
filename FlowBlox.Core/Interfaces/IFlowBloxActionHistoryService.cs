using FlowBlox.Core.Actions;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxActionHistoryService
    {
        void RegisterAutoLayoutMoves(IReadOnlyList<FlowBloxMoveAction> moveActions);
    }
}
