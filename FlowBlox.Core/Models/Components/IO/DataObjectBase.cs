using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

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
