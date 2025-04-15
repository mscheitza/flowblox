using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Models;

namespace FlowBlox.UICore.Interfaces
{
    public interface IFlowBloxProjectComponentProvider
    {
        ProjectChangelist GetCurrentChangelist();

        FlowBloxRegistry GetCurrentRegistry();
    }
}