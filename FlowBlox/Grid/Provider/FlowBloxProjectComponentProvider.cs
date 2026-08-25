using System;
using System.Collections.Generic;
using System.Linq;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Models;

namespace FlowBlox.Grid.Provider
{
    public class FlowBloxProjectComponentProvider : IFlowBloxProjectComponentProvider
    {
        private FlowBloxRegistry _currentRegistry;
        private FlowBloxUIRegistry _currentUIRegistry;
        private ProjectChangelist _currentChangelist;
        private IReadOnlyCollection<BaseFlowBlock> _selectedFlowBlocks = Array.Empty<BaseFlowBlock>();

        public FlowBloxProjectComponentProvider()
        {
            FlowBloxProjectManager.Instance.ProjectChanged += HandleProjectChanged;
        }

        private void HandleProjectChanged(object sender, ProjectChangedEventArgs eventArgs)
        {
            var project = eventArgs.NewProject;
            if (project != null)
            {
                _currentRegistry = project.FlowBloxRegistry;
                _currentUIRegistry = new FlowBloxUIRegistry();
                _currentChangelist = new ProjectChangelist();
            }
            else
            {
                _currentRegistry = null;
                _currentUIRegistry = null;
                _currentChangelist = null;
            }

            SetSelectedFlowBlocks(Array.Empty<BaseFlowBlock>());
        }

        public event EventHandler SelectedFlowBlocksChanged;

        public FlowBloxUIRegistry GetCurrentUIRegistry() => _currentUIRegistry;
        
        IFlowBloxUIRegistry IFlowBloxProjectComponentProvider.GetCurrentUIRegistry() => _currentUIRegistry;

        public ProjectChangelist GetCurrentChangelist() => _currentChangelist;
        public FlowBloxRegistry GetCurrentRegistry() => _currentRegistry;

        public IReadOnlyCollection<BaseFlowBlock> GetSelectedFlowBlocks() => _selectedFlowBlocks;

        public void SetSelectedFlowBlocks(IEnumerable<BaseFlowBlock> flowBlocks)
        {
            var snapshot = (flowBlocks ?? Enumerable.Empty<BaseFlowBlock>())
                .Where(x => x != null)
                .Distinct()
                .ToList();

            if (_selectedFlowBlocks.SequenceEqual(snapshot))
                return;

            _selectedFlowBlocks = snapshot;
            SelectedFlowBlocksChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}