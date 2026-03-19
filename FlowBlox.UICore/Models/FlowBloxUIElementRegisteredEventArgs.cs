using FlowBlox.UICore.Interfaces;

namespace FlowBlox.UICore.Models
{
    public sealed class FlowBloxUIElementRegisteredEventArgs : EventArgs
    {
        public IFlowBloxUIElement UIElement { get; }

        public FlowBloxUIElementRegisteredEventArgs(IFlowBloxUIElement uiElement)
        {
            UIElement = uiElement ?? throw new ArgumentNullException(nameof(uiElement));
        }
    }
}
