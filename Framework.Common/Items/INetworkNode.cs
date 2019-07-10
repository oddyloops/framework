using System.Collections.Generic;
using System.Net.Sockets;

namespace Framework.Common.Items
{
    /// <summary>
    /// An interface that specifies basic properties of a network node
    /// </summary>
    public interface INetworkNode
    {
        /// <summary>
        /// Node network name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Node address on network
        /// </summary>
        byte[] Address { get; set; }

        /// <summary>
        /// A handle to the node's connection socket
        /// </summary>
        Socket Socket { get; set; }
    }
}