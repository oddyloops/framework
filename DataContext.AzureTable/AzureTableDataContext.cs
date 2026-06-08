using Azure;
using Azure.Data.Tables;
using Framework.Interfaces;
using Framework.Interfaces.Configs;
using Framework.Utils;
using System.Linq.Expressions;
using System.Net;

namespace DataContext.AzureTable
{
    public class AzureTableDataContext : IClientDataContext
    {
        private readonly TableServiceClient client;
        private readonly IDataMapper _mapper;

        public object Client => client;

        public AzureTableDataContext(IDataConnection connection, IDataMapper dataMapper) 
        { 
            client = new TableServiceClient(connection.ConnectionString);
            _mapper = dataMapper;
        }

        /// <summary>
        /// A helper method for getting the partition/row key of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="obj">Entity object</param>
        /// <param name="isPartition">True for partition key and false for row key</param>
        /// <returns>Partition key or row key</returns>
        private static string? GetPartitionOrRowKey<T>(T obj,bool isPartition=true) 
        {
            ITableEntity? entity= obj as ITableEntity;
            if (entity != null)
            {
                return isPartition ? entity.PartitionKey : entity.RowKey;
            }
            return null;
        }

        /// <summary>
        /// Builds a status object based on the table operation response
        /// </summary>
        /// <param name="response">Table operation response</param>
        /// <returns>Corresponding status object</returns>
        private static IStatus<int> Status(Response response)
        {
            IStatus<int> status = Util.Container.CreateInstance<IStatus<int>>();
            status.IsSuccess = response.Status == (int)HttpStatusCode.OK;
            return status;
        }

        /// <summary>
        /// Serializes an object into a key value pair of field and values
        /// </summary>
        /// <param name="record">Object to be serialized</param>
        /// <param name="addNulls">A flag indicating if null fields should be serialized</param>
        /// <returns>A key value pair of object's field and values</returns>
        private List<KeyValuePair<string, object>> TransformObjectToParams(object record, bool addNulls = true)
        {

            List<KeyValuePair<string, object>> paramRec = new List<KeyValuePair<string, object>>(); //need to maintain order for traversals
            IList<string> fieldNames = _mapper.GetFieldNames(record.GetType());
            foreach (var field in fieldNames)
            {
                if (addNulls || _mapper.GetField(field, record) != null)
                {
                    paramRec.Add(new KeyValuePair<string, object>(field, _mapper.GetField(field, record)));
                }
            }
            return paramRec;
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public int Count<T>()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync<T>()
        {
            throw new NotImplementedException();
        }

        public IStatus<int> Delete<T>(T obj)
        {
            throw new NotImplementedException();
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

        public async Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            TableClient table = client.GetTableClient(nameof(T));
            string? partitionKey = GetPartitionOrRowKey(obj);
            string? rowKey = GetPartitionOrRowKey(obj, false);
            var response = await table.DeleteEntityAsync(partitionKey, rowKey);
            return Status(response);
        }



        public Task<IStatus<int>> DeleteAsync<T, K>(K key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
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

        public async Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            TableClient table = client.GetTableClient(nameof(T));
            var response = await table.AddEntityAsync((ITableEntity)obj);
            return Status(response);
        }

        public IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> PatchAddAsync<T, K, C>(K key, string patchPath, IList<C> childItems)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<int>> PatchRemoveAsync<T, K, C>(K key, string patchPath, Expression<Func<C, bool>> condition)
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

        public async Task<IStatus<int>> UpdateAsync<T>(T obj, bool updateNulls = false)
        {
            TableClient table = client.GetTableClient(nameof(T));
            string? partitionKey = GetPartitionOrRowKey(obj);
            string? rowKey = GetPartitionOrRowKey(obj, false);
            var changeSet = TransformObjectToParams(obj, updateNulls);
            TableEntity entity = new TableEntity(partitionKey, rowKey);
            foreach(var change in changeSet)
            {
                entity.Add(change.Key, change.Value);
            }
            var response = await table.UpdateEntityAsync(entity, ifMatch: ETag.All, mode: TableUpdateMode.Merge);
            return Status(response);
        }
    }
}