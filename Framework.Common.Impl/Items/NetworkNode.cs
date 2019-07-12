using System.Collections.Generic;
using System.Composition;
using System.Net.Sockets;
using Framework.Common.Items;

namespace Framework.Common.Impl.Items
{
    /// <summary>
    /// A concrete implementation of INetworkNode that encapsulates details
    /// about a single network node
    /// </summary>
    [Export(typeof(INetworkNode))]
    public class NetworkNode : INetworkNode
    {
        /// <summary>
        /// Node network name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Node address on network
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Node's listening port
        /// </summary>
        public int ListeningPort { get; set; }

        /// <summary>
        /// A handle to the node's connection socket
        /// </summary>
        public Socket Socket { get; set; }



        public override int GetHashCode()
        {
            int hash = 0;
            if(Address != null)
            {
                foreach(byte b in Address)
                {
                    hash ^= b;
                }
            }
            return hash;
        }
    }
}
