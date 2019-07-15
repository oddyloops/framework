using Framework.Common.Services;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using Framework.Common.Items;
using Framework.Utils;
using System.Threading;
using System.Security;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A concrete implementation of the INetworkService for a client-server architecture
    /// </summary>
    [Export(typeof(INetworkService))]
    public class NetworkService : INetworkService
    {
        /// <summary>
        /// Thread client uses to listen for incoming messages from server in a connection
        /// oriented protocol
        /// </summary>
        private Thread _clientListeningThread;

        #region Helpers
        /// <summary>
        /// Method executed by the client listening thread
        /// </summary>
        /// <param name="threadArgs">An object encapsulating the server node connection</param>
        private void MessageListenerAction(object threadArgs)
        {
            INetworkNode senderNode = (INetworkNode)threadArgs;
            Socket socket = senderNode.Socket;
            socket.ReceiveBufferSize = ReceiveBufferSize;
            if (ReceiveTimeOut > 0)
            {
                socket.ReceiveTimeout = ReceiveTimeOut;
            }

            while (true)
            {
                byte[] buffer = new byte[socket.ReceiveBufferSize];
                int received = socket.Receive(buffer);
                if (received > 0)
                {
                    if (OnReceivedStreamMessage != null)
                    {
                        SocketEventArgs args = new SocketEventArgs(buffer.Take(received).ToArray(), senderNode);
                        OnReceivedStreamMessage(socket, args);
                    }
                }
                else
                {
                    //timed out
                    socket.Disconnect(false);
                    break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Event handler that is triggered upon reciept of a message
        /// in a connection-oriented protocol
        /// </summary>
        public event ReceivedStreamHandler OnReceivedStreamMessage;


        /// <summary>
        /// A reference to a configuration component used to access config settings required by the network service
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        /// <summary>
        /// Address scheme network nodes will be based on
        /// </summary>
        public AddressFamily AddressScheme { get; set; }

        /// <summary>
        /// Intended network protocol
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// A collection of all clients connected to the server
        /// with their respective listening threads
        /// </summary>
        public IDictionary<INetworkNode, Thread> ConnectedClients { get; }

        /// <summary>
        /// Server that the current node is connected to
        /// </summary>
        public INetworkNode ConnectedServer { get; private set; }


        /// <summary>
        /// Current node
        /// </summary>
        public INetworkNode Self { get; }


        /// <summary>
        /// A flag to determine if the service protocol is connection-oriented or connectionless network
        /// </summary>
        public bool IsConnectionless { get; private set; }

    

        /// <summary>
        /// Timeout value in ms for connecting to a server
        /// </summary>
        public int ConnectTimeOut { get; set; }

        /// <summary>
        /// Timeout value in ms for receiving a client connection request
        /// </summary>
        public int ListenTimeOut { get; set; }

        /// <summary>
        /// Timeout value in ms for sending a datagram/message
        /// </summary>
        public int SendTimeOut { get; set; }

        /// <summary>
        /// Timout value in ms for receiving a datagram/message
        /// </summary>
        public int ReceiveTimeOut { get; set; }

        /// <summary>
        /// Maximum size of messages in bytes that can be received at once
        /// </summary>
        public int ReceiveBufferSize { get; private set; } = 8192;
   
        public NetworkService()
        {
            IsConnectionless = Config.GetValue(ConfigConstants.IS_CONNECTIONLESS) == "1";
            AddressScheme =(AddressFamily) Enum.Parse(typeof(AddressFamily), Config.GetValue(ConfigConstants.ADDRESS_FAMILY));
            Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), Config.GetValue(ConfigConstants.PROTOCOL));
            ConnectedClients = new Dictionary<INetworkNode, Thread>();
            Self = Util.Container.CreateInstance<INetworkNode>();
            Self.Name = Dns.GetHostName();
            

            string portStr = Config.GetValue(ConfigConstants.LISTENING_PORT);
            if(!string.IsNullOrEmpty(portStr))
            {
                Self.ListeningPort = Convert.ToInt32(portStr);
            }

            IPAddress address = (from addy in Dns.GetHostAddresses(Self.Name)
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            Self.Address = address.GetAddressBytes();
            string receiveSizeStr = Config.GetValue(ConfigConstants.RECEIVE_BUFFER_SIZE);
            if(receiveSizeStr != null)
            {
                ReceiveBufferSize = Convert.ToInt32(receiveSizeStr);
            }

        }

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<string> Connect(string name, int port)
        {
            IPAddress address = (from addy in Dns.GetHostAddresses(name)
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            return Connect(address.GetAddressBytes(), port);
        }

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<string> Connect(byte[] address, int port)
        {
            if (IsConnectionless)
            {
                throw new InvalidOperationException("Cannot establish a connection in a connectionless protocol");
            }
            IPAddress endpoint = new IPAddress(address);
            Socket client = new Socket(AddressScheme, SocketType.Stream, Protocol);
            
            if (ConnectTimeOut <= 0)
            {
                client.Connect(endpoint, port);
            }
            else
            {
                Thread connThread = new Thread(() => client.Connect(endpoint, port));
                connThread.Start();
                connThread.Join(ConnectTimeOut);
                if (connThread.ThreadState != ThreadState.Stopped)
                {
                    connThread.Abort();
                }
            }
            IStatus<string> status = Util.Container.CreateInstance<IStatus<string>>();
            if (client.Connected)
            {
                ConnectedServer = Util.Container.CreateInstance<INetworkNode>();
                ConnectedServer.Address = address;
                ConnectedServer.Socket = client;
               
                _clientListeningThread = new Thread(new ParameterizedThreadStart(MessageListenerAction));
                _clientListeningThread.Start(ConnectedServer);
                status.IsSuccess = true;
                status.StatusMessage = "Connection Successful";
            }
            else
            {
                client.Close();
                status.IsSuccess = false;
                status.StatusMessage = "Connection timed out";
                status.StatusInfo = $"Timed out after {ConnectTimeOut} ms";
            }
            return status;
        }



        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="name">Server name</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        public async Task<IStatus<string>> ConnectAsync(string name, int port)
        {
            IPAddress address = (from addy in (await Dns.GetHostAddressesAsync(name))
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            return await ConnectAsync(address.GetAddressBytes(), port);
        }

        /// <summary>
        /// Used by clients for connecting to a server in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="address">Server address</param>
        /// <param name="port">Port for connection</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        public async Task<IStatus<string>> ConnectAsync(byte[] address, int port)
        {
            if(IsConnectionless)
            {
                throw new InvalidOperationException("Cannot establish a connection in a connectionless protocol");
            }
            IPAddress endpoint = new IPAddress(address);
            Socket client = new Socket(AddressScheme, SocketType.Stream, Protocol);
            
            if (ConnectTimeOut <= 0)
            {
                await client.ConnectAsync(endpoint, port);
            }
            else
            {
                var cancelSource = new CancellationTokenSource();

                Task connectTask = Task.Run(async () => await client.ConnectAsync(endpoint, port), cancelSource.Token);

                connectTask.Wait(ConnectTimeOut);
                if (!connectTask.IsCompleted)
                {
                    cancelSource.Cancel();
                }

            }
            IStatus<string> status = Util.Container.CreateInstance<IStatus<string>>();
            if (client.Connected)
            {
                ConnectedServer = Util.Container.CreateInstance<INetworkNode>();
                ConnectedServer.Address = address;
                ConnectedServer.Socket = client;
                _clientListeningThread = new Thread(new ParameterizedThreadStart(MessageListenerAction));
                _clientListeningThread.Start(ConnectedServer);
                status.IsSuccess = true;
                status.StatusMessage = "Connection Successful";
            }
            else
            {
                client.Close();
                status.IsSuccess = false;
                status.StatusMessage = "Connection timed out";
                status.StatusInfo = $"Timed out after {ConnectTimeOut} ms";
            }
            return status;
        }


        /// <summary>
        /// Disconnects all clients from server and disposes all associated system resources
        /// </summary>
        public void Disconnect()
        {
            if (ConnectedClients != null)
            {
                foreach (var client in ConnectedClients)
                {
                    if (client.Key.Socket != null && client.Key.Socket.Connected)
                    {
                        client.Key.Socket.Disconnect(false);
                        client.Value.Abort(); //close receiving thread
                    }
                }
                ConnectedClients.Clear();
            }

            if(ConnectedServer!= null)
            {
                ConnectedServer.Socket.Disconnect(false);
                if(_clientListeningThread != null && _clientListeningThread.ThreadState != ThreadState.Stopped)
                {
                    _clientListeningThread.Abort();
                }
                
            }
        }


        /// <summary>
        /// Disconnects a client node from this server
        /// </summary>
        /// <param name="client"></param>
        public void DisconnectNode(INetworkNode client)
        {
            if (ConnectedClients.ContainsKey(client))
            {
                if (client.Socket != null && client.Socket.Connected)
                {
                    client.Socket.Disconnect(false);
                    ConnectedClients[client].Abort();
                }
                ConnectedClients.Remove(client);
            }
        }

        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol
        /// </summary>
        public void Listen()
        {
            Listen(int.MaxValue);
        }

        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol
        /// </summary>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        public void Listen( int maxConnections)
        {
            if (IsConnectionless)
            {
                throw new InvalidOperationException("Cannot establish a connection in a connectionless protocol");
            }
            if (Self.Socket == null)
            {
                Self.Socket = new Socket(AddressScheme, SocketType.Stream, Protocol);
            }
            
            Self.Socket.Bind(new IPEndPoint(IPAddress.Any, Self.ListeningPort));
            Self.Socket.Listen(maxConnections);
            while (true)
            {
                Socket newConn = null;
                if (ListenTimeOut <= 0)
                {
                    newConn = Self.Socket.Accept();
                }
                else
                {
                    Thread listenerThread = new Thread(() => newConn = Self.Socket.Accept());
                    listenerThread.Start();
                    listenerThread.Join(ListenTimeOut);
                    if (listenerThread.ThreadState != ThreadState.Stopped)
                    {
                        listenerThread.Abort();
                    }
                }
                if(newConn == null)
                {
                    break; //timed out
                }
                INetworkNode client = Util.Container.CreateInstance<INetworkNode>();
                client.Address = (newConn.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes();
                client.Socket = newConn;
                Thread receiverThread = new Thread(new ParameterizedThreadStart(MessageListenerAction));
                receiverThread.Start(client);
                ConnectedClients.Add(client, receiverThread);
            }

         

        }

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list
        /// </summary>
        /// <param name="validClients">Client white list</param>
        public void Listen( ISet<INetworkNode> validClients)
        {
            if (IsConnectionless)
            {
                throw new InvalidOperationException("Cannot establish a connection in a connectionless protocol");
            }
            if (Self.Socket == null)
            {
                Self.Socket = new Socket(AddressScheme, SocketType.Stream, Protocol);
            }
            
            Self.Socket.Bind(new IPEndPoint(IPAddress.Any, Self.ListeningPort));
            Self.Socket.Listen(int.MaxValue);
            while (true)
            {
                Socket newConn = null;
                if (ListenTimeOut <= 0)
                {
                    newConn = Self.Socket.Accept();
                }
                else
                {
                    Thread listenerThread = new Thread(() => newConn = Self.Socket.Accept());
                    listenerThread.Start();
                    listenerThread.Join(ListenTimeOut);
                    if (listenerThread.ThreadState != ThreadState.Stopped)
                    {
                        listenerThread.Abort();
                    }
                }
                if (newConn == null)
                {
                    break; //timed out
                }
                else
                {
                    INetworkNode client = Util.Container.CreateInstance<INetworkNode>();
                    client.Address = (newConn.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes();
                    client.Socket = newConn;
                    //check if it is a valid client
                    if (validClients.Contains(client))
                    {
                        Thread receiverThread = new Thread(new ParameterizedThreadStart(MessageListenerAction));
                        receiverThread.Start(client);
                        ConnectedClients.Add(client, receiverThread);
                    }
                    else
                    {
                        //disconnect and raise an exception
                        newConn.Disconnect(false);
                        throw new SecurityException($"An unauthorized address: {(newConn.RemoteEndPoint as IPEndPoint).Address} tried to connect to server");
                    }
                }
            }
        }


        /// <summary>
        /// Opens a port for receiving incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <returns>A completion token </returns>
        public async Task ListenAsync()
        {
            await ListenAsync(int.MaxValue);
        }


        /// <summary>
        /// Opens a port for receiving a limited amount of incoming connections in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="maxConnections">Maximum number of connections allowed</param>
        /// <returns>A completion token</returns>
        public Task ListenAsync(int maxConnections)
        {
            Listen( maxConnections);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Opens a port for receiving incoming connections from a client white-list asynchronously
        /// </summary>
        /// <param name="validClients">Valid Clients</param>
        /// <returns>A completion token</returns>
        public Task ListenAsync(ISet<INetworkNode> validClients)
        {
            Listen( validClients);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Waits for incoming message from any sender in a connectionless protocol
        /// </summary>
        /// <returns>Received message</returns>
        public byte[] ReceiveDatagram()
        {
            if(Self.Socket == null)
            {
                Self.Socket = new Socket(AddressScheme, SocketType.Dgram, Protocol);
                Self.Socket.Bind(new IPEndPoint(IPAddress.Any, Self.ListeningPort));
            }
            Self.Socket.ReceiveBufferSize = ReceiveBufferSize;
            byte[] buffer = new byte[ReceiveBufferSize];
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            int received = 0;
            
            if(ReceiveTimeOut <= 0)
            {
                received = Self.Socket.ReceiveFrom(buffer, ref sender);
            }
            else
            {
                Thread receiveThread = new Thread(() => received = Self.Socket.ReceiveFrom(buffer, ref sender));
                receiveThread.Start();
                receiveThread.Join(ReceiveTimeOut);
            }

            if(received > 0)
            {
                return buffer.Take(received).ToArray();
            }
            else
            {
                //timed out
                return null;
            }
            
        }

        /// <summary>
        /// Waits for incoming message from any sender in a connectionless protocol asynchrobously
        /// </summary>
        /// <returns>A completion token encapsulating the received message</returns>
        public Task<byte[]> ReceiveDatagramAsync()
        {
            return Task.FromResult(ReceiveDatagram());
        }

        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A status indicating result of the operation</returns>

        public IStatus<string> SendDatagram(byte[] message, INetworkNode receiver)
        {
            receiver.Socket.SendTo(message, receiver.Socket.RemoteEndPoint);
            IStatus<string> status = Util.Container.CreateInstance<IStatus<string>>();
            status.IsSuccess = true;
            status.StatusMessage = "Message Sent";
            return status;
        }


        /// <summary>
        /// Used by nodes to send data over a one-time link in a connectionless protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Recipient node</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        public Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver)
        {
            return Task.FromResult(SendDatagram(message, receiver));
        }

        /// <summary>
        /// Used by clients to send data over an established connection in a connection-oriented protocol
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Connected receiver of message</param>
        /// <returns>A status indicating result of the operation</returns>
        public IStatus<string> Stream(byte[] message, INetworkNode receiver)
        {
            
            Socket recipient = null;
            if(ConnectedServer != null && ConnectedServer.GetHashCode() == receiver.GetHashCode())//check if its server
            {
                recipient = ConnectedServer.Socket;
                
            }
            else
            {
                foreach(var client in ConnectedClients.Keys)
                {
                    if(client.GetHashCode() == receiver.GetHashCode())
                    {
                        recipient = client.Socket;
                    }
                }
            }

            IStatus<string> status = Util.Container.CreateInstance<IStatus<string>>();
            if (recipient == null)
            {
                status.IsSuccess = false;
                status.StatusMessage = "No matching server or client node found in the list of connected nodes";
            }
            else
            {
                
                recipient.Send(message);
                status.IsSuccess = true;
                status.StatusMessage = "Message Sent";
            }
            return status;
        }

        /// <summary>
        /// Used by clients to send data over an established connection in a connection-oriented protocol asynchronously
        /// </summary>
        /// <param name="message">Message data</param>
        /// <param name="receiver">Connected receiver of message</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        public Task<IStatus<string>> StreamAsync(byte[] message,INetworkNode receiver)
        {
            return Task.FromResult(Stream(message,receiver));
        }

      
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NetworkService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
