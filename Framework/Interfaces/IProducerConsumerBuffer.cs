using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements for a producer-consumer shared buffer
    /// </summary>
    public interface IProducerConsumerBuffer
    {
        /// <summary>
        /// A flag indicating if buffer is empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// A flag indicating if buffer is full
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// The number of items in buffer
        /// </summary>
        int Count { get;  }

        /// <summary>
        /// Wipes buffer clean
        /// </summary>
        void Clear();


        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        IConfiguration Config { get; set; }
    }
}
