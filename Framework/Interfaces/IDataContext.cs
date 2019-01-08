using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface specifying the requirements for a data provider instance
    /// </summary>
    public interface IDataContext : IDisposable
    {

        /// <summary>
        /// A reference to a data mapper component required to map query results to concrete objects
        /// </summary>
        IDataMapper Mapper { get; set; }


        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// A flag to indicate if changes made should be automatically committed to data source or not (if applicable)
        /// </summary>
        bool AutoCommit { get; set; }

        /// <summary>
        /// Connects data provider to its source
        /// </summary>
        void Connect();


        /// <summary>
        /// Connects data provider to its source addressed by supplied connection string
        /// </summary>
        /// <param name="str">Connection string</param>
        void Connect(string str);

        /// <summary>
        /// Commits data transaction to data source if applicable
        /// </summary>
        void Commit();

        /// <summary>
        /// Reverts uncommitted data transactions if applicable
        /// </summary>
        void RollBack();


        /// <summary>
        /// Adds an instance of T to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> Insert<T>(T obj);

        /// <summary>
        /// Adds an instance of T to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> InsertAsync<T>(T obj);

        /// <summary>
        /// Adds a collection of type T objects to the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> InsertAll<T>(IList<T> objList);


        /// <summary>
        /// Adds a collection of type T objects to the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">Collection of type T to be added</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> InsertAllAsync<T>(IList<T> objList);


        /// <summary>
        /// Removes a record matching the instance of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> Delete<T>(T obj);


        /// <summary>
        /// Removes a record of type T based on its key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="key">Key value for record to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> Delete<T,K>(K key);


        /// <summary>
        /// Removes a record matching the instance of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> DeleteAsync<T>(T obj);

        /// <summary>
        /// Removes a record of type T based on its key asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="key">Key value for record to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> DeleteAsync<T,K>(K key);


        /// <summary>
        /// Removes multiple records matching each instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> DeleteAll<T>(IList<T> objList);

        /// <summary>
        /// Removes multiple records of type T based on their keys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> DeleteAll<T,K>(IList<K> keyList);

        /// <summary>
        /// Removes multiple records matching each instances of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">A collection of T instances matching records to be removed</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> DeleteAllAsync<T>(IList<T> obj);



        /// <summary>
        /// Removes multiple records of type T based on their keys asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="keyList">List of matching keys</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> DeleteAllAsync<T,K>(IList<K> key);

        /// <summary>
        /// Updates a record in the data source matching the instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T matching record to be updated</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> Update<T>(T obj, bool updateNulls = false);

        /// <summary>
        /// Updates a record in the data source matching the instance of T asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Instance of T matching record to be updated</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> UpdateAsync<T>(T obj, bool updateNulls = false);

        /// <summary>
        /// Updates multiple records the data source matching the instances of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">List of matching records</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<int> UpdateAll<T>(IList<T> objList, bool updateNulls = false);


        /// <summary>
        /// Updates multiple records the data source matching the instances of T asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList">List of matching records</param>
        /// <param name="updateNulls">A flag indicating if null updates count</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus<int>> UpdateAllAsync<T>(IList<T> objList, bool updateNulls = false);

        /// <summary>
        /// Retrieves all instances of T from the data source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An iterator for traversing the returned records</returns>
        IEnumerable<T> SelectAll<T>();

        /// <summary>
        /// Retrieves all instances of T from the data source asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A completion token encapsulating the iterator for traversing the returned records</returns>
        Task<IEnumerable<T>> SelectAllAsync<T>();

        /// <summary>
        /// Retrieves all instances of T matching the specified expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>Matching instances of T</returns>
        IEnumerable<T> SelectMatching<T>(Expression<T> matcher);

        /// <summary>
        /// Retrieves all instances of T matching the specified expression asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matcher">Match predicate encoded in an expression</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        Task<IEnumerable<T>> SelectMatchingAsync<T>(Expression<T> matcher);

        /// <summary>
        /// Retrieves a single instance of T based on the specified key
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Key corresponding to the retrieved record</param>
        /// <returns>The record matching the key</returns>
        T SelectOne<T, K>(K key);

        /// <summary>
        /// Retrieves a single instance of T based on the specified key asynchronously
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <typeparam name="K">Key type</typeparam>
        /// <param name="key">Key corresponding to the retrieved record</param>
        /// <returns>A completion token encapsulating the record matching the key</returns>
        Task<T> SelectOneAsync<T, K>(K key);

        /// <summary>
        /// Returns all instances of T within a specified page
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <param name="from">Page offset</param>
        /// <param name="length">Page size</param>
        /// <returns>Instances of T within the data page</returns>
        IEnumerable<T> SelectRange<T>(int from, int length);


        /// <summary>
        /// Returns all instances of T within a specified page asynchronously
        /// </summary>
        /// <typeparam name="T">Record type</typeparam>
        /// <param name="from">Page offset</param>
        /// <param name="length">Page size</param>
        /// <returns>A completion token encapsulating the instances of T within the data page</returns>
        Task<IEnumerable<T>> SelectRangeAsync<T>(int from, int length);

        /// <summary>
        /// Run a data retrieval statement directly against data source for record type T
        /// </summary>
        /// <typeparam name="T">Record type T</typeparam>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>Matching instances of T</returns>
        IEnumerable<T> Query<T>(string query, IDictionary<string, object> parameters);

        /// <summary>
        /// Run a data retrieval statement directly against data source for record type T asynchronously
        /// </summary>
        /// <typeparam name="T">Record type T</typeparam>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating the matching instances of T</returns>
        Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, object> parameters);

        /// <summary>
        /// Runs a data retrieval statement directly against the data source for one or more arbitrary record types
        /// </summary>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>Query results with each record's fields encoded as a series of key-value pairs</returns>
        IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters);

        /// <summary>
        /// Runs a data retrieval statement directly against the data source for one or more arbitrary record types asynchronously
        /// </summary>
        /// <param name="query">Data retrieval statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating the query results with each record's fields encoded as a series of key-value pairs</returns>
        Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IDictionary<string, object> parameters);

        /// <summary>
        /// Executes a data modification statement against the data source
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A status indicating the result of the operation</returns>
        IStatus<int> NonQuery(string statement, IDictionary<string, object> parameters);


        /// <summary>
        /// Executes a data modification statement against the data source asynchronously
        /// </summary>
        /// <param name="statement">Data modification statement</param>
        /// <param name="parameters">Statement parameter mapping</param>
        /// <returns>A completion token encapsulating status indicating the result of the operation</returns>
        Task<IStatus<int>> NonQueryAsync(string statement, IDictionary<string, object> parameters);


    }
}
