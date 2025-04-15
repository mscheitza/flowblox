using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core;
using FlowBlox.Core.Interfaces;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using FlowBlox.Core.Extensions;

namespace FlowBlox.Core.Models.ObjectManager
{
    [Display(Name = "DataTableManager_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [FlowBlockUIGroup(Name = "DataTableManager_Groups_CsvTables", ControlAlignment = ControlAlignment.Fill)]
    [FlowBlockUIGroup(Name = "DataTableManager_Groups_SqlTables", ControlAlignment = ControlAlignment.Fill)]
    [FlowBlockUIGroup(Name = "DataTableManager_Groups_ExcelTables", ControlAlignment = ControlAlignment.Fill)]
    public class DataTableManager : IDockableObjectManager
    {
        private FlowBloxRegistry _registry;

        public bool IsActive => true;

        public DataTableManager()
        {
            CsvTables = new ObservableCollection<CsvTable>();
            SqlTables = new ObservableCollection<SQLTable>();
            ExcelTables = new ObservableCollection<ExcelTable>();
        }

        public DataTableManager(FlowBloxRegistry registry) : this()
        {
            _registry = registry;

            Reload();
        }

        public void Reload()
        {
            CsvTables.Clear();
            CsvTables.AddRange(_registry.GetManagedObjects<CsvTable>());

            SqlTables.Clear();
            SqlTables.AddRange(_registry.GetManagedObjects<SQLTable>());

            ExcelTables.Clear();
            ExcelTables.AddRange(_registry.GetManagedObjects<ExcelTable>());
        }

        [Display(ResourceType = typeof(FlowBloxTexts), GroupName = "DataTableManager_Groups_CsvTables")]
        [FlowBlockUI(Factory = UIFactory.ListView, DisplayLabel = false, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(CsvTable.Name), nameof(CsvTable.DataSource) })]
        public ObservableCollection<CsvTable> CsvTables { get; set; }

        [Display(ResourceType = typeof(FlowBloxTexts), GroupName = "DataTableManager_Groups_SqlTables")]
        [FlowBlockUI(Factory = UIFactory.ListView, DisplayLabel = false, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(SQLTable.Name), nameof(SQLTable.DbType) })]
        public ObservableCollection<SQLTable> SqlTables { get; set; }

        [Display(ResourceType = typeof(FlowBloxTexts), GroupName = "DataTableManager_Groups_ExcelTables")]
        [FlowBlockUI(Factory = UIFactory.ListView, DisplayLabel = false, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(ExcelTable.Name), nameof(ExcelTable.DataSource) })]
        public ObservableCollection<ExcelTable> ExcelTables { get; set; }
    }
}
