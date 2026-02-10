using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.DeepCopier;
using Newtonsoft.Json;

namespace FlowBlox.Core.Models.Components.IO
{
    public abstract class DataObjectBase : ManagedObject
    {
        private readonly List<Action> _dataSourceChangedHandlers = new List<Action>();

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public abstract byte[] Content { get; set; }

        public abstract bool CanRead();

        public void AddDataSourceChangedListener(Action listener)
        {
            _dataSourceChangedHandlers.Add(listener);
        }

        protected void TriggerDataSourceChanged()
        {
            _dataSourceChangedHandlers.ForEach(handler => handler.Invoke());
        }
    }
}
