using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interceptors;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Migration.MigrationStrategies;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Services;
using FlowBlox.Core.Services.Notifications;
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
            serviceCollection.AddSingleton<IFlowBloxLegacyTypeMappingService, FlowBloxCoreLegacyTypeMappingService>();
            serviceCollection.AddSingleton<IAiResponseInstructionParserService, AiResponseInstructionParserService>();
            serviceCollection.AddTransient<IRuntimeInterceptor, RuntimeDebuggingInterceptor>();
            serviceCollection.AddTransient<IRuntimeInterceptor, RuntimeNotificationInterceptor>();
            serviceCollection.AddSingleton<IRuntimeNotificationProvider, EmailRuntimeNotificationProvider>();
            serviceCollection.AddSingleton<IOptionsRegistration, RuntimeNotificationOptionsRegistration>();
        }
    }
}