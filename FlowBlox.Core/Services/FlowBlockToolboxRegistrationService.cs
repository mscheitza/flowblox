using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Services.Base;

namespace FlowBlox.Core.Services
{
    public class FlowBlockToolboxRegistrationService : FlowBlockToolboxRegistrationServiceBase
    {
        public override IEnumerable<FlowBloxToolboxCategoryItem> GetAllToolboxCategoriesInModule()
        {
            return
            [
                FlowBloxToolboxCategory.Regex,
                FlowBloxToolboxCategory.XPath,
                FlowBloxToolboxCategory.CounterFormat,
                FlowBloxToolboxCategory.Format,
                FlowBloxToolboxCategory.SQL,
                FlowBloxToolboxCategory.Filter,
                FlowBloxToolboxCategory.DBConnection,
                FlowBloxToolboxCategory.ChatTemplates,
                FlowBloxToolboxCategory.AIPropertyValueGenerationPrompts
            ];
        }

        public override IEnumerable<string> GetAllToolboxResourcesInModule()
        {
            return
            [
                "FlowBlox.Core.Files.globalToolbox_counterformat.json",
                "FlowBlox.Core.Files.globalToolbox_format.json",
                "FlowBlox.Core.Files.globalToolbox_regex.json",
                "FlowBlox.Core.Files.globalToolbox_regex_extended.json",
                "FlowBlox.Core.Files.globalToolbox_dbconnection.json",
                "FlowBlox.Core.Files.globalToolbox_sql_lite.json",
                "FlowBlox.Core.Files.globalToolbox_sql_mssql.json",
                "FlowBlox.Core.Files.globalToolbox_sql_mysql.json",
                "FlowBlox.Core.Files.globalToolbox_sql_oracle.json",
                "FlowBlox.Core.Files.globalToolbox_xpath.json",
                "FlowBlox.Core.Files.globalToolbox_chattemplates.json",
                "FlowBlox.Core.Files.globalToolbox_aipropertyvaluegenerationprompts.json"
            ];
        }
    }
}
