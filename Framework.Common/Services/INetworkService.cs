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
    public interface INetworkService : IService, IDisposable
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

        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Listen(int port);

        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Listen(int port, int maxConnections);

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="validClients"></param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Listen(int port, IList<INetworkNode> validClients);

        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ListenAsync(int port);

        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ListenAsync(int port, int maxConnections);

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list asynchronously
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="validClients"></param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> ListenAsync(int port, IList<INetworkNode> validClients);

        /// <summary>
        /// Waits for incoming message on a network data stream in a connection-oriented protocol
        /// </summary>
        /// <param name="client">Client node at the other end of the stream</param>
        /// <returns>Received message</returns>
        byte[] ReceiveStream(INetworkNode client);

        /// <summary>
        /// A non-blocking attempt to read off available messages in an established network data stream for connection-oriented
        /// protocols
        /// </summary>
        /// <param name="client">Client node at the other end of the stream</param>
        /// <param name="messageReceived">An output parameter that contains read message if any</param>
        /// <returns>A flag indicating success/failure of the operation</returns>
        bool TryReceiveStream(INetworkNode client, out byte[] messageReceived);

        /// <summary>
        /// Waits for incoming message from any sender in a connectionless protocol
        /// </summary>
        /// <returns>Received message</returns>
        byte[] ReceiveDatagram();

        /// <summary>
        /// A non-blocking attempt to receive message from an abitrary sender in a connectionless protocol
        /// </summary>
        /// <param name="messageReceived">An output parameter containing received message if any</param>
        /// <returns>>A flag indicating success/failure of the operation</returns>
        bool TryReceiveDatagram(out byte[] messageReceived);

        /// <summary>
        /// Waits for incoming message on a network data stream in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="client">Client node at the other end of the stream</param>
        /// <returns>A completion token encapsulating the received message</returns>
        Task<byte[]> ReceiveStreamAsync(INetworkNode client);

        /// <summary>
        /// A non-blocking attempt to read off available messages in an established network data stream for connection-oriented
        /// protocols asynchronously
        /// </summary>
        /// <param name="client">Client node at the other end of the stream</param>
        /// <param name="messageReceived">An output parameter that contains read message if any</param>
        /// <returns>A completion token encapsulating the flag indicating success/failure of the operation</returns>
        Task<bool> TryReceiveStreamAsync(INetworkNode client, out byte[] messageReceived);

        /// <summary>
        /// Waits for incoming message from any sender in a connectionless protocol asynchrobously
        /// </summary>
        /// <returns>A completion token encapsulating the received message</returns>
        Task<byte[]> ReceiveDatagramAsync();

        /// <summary>
        /// A non-blocking attempt to receive message from an abitrary sender in a connectionless protocol asynchronously
        /// </summary>
        /// <param name="messageReceived">An output parameter containing received message if any</param>
        /// <returns>>A completion token encapsulating the flag indicating success/failure of the operation</returns>
        Task<bool> TryReceiveDatagramAsync(out byte[] messageReceived);

        /// <summary>
        /// Disconnects a client node from this server
        /// </summary>
        /// <param name="client"></param>
        void DisconnectNode(INetworkNode client);

        /// <summary>
        /// Disconnects all clients from server and disposes all associated system resources
        /// </summary>
        void Disconnect();

    }

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
        /// A set of ports server is currently listening on
        /// </summary>
        HashSet<int> ListeningPort { get; set; }
    }
}
