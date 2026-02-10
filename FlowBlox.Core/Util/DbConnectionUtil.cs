using System.Data.Common;
using System.Data;

namespace FlowBlox.Core.Util
{
    public class DbConnectionUtil
    {
        public static int ExecuteSqlQuery(string sqlStatement, DbConnection connection)
        {
            return ExecuteSqlQuery(sqlStatement, new Dictionary<string, object>(), connection);
        }

        public static int ExecuteSqlQuery(string sqlStatement, Dictionary<string, object> parameters, DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key; // Parameter Name: @0
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }

            return command.ExecuteNonQuery();
        }

        public static DataTable GetOutputAsDataTable(string sqlStatement, Dictionary<string, object> parameters, DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }

            var dataTable = new DataTable();
            using (var reader = command.ExecuteReader())
            {
                dataTable.Load(reader);
            }

            return dataTable;
        }

        public static object ExecuteScalarQuery(string sqlStatement, Dictionary<string, object> parameters, DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                command.Parameters.Add(dbParameter);
            }

            return command.ExecuteScalar();
        }
    }
}
