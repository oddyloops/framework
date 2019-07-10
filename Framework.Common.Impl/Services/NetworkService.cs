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

namespace Framework.Common.Impl.Services
{
    [Export(typeof(INetworkService))]
    public class NetworkService : INetworkService
    {
        private HashSet<INetworkNode> _connectedClients;
        private INetworkNode _connectedServer;
        private INetworkNode _self;

        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        public AddressFamily AddressScheme { get; set; }

        public ProtocolType Protocol { get; set; }

        public HashSet<INetworkNode> ConnectedClients { get => _connectedClients; }

        public INetworkNode ConnectedServer { get => _connectedServer; }

        public INetworkNode Self { get => _self; }

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

        public NetworkService()
        {
            _self = Util.Container.CreateInstance<INetworkNode>();
            _self.Name = Dns.GetHostName();

            IPAddress address = (from addy in Dns.GetHostAddresses(_self.Name)
                                 where addy.AddressFamily == AddressScheme
                                 select addy).First();
            _self.Address = address.GetAddressBytes();
        }

        public bool Authenticate(NetworkCredential credentials)
        {

            throw new NotImplementedException();
        }

        public Task<bool> AuthenticateAsync(NetworkCredential credentials)
        {
            throw new NotImplementedException();
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
                _connectedServer = Util.Container.CreateInstance<INetworkNode>();
                _connectedServer.Address = address;
                _connectedServer.Socket = client;
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

        public IStatus<string> Connect(string name, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> Connect(byte[] address, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
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
                _connectedServer = Util.Container.CreateInstance<INetworkNode>();
                _connectedServer.Address = address;
                _connectedServer.Socket = client;
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

        public Task<IStatus<string>> ConnectAsync(string name, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ConnectAsync(byte[] address, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            if (_connectedClients != null)
            {
                foreach (var client in _connectedClients)
                {
                    if (client.Socket != null && client.Socket.Connected)
                    {
                        client.Socket.Disconnect(false);
                    }
                }
                _connectedClients.Clear();
            }
        }

        public void DisconnectNode(INetworkNode client)
        {
            if (_connectedClients.Contains(client))
            {
                if (client.Socket != null && client.Socket.Connected)
                {
                    client.Socket.Disconnect(false);
                }
                _connectedClients.Remove(client);
            }
        }

        public IStatus<string> Listen(int port)
        {
            return Listen(port, int.MaxValue);
        }

        public IStatus<string> Listen(int port, int maxConnections)
        {
            if (_self.Socket == null)
            {
                _self.Socket = new Socket(AddressScheme, SocketType.Stream, Protocol);
            }
            IPAddress selfAdd = new IPAddress(_self.Address);
            _self.Socket.Bind(new IPEndPoint(selfAdd, port));

            if(ListenTimeOut <= 0)
            {
                _self.Socket.Listen(maxConnections);
            }
        }

        public IStatus<string> Listen(int port, IList<INetworkNode> validClients)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ListenAsync(int port)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ListenAsync(int port, int maxConnections)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ListenAsync(int port, IList<INetworkNode> validClients)
        {
            throw new NotImplementedException();
        }

        public byte[] ReceiveDatagram()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReceiveDatagramAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] ReceiveStream(INetworkNode client)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReceiveStreamAsync(INetworkNode client)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> SendDatagram(byte[] message, INetworkNode receiver)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> SendDatagram(byte[] message, INetworkNode receiver, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> SendDatagramAsync(byte[] message, INetworkNode receiver, NetworkCredential credentials)
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

        public bool TryReceiveStream(INetworkNode client, out byte[] messageReceived)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryReceiveStreamAsync(INetworkNode client, out byte[] messageReceived)
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
