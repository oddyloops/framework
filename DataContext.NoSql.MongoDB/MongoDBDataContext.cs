using Framework.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataContext.NoSql.MongoDB
{
    [Export("NoSql.MongoDB",typeof(IDataContext))]
    public class MongoDBDataContext : IDataContext
    {
        private string _connectionString;

        private IMongoClient _client;




        [Import]
        public IDataMapper Mapper { get; set; }

        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }
        public bool AutoCommit { get; set; }
        public string DBName { get; set; }

        public MongoDBDataContext()
        {
            DBName = Config.GetValue(ConfigConstants.DEFAULT_MONGODB_DATABASE);
            
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            Connect(Config.GetValue(ConfigConstants.MONGODB_CONNECTION_STRING));
            
        }

        public void Connect(string str)
        {
            _connectionString = str;
            _client = new MongoClient(_connectionString);
            
           
        }

        public IStatus<int> Delete<T>(T obj)
        {
            var db = _client.GetDatabase(DBName);
        }

        public IStatus<int> Delete<T, K>(K key)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> DeleteAll<T>(IList<T> objList)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> DeleteAllAsync<T>(IList<T> obj)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> key)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> DeleteAsync<T, K>(K key)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> Insert<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
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

        public IEnumerable<T> SelectMatching<T>(Expression<Func<T, bool>> matcher)
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
        // ~MongoDBDataContext() {
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
