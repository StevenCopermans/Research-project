using BlackboxData.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Research_API.Repositories
{
    public static class InformationSchemeHelper
    {
        public static async Task<IEnumerable<Table>> GetTablesAsync(SqlConnection sqlConnection)
        {
            IEnumerable<Table> tablesQuery = await sqlConnection.QueryAsync<Table>("SELECT * FROM INFORMATION_SCHEMA.TABLES;");
            return tablesQuery;
        }

        public static async Task<bool> TableExistsAsync(SqlConnection sqlConnection, string table)
        {
            var tables = await GetTablesAsync(sqlConnection);
            if (tables.FirstOrDefault(x => x.TABLE_NAME == table) == null)
                return false;
            return true;
        }
    }
}
