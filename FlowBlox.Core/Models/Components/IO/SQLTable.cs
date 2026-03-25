using SkiaSharp;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
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
    [Serializable()]
    [Display(Name = "SQLTable_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("SQLTable_DisplayName_Plural", typeof(FlowBloxTexts))]
    public class SQLTable : ManagedObject, IReadableTable
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.database_cog, 16, new SKColor(220, 38, 38));

        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.database_cog, 32, new SKColor(220, 38, 38));

        private List<Action> _readableTableDataSourceChangedHandler = new List<Action>();

        [Required()]
        [Display(Name = "SQLTable_DbType",
                 Description = "SQLTable_DbType_Tooltip",
                 ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public DbTypes DbType { get; set; }

        [Required()]
        [Display(Name = "SQLTable_SQLConnectionstring",
                 Description = "SQLTable_SQLConnectionstring_Tooltip",
                 ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection, ToolboxCategory = nameof(FlowBloxToolboxCategory.DBConnection))]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true)]
        public string SQLConnectionstring { get; set; }

        [Required()]
        [Display(Name = "SQLTable_SQLStatement",
                 Description = "SQLTable_SQLStatement_Tooltip",
                 ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true, IsCodingMode = true, SyntaxHighlighting = "SQL")]
        public string SQLStatement { get; set; }

        public bool CanRead()
        {
            if (this.TryRead(out _))
                return true;

            return false;
        }

        public override void RegisterPropertyChangedEventHandlers()
        {
            foreach (FieldElement fieldElement in FlowBloxFieldHelper.GetFieldElementsFromString(SQLStatement))
            {
                fieldElement.OnValueChanged += new FieldElement.FieldElementValueChangedEventHandler(FieldElement_ValueChange);
            }

            base.RegisterPropertyChangedEventHandlers();
        }

        public void AddDataSourceChangedListener(Action listener)
        {
            _readableTableDataSourceChangedHandler.Add(listener);
        }

        private void FieldElement_ValueChange(FieldElement field, string oldValue, string newValue)
        {
            _readableTableDataSourceChangedHandler.ForEach(x => x.Invoke());
        }

        public DataTable Read()
        {
            var dbConnection = DbConnectionProvider.Instance.GetOrCreateDbConnection(DbType, SQLConnectionstring);
            if (dbConnection != null)
            {
                Dictionary<string, object> parameters;
                string sqlStatement = SQLStatement;
                FlowBloxFieldHelper.ReplaceFieldsInSQL(sqlStatement, this.DbType, out parameters);
                return DbConnectionUtil.GetOutputAsDataTable(sqlStatement, parameters, dbConnection);
            }
            throw new InvalidOperationException($"Unable to connect to \"{SQLConnectionstring}\".");
        }

        private bool TryRead(out DataTable dataTable)
        {
            try
            {
                dataTable = this.Read();
                return true;
            }
            catch (Exception)
            {
                dataTable = null;
                return false;
            }
        }

        public override List<string> GetDisplayableProperties()
            => [nameof(Name), nameof(DbType)];

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            _readableTableDataSourceChangedHandler.Clear();
        }
    }
}
