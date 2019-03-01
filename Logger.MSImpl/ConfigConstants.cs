


namespace Logger.MSImpl
{
     /// <summary>
    /// Configuration key constants for this library
    /// </summary
    internal struct ConfigConstants
    {
        /// <summary>
        /// Determines if a timestamp is included in every log message
        /// </summary>
        internal const string INCLUDE_TIMESTAMP = "INCLUDE_TIMESTAMP";

        /// <summary>
        /// Determines if a new line is added at the end of log messages
        /// </summary>
        internal const string AUTO_FLUSH = "AUTO_FLUSH";

        /// <summary>
        /// Determines if caller function name prefix is added to log messages
        /// </summary>
        internal const string INCLUDE_CALLER = "INCLUDE_CALLER";
    }
}
