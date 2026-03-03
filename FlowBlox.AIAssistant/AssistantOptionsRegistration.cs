using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.AIAssistant
{
    public class AssistantOptionsRegistration : IOptionsRegistration
    {
        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            var defaultConfiguration = new AssistantConfiguration();
            defaults.Add(new OptionElement(
                AssistantConfiguration.OptionKey,
                AssistantConfigurationJson.Serialize(defaultConfiguration),
                "JSON configuration for FlowBlox AI Assistant.",
                OptionElement.OptionType.Text));
        }
    }
}
