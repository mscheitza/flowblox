using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "TableSelectorMappingEntry_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class TableSelectorMappingEntry : FlowBloxReactiveObject
    {
        [Required()]
        [Display(Name = "TableSelectorMappingEntry_ColumnName", ResourceType = typeof(FlowBloxTexts))]
        public string ColumnName { get; set; }

        [Required()]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public FieldElement Field { get; set; }

        public bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            if (Field == null)
            {
                dependencies = null;
                return true;
            }
            
            return Field.IsDeletable(out dependencies);
        }
    }

    public class TableSelectorColumnCondition : ComparisonCondition
    {
        [Required()]
        [Display(Name = "TableSelectorColumnCondition_ColumnName", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        public string ColumnName { get; set; }
    }

    [FlowBloxUIGroup("TableReaderFlowBlock_Groups_FieldColumnMapping", 0)]
    [FlowBloxUIGroup("TableReaderFlowBlock_Groups_DatasetColumnConditions", 1)]
    [Display(Name = "TableReaderFlowBlock_DisplayName", Description = "TableReaderFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class TableReaderFlowBlock : BaseResultFlowBlock
    {
        [Display(Name = "TableReaderFlowBlock_MappingEntries", ResourceType = typeof(FlowBloxTexts), GroupName = "TableReaderFlowBlock_Groups_FieldColumnMapping", Order = 0)]
        [FlowBloxUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        public ObservableCollection<TableSelectorMappingEntry> MappingEntries { get; set; }

        [Display(Name = "TableReaderFlowBlock_DatasetColumnConditions", ResourceType = typeof(FlowBloxTexts), GroupName = "TableReaderFlowBlock_Groups_DatasetColumnConditions", Order = 0)]
        [FlowBloxUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBloxDataGrid(
            GridColumnMemberNames = new[]
            {
                nameof(TableSelectorColumnCondition.ColumnName),
                nameof(TableSelectorColumnCondition.Operator),
                nameof(TableSelectorColumnCondition.Value)
            })]
        public ObservableCollection<TableSelectorColumnCondition> ColumnConditions { get; set; }

        [Display(Name = "TableReaderFlowBlock_ReferencedTable", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBloxUI(Factory = UIFactory.Association, SelectionFilterMethod = nameof(GetPossibleReadableTables), SelectionDisplayMember = nameof(IReadableTable.Name))]
        public IReadableTable ReferencedTable { get; set; }

        public List<IReadableTable> GetPossibleReadableTables()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var tables = registry.GetManagedObjects<IReadableTable>();
            return tables.ToList();
        }

        public TableReaderFlowBlock()
        {
            this.MappingEntries = new ObservableCollection<TableSelectorMappingEntry>();
            this.ColumnConditions = new ObservableCollection<TableSelectorColumnCondition>();
        }

        public override List<FieldElement> Fields
        {
            get
            {
                return this.MappingEntries
                    .Select(x => x.Field)
                    .ExceptNull()
                    .ToList();
            }
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.table_arrow_down, 16, SKColors.Blue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.table_arrow_down, 32, SKColors.Blue);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.IO;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(ReferencedTable));
            return properties;
        }

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (ReferencedTable == null)
                    throw new Exception("The is no Table defined for TableSelectorElement [" + Name + "].");

                if (!ReferencedTable.CanRead())
                {
                    CreateNotification(runtime, TableReaderNotifications.TableNotReady);
                    GenerateResult(runtime);
                    return;
                }

                var tableData = ReferencedTable.Read();
                foreach(var columnCondition in this.ColumnConditions)
                {
                    if (!tableData.Columns.Contains(columnCondition.ColumnName))
                        throw new InvalidOperationException($"Could not find the column header \"{columnCondition.ColumnName}\" from column conditions in table data.");
                }

                foreach (var mappingEntry in this.MappingEntries)
                {
                    if (!tableData.Columns.Contains(mappingEntry.ColumnName))
                        throw new InvalidOperationException($"Could not find the column header \"{mappingEntry.ColumnName}\" from mapping entries in table data.");
                }

                List<Dictionary<FieldElement, string>> resultMap = new List<Dictionary<FieldElement, string>>();
                for (int rowCounter = 0; rowCounter < tableData.Rows.Count; rowCounter++)
                {
                    bool requirementsMet = true;
                    foreach(var condition in this.ColumnConditions)
                    {
                        var fieldValue = tableData.Rows[rowCounter][condition.ColumnName];
                        if (!condition.Compare(fieldValue))
                        {
                            requirementsMet = false;
                            break;
                        }
                    }

                    if (!requirementsMet)
                        continue;


                    Dictionary<FieldElement, string> resultEntry = new Dictionary<FieldElement, string>();
                    foreach (var mappingEntry in MappingEntries)
                    {
                        if (!tableData.Columns.Contains(mappingEntry.ColumnName))
                            throw new InvalidOperationException($"The column \"{mappingEntry.ColumnName}\" does not exist in table \"{this.ReferencedTable.Name}\".");

                        string fieldValue = tableData.Rows[rowCounter][mappingEntry.ColumnName].ToString();
                        FieldElement fieldElement = mappingEntry.Field;
                        resultEntry[fieldElement] = fieldValue;
                        
                    }
                    resultMap.Add(resultEntry);
                }
                this.GenerateResult(runtime, resultMap);
            });
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(TableReaderNotifications));
                return notificationTypes;
            }
        }

        public enum TableReaderNotifications
        {
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Underlying table not ready to read data")]
            TableNotReady
        }
    }
}
