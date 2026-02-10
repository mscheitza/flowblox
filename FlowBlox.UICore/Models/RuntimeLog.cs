using FlowBlox.Core.Enums;

namespace FlowBlox.UICore.Models
{
    public class RuntimeLog
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public FlowBloxLogLevel LogLevel { get; set; }
    }
}
