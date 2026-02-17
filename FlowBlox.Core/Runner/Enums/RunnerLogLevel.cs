using FlowBlox.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Runner.Contracts
{
    public sealed class RunnerLogMessage
    {
        public DateTime UtcTimestamp { get; set; } = DateTime.UtcNow;
        public FlowBloxLogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
}