using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
//using Oracle.ManagedDataAccess.Client;
using Framework.Utils;
using System.Data;
using System.Text;
using System.Linq;

namespace DataContext.Relational
{
    /// <summary>
    /// Supported database types
    /// </summary>
    public enum DbType
    {
        SqlServer,
        MySql,
        Oracle
    }

    /// <summary>
    /// A concrete implementation of the IDataContext interface for relational databases
    /// </summary>
    [Export("Relational", typeof(IDataContext))]
    public class SqlDataContext : IDataContext
    {

        private IDictionary<DbConnection, DbTransaction> _connectionTransactionMap;

        private DbType _dbType;

        private string _connectionString;

        private string _tablePrefix;

        /// <summary>
        /// A reference to a data mapper component required to map query results to concrete objects
        /// </summary>
        [Import]
        public IDataMapper Mapper { get; set; }

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        public IConfiguration Config { get; set; }

        /// <summary>
        /// A flag to indicate if changes made should be automatically committed to data source or not (if applicable).
        /// This should be set in a config file if using dependency injection
        /// </summary>
        public bool AutoCommit { get; set; }
        public string DBName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        #region Helpers
        /// <summary>
        /// Helper method for creating a concrete connection object based on current database platform
        /// </summary>
        /// <returns>A platform dependent database connection object</returns>
        private DbConnection CreateConnection()
        {
            if(_connectionString == null)
            {
                Connect();
            }
            DbConnection connection = null;
            switch (_dbType)
            {
                case DbType.MySql:
                    connection = new MySqlConnection(_connectionString);
                    break;
                /*case DbType.Oracle:
                    connection = new OracleConnection(_connectionString);
                    break;*/
                default:
                    connection = new SqlConnection(_connectionString); //default to sql server
                    break;
            }
            if(!AutoCommit)
            {
                var transaction = connection.BeginTransaction();
                _connectionTransactionMap.Add(connection, transaction);
            }

            return connection;
        }

       

        /// <summary>
        /// Helper method for creating a database command based on current database platform
        /// </summary>
        /// <param name="command">SQL command statement</param>
        /// <param name="parameters">SQL command parameters</param>
        /// <param name="connection">Platform dependent database connection</param>
        /// <returns>Platform dependent database command</returns>
        private DbCommand CreateCommand(string command, List<KeyValuePair<string, object>> parameters,DbConnection connection)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = command;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    DbParameter dbParam = cmd.CreateParameter();
                    dbParam.ParameterName = param.Key;
                    dbParam.Value = param.Value;
                    cmd.Parameters.Add(dbParam);
                }
            }
            return cmd;
        }

        /// <summary>
        /// Serializes an object into a key value pair of field and values
        /// </summary>
        /// <param name="record">Object to be serialized</param>
        /// <param name="addNulls">A flag indicating if null fields should be serialized</param>
        /// <returns>A key value pair of object's field and values</returns>
        private List<KeyValuePair<string,object>> TransformObjectToParams(object record, bool addNulls = true)
        {

            List<KeyValuePair<string, object>> paramRec = new List<KeyValuePair<string, object>>(); //need to maintain order for traversals
            IList<string> fieldNames = Mapper.GetFieldNames(record.GetType());
            foreach(var field in fieldNames)
            {
                if (addNulls || Mapper.GetField(field, record) != null)
                {
                    paramRec.Add(new KeyValuePair<string, object>(field, Mapper.GetField(field, record)));
                }
            }
            return paramRec;
        }

        /// <summary>
        /// Used by the update method to bring the key field to
        /// the end of the list
        /// </summary>
        /// <param name="objParam">List of fields</param>
        private void SwapKeyForLast<T>(List<KeyValuePair<string, object>> objParam)
        {
            string keyName = Mapper.GetKeyMapName(typeof(T));
            int i = 0;
            for(;i < objParam.Count; i++)
            {
                var parm = objParam[i];
                if(keyName == parm.Key)
                {
                    objParam.RemoveAt(i);
                    objParam.Add(parm);
                    return;
                }
            }
        }

        /// <summary>
        /// Replaces the key in the serialized key-value pairs with sequential parameter labels
        /// </summary>
        /// <param name="parameters">Serialized key-value pairs</param>
        /// <param name="paramOffset">Parameter label sequence offset</param>
        /// <returns>A new key-value pair with the key being sequential parameter labels</returns>
        private static List<KeyValuePair<string, object>> TransformToSqlParam(List<KeyValuePair<string, object>> parameters, int paramOffset)
        {
            List<KeyValuePair<string, object>> sqlParameters = new List<KeyValuePair<string, object>>();
            int paramCount = 0;
            foreach(var param in parameters)
            {
                sqlParameters.Add(new KeyValuePair<string, object>("@P" + (paramCount + paramOffset), param.Value == null ? DBNull.Value : param.Value));
                paramCount++;
            }

            return sqlParameters;
        }

        /// <summary>
        /// Builds SQL update statement
        /// </summary>
        /// <param name="parameters">Serialized object key-value pair with the key being field names</param>
        /// <param name="tableName">Table name in statement</param>
        /// <param name="paramOffset">Parameter label sequence offset</param>
        /// <param name="key">Key-name value pair for building the where clause</param>
        /// <returns>A parameterized update statement string</returns>
        private string BuildCommandForUpdate(List<KeyValuePair<string, object>> parameters, string tableName, int paramOffset,KeyValuePair<string,object> key)
        {
            StringBuilder command = new StringBuilder($"UPDATE {_tablePrefix}{tableName} SET ");
            bool isFirst = true;
            int paramCount = 0;
            foreach(var param in parameters)
            {
                if (param.Key != key.Key)
                { //exclude key field
                    if (!isFirst)
                    {
                        command.Append(",");
                    }
                    command.Append($"[{param.Key}] = @P{paramCount + paramOffset}");
                    isFirst = false;
                    paramCount++;
                }
            }
            command.Append($" WHERE [{key.Key}] = @P{paramCount + paramOffset};");
            return command.ToString();
        }


        /// <summary>
        /// Builds SQL insert statement
        /// </summary>
        /// <param name="parameters">Serialized object key-value pair with the key being field names</param>
        /// <param name="tableName">Table name in statement</param>
        /// <param name="paramOffset">Parameter label sequence offset</param>
        /// <returns>A parameterized insert statement string</returns>
        private string BuildCommandForInsert(List<KeyValuePair<string, object>> parameters,string tableName, int paramOffset)
        {
            StringBuilder command = new StringBuilder($"INSERT INTO {_tablePrefix}{tableName}(");
            bool isFirst = true;
            foreach(var param in parameters)
            {
                if(!isFirst)
                {
                    command.Append(",");
                }
                command.Append("[" + param.Key + "]");
                isFirst = false;
            }
            command.Append(") VALUES (");

            isFirst = true;
            for(int i =0; i < parameters.Count; i++)
            {
                if (!isFirst)
                {
                    command.Append(",");
                }
                command.Append("@P" + (paramOffset + i));
                isFirst = false;
            }
            command.Append(");");
            return command.ToString();

        }

        /// <summary>
        /// Converts a database record into a serialized key-value pair
        /// </summary>
        /// <param name="reader">Database record</param>
        /// <returns>Serialized key-value pair</returns>
        private static IDictionary<string,object> ConvertReaderToKV(DbDataReader reader)
        {
            IDictionary<string, object> keyValues = new Dictionary<string, object>();
            for(int i =0; i < reader.FieldCount; i++)
            {
                keyValues.Add(reader.GetName(i), reader.GetValue(i));
            }
            return keyValues;
        }

        /// <summary>
        /// Builds part of query responsible for paging based on the current database platform
        /// </summary>
        /// <param name="skip">Page offset</param>
        /// <param name="length">Page length</param>
        /// <returns>SQL paging statement</returns>
        private string BuildRangeQueryForProvider(int skip, int length,string keyName)
        {
            switch(_dbType)
            {
                case DbType.MySql:
                    return $"LIMIT {skip},{length}";
                default:
                    return $"ORDER BY {keyName} OFFSET {skip} ROWS FETCH NEXT {length} ROWS ONLY"; //sql server and oracle share the same syntax
                
            }

        }


        #endregion

        /// <summary>
        /// Constructs an instance of this class based on configuration values
        /// </summary>
        [ImportingConstructor]
        public SqlDataContext([Import("JsonConfig")]IConfiguration config)
        {
            Config = config;
            AutoCommit = Config.GetValue(ConfigConstants.RELATIONAL_DB_AUTOCOMMIT) == "1";

            if (!AutoCommit)
            {
                _connectionTransactionMap = new Dictionary<DbConnection, DbTransaction>();
            }

            string dbTypeCode = Config.GetValue(ConfigConstants.RELATIONAL_DB_TYPE);
            _dbType = dbTypeCode == null ? DbType.SqlServer : (DbType)Convert.ToInt32(dbTypeCode);

            string tablePrefix = Config.GetValue(ConfigConstants.RELATIONAL_TABLE_PREFIX);
            _tablePrefix = tablePrefix ?? string.Empty;
            Connect();

        }

        /// <summary>
        /// Commits data transaction to data source if applicable
        /// </summary>
        public void Commit()
        {
            if (!AutoCommit)
            {
                foreach (var connTran in _connectionTransactionMap)
                {
                    if (connTran.Key.State == System.Data.ConnectionState.Open && connTran.Value != null)
                    {
                        connTran.Value.Commit();
                        connTran.Key.Close();
                    }
                }
                _connectionTransactionMap.Clear();
            }
        }

        /// <summary>
        /// Connects data provider to its source
        /// </summary>
        public void Connect()
        {
            Connect(ConfigConstants.RELATIONAL_CONNECTION_STRING);
        }


        /// <summary>
        /// Connects data provider to its source addressed by supplied connection string
        /// </summary>
        /// <param name="str">Connection string</param>
        public void Connect(string str)
        {
            _connectionString = Config.GetValue(str);
        }


        /// <summary>
        /// Removes a record matching the instance of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Delete<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>( "@P0", Mapper.GetKeyValue(obj))
                };
                var cmd = CreateCommand(cmdStr,parameters,connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if(AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Removes a record of type T based on its key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="key">Key value for record to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Delete<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>( "@P0", key)
                };
                var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Removes multiple records matching each instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> DeleteAll<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                StringBuilder keyParmStr = new StringBuilder();
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < objList.Count; i++)
                {
                    if (i > 0)
                    {
                        keyParmStr.Append(",");
                    }
                    keyParmStr.Append($"@P{i}");
                    parameters.Add(new KeyValuePair<string, object>($"@P{i}", Mapper.GetKeyValue(objList[i])));
                }

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN ({keyParmStr})";
                var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Removes multiple records of type T based on their keys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                StringBuilder keyParmStr = new StringBuilder();
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < keyList.Count; i++)
                {
                    if (i > 0)
                    {
                        keyParmStr.Append(",");
                    }
                    keyParmStr.Append($"@P{i}");
                    parameters.Add(new KeyValuePair<string, object>($"@P{i}", keyList[i]));
                }

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN ({keyParmStr})"; var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }

        }

        /// <summary>
        /// Removes multiple records matching each instances of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAllAsync<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                StringBuilder keyParmStr = new StringBuilder();
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < objList.Count; i++)
                {
                    if (i > 0)
                    {
                        keyParmStr.Append(",");
                    }
                    keyParmStr.Append($"@P{i}");
                    parameters.Add(new KeyValuePair<string, object>($"@P{i}", Mapper.GetKeyValue(objList[i])));
                }

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  ({keyParmStr})"; var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Removes multiple records of type T based on their keys asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                StringBuilder keyParmStr = new StringBuilder();
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < keyList.Count; i++)
                {
                    if(i > 0 )
                    {
                        keyParmStr.Append(",");
                    }
                    keyParmStr.Append($"@P{i}");
                    parameters.Add(new KeyValuePair<string, object>($"@P{i}", keyList[i]));
                }

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  ({keyParmStr})";
                var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == keyList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }

        }

        /// <summary>
        /// Removes a record matching the instance of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>( "@P0", Mapper.GetKeyValue(obj))
                };
                var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Removes a record of type T based on its key asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="key">Key value for record to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAsync<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>( "@P0", key)
                };
                var cmd = CreateCommand(cmdStr, parameters, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Adds an instance of T to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Insert<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));

                List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName,0);
                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters,0);
                var cmd = CreateCommand(cmdStr, sqlParams, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Adds a collection of type T objects to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                List<KeyValuePair<string, object>> combinedSqlParam = new List<KeyValuePair<string, object>>();
                foreach(var obj in objList)
                {
                    List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
                    foreach(var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }
                   
                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Adds a collection of type T objects to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                List<KeyValuePair<string, object>> combinedSqlParam = new List<KeyValuePair<string, object>>();
                foreach (var obj in objList)
                {
                    List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
                    foreach (var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }
                    

                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Adds an instance of T to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));

                List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName, 0);
                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters,0);
                var cmd = CreateCommand(cmdStr, sqlParams, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Executes a data modification statement against the data source
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A status indicating the result of the operation</returns>
        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                List<KeyValuePair<string, object>> parametersLst = new List<KeyValuePair<string, object>>();
                foreach(var parm in parameters)
                {
                    parametersLst.Add(parm);
                }

                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parametersLst,0);
                var cmd = CreateCommand(statement, sqlParams, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Executes a data modification statement against the data source asynchronously
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating status indicating the result of the operation</returns>
        public async Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                List<KeyValuePair<string, object>> parametersLst = new List<KeyValuePair<string, object>>();
                foreach (var parm in parameters)
                {
                    parametersLst.Add(parm);
                }

                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parametersLst,0);
                var cmd = CreateCommand(statement, sqlParams, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Run a data retrieval statement directly against data source for record type T
        /// </summary>
        /// <typeparam name="T">Record type T</typeparam>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>Matching instances of T</returns>
        public IEnumerable<T> Query<T>(string query, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                List<KeyValuePair<string, object>> parametersLst = new List<KeyValuePair<string, object>>();
                foreach (var parm in parameters)
                {
                    parametersLst.Add(parm);
                }
                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parametersLst,0);
                var cmd = CreateCommand(query, sqlParams, connection);
                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    T record = Util.Container.CreateInstance<T>();
                    record = Mapper.CreateInstanceFromFields(record,ConvertReaderToKV(reader));
                    yield return record;
                }
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Runs a data retrieval statement directly against the data source for one or more arbitrary record types
        /// </summary>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>Query results with each record's fields encoded as a series of key-value pairs</returns>
        public IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                List<KeyValuePair<string, object>> parametersLst = new List<KeyValuePair<string, object>>();
                foreach (var parm in parameters)
                {
                    parametersLst.Add(parm);
                }
                List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parametersLst,0);
                var cmd = CreateCommand(query, sqlParams, connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    yield return ConvertReaderToKV(reader);
                }
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Run a data retrieval statement directly against data source for record type T asynchronously
        /// </summary>
        /// <typeparam name="T">Record type T</typeparam>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        public Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, object> parameters)
        {
            return Task.FromResult(Query<T>(query, parameters));
        }

        /// <summary>
        /// Runs a data retrieval statement directly against the data source for one or more arbitrary record types asynchronously
        /// </summary>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating the query results with each record's fields encoded as a series of key-value pairs</returns>
        public Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IDictionary<string, object> parameters)
        {
            return Task.FromResult(Query(query, parameters));
        }


        /// <summary>
        /// Reverts uncommitted data transactions if applicable
        /// </summary>
        public void RollBack()
        {
            if (!AutoCommit)
            {
                foreach (var connTran in _connectionTransactionMap)
                {
                    if (connTran.Key.State == System.Data.ConnectionState.Open && connTran.Value != null)
                    {
                        connTran.Value.Rollback();
                        connTran.Key.Close();
                    }
                }
                _connectionTransactionMap.Clear();
            }
        }


        /// <summary>
        /// Retrieves all instances of T from the data source
        /// </summary>
        /// <param name="orderbyCols">The list of columns to order by</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>An iterator for traversing the returned records</returns>
        public IEnumerable<T> SelectAll<T>()
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                var cmd = CreateCommand($"SELECT * FROM {_tablePrefix}{Mapper.GetObjectMapName(typeof(T))}" , null, connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    T record = Util.Container.CreateInstance<T>();
                    record = Mapper.CreateInstanceFromFields(record, ConvertReaderToKV(reader));
                    yield return record;
                }
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Retrieves all instances of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A completion token encapsulating the iterator for traversing the returned records</returns>
        public Task<IEnumerable<T>> SelectAllAsync<T>()
        {
            return Task.FromResult(SelectAll<T>());
        }



        /// <summary>
        /// Retrieves all instances of T matching the specified expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>Matching instances of T</returns>
        public IEnumerable<T> SelectMatching<T>(Expression<Func<T, bool>> matcher)
        {
            return from record in SelectAll<T>() where matcher.Compile()(record) select record;
        }



        /// <summary>
        /// Retrieves all instances of T matching the specified expression asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        { 
            return Task.FromResult(SelectMatching<T>(matcher));
        }


        /// <summary>
        /// Retrieves a single instance of T based on the specified key
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Key corresponding to the retrieved record</param>
        /// <returns>The record matching the key</returns>
        public T SelectOne<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                var cmd = CreateCommand($"SELECT TOP 1 * FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0", new List<KeyValuePair<string, object>> { 
                    new KeyValuePair<string, object>("@P0",key) } , connection);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    T record = Util.Container.CreateInstance<T>();
                    record = Mapper.CreateInstanceFromFields(record, ConvertReaderToKV(reader));
                    return record;
                }
                return default(T);
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Retrieves a single instance of T based on the specified key asynchronously
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Key corresponding to the retrieved record</param>
        /// <returns>A completion token encapsulating the record matching the key</returns>
        public async Task<T> SelectOneAsync<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                var cmd = CreateCommand($"SELECT TOP 1 * FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0", new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>("@P0",key) }, connection);
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    T record = Util.Container.CreateInstance<T>();
                    record = Mapper.CreateInstanceFromFields(record, ConvertReaderToKV(reader));
                    return record;
                }
                return default(T);
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Returns all instances of T within a specified page
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <param name="from">Page offset</param>
        /// <param name="length">Page size</param>
        /// <returns>Instances of T within the data page</returns>
        public IEnumerable<T> SelectRange<T>(int from, int length)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                var cmd = CreateCommand($"SELECT * FROM {_tablePrefix}{tableName} {BuildRangeQueryForProvider(from,length,keyName)}", null, connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    T record = Util.Container.CreateInstance<T>();
                    record = Mapper.CreateInstanceFromFields(record, ConvertReaderToKV(reader));
                    yield return record;
                }
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Returns all instances of T within a specified page asynchronously
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <param name="from">Page offset</param>
        /// <param name="length">Page size</param>
        /// <returns>A completion token encapsulating the instances of T within the data page</returns>
        public Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length)
        {
            return Task.FromResult(SelectRange<T>(from, length));
        }

        /// <summary>
        /// Updates a record in the data source matching the instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T matching record to be updated</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Update<T>(T obj, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                object key = Mapper.GetKeyValue(obj);
                List<KeyValuePair<string, object>> objParam = TransformObjectToParams(obj, updateNulls);
                SwapKeyForLast<T>(objParam);
                List<KeyValuePair<string, object>> sqlParam = TransformToSqlParam(objParam, 0);
                var cmd = CreateCommand(BuildCommandForUpdate(objParam,tableName,0, 
                    new KeyValuePair<string,object>(Mapper.GetKeyMapName(typeof(T)), key)), sqlParam, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// Updates multiple records the data source matching the instances of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">List of matching records</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> UpdateAll<T>(IList<T> objList, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                List<KeyValuePair<string, object>> combinedSqlParam = new List<KeyValuePair<string, object>>();
                foreach (var obj in objList)
                {
                    List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj,updateNulls);
                    SwapKeyForLast<T>(parameters);
                    cmdStr.Append(BuildCommandForUpdate(parameters, tableName, combinedSqlParam.Count,
                        new KeyValuePair<string, object>(Mapper.GetKeyMapName(typeof(T)), Mapper.GetKeyValue(obj))) + ";\n");
                    List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
                    foreach (var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }

                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
                int result = cmd.ExecuteNonQuery();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Updates multiple records the data source matching the instances of T asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">List of matching records</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> UpdateAllAsync<T>(IList<T> objList, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                List<KeyValuePair<string, object>> combinedSqlParam = new List<KeyValuePair<string, object>>();
                foreach (var obj in objList)
                {
                    List<KeyValuePair<string, object>> parameters = TransformObjectToParams(obj, updateNulls);
                    SwapKeyForLast<T>(parameters);
                    cmdStr.Append(BuildCommandForUpdate(parameters, tableName, combinedSqlParam.Count,
                        new KeyValuePair<string, object>(Mapper.GetKeyMapName(typeof(T)), Mapper.GetKeyValue(obj))) + ";\n");
                    List<KeyValuePair<string, object>> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
                    foreach (var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }

                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result == objList.Count;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }



        /// <summary>
        /// Updates a record in the data source matching the instance of T asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T matching record to be updated</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> UpdateAsync<T>(T obj, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                object key = Mapper.GetKeyValue(obj);
                List<KeyValuePair<string, object>> objParam = TransformObjectToParams(obj, updateNulls);
                SwapKeyForLast<T>(objParam);
                List<KeyValuePair<string, object>> sqlParam = TransformToSqlParam(objParam, 0);
                var cmd = CreateCommand(BuildCommandForUpdate(objParam, tableName, 0,
                    new KeyValuePair<string, object>(Mapper.GetKeyMapName(typeof(T)), key)), sqlParam, connection);
                int result = await cmd.ExecuteNonQueryAsync();
                IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
                status.IsSuccess = result > 0;
                status.StatusInfo = result;
                return status;
            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if(_connectionTransactionMap != null)
                    {
                        foreach(var connMap in _connectionTransactionMap)
                        {
                            connMap.Key.Close();
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SqlDataContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public int Count<T>()
        {
            DbConnection connection = CreateConnection();
            try
            {
                DbCommand command = CreateCommand($"SELECT COUNT(1) FROM {_tablePrefix}{Mapper.GetObjectMapName(typeof(T))}",
                   null, connection);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return Convert.ToInt32(reader[0]);
                }

            }
            finally
            {
                if(AutoCommit)
                {
                    connection.Close();
                }
            }
            return 0;
        }

        public async Task<int> CountAsync<T>()
        {
            DbConnection connection = CreateConnection();
            try
            {
                DbCommand command = CreateCommand($"SELECT COUNT(1) FROM {_tablePrefix}{Mapper.GetObjectMapName(typeof(T))}",
                   null, connection);
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Convert.ToInt32(reader[0]);
                }

            }
            finally
            {
                if (AutoCommit)
                {
                    connection.Close();
                }
            }
            return 0;
        }
        #endregion

    }
}
