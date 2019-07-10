using Framework.Common.Services;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Impl.Services
{
    [Export(typeof(INetworkService))]
    public class NetworkService : INetworkService
    {
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        public AddressFamily AddressScheme { get; set; }

        public ProtocolType Protocol { get; set; }

        public HashSet<INetworkNode> ConnectedClients { get; set; }

        public INetworkNode ConnectedServer { get; set; }

        public bool IsConnectionless { get; set; }

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
            throw new NotImplementedException();
        }

        public IStatus<string> Connect(byte[] address, int port)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> Connect(string name, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> Connect(byte[] address, int port, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ConnectAsync(string name, int port)
        {
            throw new NotImplementedException();
        }

        public Task<IStatus<string>> ConnectAsync(byte[] address, int port)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void DisconnectNode(INetworkNode client)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> Listen(int port)
        {
            throw new NotImplementedException();
        }

        public IStatus<string> Listen(int port, int maxConnections)
        {
            throw new NotImplementedException();
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
