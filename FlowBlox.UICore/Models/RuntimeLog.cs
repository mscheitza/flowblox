using FlowBlox.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.UICore.Models
{
    public class RuntimeLog
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public FlowBloxLogLevel LogLevel { get; set; }
    }
}
