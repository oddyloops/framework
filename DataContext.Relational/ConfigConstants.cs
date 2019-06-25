

namespace DataContext.Relational
{
    /// <summary>
    /// Configuration key constants for this library
    /// </summary
    internal struct ConfigConstants
    {
        /// <summary>
        /// Indicates whether transactions are used or not (a false means yes, thereby
        /// giving the caller the responsibility of committing transactions) [0-for false, 1-for true]
        /// </summary>
        internal const string RELATIONAL_DB_AUTOCOMMIT = "RELATIONAL_DB_AUTOCOMMIT";

        /// <summary>
        /// Specifies the connection string for the relational database
        /// </summary>
        internal const string RELATIONAL_CONNECTION_STRING = "RELATIONAL_CONNECTION_STRING";

        /// <summary>
        /// Specifies the type of database being used 
        /// (0-Sql Server, 1 - MySql, 2 - Oracle)
        /// </summary>
        internal const string RELATIONAL_DB_TYPE = "RELATIONAL_DB_TYPE";

        /// <summary>
        /// Specifies the table schema prefixes (if any) [e.g dbo.Table]
        /// </summary>
        internal const string RELATIONAL_TABLE_PREFIX = "RELATIONAL_TABLE_PREFIX";
    }

}
