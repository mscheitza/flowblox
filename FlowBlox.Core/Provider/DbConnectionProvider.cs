using FlowBlox.Core.Enums;
using FlowBlox.Core.Factories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Provider
{
    public class DbConnectionProvider
    {
        public DbConnectionProvider()
        {
        }

        private static DbConnectionProvider _dbConnectionProvider;
        public static DbConnectionProvider Instance
        {
            get
            {
                if ( _dbConnectionProvider == null )
                    _dbConnectionProvider = new DbConnectionProvider();
                return _dbConnectionProvider;
            }
        }

        private Dictionary<string, DbConnection> _nameToDbConnection { get; } = new Dictionary<string, DbConnection>();

        private static void EnsureOpen(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
        }

        public DbConnection GetOrCreateDbConnection(DbTypes dbType, string connectionstring)
        {
            DbConnection dbConnection;
            if (_nameToDbConnection.ContainsKey(connectionstring))
            {
                dbConnection = _nameToDbConnection[connectionstring];
            }
            else
            {
                dbConnection = DbConnectionFactory.Create(dbType, connectionstring);
                if (dbConnection != null)
                    _nameToDbConnection[connectionstring] = dbConnection;
            }
            EnsureOpen(dbConnection);
            return dbConnection;
        }
    }
}
