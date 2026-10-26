using FlowBlox.Core.Actions;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBloxActionHistoryService
    {
        void RegisterAction(FlowBloxBaseAction action);
        void RegisterAutoLayoutMoves(IReadOnlyList<FlowBloxMoveAction> moveActions);
    }
}
