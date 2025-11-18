using FlowBlox.Grid.Elements.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
