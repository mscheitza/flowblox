using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interceptors;
using FlowBlox.Core.Interfaces;
using FlowBlox.Grid.Provider;
using FlowBlox.UICore.Factory;
using Microsoft.Extensions.DependencyInjection;
using FlowBlox.UICore.Interfaces;
using FlowBlox.Services;
using FlowBlox.Interfaces;
using FlowBlox.Core.Services;
using FlowBlox.WebServices;

namespace FlowBlox
{
    internal class ServiceRegistration : IFlowBloxServiceRegistration
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRuntimeInterceptor, RuntimeBacktraceInterceptor>();
            var componentProvider = new FlowBloxProjectComponentProvider();
            serviceCollection.AddSingleton(componentProvider);
            serviceCollection.AddSingleton<IFlowBloxProjectComponentProvider>(componentProvider);
            serviceCollection.AddSingleton<IPropertyWindowViewFactory, TestDefinitionViewFactory>();
            serviceCollection.AddSingleton<IFlowBloxMessageBoxService, FlowBloxMessageBoxService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<IOwnerService, OwnerService>();
            serviceCollection.AddSingleton<IAssemblyPreloader, AssemblyPreloader>();
            serviceCollection.AddSingleton<IFlowBloxUIEvaluationService, FlowBloxUIEvaluationService>();
            serviceCollection.AddSingleton<IAppNotificationService, AppNotificationService>();
            serviceCollection.AddSingleton<IFlowBloxInstallerService, FlowBloxInstallerService>();
            serviceCollection.AddSingleton<IFlowBloxActionHistoryService, FlowBloxActionHistoryService>();
        }
    }
}
