using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    public sealed class AIResponse
    {
        public bool Success { get; set; }
        public string Text { get; set; }
        public string Error { get; set; }

        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
    }
}
