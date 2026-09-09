using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Runtime;
using System.Data;

namespace FlowBloxTest.FlowBlocks.IO
{
    internal sealed class RecordingTable : ManagedObject, IReadableTable, IWritableTable
    {
        private readonly List<Action> _listeners = new();

        public int ReadCount { get; private set; }
        public int ListenerCount => _listeners.Count;

        public bool CanRead(BaseRuntime runtime = null) => true;

        public void AddDataSourceChangedListener(Action value)
        {
            if (value != null && !_listeners.Contains(value))
                _listeners.Add(value);
        }

        public void RemoveDataSourceChangedListener(Action value)
        {
            if (value != null)
                _listeners.Remove(value);
        }

        public DataTable Read()
        {
            ReadCount++;
            return new DataTable();
        }

        public void Write(DataTable dataTable)
        {
        }

        public void RaiseDataSourceChanged()
        {
            foreach (var listener in _listeners.ToList())
                listener();
        }
    }
}
