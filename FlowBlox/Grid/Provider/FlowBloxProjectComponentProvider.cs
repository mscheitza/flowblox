using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Grid.Provider
{
    public class FlowBloxProjectComponentProvider : IFlowBloxProjectComponentProvider
    {
        private FlowBloxRegistry _currentRegistry;
        private FlowBloxUIRegistry _currentUIRegistry;
        private ProjectChangelist _currentChangelist;

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
        }

        public FlowBloxUIRegistry GetCurrentUIRegistry() => _currentUIRegistry;
        public ProjectChangelist GetCurrentChangelist() => _currentChangelist;
        public FlowBloxRegistry GetCurrentRegistry() => _currentRegistry;
    }
}
