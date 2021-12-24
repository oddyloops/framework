using Framework.Interfaces;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.CsvArchiveDB
{
    /// <summary>
    /// A datacontext implementation for archiving data in csv format
    /// using the Azure blob storage (Note: This doesnt work for nested objects)
    /// TODO: Implement azure blob storage
    /// </summary>
    [Export("CsvArchive",typeof(IDataContext))]
    public class CsvAzureBlobDataContext : IDataContext
    {
        private static IDictionary<Type, int> typeCode
            = new Dictionary<Type, int> { 
                { typeof(byte), 0 },
                { typeof(short), 1 },
                { typeof(int), 2 },
                { typeof(long), 3 },
                { typeof(float), 4 },
                { typeof(double), 5 },
                { typeof(string), 6 },
                { typeof(bool), 7 },
                { typeof(Guid), 8 },
                { typeof(DateTime), 9 }
            };

        private static string nullCode = "<null>";

        [Import]
        public IDataMapper Mapper { get; set; }

        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }
        public bool AutoCommit { get; set; }
        public string DBName { get; set; }

        private string TypeFileName<T>()
        {
            return $"{typeof(T).FullName.ToLower().Replace(".", "_")}.csv";
        }
        
        private string GetFilePath<T>()
        {
            return $"{DBName}/{TypeFileName<T>()}";
        }


        private static int GetTypeCode(object obj)
        {
            if(obj == null)
            {
                return typeCode[typeof(string)]; //string being the only nullable type
            }
            if (!typeCode.TryGetValue(obj.GetType(), out int t))
            {
                throw new Exception($"Unsupported type: {obj.GetType().Name} in csv archiver");
            }
            return t;
            
        }

        private string ObjectToCsvRow(object obj)
        {
            string keyField = Mapper.GetKeyMapName(obj.GetType());
            StringBuilder csvRow = new StringBuilder();
            string keyValue = Mapper.GetKeyValue(obj).ToString();
            csvRow.Append($"\"{keyValue}\"");
            var fields = Mapper.GetFieldNames(obj.GetType()).OrderBy(x => x);

            foreach(string field in fields)
            {
                if (field != keyField)
                {
                    object fieldValue = Mapper.GetField(field, obj);
                    int typeCode = GetTypeCode(fieldValue);
                    string str;
                    if(fieldValue == null)
                    {
                        str = nullCode;
                    }
                    else
                    {
                        str = fieldValue.ToString();
                    }
                    csvRow.Append($",\"{str}\"");
                }
            }
            return csvRow.ToString();
        }

        private T CsvRowToObject<T>(string csv)
        {
            string keyField = Mapper.GetKeyMapName(typeof(T));
            var fields = Mapper.GetFieldNames(typeof(T)).OrderBy(x => x);
            IDictionary<string, object> fieldValues = new Dictionary<string, object>();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Connect(string str)
        {
            throw new NotImplementedException();
        }

        public int Count<T>()
        {
            string filePath = GetFilePath<T>();
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).Length;
            }
            return 0;
        }

        public Task<int> CountAsync<T>()
        {
            return Task.FromResult(Count<T>());
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

        public Task<IStatus<int>> DeleteAsync<T>(T obj)
        {
            throw new NotImplementedException();
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
            string csv = ObjectToCsvRow(obj);
            File.AppendAllLines(GetFilePath<T>(),new string[] { csv });
            IStatus<int> success = Util.Container.CreateInstance<IStatus<int>>();
            success.IsSuccess = true;
            success.StatusInfo = 1;
            return success;
        }

        public IStatus<int> InsertAll<T>(IList<T> objList)
        {
            string[] csvRows = new string[objList.Count];
            for(int i = 0; i < objList.Count; i++)
            {
                csvRows[i] = ObjectToCsvRow(objList[i]);
            }
            File.AppendAllLines(GetFilePath<T>(), csvRows);
            IStatus<int> success = Util.Container.CreateInstance<IStatus<int>>();
            success.IsSuccess = true;
            success.StatusInfo = objList.Count;
            return success;
        }

        public Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList)
        {
            return Task.FromResult(InsertAll(objList));
        }

        public Task<IStatus<int>> InsertAsync<T>(T obj)
        {
            return Task.FromResult(Insert(obj));
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
            string[] lines = File.ReadAllLines(GetFilePath<T>());
            
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
    }
}
