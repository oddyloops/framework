namespace DataContext.NoSql.MongoDB
{
    internal struct ConfigConstants
    {
        /// <summary>
        /// Connection string for mongo DB
        /// </summary>
        internal const string MONGODB_CONNECTION_STRING = "MONGODB_CONNECTION_STRING";

        /// <summary>
        /// Mongo DB database
        /// </summary>
        internal const string DEFAULT_MONGODB_DATABASE = "DEFAULT_MONGODB_DATABASE";

        /// <summary>
        /// Indicates whether transactions are used or not (a false means yes, thereby
        /// giving the caller the responsibility of committing transactions) [0-for false, 1-for true]
        /// </summary>
        internal const string MONGODB_AUTOCOMMIT = "MONGODB_AUTOCOMMIT";
    }
}
