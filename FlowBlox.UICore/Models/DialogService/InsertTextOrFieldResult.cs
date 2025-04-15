using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.DialogService
{
    public class InsertTextOrFieldResult
    {
        public bool Success { get; set; }
        public string InsertedValue { get; set; }
        public bool IsSelectedFieldRequired { get; set; }
        public FieldElement SelectedField { get; set; }
    }
}
