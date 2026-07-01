using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    public sealed class AIChatRequest
    {
        public List<AIChatMessage> SystemMessages { get; } = new();
        public List<AIChatMessage> Messages { get; } = new();
        public string Model { get; set; }
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public int? TimeoutSecondsOverride { get; set; }
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object> Meta { get; set; } = new();
    }
}
