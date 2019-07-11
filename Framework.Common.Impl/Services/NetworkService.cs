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
    [Export(typeof(INetworkService))]
    public class NetworkService : INetworkService
    {
        
        private Thread _clientListeningThread;

        #region Helpers
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
                        SocketEventArgs args = new SocketEventArgs(buffer, senderNode);
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

        public event ReceivedStreamHandler OnReceivedStreamMessage;

        public event ReceivedDatagramHandler OnReceivedDatagram;


        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        public AddressFamily AddressScheme { get; set; }

        public ProtocolType Protocol { get; set; }

        public IDictionary<INetworkNode, Thread> ConnectedClients { get; }

        public INetworkNode ConnectedServer { get; private set; }

        public INetworkNode Self { get; }

        public bool IsConnectionless { get; set; }

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
            ConnectedClients = new Dictionary<INetworkNode, Thread>();
            Self = Util.Container.CreateInstance<INetworkNode>();
            Self.Name = Dns.GetHostName();
            
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


        public IStatus<string> Connect(string name, int port)
        {
            IPAddress address = (from addy in Dns.GetHostAddresses(name)
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            return Connect(address.GetAddressBytes(), port);
        }

        public IStatus<string> Connect(byte[] address, int port)
        {

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

        

        public async Task<IStatus<string>> ConnectAsync(string name, int port)
        {
            IPAddress address = (from addy in (await Dns.GetHostAddressesAsync(name))
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            return await ConnectAsync(address.GetAddressBytes(), port);
        }

        public async Task<IStatus<string>> ConnectAsync(byte[] address, int port)
        {
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

        public void Listen(int port)
        {
            Listen(port, int.MaxValue);
        }

        public void Listen(int port, int maxConnections)
        {
            if (Self.Socket == null)
            {
                Self.Socket = new Socket(AddressScheme, SocketType.Stream, Protocol);
            }
            IPAddress selfAdd = new IPAddress(Self.Address);
            Self.Socket.Bind(new IPEndPoint(selfAdd, port));
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

        public void Listen(int port, ISet<INetworkNode> validClients)
        {
            if (Self.Socket == null)
            {
                Self.Socket = new Socket(AddressScheme, SocketType.Stream, Protocol);
            }
            IPAddress selfAdd = new IPAddress(Self.Address);
            Self.Socket.Bind(new IPEndPoint(selfAdd, port));
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

        public async Task ListenAsync(int port)
        {
            await ListenAsync(port, int.MaxValue);
        }

        public Task ListenAsync(int port, int maxConnections)
        {
            Listen(port, maxConnections);
            return Task.FromResult(0);
        }

        public Task ListenAsync(int port, ISet<INetworkNode> validClients)
        {
            Listen(port, validClients);
            return Task.FromResult(0);
        }

        public byte[] ReceiveDatagram()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReceiveDatagramAsync()
        {
            throw new NotImplementedException();
        }

      

        public IStatus<string> SendDatagram(byte[] message, INetworkNode receiver)
        {
            throw new NotImplementedException();
        }

      

        public Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver)
        {
            throw new NotImplementedException();
        }

      
        public IStatus<string> Stream(byte[] message)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> StreamAsync(byte[] message)
        {
            throw new NotImplementedException();
        }

        public bool TryReceiveDatagram(out byte[] messageReceived)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryReceiveDatagramAsync(out byte[] messageReceived)
        {
            throw new NotImplementedException();
        }

      
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
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
