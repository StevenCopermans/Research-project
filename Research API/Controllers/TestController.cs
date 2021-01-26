using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Research_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace Research_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly CMS_DBContext _context;
        private readonly Content_DBContext _contentContext;
        private static string connectionString = null;

        public TestController(CMS_DBContext context, Content_DBContext contentContext)
        {
            this._context = context;
            this._contentContext = contentContext;
            if (connectionString == null)
                connectionString = "Server=.\\SQLEXPRESS;Database=Pizza_DB;Trusted_Connection=True; MultipleActiveResultSets=true";
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetTestController()
        {
            var result = await ExecuteSqlQuery("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';");
            return Ok(JsonConvert.SerializeObject(result.rows));
        }

        private async Task<IEnumerable<string>> GetTables()
        {
            var results = await ExecuteSqlQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';");
            List<string> tableList = new List<string>();

            foreach (var row in results.rows)
            {
                tableList.Add((string)row["TABLE_NAME"]);
            }

            return tableList;
        }

        [HttpPost]
        public async Task<ActionResult<string>> SetConnectionString(object jsonObject)
        {
            JObject parsedObject = JObject.Parse(jsonObject.ToString());
            connectionString = (string)parsedObject["connectionString"];

            Console.WriteLine(connectionString);

            return NoContent();
        }

        [HttpGet("{table}")]
        public async Task<ActionResult<string>> GetByTableName(string table)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            List<string> tables = await GetTables() as List<string>;

            if (tables.Contains(table))
            {
                string body;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    body = await reader.ReadToEndAsync();
                }

                JObject parameters = new JObject();
                if (!string.IsNullOrEmpty(body))
                {
                    parameters = JObject.Parse(body);
                }

                Console.WriteLine(body);

                string whereStatement = "";
                if (parameters.Count > 0)
                {
                    var structureList = await ExecuteSqlQuery("SELECT columns.COLUMN_NAME, IS_NULLABLE, DATA_TYPE, t2.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.COLUMNS as columns LEFT JOIN(SELECT col.COLUMN_NAME, CONSTRAINT_TYPE from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Col.Table_Name = '" + table + "') as t2 ON columns.COLUMN_NAME = t2.COLUMN_NAME WHERE TABLE_NAME = '" + table + "'");

                    foreach (var parameter in parameters)
                    {

                        var containsParameter = structureList.rows.Any(x => (string)x["COLUMN_NAME"] == parameter.Key);

                        if (containsParameter)
                        {
                            try
                            {
                                if (whereStatement.Length > 0)
                                    whereStatement += " AND ";

                                whereStatement += parameter.Key + "='" + Convert.ToString(CastSqlType(Convert.ToString(parameter.Value, CultureInfo.InvariantCulture), (string)structureList.rows.Find(x => (string)x["COLUMN_NAME"] == parameter.Key)["DATA_TYPE"])) + "'";

                                Console.WriteLine(whereStatement);
                            }
                            catch
                            {
                                whereStatement = whereStatement.Substring(0, whereStatement.Length - 5);
                                warnings.Add("Parameter " + parameter.Key + " is of the wrong type");
                            }
                        }
                        else
                        {
                            warnings.Add("No parameter " + parameter.Key + " exists in this table");
                        }
                    }

                    if (whereStatement.Length > 0)
                        whereStatement = " WHERE " + whereStatement;
                }

                Console.WriteLine("Wherestatement: " + whereStatement);

                var result = await ExecuteSqlQuery("SELECT * FROM " + table + whereStatement);

                return Ok(new { data = result.rows, warnings = warnings });
            }

            return NotFound(new { error = "Table does not exist" });
        }

        [HttpGet("{table}/Structure")]
        public async Task<ActionResult<string>> GetTableStructure(string table)
        {
            List<string> tables = await GetTables() as List<string>;

            if (tables.Contains(table))
            {
                var result = await ExecuteSqlQuery("SELECT columns.COLUMN_NAME, IS_NULLABLE, DATA_TYPE, t2.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.COLUMNS as columns LEFT JOIN(SELECT col.COLUMN_NAME, CONSTRAINT_TYPE from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Col.Table_Name = '" + table + "') as t2 ON columns.COLUMN_NAME = t2.COLUMN_NAME WHERE TABLE_NAME = '" + table + "'");

                if (result.rows.Count > 0)
                    return Ok(new { data = result.rows });
                else
                    return NotFound(new { error = "Resource does not exist" });
            }

            return BadRequest(new { error = "Table does not exist" });
        }

        [HttpGet("{table}/{id}")]
        public async Task<ActionResult<string>> GetByTableName(string table, string id)
        {
            List<string> tables = await GetTables() as List<string>;

            if (tables.Contains(table))
            {
                var primaryKeys = await ExecuteSqlQuery("SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY' AND Col.Table_Name = '" + table + "'");

                string idStatement = "";
                foreach (var key in primaryKeys.rows)
                {
                    idStatement += key["Column_Name"] + "='" + id + "' AND";
                }
                idStatement = idStatement.Remove(idStatement.Length - 3, 3);
                Console.WriteLine("ID Statements: " + idStatement);

                var result = await ExecuteSqlQuery("SELECT * FROM " + table + " WHERE " + idStatement);

                if (result.rows.Count > 0)
                    return Ok(new { data = result.rows[0] });
                else
                    return NotFound(new { error = "Resource does not exist" });
            }

            return BadRequest(new { error = "Table does not exist" });
        }

        [HttpDelete("{table}/{id}")]
        public async Task<ActionResult<string>> DeleteByTableNameID(string table, string id)
        {
            List<string> tables = await GetTables() as List<string>;

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (tables.Contains(table))
            {
                var primaryKeys = await ExecuteSqlQuery("SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY' AND Col.Table_Name = '" + table + "'");

                string idStatement = "";
                foreach (var key in primaryKeys.rows)
                {
                    if (idStatement.Length > 0)
                        idStatement += " AND ";
                    idStatement += key["Column_Name"] + "='" + id + "'";
                }

                (int count, List<IDictionary<string, object>> rows) result = (-1, new List<IDictionary<string, object>>());

                try
                {
                    result = await ExecuteSqlQuery("DELETE FROM " + table + " WHERE " + idStatement);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        errors.Add("Foreign Key constraint on another table");
                    }
                    else
                    {
                        errors.Add("Unknown error occured");
                    }

                    return BadRequest(new { errors = errors, warnings = warnings });
                }


                if (result.count > 0)
                    return NoContent();
                else
                    return NotFound(new { error = "Resource does not exist" });
            }

            return BadRequest(new { error = "Table does not exist" });
        }

        [HttpPost("{table}")]
        public async Task<ActionResult<string>> CreateByTable(string table, object tableObject)
        {
            List<string> tables = await GetTables() as List<string>;
            string idColumn = "";
            bool hasIdentity = false;
            List<string> errors = new List<string>();

            if (tables.Contains(table))
            {
                JObject parsedObject;
                try
                {
                    parsedObject = JObject.Parse(tableObject.ToString());


                    if (parsedObject == null)
                    {
                        Console.WriteLine("Insufficient data");
                        errors.Add("Insufficient data");
                        Console.WriteLine(JsonConvert.SerializeObject(new { errors = errors }));

                        return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
                    }
                }
                catch
                {
                    Console.WriteLine("Error processing json");
                    errors.Add("Error processing the json");
                    Console.WriteLine(JsonConvert.SerializeObject(new { errors = errors }));
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
                }


                //Structure of the table
                var structureList = await ExecuteSqlQuery("SELECT columns.COLUMN_NAME, IS_NULLABLE, DATA_TYPE, t2.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.COLUMNS as columns LEFT JOIN(SELECT col.COLUMN_NAME, CONSTRAINT_TYPE from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Col.Table_Name = '" + table + "') as t2 ON columns.COLUMN_NAME = t2.COLUMN_NAME WHERE TABLE_NAME = '" + table + "'");

                //Identity Column(s)
                var identityColumns = await ExecuteSqlQuery("select COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS where (COLUMNPROPERTY(object_id('" + table + "'), COLUMN_NAME, 'IsRowGuidCol') = 1 AND TABLE_NAME='" + table + "') Or (COLUMNPROPERTY(object_id('" + table + "'), COLUMN_NAME, 'IsIdentity') = 1 AND TABLE_NAME='" + table + "') order by TABLE_NAME ");

                List<string> warnings = new List<string>();

                string propertiesString = "";
                string valuesString = "";
                foreach (var column in structureList.rows)
                {
                    string property = (string)column["COLUMN_NAME"];

                    //Check if column is identity
                    bool isIdentity = false;
                    foreach (var identity in identityColumns.rows)
                    {
                        if (property == (string)identity["COLUMN_NAME"])
                        {
                            idColumn = property;
                            isIdentity = true;
                            break;
                        }
                    }

                    //Check if property is null, and if it's allowed to be null
                    if ((string)parsedObject[property] == null && (string)column["IS_NULLABLE"] == "NO" && !isIdentity)
                    {
                        errors.Add(property + " cannot be null");
                    }
                    else if ((string)parsedObject[property] != null)
                    {
                        //If identity column is found
                        if (isIdentity)
                        {
                            hasIdentity = true;
                            warnings.Add(property + " is optional as it's an identity column, object will be created with given value");
                        }

                        try
                        {
                            //Cast json property to correct type
                            object prop = CastSqlType((string)parsedObject[property], (string)column["DATA_TYPE"]);

                            propertiesString += property + ",";
                            valuesString += "'" + Convert.ToString(prop, CultureInfo.InvariantCulture) + "',";
                        }
                        catch
                        {
                            //Type casting failed -> error
                            errors.Add(property + " must be of type " + (string)column["DATA_TYPE"]);
                        }
                    }
                }
                if (propertiesString.Length > 0)
                {
                    propertiesString = propertiesString.Remove(propertiesString.Length - 1, 1);
                    valuesString = valuesString.Remove(valuesString.Length - 1, 1);
                }

                if (errors.Count > 0)
                {
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors, warnings = warnings }));
                }

                try
                {
                    string query = hasIdentity ? "SET IDENTITY_INSERT " + table + " ON; " : "";
                    query += "INSERT INTO " + table + " (" + propertiesString + ") OUTPUT Inserted.*" + " VALUES (" + valuesString + ");";
                    query += hasIdentity ? " SET IDENTITY_INSERT " + table + " OFF;" : "";

                    var newRow = await ExecuteSqlQuery(query);
                    Console.WriteLine(newRow.rows[0][idColumn]);
                    return Created(table + "/" + newRow.rows[0][idColumn], JsonConvert.SerializeObject(new { data = newRow.rows, warnings = warnings }));
                }
                catch
                {
                    errors.Add("Unknown error when trying to add new entry to " + table);
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
                }
            }
            errors.Add(table + " not found");
            return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
        }

        [HttpPut("{table}/{id}")]
        public async Task<ActionResult<string>> UpdateByTable(string table, string id, object tableObject)
        {
            List<string> tables = await GetTables() as List<string>;
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (tables.Contains(table))
            {
                var parsedObject = JObject.Parse(tableObject.ToString());

                if (parsedObject == null)
                {
                    errors.Add("Insufficient data");
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
                }

                //Structure of the table
                var structureList = await ExecuteSqlQuery("SELECT columns.COLUMN_NAME, IS_NULLABLE, DATA_TYPE, t2.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.COLUMNS as columns LEFT JOIN(SELECT col.COLUMN_NAME, CONSTRAINT_TYPE from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Col.Table_Name = '" + table + "') as t2 ON columns.COLUMN_NAME = t2.COLUMN_NAME WHERE TABLE_NAME = '" + table + "'");

                //Identity Column(s)
                var identityColumns = await ExecuteSqlQuery("select COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS where (COLUMNPROPERTY(object_id('" + table + "'), COLUMN_NAME, 'IsRowGuidCol') = 1 AND TABLE_NAME='" + table + "') Or (COLUMNPROPERTY(object_id('" + table + "'), COLUMN_NAME, 'IsIdentity') = 1 AND TABLE_NAME='" + table + "') order by TABLE_NAME ");

                IDictionary<string, string> propertyList = new Dictionary<string, string>();
                string idColumn = "";

                foreach (var column in structureList.rows)
                {
                    string property = (string)column["COLUMN_NAME"];

                    //Check if column is identity
                    bool isIdentity = false;
                    foreach (var identity in identityColumns.rows)
                    {
                        if (property == (string)identity["COLUMN_NAME"])
                        {
                            idColumn = property;
                            isIdentity = true;
                            break;
                        }
                    }

                    //Check if property is null, and if it's allowed to be null
                    if ((string)parsedObject[property] == null && (string)column["IS_NULLABLE"] == "NO" && !isIdentity)
                    {
                        errors.Add(property + " cannot be null");
                    }
                    else if ((string)parsedObject[property] != null)
                    {
                        //If identity column is found
                        if (isIdentity)
                        {
                            warnings.Add(property + " is an identity column and cannot be updated.");
                            continue;
                        }

                        try
                        {
                            //Cast json property to correct type
                            object prop = CastSqlType((string)parsedObject[property], (string)column["DATA_TYPE"]);

                            propertyList.Add(property, Convert.ToString(prop, CultureInfo.InvariantCulture));
                        }
                        catch
                        {
                            //Type casting failed -> error
                            errors.Add(property + " must be of type " + (string)column["DATA_TYPE"]);
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors, warnings = warnings }));
                }

                try
                {
                    string query = "UPDATE " + table + " SET ";

                    foreach (var property in propertyList)
                    {
                        query += property.Key + "='" + property.Value + "',";
                    }
                    query = query.Remove(query.Length - 1, 1);
                    query += " WHERE " + idColumn + "='" + id + "';";

                    Console.WriteLine(query);

                    var updatedRow = await ExecuteSqlQuery(query);
                    return NoContent();
                }
                catch
                {
                    errors.Add("Unknown error when trying to add new entry to " + table);
                    return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
                }
            }
            errors.Add(table + " not found");
            return BadRequest(JsonConvert.SerializeObject(new { errors = errors }));
        }

        private async Task<(int count, List<IDictionary<string, object>> rows)> ExecuteSqlQuery(string query)
        {
            Console.WriteLine(connectionString);
            this._contentContext.Database.GetDbConnection().ConnectionString = connectionString;
            this._contentContext.Database.OpenConnection();

            using (var command = this._contentContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                List<IDictionary<string, object>> objects = new List<IDictionary<string, object>>();

                int lineCount = 0;

                using (var result = await command.ExecuteReaderAsync())
                {
                    lineCount = result.RecordsAffected;

                    var names = Enumerable.Range(0, result.FieldCount).Select(result.GetName).ToList();
                    Console.WriteLine("QueryRecors: " + result.RecordsAffected);

                    foreach (IDataRecord record in result as IEnumerable)
                    {
                        var expando = new ExpandoObject() as IDictionary<string, object>;
                        foreach (var name in names)
                        {
                            expando[name] = record[name];
                        }

                        objects.Add(expando);
                    }

                    result.Close();
                }

                this._contentContext.Database.CloseConnection();

                return (lineCount, objects);
            }
        }

        private dynamic CastSqlType(dynamic variable, string sqlType)
        {
            switch (sqlType)
            {
                default:
                    return variable;

                case "bigint":
                    return Int64.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "binary":
                case "varbinary":
                case "image":
                case "rowversion":
                case "timestamp":
                    return Encoding.ASCII.GetBytes(variable);

                case "bit":
                    return Convert.ToBoolean(variable, CultureInfo.InvariantCulture);

                case "char":
                case "nchar":
                case "nxtext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return Convert.ToString(variable, CultureInfo.InvariantCulture);

                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return Convert.ToDateTime(variable, CultureInfo.InvariantCulture);

                case "datetimeoffset":
                    return DateTimeOffset.Parse(variable, CultureInfo.InvariantCulture);

                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return decimal.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "float":
                    return double.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "int":
                    return int.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "real":
                    return float.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "smallint":
                    return short.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "sql_variant":
                    return (object)variable;

                case "time":
                    return TimeSpan.Parse(variable, CultureInfo.InvariantCulture);

                case "tinyint":
                    return byte.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);

                case "uniqueidentifier":
                    return Guid.Parse(variable);
            }
        }
    }
}
