using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using FlowBlox.Core.Enums;

namespace FlowBlox.Core.Factories
{
    public static class DbConnectionFactory
    {
        public static DbConnection Create(DbTypes dbType, string connectionString)
        {
            switch (dbType)
            {
                case DbTypes.Oracle:
                    return new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString);
                case DbTypes.MSSQL:
                    return new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                case DbTypes.MySQL:
                    return new MySqlConnection(connectionString);
                case DbTypes.SQLite:
                    return new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
                default:
                    throw new NotSupportedException($"Datenbanktyp {dbType} wird nicht unterstützt");
            }
        }
    }
   
}
