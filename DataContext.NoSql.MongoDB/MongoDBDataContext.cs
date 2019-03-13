using Framework.Interfaces;
using Framework.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataContext.NoSql.MongoDB
{
    [Export("NoSql.MongoDB",typeof(IDataContext))]
    public class MongoDBDataContext : IDataContext
    {
        private string _connectionString;

        private IMongoClient _client;


        #region Helpers

        private IMongoCollection<T> GetCollectionForType<T>()
        {
            var db = _client.GetDatabase(DBName);
            string collectionName = Mapper.GetNoSqlCollectionName(typeof(T));
            return db.GetCollection<T>(collectionName);
        }

        private FilterDefinition<T> BuildFilterDefinitionForObject<T>(T obj)
        {
            var builder = Builders<T>.Filter;
            return builder.Eq(x => Mapper.GetKeyValue(x).ToString(), Mapper.GetKeyValue(obj).ToString());
        }

        private FilterDefinition<T> BuildFilterDefinitionForObjectKey<T,K>(K key)
        {
            var builder = Builders<T>.Filter;
            return builder.Eq(x => Mapper.GetKeyValue(x).ToString(), key.ToString());
        }
        #endregion


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
            var result = GetCollectionForType<T>().DeleteOne(BuildFilterDefinitionForObject(obj));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public IStatus<int> Delete<T, K>(K key)
        {
            var result = GetCollectionForType<T>().DeleteOne(BuildFilterDefinitionForObjectKey<T,K>(key));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public IStatus<int> DeleteAll<T>(IList<T> objList)
        {
            IList<string> keyList = new List<string>(objList.Count);

            foreach(T obj in objList)
            {
                keyList.Add(Mapper.GetKeyValue(obj).ToString());
            }
            var result = GetCollectionForType<T>().DeleteMany<T>( x => keyList.Contains( Mapper.GetKeyValue(x).ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            var result = GetCollectionForType<T>().DeleteMany<T>(x => keyList.Contains((K)Mapper.GetKeyValue(x)));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public async Task<IStatus<int>> DeleteAllAsync<T>(IList<T> objList)
        {
            IList<string> keyList = new List<string>(objList.Count);

            foreach(T obj in objList)
            {
                keyList.Add(Mapper.GetKeyValue(obj).ToString());
            }
            var result = await GetCollectionForType<T>().DeleteManyAsync<T>( x => keyList.Contains( Mapper.GetKeyValue(x).ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public async Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> keyList)
        {
            var result =await GetCollectionForType<T>().DeleteManyAsync<T>(x => keyList.Contains((K)Mapper.GetKeyValue(x)));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            var result = await GetCollectionForType<T>().DeleteOneAsync(BuildFilterDefinitionForObject(obj));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public async Task<IStatus<int>> DeleteAsync<T, K>(K key)
        {
            var result = await GetCollectionForType<T>().DeleteOneAsync(BuildFilterDefinitionForObjectKey<T, K>(key));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }

        public IStatus<int> Insert<T>(T obj)
        {
            var collection = GetCollectionForType<T>();
            collection.InsertOne(obj);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess =true;
            status.StatusInfo = 1;
            return status;
        }

        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            var collection = GetCollectionForType<T>();
            collection.InsertMany(objList);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = objList.Count;
            return status;
        }

        public async Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            var collection = GetCollectionForType<T>();
            await collection.InsertManyAsync(objList);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = objList.Count;
            return status;
        }

        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            var collection = GetCollectionForType<T>();
            await collection.InsertOneAsync(obj);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = 1;
            return status;
        }

        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            var db = _client.GetDatabase(DBName);
            var command = new JsonCommand<BsonDocument>(statement);
            db.run
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
            var collection = GetCollectionForType<T>();
            return collection.AsQueryable();
        }

        public Task<IEnumerable<T>> SelectAllAsync<T>()
        {
            return Task.FromResult(SelectAll<T>());
        }

        public IEnumerable<T> SelectMatching<T>(Expression<Func<T, bool>> matcher)
        {
            return (from x in SelectAll<T>() where matcher.Compile()(x) select x);
        }

        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            return Task.FromResult(SelectMatching(matcher));
        }

        public T SelectOne<T, K>(K key)
        {
            return (from x in SelectAll<T>() where Mapper.GetKeyValue(x).ToString().Equals(key.ToString()) select x).First();
        }

        public Task<T> SelectOneAsync<T, K>(K key)
        {
            return Task.FromResult(SelectOne<T,K>(key));
        }

        public IEnumerable<T> SelectRange<T>(int from, int length)
        {
            return SelectAll<T>().Skip(from).Take(length);
        }

        public Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length)
        {
            return Task.FromResult(SelectRange<T>(from, length));
        }

        public IStatus<int> Update<T>(T obj, bool updateNulls = false)
        {
            var collection = GetCollectionForType<T>();
            string keyString = Mapper.GetKeyValue(obj).ToString();
            T record = SelectOne<T,string>(keyString);
            Util.DeepCopy(obj, record, !updateNulls, Mapper.GetKeyName(typeof(T)));
            var result = collection.ReplaceOne(BuildFilterDefinitionForObjectKey<T, string>(keyString), record);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.ModifiedCount;
            return status;
        }

        public IStatus<int> UpdateAll<T>(IList<T> objList, bool updateNulls = false)
        {
            var collection = GetCollectionForType<T>();
            IList<string> keyList = new List<string>(objList.Count);

            foreach (T obj in objList)
            {
                keyList.Add(Mapper.GetKeyValue(obj).ToString());
            }
            IList<T> records = SelectMatching<T>( x => keyList.Contains(Mapper.GetKeyValue(x).ToString())).ToList();
            
            foreach(var record in records)
            {
                record.
            }
            Util.DeepCopy(obj, record, !updateNulls, Mapper.GetKeyName(typeof(T)));
            var result = collection.ReplaceOne(BuildFilterDefinitionForObjectKey<T, string>(keyString), record);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.ModifiedCount;
            return status;
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
