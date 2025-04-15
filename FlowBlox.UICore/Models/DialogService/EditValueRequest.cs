using FlowBlox.UICore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.DialogService
{
    public class EditValueRequest
    {
        public string Value { get; set; }

        public EditMode EditMode { get; set; }

        public string ParameterName { get; set; }

        public bool IsRegex { get; set; }

        public bool IsMultiline { get; set; }
    }
}
