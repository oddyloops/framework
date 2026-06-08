namespace Framework.Interfaces
{
    /// <summary>
    /// A specialized data context for client based data sources 
    /// </summary>
    public interface IClientDataContext : IDataContext
    {
        /// <summary>
        /// Returns the client instance used to access the data source
        /// </summary>
        object Client { get; }
    }
}
