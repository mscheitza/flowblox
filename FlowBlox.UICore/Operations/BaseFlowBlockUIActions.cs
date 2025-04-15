using FlowBlox.Core;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Views;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Operations
{
    public class BaseFlowBlockUIActions : ComponentUIActions<BaseFlowBlock>
    {
        public BaseFlowBlockUIActions(BaseFlowBlock component) : base(component)
        {
        }

        public bool CanGenerate()
        {
            if (!Component.TestDefinitions.Any()) 
                return false;

            if (!Component.GenerationStrategies.Any())
                return false;

            return true;
        }

        [Display(Name = "BaseResultFlowBlockUIActions_Generate", ResourceType = typeof(FlowBloxTexts))]
        public void Generate()
        {
            var generationView = new GenerationView(Component);
            var dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            dialogService.ShowWPFDialog(generationView);
        }
    }
}