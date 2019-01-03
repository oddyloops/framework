using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements for a consumer implementation within a producer-consumer context
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// A reference to the buffer shared between this consumer and one or more producers
        /// </summary>
        IProducerConsumerBuffer Buffer { get; }

        /// <summary>
        /// Waits for a string from a producer and returns it
        /// </summary>
        /// <returns>String message</returns>
        string ReadString();

        /// <summary>
        /// A non-blocking attempt to read string message from a producer
        /// </summary>
        /// <param name="result">string message</param>
        /// <returns>A flag indicating a successful read</returns>
        bool TryReadString(out string result);

        /// <summary>
        /// Waits for a binary message from a producer and returns it
        /// </summary>
        /// <returns>Binary message</returns>
        byte[] Read();

        /// <summary>
        /// A non-blocking attempt to read binary message from a producer
        /// </summary>
        /// <param name="result">Binary message</param>
        /// <returns>A flag indicating a successful read</returns>
        bool TryRead(out byte[] result);

        /// <summary>
        /// Waits for an object message from a producer
        /// </summary>
        /// <returns>Object encapsulating message</returns>
        object ReadObject();

        /// <summary>
        /// A non-blocking attempt to read an object message from a producer
        /// </summary>
        /// <param name="obj">Object message</param>
        /// <returns>A flag indicating a successful read</returns>
        bool TryReadObject(out object obj);


        /// <summary>
        /// Waits for a string from a producer and returns it asynchronously
        /// </summary>
        /// <returns>A completion token encapsulating the string message</returns>
        Task<string> ReadStringAsync();


        /// <summary>
        /// A non-blocking attempt to read string message from a producer asynchronously
        /// </summary>
        /// <param name="result">string message</param>
        /// <returns>A completion token encapsulating the flag indicating a successful read</returns>
        Task<bool> TryReadStringAsync(out string result);


        /// <summary>
        /// Waits for a binary message from a producer and returns it asynchronously
        /// </summary>
        /// <returns>A completion token encapsulating the binary message</returns>
        Task<byte[]> ReadAsync();

        /// <summary>
        /// A non-blocking attempt to read binary message from a producer asynchronously
        /// </summary>
        /// <param name="result">Binary message</param>
        /// <returns>A completion token encapsulating the flag indicating a successful read</returns>
        Task<bool> TryReadAsync(out byte[] result);

        /// <summary>
        /// Waits for an object message from a producer asynchronously
        /// </summary>
        /// <returns>A completion token encapsulating the object</returns>
        Task<object> ReadObjectAsync();


        /// <summary>
        /// A non-blocking attempt to read an object message from a producer asynchronously
        /// </summary>
        /// <param name="obj">Object message</param>
        /// <returns>A completion token encapsulating the flag indicating a successful read</returns>
        Task<bool> TryReadObjectAsync(out bool result);
    }
}
