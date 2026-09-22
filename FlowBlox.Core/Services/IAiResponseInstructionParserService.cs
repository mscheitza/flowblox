using FlowBlox.Core.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.Core.Services
{
    public interface IAiResponseInstructionParserService
    {
        AiResponseInstructionParseResult Parse(string output, AIProviderBase? provider = null);
        AiResponseInstructionParseResult ParseFirstJsonObject(string output);
    }
}
