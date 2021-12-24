using Framework.Interfaces;
using Framework.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataContext.NoSql.MongoDB
{
    /// <summary>
    /// A concrete implementation of the IDataContext interface for mongo DB
    /// </summary>
    [Export("Mongo",typeof(IDataContext))]
    public class MongoDBDataContext : IDataContext
    {
        private string _connectionString;

        private IMongoClient _client;

        private IClientSessionHandle _transSession;


        #region Helpers
        /// <summary>
        /// Helper method for extracting collection mapped to type with the MapAttribute
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>MongoDB collection object</returns>
        private IMongoCollection<T> GetCollectionForType<T>()
        {
            var db = _client.GetDatabase(DBName);
            string collectionName = Mapper.GetObjectMapName(typeof(T));
            return db.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Helper method for extracting document collection mapped to type with the MapAttribute
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>MongoDB document collection object</returns>
        private IMongoCollection<BsonDocument> GetDocumentCollection<T>()
        {
            var db = _client.GetDatabase(DBName);
            string collectionName = Mapper.GetObjectMapName(typeof(T));
            return db.GetCollection<BsonDocument>(collectionName);
        }

        /// <summary>
        /// Helper method for defining a collection filter for a mapped object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Mapped object</param>
        /// <returns>A filter predicate matching object's attributes</returns>
        private FilterDefinition<T> BuildFilterDefinitionForObject<T>(T obj)
        {
            var builder = Builders<T>.Filter;
            return builder.Eq(x => Mapper.GetKeyValue(x).ToString(), Mapper.GetKeyValue(obj).ToString());
        }

       

        /// <summary>
        /// Helper method for builing a jsoncommand object from a raw mongo db command string
        /// </summary>
        /// <param name="query">Command string</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>JsonCommand object wrapping the parameterized mongodb command</returns>
        private static JsonCommand<BsonDocument> BuildCommandObject(string query,IDictionary<string,object> parameters)
        {
            foreach(var parm in parameters)
            {
                if(parm.Value is string || parm.Value is DateTime)
                {
                    query = query.Replace(parm.Key, $"'{parm.Value.ToString()}'");
                }
                else
                {
                    query = query.Replace(parm.Key, parm.Value.ToString());
                }
            }
            return new JsonCommand<BsonDocument>(query);
        }

   
        #endregion


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

        /// <summary>
        /// Name of the mongo DB database
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// Cosntructs an instance of this class based on configuration values
        /// </summary>
        [ImportingConstructor]
        public MongoDBDataContext([Import("JsonConfig")] IConfiguration config)
        {
            Config = config;
            DBName = Config.GetValue(ConfigConstants.DEFAULT_MONGODB_DATABASE);
            AutoCommit = Config.GetValue(ConfigConstants.MONGODB_AUTOCOMMIT) == "1";
            
        }


        /// <summary>
        /// Commits data transaction to data source if applicable
        /// </summary>
        public void Commit()
        {
            
            if(!AutoCommit)
            {
                _transSession.CommitTransaction();
                _transSession = _client.StartSession();
            }
        }


        /// <summary>
        /// Connects data provider to its source
        /// </summary>
        public virtual void Connect()
        {
            if (_client == null)
            {
                Connect(Config.GetValue(ConfigConstants.MONGODB_CONNECTION_STRING));
            }
        }


        /// <summary>
        /// Connects data provider to its source addressed by supplied connection string
        /// </summary>
        /// <param name="str">Connection string</param>
        public virtual void Connect(string str)
        {
            if (_client == null)
            {
                _connectionString = str;
                _client = new MongoClient(_connectionString);
                if (!AutoCommit)
                {
                    _transSession = _client.StartSession();
                }
            }
            
           
        }


        /// <summary>
        /// Removes a record matching the instance of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Delete<T>(T obj)
        {
            Connect();
            var result = GetDocumentCollection<T>().DeleteOne(new BsonDocument("_id", Mapper.GetKeyValue(obj).ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
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
            Connect();
            var result = GetDocumentCollection<T>().DeleteOne(new BsonDocument("_id", key.ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }


        /// <summary>
        /// Removes multiple records matching each instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> DeleteAll<T>(IList<T> objList)
        {
            Connect();
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

        /// <summary>
        /// Removes multiple records of type T based on their keys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> DeleteAll<T, K>(IList<K> keyList)
        {
            Connect();
            var result = GetCollectionForType<T>().DeleteMany<T>(x => keyList.Contains((K)Mapper.GetKeyValue(x)));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }


        /// <summary>
        /// Removes multiple records matching each instances of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAllAsync<T>(IList<T> objList)
        {
            Connect();
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


        /// <summary>
        /// Removes multiple records of type T based on their keys asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAllAsync<T, K>(IList<K> keyList)
        {
            Connect();
            var result =await GetCollectionForType<T>().DeleteManyAsync<T>(x => keyList.Contains((K)Mapper.GetKeyValue(x)));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }


        /// <summary>
        /// Removes a record matching the instance of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            Connect();
            var result = await GetDocumentCollection<T>().DeleteOneAsync(new BsonDocument("_id",Mapper.GetKeyValue(obj).ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
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
            Connect();
            var result = await GetDocumentCollection<T>().DeleteOneAsync(new BsonDocument("_id", key.ToString()));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.DeletedCount;
            return status;
        }


        /// <summary>
        /// Adds an instance of T to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Insert<T>(T obj)
        {
            Connect();
            var collection = GetCollectionForType<T>();
            collection.InsertOne(obj);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess =true;
            status.StatusInfo = 1;
            return status;
        }


        /// <summary>
        /// Adds a collection of type T objects to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            Connect();
            var collection = GetCollectionForType<T>();
            collection.InsertMany(objList);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = objList.Count;
            return status;
        }


        /// <summary>
        /// Adds a collection of type T objects to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            Connect();
            var collection = GetCollectionForType<T>();
            await collection.InsertManyAsync(objList);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = objList.Count;
            return status;
        }


        /// <summary>
        /// Adds an instance of T to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            Connect();
            var collection = GetCollectionForType<T>();
            await collection.InsertOneAsync(obj);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = 1;
            return status;
        }


        /// <summary>
        /// Executes a data modification statement against the data source
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A status indicating the result of the operation</returns>
        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            Connect();
            var result = _client.GetDatabase(DBName).RunCommand(BuildCommandObject(statement, parameters));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = 0;
            return status;
        }


        /// <summary>
        /// Executes a data modification statement against the data source asynchronously
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating status indicating the result of the operation</returns>
        public async Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            Connect();
            var result = await _client.GetDatabase(DBName).RunCommandAsync(BuildCommandObject(statement, parameters));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = true;
            status.StatusInfo = 0;
            return status;
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
            Connect();
            var result = _client.GetDatabase(DBName).RunCommand(BuildCommandObject(query, parameters));
            foreach(var item in result.AsBsonArray)
            {
                yield return BsonSerializer.Deserialize<T>(item.AsBsonDocument);
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
            Connect();
            var result = _client.GetDatabase(DBName).RunCommand(BuildCommandObject(query, parameters));
            foreach (var item in result.AsBsonArray)
            {
                yield return item.AsBsonDocument.ToDictionary();
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
            if(!AutoCommit)
            {
                _transSession.AbortTransaction();
                _transSession = _client.StartSession();
            }
        }


        /// <summary>
        /// Retrieves all instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An iterator for traversing the returned records</returns>
        public IEnumerable<T> SelectAll<T>()
        {
            Connect();
            var collection = GetCollectionForType<T>();
            return collection.AsQueryable();
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
            Connect();
            return (from x in SelectAll<T>() where matcher.Compile()(x) select x);
        }


        /// <summary>
        /// Retrieves all instances of T matching the specified expression asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        public async Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            Connect();
            var collection = GetCollectionForType<T>();
            return (await collection.FindAsync(matcher)).ToEnumerable();
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
            Connect();
            var collection = GetDocumentCollection<T>();
            var result = collection.Find(new BsonDocument ( "_id", key.ToString())).FirstOrDefault();
            return result == null ? default : BsonSerializer.Deserialize<T>(result);
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
            Connect();
            var collection = GetDocumentCollection<T>();
            var result = (await collection.FindAsync(new BsonDocument("_id", key.ToString()))).FirstOrDefault();
            return result == null ? default : BsonSerializer.Deserialize<T>(result);
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
            return SelectAll<T>().Skip(from).Take(length);
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
            Connect();
            var collection = GetDocumentCollection<T>();
            string keyString = Mapper.GetKeyValue(obj).ToString();
            T record = SelectOne<T,string>(keyString);
            Util.DeepCopy(obj, record, !updateNulls, Mapper.GetKeyName(typeof(T)));
            var result = collection.ReplaceOne(new BsonDocument("_id", keyString), record.ToBsonDocument());
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.ModifiedCount;
            return status;
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
            Connect();
            int updateCount = 0;
            foreach (T obj in objList)
            {
                var result = Update(obj, updateNulls);
                if(result.IsSuccess)
                {
                    updateCount++;
                }
            }
         
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = updateCount == objList.Count;
            status.StatusInfo = updateCount;
            return status;
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
            Connect();
            int updateCount = 0;
            foreach (T obj in objList)
            {
                var result = await UpdateAsync(obj, updateNulls);
                if (result.IsSuccess)
                {
                    updateCount++;
                }
            }

            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = updateCount == objList.Count;
            status.StatusInfo = updateCount;
            return status;
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
            Connect();
            var collection = GetDocumentCollection<T>();
            string keyString = Mapper.GetKeyValue(obj).ToString();
            T record = await SelectOneAsync<T, string>(keyString);
            Util.DeepCopy(obj, record, !updateNulls, Mapper.GetKeyName(typeof(T)));
            var result = await collection.ReplaceOneAsync(new BsonDocument("_id", keyString), record.ToBsonDocument());
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.IsAcknowledged;
            status.StatusInfo = (int)result.ModifiedCount;
            return status;
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

        public int Count<T>()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync<T>()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
