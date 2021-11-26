using BlackboxData.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Research_API.Repositories
{
    public class InformationSchemeHelper
    {
        private static string _connectionString;

        public InformationSchemeHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Table>> GetTablesAsync()
        {
            using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
            {
                IEnumerable<Table> tablesQuery = await sqlConnection.QueryAsync<Table>("SELECT * FROM INFORMATION_SCHEMA.TABLES;");
                return tablesQuery;
            }
        }

        public async Task<bool> TableExistsAsync(string table)
        {
            var tables = await GetTablesAsync();
            if (tables.FirstOrDefault(x => x.TABLE_NAME == table) == null)
                return false;
            return true;
        }
    }
}
