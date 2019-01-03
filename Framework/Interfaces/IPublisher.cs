using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements for a publisher implementation in a publish-subscribe context
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Notify subscribers that a new publication is available
        /// </summary>
        void Publish();

        /// <summary>
        /// Notify subscribers that a new publication data is available
        /// </summary>
        /// <param name="publishData">Publication data</param>
        void Publish(EventArgs publishData);

        /// <summary>
        /// Subscriber list
        /// </summary>
        IList<ISubscriber> Subscribers { get; }

        /// <summary>
        /// Removes all subscribers tied to this publisher
        /// </summary>
        void ClearSubscribers();
    }
}
