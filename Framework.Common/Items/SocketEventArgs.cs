

namespace Framework.Common.Items
{
    /// <summary>
    /// Event information when a message is received over a connection
    /// </summary>
    public class SocketEventArgs
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="message">Received message</param>
        /// <param name="sender">Sender node</param>
        public SocketEventArgs(byte[] message,INetworkNode sender)
        {
            Message = message;
            Sender = sender;
        }

        /// <summary>
        /// Received message
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// Sender node
        /// </summary>
        public INetworkNode Sender { get; set; }
    }
}
