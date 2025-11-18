using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Grid.Elements;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Grid.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlowBlox.Grid
{
    public class FlowBloxUIRegistry
    {
        private readonly Dictionary<BaseFlowBlock, FlowBlockUIElement> _gridElementToUIElement;

        public IEnumerable<FlowBlockUIElement> UIElements => _gridElementToUIElement.Values;

        /// <summary>
        /// Triggered after a new FlowBlockUIElement is registered.
        /// </summary>
        public event EventHandler<FlowBlockUIElementRegisteredEventArgs> FlowBlockUIElementRegistered;

        public FlowBloxUIRegistry()
        {
            _gridElementToUIElement = new Dictionary<BaseFlowBlock, FlowBlockUIElement>();
        }

        public void RegisterGridUIElement(FlowBlockUIElement uiElement)
        {
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            var flowBlock = uiElement.InternalFlowBlock;
            if (flowBlock == null)
                throw new ArgumentNullException(nameof(uiElement));

            if (!_gridElementToUIElement.ContainsKey(flowBlock))
            {
                _gridElementToUIElement[flowBlock] = uiElement;
                OnFlowBlockUIElementRegistered(uiElement);
            }
        }

        public FlowBlockUIElement GetUIElementToGridElement(BaseFlowBlock gridElement)
        {
            if (gridElement == null)
                return null;

            if (_gridElementToUIElement.TryGetValue(gridElement, out var uiElement))
                return uiElement;

            return null;
        }

        internal void RemoveUIElement(FlowBlockUIElement recentElement)
        {
            if (recentElement == null)
                return;

            var gridElement = recentElement.InternalFlowBlock;
            _gridElementToUIElement.Remove(gridElement);
        }

        protected virtual void OnFlowBlockUIElementRegistered(FlowBlockUIElement uiElement)
        {
            FlowBlockUIElementRegistered?.Invoke(
                this,
                new FlowBlockUIElementRegisteredEventArgs(uiElement));
        }
    }
}
