using System.Data;

namespace FlowBlox.Core.Interfaces
{
    public interface IReadableTable : IManagedObject
    {   
        DataTable Read();

        bool CanRead();

        void AddDataSourceChangedListener(Action value);
    }
}