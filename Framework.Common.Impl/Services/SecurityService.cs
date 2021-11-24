using Framework.Common.Services;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A concrete implementation of the ISecurityService interface
    /// </summary>
    [Export(typeof(ISecurityService))]
    public class SecurityService : ISecurityService
    {

        private const int DEFAULT_SALT_LEN = 20;

        /// <summary>
        /// A reference to the configuration component for accessing setting values
        /// </summary>
        [Import("Env")]
        public IConfiguration Config { get; set; }

        [Import]
        public IKeyStoreService KeyStore { get; set; }


        #region HelperMethods
        /// <summary>
        /// Helper method for creating an AES encryption object
        /// </summary>
        /// <param name="key">Encryption key</param>
        /// <param name="blockSize">Encryption block size in bytes</param>
        /// <returns>A concree native windows implementation of the AES encryption algorithm</returns>
        private static Aes CreateAes(byte[] key, int blockSize)
        {
            Aes aes = new AesCryptoServiceProvider();

            aes.KeySize = key.Length * 8;
            aes.BlockSize = blockSize * 8;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();


            return aes;
        }

        #endregion

        /// <summary>
        /// Decrypts an AES encrypted message
        /// </summary>
        /// <param name="encryptedMessage">Encrypted byte buffer</param>
        /// <param name="key">Key used for decryption</param>
        /// <returns>Decrypted message</returns>
        public byte[] Decrypt(byte[] encryptedMessage, byte[] key)
        {
            int blockSize = int.Parse(Config.GetValue(ConfigConstants.ENCRYPTION_BLOCK_SIZE_BYTES));
            Aes aes = CreateAes(key, blockSize);
            byte[] decrypted = null;
            //read iv out of cipher buffer
            byte[] iv = new byte[encryptedMessage[0]]; //1st byte for iv length
            for (int i = 1; i <= iv.Length; i++)
            {
                iv[i - 1] = encryptedMessage[i];
            }
            aes.IV = iv;
            using (ICryptoTransform crypto = aes.CreateDecryptor())
            {
                //decrypt message
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, iv.Length + 1, encryptedMessage.Length - iv.Length - 1);
                    }
                    decrypted = ms.ToArray();
                }

            }
            aes.Clear();
            return decrypted;

        }

        /// <summary>
        /// Decrypts an AES encrypted credential into a username-password pair with scheme used for encryption
        /// </summary>
        /// <param name="encryptedCredentials">Encrypted credential byte buffer</param>
        /// <param name="key">Key used for decryption</param>
        /// <returns>The decrypted username-password pair</returns>
        public Tuple<string, string> DecryptCredentials(byte[] encryptedCredentials, byte[] key)
        {
            byte[] decryptedCredentials = Decrypt(encryptedCredentials, key);
            int userLength = decryptedCredentials[0];
            int pwdLength = decryptedCredentials[userLength + 1];

            byte[] userBuffer = new byte[userLength];
            byte[] pwdBuffer = new byte[pwdLength];
            Array.Copy(decryptedCredentials, 1, userBuffer, 0, userLength);
            Array.Copy(decryptedCredentials, userLength + 2, pwdBuffer, 0, pwdLength);

            return new Tuple<string, string>(Encoding.UTF8.GetString(userBuffer),
                Encoding.UTF8.GetString(pwdBuffer));
        }


        /// <summary>
        /// Encrypts a message using AES
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="key">Key used for encryption</param>
        /// <returns>Encrypted message</returns>
        public byte[] Encrypt(byte[] message, byte[] key)
        {
            int blockSize = int.Parse(Config.GetValue(ConfigConstants.ENCRYPTION_BLOCK_SIZE_BYTES));
            Aes aes = CreateAes(key, blockSize);
            byte padLength = (byte)(message.Length < blockSize ? blockSize - message.Length : message.Length % blockSize);
            byte[] encrypted = null;
            using (ICryptoTransform crypto = aes.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    //encrypt
                    using (var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write))
                    {
                        //add iv length and iv to buffer unencrypted
                        ms.Write(new byte[] { (byte)aes.IV.Length }, 0, 1);
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        cs.Write(message, 0, message.Length);
                    }
                    encrypted = ms.ToArray();
                }

            }
            aes.Clear();
            return encrypted;
        }


        /// <summary>
        /// Encrypts a username-password pair using AES with a scheme for combining them
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="key">Key used for encryption</param>
        /// <returns>An encrypted credential byte buffer</returns>
        public byte[] EncryptCredentials(string username, string password, byte[] key)
        {
            if (username.Length > byte.MaxValue)
            {
                throw new Exception("Username exceeds 255 characters");
            }

            if (password.Length > byte.MaxValue)
            {
                throw new Exception("Password exceeds 255 characters");
            }
            byte userLength = (byte)username.Length;
            byte pwdLength = (byte)password.Length;
            byte[] credentialsBuffer = new byte[2 + userLength + pwdLength]; //+2 to store their lengths
            credentialsBuffer[0] = userLength;
            Array.Copy(Encoding.UTF8.GetBytes(username), 0, credentialsBuffer, 1, userLength);
            credentialsBuffer[userLength + 1] = pwdLength;
            Array.Copy(Encoding.UTF8.GetBytes(password), 0, credentialsBuffer, userLength + 2, pwdLength);
            //forms structure ULength : username : PLength : Password
            return Encrypt(credentialsBuffer, key);
        }

        /// <summary>
        /// Compute Message Hash using SHA256
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <returns>Computed message hash</returns>
        public byte[] Hash(byte[] message)
        {
            SHA256 sha = new SHA256Managed();
            return sha.ComputeHash(message);
        }

        /// <summary>
        /// Compute Message Hash
        /// </summary>
        /// <param name="message">Message String</param>
        /// <returns>Computed message hash</returns>
        public byte[] Hash(string message)
        {
            return Hash(Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Adds a salt to message 
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="salt">Generated salt</param>
        /// <returns>Salted message</returns>
        public byte[] Salt(byte[] message, out byte[] salt)
        {
            return Salt(message, out salt, DEFAULT_SALT_LEN);
        }


        /// <summary>
        /// Adds a salt to message 
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="salt">Generated salt</param>
        /// <param name="saltLength">Length of salt</param>
        /// <returns>Salted message</returns>
        public byte[] Salt(byte[] message, out byte[] salt, int saltLength)
        {
            salt = new byte[saltLength];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            byte[] saltedMessage = new byte[message.Length + salt.Length];
            Array.Copy(message, saltedMessage, message.Length);
            Array.Copy(salt, 0, saltedMessage, message.Length, salt.Length);
            return saltedMessage;
        }

        /// <summary>
        /// Creates a digital signature for the input data
        /// using a SHA 256 hashing implementation
        /// </summary>
        /// <param name="input">Input data</param>
        /// <returns>Digital signature</returns>
        public byte[] CreateDigitalSignature(byte[] input)
        {
            KeyStore.CreateAsymKeyIfNotExists();
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters() { KeyContainerName = assymKeyContainer };
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
            {
                return rsa.SignData(input, SHA256.Create());
            }
        }


        /// <summary>
        /// Checks the authenticity and integrity of input data
        /// using SHA 256 for hashing
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="signature">Digital signature</param>
        /// <returns>Is data valid?</returns>
        public bool VerifyDigitalSignature(byte[] input, byte[] signature)
        {
            string assymKeyContainer = Config.GetValue(ConfigConstants.ASYM_KEY_PATH);
            CspParameters csp = new CspParameters() { KeyContainerName = assymKeyContainer };
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp))
            {
                return rsa.VerifyData(input,SHA256.Create(),signature);
            }
        }

        public bool VerifyDigitalSignature(byte[] input, byte[] signature, byte[] publicKey)
        {
            int asymKeySize = int.Parse(Config.GetValue(ConfigConstants.ASYM_KEY_SIZE_BITS));
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(asymKeySize))
            {
                rsa.ImportCspBlob(publicKey);
                return rsa.VerifyData(input, SHA256.Create(), signature);
            }
        }
    }
}
