using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models.DialogService
{
    public class EditValueResult
    {
        public bool Success { get; set; }
        public string Value { get; set; }
        public bool IsMaskedRegexString { get; set; }
    }
}
