using System;
using Framework.Interfaces;

namespace Framework.Common.Services
{
    /// <summary>
    /// Contracts that all key stores must satisfy
    /// </summary>
    public interface IKeyStoreService : IService, IDisposable
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the key store service
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Creates an asymmetric key in the operating system's
        /// keys store if none exists
        /// </summary>
        void CreateAsymKeyIfNotExists();

        /// <summary>
        /// Adds or replaces symmetric key to the store
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        /// <param name="key">key to be stored</param>
        void AddOrUpdateKey(string index, byte[] key);

        /// <summary>
        /// Retrieves a symmetric key from store by index
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        /// <returns>Key to be retrieved</returns>
        byte[] GetKey(string index);

        /// <summary>
        /// Removes key from store by the specified index
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        void DeleteKey(string index);

        /// <summary>
        /// Flushes the entire key store along with all resources it uses
        /// </summary>
        void Clear();

    }
}
