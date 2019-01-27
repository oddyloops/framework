﻿using Framework.Interfaces;
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

            foreach(var param in parameters)
            {
                cmd.Parameters.Add(CreateParameter(param.Key, param.Value));
            }
            return cmd;
        }

        private IDictionary<string,object> TransformObjectToParams(object record)
        {
         
            IDictionary<string, object> paramRec = new SortedDictionary<string, object>(); //need to maintain order for traversals
            IList<string> fieldNames = Mapper.GetFieldNames(record.GetType());
            foreach(var field in fieldNames)
            {
                paramRec.Add(field, Mapper.GetField(field, record));
            }
            return paramRec;
        }

        private IDictionary<string,object> TransformToSqlParam(IDictionary<string,object> parameters)
        {
            IDictionary<string, object> sqlParameters = new Dictionary<string, object>();
            int paramCount = 0;
            foreach(var param in parameters)
            {
                sqlParameters.Add("@P" + paramCount, param.Value);
                paramCount++;
            }

            return sqlParameters;
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
                    isFirst = false;
                }
                command.Append(param.Key);
            }
            command.Append(") VALUES (");

            for(int i =0; i < parameters.Count; i++)
            {
                if (!isFirst)
                {
                    command.Append(",");
                    isFirst = false;
                }
                command.Append("@P" + (paramOffset + i));
            }
            return command.ToString();

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
                string tableName = Mapper.GetObjectName(obj.GetType());
                string keyName = Mapper.GetKeyName(obj.GetType());

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
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));

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
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));
                

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

        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                connection.Open();
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));


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
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));


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

        public async Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> keyList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {

                await connection.OpenAsync();
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));


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

        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectName(obj.GetType());
                string keyName = Mapper.GetKeyName(obj.GetType());

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
                string tableName = Mapper.GetObjectName(typeof(T));
                string keyName = Mapper.GetKeyName(typeof(T));

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
                string tableName = Mapper.GetObjectName(typeof(T));
                
                IDictionary<string, object> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName,0);
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters);
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
                string tableName = Mapper.GetObjectName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();
                int totalParams = 0;
                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach(var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, totalParams) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters);
                    foreach(var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }

                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
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

        public async Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectName(typeof(T));
                StringBuilder cmdStr = new StringBuilder();
                int totalParams = 0;
                IDictionary<string, object> combinedSqlParam = new Dictionary<string, object>();
                foreach (var obj in objList)
                {
                    IDictionary<string, object> parameters = TransformObjectToParams(obj);
                    cmdStr.Append(BuildCommandForInsert(parameters, tableName, totalParams) + ";\n");
                    IDictionary<string, object> sqlParams = TransformToSqlParam(parameters);
                    foreach (var param in sqlParams)
                    {
                        combinedSqlParam.Add(param);
                    }
                    totalParams += sqlParams.Count;

                }
                var cmd = CreateCommand(cmdStr.ToString(), combinedSqlParam, connection);
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

        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            DbConnection connection = null;
            connection = CreateConnection();

            try
            {
                await connection.OpenAsync();
                string tableName = Mapper.GetObjectName(typeof(T));

                IDictionary<string, object> parameters = TransformObjectToParams(obj);
                string cmdStr = BuildCommandForInsert(parameters, tableName, 0);
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters);
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
                

             
 
                IDictionary<string, object> sqlParams = TransformToSqlParam(parameters);
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

        public Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectAll<T>()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> SelectAllAsync<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectMatching<T>(Expression<T> matcher)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            throw new NotImplementedException();
        }

        public T SelectOne<T, K>(K key)
        {
            throw new NotImplementedException();
        }

        public Task<T> SelectOneAsync<T, K>(K key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectRange<T>(int from, int length)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> Update<T>(T obj, bool updateNulls = false)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> UpdateAll<T>(IList<T> objList, bool updateNulls = false)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> UpdateAllAsync<T>(IList<T> objList, bool updateNulls = false)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> UpdateAsync<T>(T obj, bool updateNulls = false)
        {
            throw new NotImplementedException();
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
