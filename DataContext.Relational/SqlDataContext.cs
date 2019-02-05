using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Framework.Utils;
using System.Data;
using System.Text;
using System.Linq;

namespace DataContext.Relational
{

    public enum DbType
    {
        SqlServer,
        MySql,
        Oracle
    }

    [Export("Relational", typeof(IDataContext))]
    public class SqlDataContext : IDataContext
    {

        private IDictionary<DbConnection, DbTransaction> _connectionTransactionMap;

        private DbType _dbType;

        private string _connectionString;

        private string _tablePrefix;


        [Import]
        public IDataMapper Mapper { get; set; }

        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }


        public bool AutoCommit { get; set; }


        #region Helpers
        private DbConnection CreateConnection()
        {
            DbConnection connection = null;
            switch (_dbType)
            {
                case DbType.MySql:
                    connection = new MySqlConnection(_connectionString);
                    break;
                case DbType.Oracle:
                    connection = new OracleConnection(_connectionString);
                    break;
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

        private DbParameter CreateParameter(string key, object value)
        {
            switch (_dbType)
            {
                case DbType.MySql:
                    return new MySqlParameter(key, value);
                case DbType.Oracle:
                    return new OracleParameter(key, value);
                default:
                    return new SqlParameter(key, value); //default to sql server
            }
        }

        private DbCommand CreateCommand(string command, IDictionary<string, object> parameters,DbConnection connection)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = command;
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(CreateParameter(param.Key, param.Value));
                }
            }
            return cmd;
        }

        private IDictionary<string,object> TransformObjectToParams(object record, bool addNulls = true)
        {
         
            IDictionary<string, object> paramRec = new SortedDictionary<string, object>(); //need to maintain order for traversals
            IList<string> fieldNames = Mapper.GetFieldNames(record.GetType());
            foreach(var field in fieldNames)
            {
                if (addNulls || Mapper.GetField(field, record) != null)
                {
                    paramRec.Add(field, Mapper.GetField(field, record));
                }
            }
            return paramRec;
        }

        private IDictionary<string,object> TransformToSqlParam(IDictionary<string,object> parameters, int paramOffset)
        {
            IDictionary<string, object> sqlParameters = new SortedDictionary<string, object>();
            int paramCount = 0;
            foreach(var param in parameters)
            {
                sqlParameters.Add("@P" + (paramCount + paramOffset), param.Value);
                paramCount++;
            }

            return sqlParameters;
        }

        private string BuildCommandForUpdate(IDictionary<string,object> parameters, string tableName, int paramOffset)
        {
            StringBuilder command = new StringBuilder($"UPDATE {_tablePrefix}{tableName} SET ");
            bool isFirst = true;
            int paramCount = 0;
            foreach(var param in parameters)
            {
                if(!isFirst)
                {
                    command.Append(",");
                }
                command.Append($"{param.Key} = @P{paramCount + paramOffset}");
                isFirst = false;
            }
            return command.ToString();
        }

        private string BuildCommandForInsert(IDictionary<string,object> parameters,string tableName, int paramOffset)
        {
            StringBuilder command = new StringBuilder($"INSERT INTO {_tablePrefix}{tableName}(");
            bool isFirst = true;
            foreach(var param in parameters)
            {
                if(!isFirst)
                {
                    command.Append(",");
                }
                command.Append(param.Key);
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
            return command.ToString();

        }


        private IDictionary<string,object> ConvertReaderToKV(DbDataReader reader)
        {
            IDictionary<string, object> keyValues = new Dictionary<string, object>();
            for(int i =0; i < reader.FieldCount; i++)
            {
                keyValues.Add(reader.GetName(i), reader.GetValue(i));
            }
            return keyValues;
        }

        private string BuildRangeQueryForProvider(int skip, int length)
        {
            switch(_dbType)
            {
                case DbType.MySql:
                    return $"LIMIT {skip},{length}";
                default:
                    return $"OFFSET {skip} ROWS FETCH NEXT {length} ROWS ONLY"; //sql server and oracle share the same syntax
                
            }

        }

        #endregion


        public SqlDataContext()
        {
            AutoCommit = Config.GetValue(ConfigConstants.RELATIONAL_DB_AUTOCOMMIT) == "1";

            if (!AutoCommit)
            {
                _connectionTransactionMap = new Dictionary<DbConnection, DbTransaction>();
            }

            string dbTypeCode = Config.GetValue(ConfigConstants.RELATIONAL_DB_TYPE);
            _dbType = dbTypeCode == null ? DbType.SqlServer : (DbType)Convert.ToInt32(dbTypeCode);

            string tablePrefix = Config.GetValue(ConfigConstants.RELATIONAL_TABLE_PREFIX);
            _tablePrefix = tablePrefix ?? string.Empty;


        }

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

        public void Connect()
        {
            Connect(ConfigConstants.RELATIONAL_CONNECTION_STRING);
        }

        public void Connect(string str)
        {
            _connectionString = Config.GetValue(str);
        }

        public IStatus<int> Delete<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(obj.GetType());
                string keyName = Mapper.GetKeyMapName(obj.GetType());

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", Mapper.GetKeyValue(obj) }
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
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", key }
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

        public IStatus<int> DeleteAll<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));
                

                //create data table

                DataTable keyTable = new DataTable("KeyTable");
                keyTable.Columns.Add("ID", Mapper.GetFieldByName(keyName, typeof(T)).PropertyType);
                foreach(var obj  in objList)
                {
                    keyTable.Rows.Add(Mapper.GetKeyValue(obj));
                }


                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", keyTable }
                };
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

        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));


                //create data table

                DataTable keyTable = new DataTable("KeyTable");
                keyTable.Columns.Add("ID", typeof(K));
                foreach (var key in keyList)
                {
                    keyTable.Rows.Add(key);
                }


                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", keyTable }
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

        public async Task<IStatus<int>> DeleteAllAsync<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));


                //create data table

                DataTable keyTable = new DataTable("KeyTable");
                keyTable.Columns.Add("ID", Mapper.GetFieldByName(keyName, typeof(T)).PropertyType);
                foreach (var obj in objList)
                {
                    keyTable.Rows.Add(Mapper.GetKeyValue(obj));
                }


                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", keyTable }
                };
                var cmd = CreateCommand(cmdStr, parameters, connection);
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

        public async Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));


                //create data table

                DataTable keyTable = new DataTable("KeyTable");
                keyTable.Columns.Add("ID", typeof(K));
                foreach (var key in keyList)
                {
                    keyTable.Rows.Add(key);
                }


                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} IN  = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", keyTable }
                };
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

        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(obj.GetType());
                string keyName = Mapper.GetKeyMapName(obj.GetType());

                string cmdStr = $"DELETE FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0";
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", Mapper.GetKeyValue(obj) }
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
                IDictionary<string, object> parameters = new Dictionary<string, object> {
                    { "@P0", key }
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

        public IStatus<int> Insert<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                
                IDictionary<string, object> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName,0);
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
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

        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();
                
                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach(var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
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

        public async Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();
               
                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach (var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
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

        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));

                IDictionary<string, object> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName, 0);
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
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

        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
 
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
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

        public async Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();




                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
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

        public IEnumerable<T> Query<T>(string query, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
                var cmd = CreateCommand(query, sqlParams, connection);
                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    T record = Mapper.CreateInstanceFromFields<T>(ConvertReaderToKV(reader));
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

        public IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters,0);
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

        public Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, object> parameters)
        {
            return Task.FromResult(Query<T>(query, parameters));
        }

        public Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IDictionary<string, object> parameters)
        {
            return Task.FromResult(Query(query, parameters));
        }

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
                    T record = Mapper.CreateInstanceFromFields<T>(ConvertReaderToKV(reader));
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

        public Task<IEnumerable<T>> SelectAllAsync<T>()
        {
            return Task.FromResult(SelectAll<T>());
        }

        public IEnumerable<T> SelectMatching<T>(Expression<Func<T,bool>> matcher)
        {
            return from record in SelectAll<T>() where matcher.Compile()(record) select record;
        }

        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            return Task.FromResult(SelectMatching<T>(matcher));
        }

        public T SelectOne<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                var cmd = CreateCommand($"SELECT TOP 1 * FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0", new Dictionary<string, object> { { "@P0",key} }, connection);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    T record = Mapper.CreateInstanceFromFields<T>(ConvertReaderToKV(reader));
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

        public async Task<T> SelectOneAsync<T, K>(K key)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                string keyName = Mapper.GetKeyMapName(typeof(T));

                var cmd = CreateCommand($"SELECT TOP 1 * FROM {_tablePrefix}{tableName} WHERE {keyName} = @P0", new Dictionary<string, object> { { "@P0", key } }, connection);
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    T record = Mapper.CreateInstanceFromFields<T>(ConvertReaderToKV(reader));
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

        public IEnumerable<T> SelectRange<T>(int from, int length)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                var cmd = CreateCommand($"SELECT * FROM {_tablePrefix}{tableName} {BuildRangeQueryForProvider(from,length)}", null, connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    T record = Mapper.CreateInstanceFromFields<T>(ConvertReaderToKV(reader));
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

        public Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length)
        {
            return Task.FromResult(SelectRange<T>(from, length));
        }

        public IStatus<int> Update<T>(T obj, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                object key = Mapper.GetKeyValue(obj);
                IDictionary<string, object> objParam = TransformObjectToParams(obj, updateNulls);
                IDictionary<string, object> sqlParam = TransformToSqlParam(objParam, 0);
                var cmd = CreateCommand(BuildCommandForUpdate(objParam,tableName,0), sqlParam, connection);
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

        public IStatus<int> UpdateAll<T>(IList<T> objList, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                connection.Open();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach (var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForUpdate(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
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

        public async Task<IStatus<int>> UpdateAllAsync<T>(IList<T> objList, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectMapName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();

                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach (var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForUpdate(parameters, tableName, combinedSqlParam.Count) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters, combinedSqlParam.Count);
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

        public async Task<IStatus<int>> UpdateAsync<T>(T obj, bool updateNulls = false)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();

                string tableName = Mapper.GetObjectMapName(typeof(T));
                object key = Mapper.GetKeyValue(obj);
                IDictionary<string, object> objParam = TransformObjectToParams(obj, updateNulls);
                IDictionary<string, object> sqlParam = TransformToSqlParam(objParam, 0);
                var cmd = CreateCommand(BuildCommandForUpdate(objParam, tableName, 0), sqlParam, connection);
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
        #endregion

    }
}
