namespace DataContext.NoSql.DocumentDB
{
    internal struct ConfigConstants
    {
        /// <summary>
        /// Connection string to document db instance
        /// </summary>
        internal const string DOCUMENT_DB_ENDPOINT = "DOCUMENT_DB_ENDPOINT";

        /// <summary>
        /// Authorization key used to access document db instance
        /// </summary>
        internal const string DOCUMENT_DB_AUTH_KEY = "DOCUMENT_DB_AUTH_KEY";

        /// <summary>
        /// Document database used by default
        /// </summary>
        internal const string DOCUMENT_DB_DATABASE = "DOCUMENT_DB_DATABASE";
    }
}
