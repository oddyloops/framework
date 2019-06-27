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
    [Export("NoSql.DocumentDB",typeof(IDataContext))]
    public class DocumentDBDataContext : IDataContext
    {

        DocumentClient _client;

        [Import]
        public IDataMapper Mapper { get; set; }

        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        public bool AutoCommit { get; set; }

        public string DBName { get; set; }

        private IList<HttpStatusCode> _validCodes = new List<HttpStatusCode>()
        {
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent
        };

        #region Helpers

        private void ThrowOnFailure(HttpStatusCode response)
        {
            if(!_validCodes.Contains(response))
            {
                throw new Exception($"An error occured with request to Document DB with HTTP status code: {(int)response}");
            }
        }

       
        private Uri UriForType<T>()
        {
            return UriFactory.CreateDocumentCollectionUri(DBName, Mapper.GetObjectMapName(typeof(T)));
        }


        private Uri UriForDocument(object document)
        {
            return UriFactory.CreateDocumentUri(DBName, Mapper.GetObjectMapName(document.GetType()), Mapper.GetKeyValue(document).ToString());
        }


        private Uri UriForDocumentKey<T>(string key)
        {
            return UriFactory.CreateDocumentUri(DBName, Mapper.GetObjectMapName(typeof(T)), key);
        }

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


        public DocumentDBDataContext()
        {
            DBName = Config.GetValue(ConfigConstants.DOCUMENT_DB_DATABASE);
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            Connect(ConfigConstants.DOCUMENT_DB_ENDPOINT);
        }

        public void Connect(string str)
        {
            _client = new DocumentClient(new Uri(Config.GetValue(str)), ConfigConstants.DOCUMENT_DB_AUTH_KEY);
            var task = _client.CreateDatabaseIfNotExistsAsync(new Database() { Id = DBName });
            task.RunSynchronously();
            ThrowOnFailure(task.Result.StatusCode);
        }

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

        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            var result = await _client.DeleteDocumentAsync(UriForDocument(obj));
            ThrowOnFailure(result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
            return status;
        }

        public async Task<IStatus<int>> DeleteAsync<T, K>(K key)
        {
            var result = await _client.DeleteDocumentAsync(UriForDocumentKey<T>(key.ToString()));
            ThrowOnFailure(result.StatusCode);
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = result.StatusCode == HttpStatusCode.OK;
            status.StatusInfo = status.IsSuccess ? 1 : 0;
            return status;
        }

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

        public IEnumerable<T> SelectAll<T>()
        {
            return _client.CreateDocumentQuery<T>(UriForType<T>());
        }

        public Task<IEnumerable<T>> SelectAllAsync<T>()
        {
            return Task.FromResult(SelectAll<T>());
        }

        public IEnumerable<T> SelectMatching<T>(Expression<Func<T, bool>> matcher)
        {
            return (from record in SelectAll<T>() where matcher.Compile()(record) select record);
        }

        public Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<Func<T, bool>> matcher)
        {
            return Task.FromResult(SelectMatching(matcher));
        }

        public T SelectOne<T, K>(K key)
        {
            var task = _client.ReadDocumentAsync<T>(UriForDocumentKey<T>(key.ToString()));
            task.RunSynchronously();
            return task.Result;
        }

        public Task<T> SelectOneAsync<T, K>(K key)
        {
            return Task.FromResult(SelectOne<T, K>(key));
        }

        public IEnumerable<T> SelectRange<T>(int from, int length)
        {
            return (from record in SelectAll<T>() select record).Skip(from).Take(length);
        }

        public Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length)
        {
            return Task.FromResult(SelectRange<T>(from, length));
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
        #endregion
    }
}
