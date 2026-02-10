using Microsoft.Extensions.DependencyInjection;

namespace FlowBlox.Core.DependencyInjection
{
    public interface IFlowBloxServiceRegistration
    {
        void RegisterServices(IServiceCollection serviceCollection);
    }
}
