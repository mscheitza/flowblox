using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.DialogService
{
    public class FieldSelectionResult
    {
        public bool Success { get; set; }
        public List<FieldElement> SelectedFields { get; set; }
        public bool IsRequired { get; set; }
        public BaseFlowBlock Target { get; set; }
    }
}   
