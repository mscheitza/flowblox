using FlowBlox.Grid.Elements.UserControls;
using System;

namespace FlowBlox.Grid.Events
{
    public class FlowBlockUIElementRegisteredEventArgs : EventArgs
    {
        public FlowBlockUIElementRegisteredEventArgs(FlowBlockUIElement uiElement)
        {
            UIElement = uiElement ?? throw new ArgumentNullException(nameof(uiElement));
        }

        public FlowBlockUIElement UIElement { get; }
    }
}
