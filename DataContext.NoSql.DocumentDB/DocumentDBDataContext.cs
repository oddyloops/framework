using Framework.Interfaces;
using Framework.Utils;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace DataContext.NoSql.DocumentDB
{
    /// <summary>
    /// A concrete implementation of IDataContext for an Azure Document DB data source
    /// </summary>
    [Export("NoSql.DocumentDB",typeof(IDataContext))]
    public class DocumentDBDataContext : IDataContext
    {

        DocumentClient _client;

        /// <summary>
        /// A reference to a data mapper component required to map query results to concrete objects
        /// </summary>
        [Import]
        public IDataMapper Mapper { get; set; }

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        public bool AutoCommit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Database name for the Document DB data source
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// List of valid HTTP status codes (anything outside this is considered an operational or network error)
        /// </summary>
        private IList<HttpStatusCode> _validCodes = new List<HttpStatusCode>()
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent
        };

        #region Helpers
        /// <summary>
        /// Helper method for determining whether the supplied response code should raise an exception
        /// </summary>
        /// <param name="response">HTTP response code</param>
        private void ThrowOnFailure(HttpStatusCode response)
        {
            if(!_validCodes.Contains(response))
            {
                throw new Exception($"An error occured with request to Document DB with HTTP status code: {(int)response}");
            }
        }

       /// <summary>
       /// Builds document DB Uri for collection mapped to type T
       /// </summary>
       /// <typeparam name="T">Type T</typeparam>
       /// <returns>Type T document collection Uri</returns>
        private Uri UriForType<T>()
        {
            return UriFactory.CreateDocumentCollectionUri(DBName, Mapper.GetObjectMapName(typeof(T)));
        }

        /// <summary>
        /// Builds document DB URI for record
        /// </summary>
        /// <param name="document">Record</param>
        /// <returns>Record's document DB URI</returns>
        private Uri UriForDocument(object document)
        {
            return UriFactory.CreateDocumentUri(DBName, Mapper.GetObjectMapName(document.GetType()), Mapper.GetKeyValue(document).ToString());
        }

        /// <summary>
        /// Builds document DB URI for record with the key value
        /// </summary>
        /// <typeparam name="T">Record type that is mapped to a collection</typeparam>
        /// <param name="key">Key value used to identify the record</param>
        /// <returns>Record's document DB URI</returns>
        private Uri UriForDocumentKey<T>(string key)
        {
            return UriFactory.CreateDocumentUri(DBName, Mapper.GetObjectMapName(typeof(T)), key);
        }

        /// <summary>
        /// Transforms a key-value pair of query parameters into an Sql parameter collection
        /// </summary>
        /// <param name="parameters">KV-Pair of parameters</param>
        /// <returns>An equivalent Sql parameter collection</returns>
        private static SqlParameterCollection ConvertParameters(IDictionary<string, object> parameters)
        {
            SqlParameterCollection coll = new SqlParameterCollection();
            foreach(var parm in parameters)
            {
                coll.Add(new SqlParameter(parm.Key, parm.Value));
            }
            return coll;
        }
        #endregion

        /// <summary>
        /// Constructs the object based on its configuration values
        /// </summary>
        public DocumentDBDataContext()
        {
            DBName = Config.GetValue(ConfigConstants.DOCUMENT_DB_DATABASE);
        }

       
        public void Commit()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Connects data provider to its source
        /// </summary>
        public void Connect()
        {
            Connect(ConfigConstants.DOCUMENT_DB_ENDPOINT);
        }


        /// <summary>
        /// Connects data provider to its source addressed by supplied connection string
        /// </summary>
        /// <param name="str">Connection string</param>
        public void Connect(string str)
        {
            _client = new DocumentClient(new Uri(Config.GetValue(str)), ConfigConstants.DOCUMENT_DB_AUTH_KEY);
            var task = _client.CreateDatabaseIfNotExistsAsync(new Database() { Id = DBName });
            task.RunSynchronously();
            ThrowOnFailure(task.Result.StatusCode);
        }


        /// <summary>
        /// Removes a record matching the instance of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<int> Delete<T>(T obj)
        {
            var task = _client.DeleteDocumentAsync(UriForDocument(obj));
            task.RunSynchronously();
            ThrowOnFailure(task.Result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = task.Result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
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
            var task = _client.DeleteDocumentAsync(UriForDocumentKey<T>(key.ToString()));
            task.RunSynchronously();
            ThrowOnFailure(task.Result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = task.Result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();
      
            foreach (T obj in objList)
            {
                tasks.Add(_client.DeleteDocumentAsync(UriForDocument(obj)));
            }
            Task.WaitAll(tasks.ToArray());
            int successCount = 0;
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();

            foreach (K key in keyList)
            {
                tasks.Add(_client.DeleteDocumentAsync(UriForDocumentKey<T>(key.ToString())));
            }
            Task.WaitAll(tasks.ToArray());
            int successCount = 0;
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();
            foreach (T obj in objList)
            {
                tasks.Add(_client.DeleteDocumentAsync(UriForDocument(obj)));
            }
            await Task.WhenAll(tasks.ToArray());
            int successCount = 0;
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();
            foreach (K key in keyList)
            {
                tasks.Add(_client.DeleteDocumentAsync(UriForDocumentKey<T>(key.ToString())));
            }
            await Task.WhenAll(tasks.ToArray());
            int successCount = 0;
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            var result = await _client.DeleteDocumentAsync(UriForDocument(obj));
            ThrowOnFailure(result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
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
            var result = await _client.DeleteDocumentAsync(UriForDocumentKey<T>(key.ToString()));
            ThrowOnFailure(result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
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
            var task = _client.CreateDocumentAsync(UriForType<T>(), obj);
            task.RunSynchronously();
            ThrowOnFailure(task.Result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = task.Result.StatusCode == HttpStatusCode.Created;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();
            Uri uri = UriForType<T>();
            foreach(T obj in objList)
            {
                tasks.Add(_client.CreateDocumentAsync(uri,obj));
            }
            Task.WaitAll(tasks.ToArray());
            int successCount = 0;
            foreach(var task in tasks)
            {
                if(task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            IList<Task<ResourceResponse<Document>>> tasks = new List<Task<ResourceResponse<Document>>>();
            Uri uri = UriForType<T>();
            foreach (T obj in objList)
            {
                tasks.Add(_client.CreateDocumentAsync(uri, obj));
            }
            await Task.WhenAll(tasks.ToArray());
            int successCount = 0;
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == tasks.Count;
            status.StatusInfo = successCount;
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
            var result = await _client.CreateDocumentAsync(UriForType<T>(), obj);
            ThrowOnFailure(result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.StatusCode == HttpStatusCode.Created;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
            return status;

        }

        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
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
            var sqlParams = ConvertParameters(parameters);
            return _client.CreateDocumentQuery<T>(UriForType<T>(), new SqlQuerySpec()
            { QueryText = query, Parameters = sqlParams });
        }

        public IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
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

        public Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Retrieves all instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An iterator for traversing the returned records</returns>
        public IEnumerable<T> SelectAll<T>()
        {
            return _client.CreateDocumentQuery<T>(UriForType<T>());
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
            return (from record in SelectAll<T>() where matcher.Compile()(record) select record);
        }

        /// <summary>
        /// Retrieves all instances of T matching the specified expression asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            return Task.FromResult(SelectMatching(matcher));
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
            var task = _client.ReadDocumentAsync<T>(UriForDocumentKey<T>(key.ToString()));
            task.RunSynchronously();
            return task.Result;
        }


        /// <summary>
        /// Retrieves a single instance of T based on the specified key asynchronously
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Key corresponding to the retrieved record</param>
        /// <returns>A completion token encapsulating the record matching the key</returns>
        public Task<T> SelectOneAsync<T, K>(K key)
        {
            return Task.FromResult(SelectOne<T, K>(key));
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
            return (from record in SelectAll<T>() select record).Skip(from).Take(length);
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
            T oldRecord = SelectOne<T, object>(Mapper.GetKeyValue(obj));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            string keyName = Mapper.GetKeyName(obj.GetType());
            if(oldRecord != null)
            {
                Util.DeepCopy(obj, oldRecord, !updateNulls, keyName);
                var task = _client.ReplaceDocumentAsync(UriForDocument(oldRecord), oldRecord);
                task.RunSynchronously();
                status.IsSuccess = task.Result.StatusCode == HttpStatusCode.OK;
                status.StatusInfo = status.IsSuccess ? 1 : 0;
            }
            else
            {
                status.IsSuccess = false;
                status.StatusInfo = 0;
                status.StatusMessage = "Matching record not found!";
            }
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
 
            IList<string> keyList = new List<string>(objList.Count);

            foreach (T obj in objList)
            {
                keyList.Add(Mapper.GetKeyValue(obj).ToString());
            }

            IEnumerable<T> records = SelectMatching<T>(x => keyList.Contains(Mapper.GetKeyValue(x).ToString()));
            IDictionary<string, T> recordMap = new Dictionary<string, T>();
            foreach(var rec in records)
            {
                recordMap.Add(Mapper.GetKeyValue(rec).ToString(), rec);
            }
            string keyName = Mapper.GetKeyName(typeof(T));
          
            IList<Task<ResourceResponse<Document>>> updTasks = new List<Task<ResourceResponse<Document>>>();

            for (int i = 0; i < keyList.Count; i++)
            {
                Util.DeepCopy(objList[i], recordMap[keyList[i]], !updateNulls, keyName);
                updTasks.Add(_client.ReplaceDocumentAsync(UriForDocument(recordMap[keyList[i]]), recordMap[keyList[i]]));    
            }
            Task.WaitAll(updTasks.ToArray());

            int successCount = 0;
            foreach (var task in updTasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == updTasks.Count;
            status.StatusInfo = successCount;
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
            IList<string> keyList = new List<string>(objList.Count);

            foreach (T obj in objList)
            {
                keyList.Add(Mapper.GetKeyValue(obj).ToString());
            }

            IEnumerable<T> records = await SelectMatchingAsync<T>(x => keyList.Contains(Mapper.GetKeyValue(x).ToString()));
            IDictionary<string, T> recordMap = new Dictionary<string, T>();
            foreach (var rec in records)
            {
                recordMap.Add(Mapper.GetKeyValue(rec).ToString(), rec);
            }
            string keyName = Mapper.GetKeyName(typeof(T));

            int successCount = 0;
            IList<Task<ResourceResponse<Document>>> updTasks = new List<Task<ResourceResponse<Document>>>();
            for (int i = 0; i < keyList.Count; i++)
            {
                Util.DeepCopy(objList[i], recordMap[keyList[i]], !updateNulls, keyName);
                updTasks.Add(_client.ReplaceDocumentAsync(UriForDocument(recordMap[keyList[i]]), recordMap[keyList[i]]));
            }
            await Task.WhenAll(updTasks.ToArray());

            foreach (var task in updTasks)
            {
                if (task.Result.StatusCode == HttpStatusCode.Created)
                {
                    successCount++;
                }
            }
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = successCount == updTasks.Count;
            status.StatusInfo = successCount;
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
            T oldRecord = SelectOne<T, object>(Mapper.GetKeyValue(obj));
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            string keyName = Mapper.GetKeyName(obj.GetType());
            if (oldRecord != null)
            {
                Util.DeepCopy(obj, oldRecord, !updateNulls, keyName);
                var result = await _client.ReplaceDocumentAsync(UriForDocument(oldRecord), oldRecord);
                status.IsSuccess = result.StatusCode == HttpStatusCode.OK;
                status.StatusInfo = status.IsSuccess ? 1 : 0;
            }
            else
            {
                status.IsSuccess = false;
                status.StatusInfo = 0;
                status.StatusMessage = "Matching record not found!";
            }
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
        // ~DocumentDBDataContext() {
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
