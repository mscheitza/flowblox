using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.UICore.PopUp.Provider;
using FlowBlox.UICore.PopUp.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.UICore.PopUp
{
    public class ComponentPopupServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<SequenceDetectionPopUpProvider>();
            serviceCollection.AddSingleton<IComponentPopupProvider>(sp =>
                sp.GetRequiredService<SequenceDetectionPopUpProvider>());
            serviceCollection.AddSingleton<IOptionsRegistration>(sp =>
                sp.GetRequiredService<SequenceDetectionPopUpProvider>());

            serviceCollection.AddSingleton<ComponentValidationFailedPopUpProvider>();
            serviceCollection.AddSingleton<IComponentPopupProvider>(sp =>
                sp.GetRequiredService<ComponentValidationFailedPopUpProvider>());
            serviceCollection.AddSingleton<IOptionsRegistration>(sp =>
                sp.GetRequiredService<ComponentValidationFailedPopUpProvider>());

            serviceCollection.AddSingleton<IComponentPopupService, ComponentPopupService>();
        }
    }
}
