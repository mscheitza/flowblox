using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    public sealed class AIRequest
    {
        public string Prompt { get; set; }
        public string SystemInstruction { get; set; }
        public string Model { get; set; }
        public double Temperature { get; set; } = 0.0;
        public int? MaxTokens { get; set; }
        public int? TimeoutSecondsOverride { get; set; }
        public Dictionary<string, object> Meta { get; set; } = new();
    }
}
