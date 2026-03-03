using FlowBlox.Core.Util;

namespace FlowBlox.AIAssistant.Services
{
    public class DefaultOptionsProvider : IOptionsProvider
    {
        public string GetOptionValue(string optionKey)
        {
            if (string.IsNullOrWhiteSpace(optionKey))
                return string.Empty;

            return FlowBloxOptions.GetOptionInstance().GetOption(optionKey)?.Value ?? string.Empty;
        }
    }
}
