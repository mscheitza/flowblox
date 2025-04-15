using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlowBlox.Core.Models.Components;
using FlowBlox.Grid.Elements;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Grid.Elements.UserControls;
using FlowBlox.Core.Provider;

namespace FlowBlox.Grid
{
    public class FlowBloxUIRegistry
    {
        private Dictionary<BaseFlowBlock, FlowBlockUIElement> _gridElementToUIElement;

        public IEnumerable<FlowBlockUIElement> UIElements => _gridElementToUIElement.Values;

        public FlowBloxUIRegistry() 
        {
            _gridElementToUIElement = new Dictionary<BaseFlowBlock, FlowBlockUIElement>();
        }

        public void RegisterGridUIElement(FlowBlockUIElement uiElement)
        {
            if (!_gridElementToUIElement.ContainsKey(uiElement.InternalFlowBlock))
                _gridElementToUIElement[uiElement.InternalFlowBlock] = uiElement;
        }

        public FlowBlockUIElement GetUIElementToGridElement(BaseFlowBlock gridElement)
        {
            if (_gridElementToUIElement.ContainsKey(gridElement))
                return _gridElementToUIElement[gridElement];
            
            return null;
        }

        internal void RemoveUIElement(FlowBlockUIElement recentElement)
        {
            var gridElement = recentElement.InternalFlowBlock;
            this._gridElementToUIElement.Remove(gridElement);
        }
    }
}
