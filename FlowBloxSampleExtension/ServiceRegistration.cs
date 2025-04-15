using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Services;
using FlowBloxSampleExtension.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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