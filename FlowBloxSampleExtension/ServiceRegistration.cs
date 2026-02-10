using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBloxSampleExtension.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBloxSampleExtension
{
    internal class ServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IFlowBloxCategoryRegistrationService, SampleCategoryRegistrationService>();
        }
    }
}