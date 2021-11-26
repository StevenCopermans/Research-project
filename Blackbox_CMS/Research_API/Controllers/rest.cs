using BlackboxData.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Research_API.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Research_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class rest : ControllerBase
    {
        private const string connectionString = "Data Source=(local);Initial Catalog=Cinema_DB;Integrated Security=true";

        private static SqlConnection sqlConnection = new SqlConnection(connectionString);

        private InformationSchemeHelper schemeHelper = new InformationSchemeHelper(connectionString);


        // GET: api/<rest>
        [HttpGet]
        public async Task<ActionResult<string>> GetTables()
        {
            return Ok(JsonConvert.SerializeObject(await schemeHelper.GetTablesAsync()));
        }

        [HttpGet("{table}")]
        public async Task<ActionResult<string>> GetTableByName(string table)
        {
            Console.WriteLine(table);

            if (!(await schemeHelper.TableExistsAsync(table)))
                return NotFound();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                var result = await sqlConnection.QueryAsync(string.Format("SELECT * FROM [{0}]", table));
                return Ok(JsonConvert.SerializeObject(result));
            }
            
        }

        [HttpGet("{table}/{id}")]
        public async Task<ActionResult<string>> GetTableByName(string table, string id)
        {
            Console.WriteLine(id);
            List<Table> tables;

            tables = await schemeHelper.GetTablesAsync() as List<Table>;

            if (tables.FirstOrDefault(x => x.TABLE_NAME == table) == null)
                return NotFound();

            List<string> primaryKeys = await GetPrimaryKeys(table);
            if (primaryKeys.Count > 1)
                return NotFound(new { message = "The table you're trying to acces has more than 1 primary key, please use a query url", primaryKeys = primaryKeys });

            DataTable columns = await GetColumnsAsync(table);

            DataRowCollection a = columns.Rows;


            try
            {
                dynamic convertedId = ConvertStringToType(id, "");
            } catch (NotImplementedException e)
            {
                return BadRequest();
            }



            sqlConnection.Open();

            string qry = string.Format("SELECT * FROM {0} WHERE @PrimaryKey = '@Id'", table);
            string qry2 = string.Format("SELECT * FROM {0} WHERE {1} = '@Id'", table, primaryKeys.First());
            var result = sqlConnection.Query(qry2, new { Id = id }).ToList();

            sqlConnection.Close();

            return Ok(JsonConvert.SerializeObject(result));
        }

        public async Task<DataRowCollection> GetDataTypes()
        {
            sqlConnection.Open();
            DataTable dataTypes = await sqlConnection.GetSchemaAsync("DataTypes");
            sqlConnection.Close();

            return dataTypes.Rows;
        }

        [HttpGet("Test/{table}")]
        public async Task<ActionResult<string>> Test(string table)
        {
            return Ok(JsonConvert.SerializeObject(await GetPrimaryKeys(table)));
        }

        [HttpGet("pk/{table}")]
        public async Task<List<string>> GetPrimaryKeys(string table)
        {
            sqlConnection.Open();

            string qry = string.Format("SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY' AND Col.Table_Name = '{0}'", table);
            List<string> result = (List<string>)await sqlConnection.QueryAsync<string>(qry);

            sqlConnection.Close();

            return result;
        }

        public async Task<DataTable> GetColumnsAsync(string table)
        {
            string[] constraints = new string[3];
            constraints[2] = table;

            sqlConnection.Open();
            DataTable dataTypes = await sqlConnection.GetSchemaAsync("Columns", constraints);
            sqlConnection.Close();

            return dataTypes;
        }

        public dynamic ConvertType(dynamic ItemToCast, string CastToType)
        {
            try
            {
                var foo = TypeDescriptor.GetConverter(Type.GetType(CastToType));
                var test = Convert.ChangeType((foo.ConvertFromInvariantString(ItemToCast)), Type.GetType(CastToType));

                return test;
            }
            catch
            {
                //Console.WriteLine("Converting with method 1 has failed");
                try
                {
                    return System.Activator.CreateInstance(Type.GetType(CastToType), ItemToCast);
                }
                catch
                {
                    //Console.WriteLine("Converting with method 2 has failed");
                    try
                    {
                        return Convert.ChangeType(ItemToCast, Type.GetType(CastToType));
                    }
                    catch
                    {
                        //Console.WriteLine("Converting with method 3 has failed");
                        return null;
                    }
                }
            }
        }

        public async Task<dynamic> ConvertType(dynamic ItemToCast, string table, string column)
        {
            DataTable cols = await GetColumnsAsync(table);
            string dataTypeOfCol = (string)cols.Rows[0].Table.Select("COLUMN_NAME=" + "'" + column + "'")[0]["DATA_TYPE"];

            DataRowCollection dataTypes = await GetDataTypes();
            string systemDataType = (string)dataTypes[0].Table.Select("TypeName='" + dataTypeOfCol + "'")[0]["DataType"];

            return ConvertType(ItemToCast, systemDataType);            
        }

        [HttpGet("test")]
        public async Task<ActionResult<string>> test()
        {
            return JsonConvert.SerializeObject(GetDataTypes());

            //return NotFound();
               
        }

        [HttpGet("convert/{table}/{col}/{id}")]
        public ActionResult<string> convert(string table, string col, string id)
        {
            sqlConnection.Open();

            string qry = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", table, col, id);
            var result = sqlConnection.Query(qry).ToList();

            sqlConnection.Close();

            return Ok(result);
        }

        public dynamic ConvertStringToType(string item, string type)
        {
            type = type.ToLowerInvariant().Replace("system.","");
            switch(type)
            {
                case "string":
                    return "Hello";
                case "int64":
                    return "";
                case "byte[]":
                    return "";
                default:
                    return null;
            }
        }

    }
}
