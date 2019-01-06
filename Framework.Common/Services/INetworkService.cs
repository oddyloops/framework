using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Framework.Common.Services
{
    /// <summary>
    /// An interface that specifies the requirements for a networking framework implementation
    /// </summary>
    public interface INetworkService : IService
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the data provider
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Address scheme network nodes will be based on
        /// </summary>
        AddressFamily AddressScheme { get; }

        /// <summary>
        /// Intended network protocol
        /// </summary>
        ProtocolType Protocol { get; }

        /// <summary>
        /// A collection of all clients connected to the server
        /// </summary>
        HashSet<INetworkNode> ConnectedClients { get;  }

        /// <summary>
        /// Server that the current node is connected to
        /// </summary>
        INetworkNode ConnectedServer { get; }

        /// <summary>
        /// A flag to determine if the service protocol is connection-oriented or connectionless network
        /// </summary>
        bool IsConnectionless { get;  }

        /// <summary>
        /// Authenticates clients attempting to connect to this service
        /// </summary>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A flag indicating the success/failure of authentication</returns>
        bool Authenticate(NetworkCredential credentials);

        /// <summary>
        /// Authenticates clients attempting to connect to this service asynchronously
        /// </summary>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A completion token encapsulating the flag indicating success/failure of authentication</returns>
        Task<bool> AuthenticateAsync(NetworkCredential credentials);

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Connect(string name, int port);

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Connect(byte[] address, int port);

        /// <summary>
        /// Used by clients for securely connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Connect(string name, int port,NetworkCredential credentials);

        /// <summary>
        /// Used by clients for securely connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Connect(byte[] address, int port, NetworkCredential credentials);

        /// <summary>
        /// Used by clients to send data over an established connection in a connection-oriented protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Stream(byte[] message);

        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> SendDatagram(byte[] message, INetworkNode receiver);

        /// <summary>
        /// Used by nodes to securely send data over a one-time link in a connectionless protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <param name="credentials">Sender credentials</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> SendDatagram(byte[] message, INetworkNode receiver, NetworkCredential credentials);

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ConnectAsync(string name, int port);

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ConnectAsync(byte[] address, int port);
      

        /// <summary>
        /// Used by clients for securely connecting to a server in a connection-oriented protocol asynchrously
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ConnectAsync(string name, int port, NetworkCredential credentials);

        /// <summary>
        /// Used by clients for securely connecting to a server in a connection-oriented protocol asychronously
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <param name="credentials">Client credentials</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ConnectAsync(byte[] address, int port, NetworkCredential credentials);

        /// <summary>
        /// Used by clients to send data over an established connection in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> StreamAsync(byte[] message);

        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver);

        /// <summary>
        /// Used by nodes to securely send data over a one-time link in a connectionless protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <param name="credentials">Sender credentials</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver, NetworkCredential credentials);


        IStatus<string> Listen(int port);

        IStatus<string> Listen(int port, int maxConnections);

        IStatus<string> Listen(int port, IList<INetworkNode> validClients);

        Task<IStatus<string>> ListenAsync(int port);

        Task<IStatus<string>> ListenAsync(int port, int maxConnections);

        Task<IStatus<string>> ListenAsync(int port, IList<INetworkNode> validClients);

        byte[] ReceiveStream(INetworkNode client);

        bool TryReceiveStream(INetworkNode client, out byte[] messageReceived);

        byte[] ReceiveDatagram();

        bool TryReceiveDatagram(out byte[] messageReceived);

        Task<byte[]> ReceiveStreamAsync(INetworkNode client);

        Task<bool> TryReceiveStreamAsync(INetworkNode client, out byte[] messageReceived);

        Task<byte[]> ReceiveDatagramAsync();

        Task<bool> TryReceiveDatagramAsync(out byte[] messageReceived);




        void Disconnect();

    }

    public interface INetworkNode
    {

        string Name { get; set; }

        byte[] Address { get; set; }

        int ListeningPort { get; set; }
    }
}
