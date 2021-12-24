using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface for caching
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Adds/Updates an entry in the cache
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        Task SetAsync<T>(string key, T value);

        /// <summary>
        /// Adds/Updates an entry in the cache that expires after not being 
        /// accessed within the sliding expiration duration
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="key">Entry key</param>
        /// <param name="value">Entry value</param>
        /// <param name="slidingExpiration">Valid duration without being accessed</param>
        Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration);

        /// <summary>
        /// Gets an entry from the cache
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Entry key</param>
        /// <returns>Entry value or null if not found</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Resets the sliding expiration for entry
        /// </summary>
        /// <param name="key">Entry key</param>
        Task RefreshAsync(string key);

        /// <summary>
        /// Deletes an entry from the cache
        /// </summary>
        /// <param name="key">Entry key</param>
        Task RemoveAsync(string key);


    }
}
