using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestResult
    {
        public FlowBloxTestResult(bool success, Dictionary<string, string> fieldValueAssignments)
        {
            Success = success;
            FieldValueAssignments = fieldValueAssignments;
        }

        public bool Success { get; set; }

        public Dictionary<string, string> FieldValueAssignments { get; set; }
    }
}
