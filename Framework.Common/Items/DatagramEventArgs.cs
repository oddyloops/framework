

using System;

namespace Framework.Common.Items
{
    /// <summary>
    /// Event information when a datagram is received
    /// </summary>
    public class DatagramEventArgs : EventArgs
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="message">Datagram message</param>
        /// <param name="sourceAddress"> Sender address</param>
        public DatagramEventArgs(byte[] message, byte[] sourceAddress)
        {
            Message = message;
            SourceAddress = sourceAddress;
        }
        /// <summary>
        /// Datagram message
        /// </summary>
       public byte[] Message { get; set; }

        /// <summary>
        /// Sender address
        /// </summary>
       public byte[] SourceAddress { get; set; }
    }
}
