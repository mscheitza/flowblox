using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Migration.MigrationStrategies;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.Core
{
    internal class ServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<FlowBloxProjectManager>();
            serviceCollection.AddTransient<IFlowBloxMigrationStrategy, FlowBloxComponentMigrationStrategy_1_0_0>();
            serviceCollection.AddSingleton<IFlowBloxCategoryRegistrationService, FlowBlockCategoryRegistrationService>();
            serviceCollection.AddSingleton<IFlowBlockToolboxRegistrationService, FlowBlockToolboxRegistrationService>();
        }
    }
}