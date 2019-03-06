


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

        /// <summary>
        /// Specifies the log level for the default console log listener implementation
        /// </summary>
        internal const string CONSOLE_LOG_LEVEL = "CONSOLE_LOG_LEVEL";

        /// <summary>
        /// Specifies the log level for the default file log listener implementation
        /// </summary>
        internal const string FILE_LOG_LEVEL = "FILE_LOG_LEVEL";

        /// <summary>
        /// A flag determining if log file is going to be broken into sequential parts
        /// </summary>
        internal const string IS_ROLLING_FILE = "IS_ROLLING_FILE";

        /// <summary>
        /// Maximum log file size in bytes before creating a new file sequence
        /// </summary>
        internal const string MAX_ROLL_FILE_SIZE = "MAX_ROLL_FILE_SIZE";

        /// <summary>
        /// Folder where log files are written
        /// </summary>
        internal const string LOG_FOLDER_PATH = "LOG_FOLDER_PATH";
    }
}
