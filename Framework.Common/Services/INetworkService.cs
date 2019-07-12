using Framework.Common.Items;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Common.Services
{


    public delegate void ReceivedStreamHandler(object sender, SocketEventArgs e);
   
    /// <summary>
    /// An interface that specifies the requirements for a networking framework implementation
    /// </summary>
    public interface INetworkService : IService, IDisposable
    {
        event ReceivedStreamHandler OnReceivedStreamMessage;


        /// <summary>
        /// A reference to a configuration component used to access config settings required by the network service
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
        IDictionary<INetworkNode,Thread> ConnectedClients { get;  }

        /// <summary>
        /// Server that the current node is connected to
        /// </summary>
        INetworkNode ConnectedServer { get; }

        /// <summary>
        /// Current node
        /// </summary>
        INetworkNode Self { get; }

        /// <summary>
        /// A flag to determine if the service protocol is connection-oriented or connectionless network
        /// </summary>
        bool IsConnectionless { get;  }

        /// <summary>
        /// Timeout value in ms for connecting to a server
        /// </summary>
        int ConnectTimeOut { get; set; }

        /// <summary>
        /// Timeout value in ms for receiving a client connection request
        /// </summary>
        int ListenTimeOut { get; set; }

        /// <summary>
        /// Timeout value in ms for sending a datagram/message
        /// </summary>
        int SendTimeOut { get; set; }

        /// <summary>
        /// Timeout value in ms for receiving a datagram/message
        /// </summary>
        int ReceiveTimeOut { get; set; }

        /// <summary>
        /// Maximum size of messages in bytes that can be received at once
        /// </summary>
        int ReceiveBufferSize { get; }

        

 
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
        /// Used by clients to send data over an established connection in a connection-oriented protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Connected receiver of message</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> Stream(byte[] message, INetworkNode receiver);

        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A status indicating result of the operation</returns>
        IStatus<string> SendDatagram(byte[] message, INetworkNode receiver);

     
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
        /// Used by clients to send data over an established connection in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Connected receiver of message</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> StreamAsync(byte[] message,INetworkNode receiver);

        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver);


        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol
        /// </summary>
        /// <returns>A status indicating result of the operation</returns>
        void Listen();

        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol
        /// </summary>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        /// <returns>A status indicating result of the operation</returns>
        void Listen(int maxConnections);

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list
        /// </summary>
        /// <param name="validClients"></param>
        /// <returns>A status indicating result of the operation</returns>
        void Listen( ISet<INetworkNode> validClients);

        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task ListenAsync();

        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task ListenAsync( int maxConnections);

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list asynchronously
        /// </summary>
        /// <param name="validClients"></param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        Task ListenAsync( ISet<INetworkNode> validClients);

        

       

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

    
}
