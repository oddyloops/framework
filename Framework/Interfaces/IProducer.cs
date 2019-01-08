using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements for a producer implementation within a producer-consumer context
    /// </summary>
    public interface IProducer
    {
        

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the producer
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// A reference to the buffer shared between this producer and one or more consumers
        /// </summary>
        IProducerConsumerBuffer Buffer { get; }

        /// <summary>
        /// Produces a string message for a consumer
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus Write(string message);

        /// <summary>
        /// Produces a binary message for a consumer
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus Write(byte[] message);

        /// <summary>
        /// Produces an object for the consumer
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus Write(object obj);
        
        /// <summary>
        /// Produces a string message for the consumer asynchronously
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus> WriteAsync(string message);

        /// <summary>
        /// Produces a binary message for the consumer asynchronously
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus> WriteAsync(byte[] message);

        /// <summary>
        /// Produces an object for the consumer asynchronously
        /// </summary>
        /// <param name="message">Object</param>
        /// <returns>A completion token encapsulating the status indicating the result of the operation</returns>
        Task<IStatus> WriteAsync(object obj);

    }
}
