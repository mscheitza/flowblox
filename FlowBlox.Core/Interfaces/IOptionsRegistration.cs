using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Interfaces
{
    public interface IOptionsRegistration
    {
        public void OptionsInit(List<OptionElement> defaults, List<OptionElement> currentOptions);
    }
}
