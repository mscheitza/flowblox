using System.Data;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Interfaces
{
    public interface IReadableTable : IManagedObject
    {
        DataTable Read();

        bool CanRead(BaseRuntime runtime = null);

        void AddDataSourceChangedListener(Action value);
    }
}
