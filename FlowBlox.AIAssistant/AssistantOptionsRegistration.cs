using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;

namespace FlowBlox.AIAssistant
{
    public class AssistantOptionsRegistration : IOptionsRegistration
    {
        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions)
        {
            defaults.Add(new OptionElement(
                "AI.AssistantConfiguration",
                "{\"provider\":{\"type\":\"OpenAI\"},\"model\":\"gpt-5.2\",\"temperature\":0.2,\"maxTokens\":4000}",
                "JSON configuration for FlowBlox AI Assistant.",
                OptionElement.OptionType.Text));
        }
    }
}
