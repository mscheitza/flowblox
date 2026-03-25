using SkiaSharp;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.DeepCopier;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Components.IO
{
    [Display(Name = "TypeNames_DataObject", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("TypeNames_DataObject_Plural", typeof(FlowBloxTexts))]
    public abstract class DataObjectBase : ManagedObject
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.database_outline, 16, new SKColor(13, 148, 136));

        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.database_outline, 32, new SKColor(13, 148, 136));

        private readonly List<Action> _dataSourceChangedHandlers = new List<Action>();

        [JsonIgnore()]
        [DeepCopierIgnore()]
        public abstract byte[] Content { get; set; }

        public abstract bool CanRead();

        public override List<string> GetDisplayableProperties()
            => [nameof(Name)];

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
