using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.ViewModels.FieldSelection
{
    public sealed class OptionRowViewModel
    { 
        public OptionElement OptionElement { get; }  
        public string Name => OptionElement?.Name ?? "";
        public string Type => OptionElement?.Type.ToString() ?? "";
        public string Description => OptionElement?.Description ?? "";
        public string Value => OptionElement?.Value ?? "";

        public OptionRowViewModel(OptionElement element)
        {
            OptionElement = element;
        }
    }
}