using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies the fields required for a network service
    /// </summary>
    public interface INetworkConfig : IConfig
    {
        /// <summary>
        /// If network protocol requires a persisting connection
        /// </summary>
        bool IsConnectionless { get; set; }
        
        /// <summary>
        /// What family of network does the protocol belong to? (IPv4, IPv6, Token ring, etc)
        /// </summary>
        string AddressFamily { get; set; }

        /// <summary>
        /// Network protocol (e.g: TCP, UDP, IP, etc)
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        /// Maximum number of bytes that can be received at once
        /// </summary>
        string ReceivedBufferSize { get; set; }

        /// <summary>
        /// Port for receiving connections/messages
        /// </summary>
        string ListeningPort { get; set; }
    }
}
