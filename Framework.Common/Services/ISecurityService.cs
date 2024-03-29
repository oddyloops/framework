﻿
using Framework.Interfaces;
using System;

namespace Framework.Common.Services
{
    /// <summary>
    /// Contracts that all security services must satisfy
    /// </summary>
    public interface ISecurityService : IService
    {

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the security service
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Compute Message Hash
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <returns>Computed message hash</returns>
        byte[] Hash(byte[] message);

        /// <summary>
        /// Compute Message Hash
        /// </summary>
        /// <param name="message">Message String</param>
        /// <returns>Computed message hash</returns>
        byte[] Hash(string message);


        /// <summary>
        /// Adds a salt to message 
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="salt">Generated salt</param>
        /// <returns>Salted message</returns>
        byte[] Salt(byte[] message,out byte[] salt);

        /// <summary>
        /// Adds a salt to message 
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="salt">Generated salt</param>
        /// <param name="saltLength">Length of salt</param>
        /// <returns>Salted message</returns>
        byte[] Salt(byte[] message, out byte[] salt, int saltLength);


        /// <summary>
        /// Encrypts a message 
        /// </summary>
        /// <param name="message">Message byte buffer</param>
        /// <param name="key">Key used for encryption</param>
        /// <returns>Encrypted message</returns>
        byte[] Encrypt(byte[] message, byte[] keyl);

        /// <summary>
        /// Decrypts an encrypted message
        /// </summary>
        /// <param name="encryptedMessage">Encrypted byte buffer</param>
        /// <param name="key">Key used for decryption</param>
        /// <returns>Decrypted message</returns>
        byte[] Decrypt(byte[] encryptedMessage, byte[] key);

        /// <summary>
        /// Encrypts a username-password pair with a scheme for combining them
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="key">Key used for encryption</param>

        /// <returns>An encrypted credential byte buffer</returns>
        byte[] EncryptCredentials(string username, string password, byte[] key);

        /// <summary>
        /// Decrypts an encrypted credential into a username-password pair with scheme used for encryption
        /// </summary>
        /// <param name="encryptedCredentials">Encrypted credential byte buffer</param>
        /// <param name="key">Key used for decryption</param>
      
        /// <returns>The decrypted username-password pair</returns>
        Tuple<string, string> DecryptCredentials(byte[] encryptedCredentials, byte[] key);

        /// <summary>
        /// Creates a digital signature for the input data
        /// </summary>
        /// <param name="input">Input data</param>
        /// <returns>Digital signature</returns>
        byte[] CreateDigitalSignature(byte[] input);

        /// <summary>
        /// Verifies the data's integrity with the supplied signature
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="signature">Digital signature</param>
        /// <returns>Is data valid?</returns>
        bool VerifyDigitalSignature(byte[] input, byte[] signature);

        /// <summary>
        /// Verifies the data's integrity with the supplied signature
        /// and public key
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="signature">Digital signature</param>
        /// <param name="publicKey">Public key used for verification</param>
        /// <returns>Is data valid?</returns>
        bool VerifyDigitalSignature(byte[] input, byte[] signature, byte[] publicKey);
    }
}
