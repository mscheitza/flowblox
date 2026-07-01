using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    public enum AIChatCacheBehavior
    {
        Default = 0,
        PreferCache = 1
    }

    public sealed class AIChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public AIChatCacheBehavior CacheBehavior { get; set; } = AIChatCacheBehavior.Default;
    }
}
