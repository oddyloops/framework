using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements of a subscriber within a publish-subscribe context
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the subscriber
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Subscribe to specified publisher
        /// </summary>
        /// <param name="publisher">Publisher</param>
        void Subscribe(IPublisher publisher);

        /// <summary>
        /// Notifies this subscriber of a new publication
        /// </summary>
        /// <param name="publisher">Source of publication</param>
        void ReceivePublication(IPublisher publisher);

        /// <summary>
        /// Notifies this subscriber of a new publication data
        /// </summary>
        /// <param name="publisher">Source of publication data</param>
        /// <param name="data">Publication data</param>
        void ReceivePublication(IPublisher publisher, EventArgs data);

    }
}
