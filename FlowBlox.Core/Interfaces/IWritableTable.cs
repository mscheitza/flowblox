using System.Data;

namespace FlowBlox.Core.Interfaces
{
    public interface IWritableTable : IManagedObject
    {
        void Write(DataTable dataTable);
    }
}
