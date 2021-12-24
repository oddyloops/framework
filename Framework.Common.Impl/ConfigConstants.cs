namespace Framework.Common.Impl
{
    /// <summary>
    /// Configuration key constants for this library
    /// </summary>
    internal struct ConfigConstants
    {
        /// <summary>
        /// File path where encrypted keys are being stored
        /// </summary>
        internal const string KEY_STORE_PATH = "KEY_STORE_PATH";

        /// <summary>
        /// Path to the windows RSA key container that is used in encrypting the key store
        /// </summary>
        internal const string ASYM_KEY_PATH = "ASYM_KEY_PATH";

        /// <summary>
        /// RSA key size in bits
        /// </summary>
        internal const string ASYM_KEY_SIZE_BITS = "ASYM_KEY_SIZE_BITS";

        /// <summary>
        /// Block size used for symmetric encryption in bits
        /// </summary>
        internal const string ENCRYPTION_BLOCK_SIZE_BYTES = "ENCRYPTION_BLOCK_SIZE_BYTES";

        /// <summary>
        /// Path to encrypted credentials for sending emails true to send grid API
        /// </summary>
        internal const string SEND_GRID_ENCRYPTED = "SEND_GRID_ENCRYPTED";

        /// <summary>
        /// Index used to store symmetric key used for encryption in the key store
        /// </summary>
        internal const string SYMMETRIC_KEY_INDEX = "SYMMETRIC_KEY_INDEX";

        /// <summary>
        /// Fixed part of the recovery email link
        /// </summary>
        internal const string RECOVERY_LINK_PREFIX = "RECOVERY_LINK_PREFIX";

        /// <summary>
        /// File path for recovery email body template 
        /// </summary>
        internal const string RECOVERY_MAIL_TEMPLATE_PATH = "RECOVERY_MAIL_TEMPLATE_PATH";

        /// <summary>
        /// Recovery email subject
        /// </summary>
        internal const string RECOVERY_MAIL_SUBJECT = "RECOVERY_MAIL_SUBJECT";

        /// <summary>
        /// Used as the email sender alias when sending an email recovery message
        /// </summary>
        internal const string RECOVERY_SENDER_ALIAS = "RECOVERY_SENDER_ALIAS";

        /// <summary>
        /// Maximum size in bytes of socket data that can be received at once
        /// </summary>
        internal const string RECEIVE_BUFFER_SIZE = "RECEIVE_BUFFER_SIZE";
        
        /// <summary>
        /// A flag indicating if protocol requires a connection or not
        /// </summary>
        internal const string IS_CONNECTIONLESS = "IS_CONNECTIONLESS";

        /// <summary>
        /// Network protocol (e.g: TCP, UDP, IP, etc)
        /// </summary>
        internal const string PROTOCOL = "PROTOCOL";

        /// <summary>
        /// The addressing scheme of protocol in use (e.g: IPv4/b6, Data-link, Apple-Talk, etc)
        /// </summary>
        internal const string ADDRESS_FAMILY = "ADDRESS_FAMILY";
        
        /// <summary>
        /// The server's listening port
        /// </summary>
        internal const string LISTENING_PORT = "LISTENING_PORT";

        /// <summary>
        /// SFTP Server Host Url
        /// </summary>
        internal const string SFTP_HOST = "SFTP_HOST";

        /// <summary>
        /// SFTP username
        /// </summary>
        internal const string SFTP_USERNAME = "SFTP_USERNAME";

        /// <summary>
        /// SFTP Password
        /// </summary>
        internal const string SFTP_PASSWORD = "SFTP_PASSWORD";
    }



}
