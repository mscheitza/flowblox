using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FlowBlox.Core.Models.Components.IO
{
    public abstract class DataTableBase : ManagedObject, IReadableTable, IWritableTable
    {
        [Required()]
        [Display(Name = "PropertyNames_DataSource", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association,
            SelectionFilterMethod = nameof(GetPossibleDataSources),
            SelectionDisplayMember = nameof(DataObjectBase.Name))]
        public DataObjectBase DataSource { get; set; }

        protected IEnumerable<DataObjectBase> GetPossibleDataSources()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var dataSources = registry.GetManagedObjects<DataObjectBase>();
            return dataSources;
        }

        [Display(Name = "DataTableBase_FirstRowHeader", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public bool FirstRowHeader { get; set; } = true;

        protected DataTableBase()
        {
            this.FirstRowHeader = true;
        }

        public bool CanRead()
        {
            return DataSource.CanRead();
        }

        public void AddDataSourceChangedListener(Action value)
        {
            DataSource.AddDataSourceChangedListener(value);
        }

        public abstract DataTable Read();

        public abstract void Write(DataTable dataTable);
       
    }
}
