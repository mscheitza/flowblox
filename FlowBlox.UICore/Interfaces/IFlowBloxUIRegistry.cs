using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Models;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxUIRegistry
    {
        IEnumerable<IFlowBloxUIElement> UIElements { get; }

        event EventHandler<FlowBloxUIElementRegisteredEventArgs> UIElementRegistered;

        IFlowBloxUIElement GetUIElementToGridElement(BaseFlowBlock gridElement);
    }
}
