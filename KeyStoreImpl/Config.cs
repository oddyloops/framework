namespace KeyStoreImpl
{
    /// <summary>
    /// Configuration key constants for this library
    /// </summary>
    internal struct Config
    {
        /// <summary>
        /// File path where encrypted keys are being stored
        /// </summary>
        internal const string KEY_STORE_PATH = "";

        /// <summary>
        /// Path to the windows RSA key container that is used in encrypting the key store
        /// </summary>
        internal const string ASYM_KEY_PATH = "";

        /// <summary>
        /// RSA key size in bits
        /// </summary>
        internal const string ASYM_KEY_SIZE_BITS = "";
    }
}
