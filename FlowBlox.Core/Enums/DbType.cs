using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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