using Framework.Common.Services;
using Framework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using Framework.Common.Impl;

namespace Framrwork.Common.Impl.Services
{
    /// <summary>
    /// A data structure for a single entry in the key store
    /// </summary>
    class KeyData : IEquatable<KeyData>
    {
        /// <summary>
        /// Entry name
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Entry value which is a symmetric key encoded as a base 64 string
        /// </summary>
        public string Key { get; set; }

        public override bool Equals(object obj)
        {
            if(obj != null && obj is KeyData)
            {
                return Index.Equals(((KeyData)obj).Index);
            }
            return false;
        }

        public bool Equals(KeyData other)
        {
            if (other != null)
            {
                return Index.Equals(other.Index);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }

    /// <summary>
    /// A concrete implementation of the IKeyStoreService interface that uses the native windows RSA assymetric
    /// encryption for securing keys
    /// </summary>
    [Export(typeof(IKeyStoreService))]
    public class RTKeyStoreService : IKeyStoreService
    {
        /// <summary>
        /// A reference to the configuration handle for accessing setting values
        /// </summary>
        [Import("Env")]
        public IConfiguration Config { get; set; }


        private IList<KeyData> keyCache = new List<KeyData>();

        #region HelperMethods
        /// <summary>
        /// A helper method for updating in-memory key cache from key store file on disk
        /// </summary>
        private void UpdateCache()
        {
            string storageFile = Config.GetValue(ConfigConstants.KEY_STORE_PATH);
            if (!File.Exists(storageFile))
            {
                using (File.Create(storageFile))
                {
                    return;
                }
            }
            string json = File.ReadAllText(storageFile);
            if(string.IsNullOrEmpty(json))
            {
                return;
            }
            JArray keys = JArray.Parse(json);
            keyCache.Clear();
            foreach (var key in keys)
            {
                keyCache.Add(new KeyData() { Index = key["Index"].ToString(), Key = key["Key"].ToString() });
            }
        }

        /// <summary>
        /// A helper method for committing changes in in-memory key cache to disk
        /// </summary>
        private void FlushCacheToDisk()
        {
            string storageFile = Config.GetValue(ConfigConstants.KEY_STORE_PATH);
            string json = JsonConvert.SerializeObject(keyCache, Formatting.Indented);
            File.WriteAllText(storageFile, json);
        }

        

        /// <summary>
        /// A helper method for encrypting symmetric key to be stored with RSA encryption
        /// </summary>
        /// <param name="symmKey">Symmetric key to be encrypted</param>
        /// <returns>RSA Encrypted symmetric key</returns>
        private byte[] EncryptSymmKey(byte[] symmKey)
        {
            CreateAsymKeyIfNotExists();
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters() { KeyContainerName = assymKeyContainer };
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
            {
                return rsa.Encrypt(symmKey, true);
            }

        }

        /// <summary>
        /// A helper method for decrypting retrieved RSA-Encrypted symmetric key
        /// </summary>
        /// <param name="encSymmKey">RSA-Encrypted symmetric key</param>
        /// <returns>Decrypted symmetric key</returns>
        private byte[] DecryptSymmKey(byte[] encSymmKey)
        {
            CreateAsymKeyIfNotExists();
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters() { KeyContainerName = assymKeyContainer };
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
            {
                return rsa.Decrypt(encSymmKey, true);
            }
        }

        /// <summary>
        /// A helper method for removing RSA key container from windows
        /// </summary>
        private void DeleteAsymmetricKey()
        {
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters() { KeyContainerName = assymKeyContainer };
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
            {
                rsa.Clear();
            }
        }
        #endregion


        /// <summary>
        /// Generates RSA asymmetric key in the configured key container if none exists
        /// </summary>
        public void CreateAsymKeyIfNotExists()
        {
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters()
            {
                KeyContainerName = assymKeyContainer,
                Flags = CspProviderFlags.UseMachineKeyStore
            };
            CspKeyContainerInfo cspKeyContainer = new CspKeyContainerInfo(csp);

            if (!cspKeyContainer.Accessible)
            {
                int keySize = int.Parse(Config.GetValue(ConfigConstants.ASYM_KEY_SIZE_BITS));
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize, csp))
                {
                    rsa.PersistKeyInCsp = true;
                }
            }
        }

        /// <summary>
        /// Adds or replaces symmetric key to the store
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        /// <param name="key">key to be stored</param>s
        public void AddOrUpdateKey(string index, byte[] key)
        {
            if (keyCache.Count == 0)
            {
                UpdateCache();
            }

            //encrypt key
            string encKey = Convert.ToBase64String(EncryptSymmKey(key));
            int existingIndex = keyCache.IndexOf(new KeyData() { Index = index });
            if (existingIndex >= 0)
            {
                //exists
                keyCache[existingIndex].Key = encKey;
            }
            else
            {
                keyCache.Add(new KeyData() { Index = index, Key = encKey });
            }
            FlushCacheToDisk();
        }

        /// <summary>
        /// Retrieves a symmetric key from store by index
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        /// <returns>Key to be retrieved</returns>
        public void DeleteKey(string index)
        {
            if (keyCache.Count == 0)
            {
                UpdateCache();
            }

            if (keyCache.Remove(new KeyData() { Index = index }))
            {
                FlushCacheToDisk();
            }
        }

        /// <summary>
        /// Removes key from store by the specified index
        /// </summary>
        /// <param name="index">Index for identifying the key</param>
        public byte[] GetKey(string index)
        {
            if(keyCache.Count == 0)
            {
                UpdateCache();
            }

            var result =  from k in keyCache where k.Index == index select k.Key;

            if(result != null && result.Count() > 0)
            {
                return DecryptSymmKey( Convert.FromBase64String( result.First()));
            }
            return null;
        }


        /// <summary>
        /// Flushes the entire key store along with all resources it uses
        /// </summary>
        public void Clear()
        {
            keyCache.Clear();
            string storageFile = Config.GetValue(ConfigConstants.KEY_STORE_PATH);
            if (File.Exists(storageFile))
            {
                File.Delete(storageFile);
            }
            DeleteAsymmetricKey();
        }


        /// <summary>
        /// Commits and clears in-memory cache and disposes of the key store object
        /// </summary>
        public void Dispose()
        {
            FlushCacheToDisk();
            keyCache.Clear();
        }

    }

}
