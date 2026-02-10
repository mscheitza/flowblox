using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum DbTypes
    {
        [Display(Name = "MSSQL")]
        MSSQL,
        [Display(Name = "Oracle")]
        Oracle,
        [Display(Name = "MySQL")]
        MySQL,
        [Display(Name = "SQLite")]
        SQLite
    }
}