using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.AIAssistant
{
    internal class ServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IOptionsRegistration, AssistantOptionsRegistration>();
        }
    }
}
