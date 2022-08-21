
namespace Framework.Interfaces.Configs
{
    /// <summary>
    /// Specifies the fields needed for establishing a connection with a database
    /// </summary>
    public interface IDataConnection : IConfig
    {
        /// <summary>
        /// DB Connection String
        /// </summary>
        string ConnectionString { get; set; }
        /// <summary>
        /// DB Username
        /// </summary>
        string Username { get; set; }
        /// <summary>
        /// DB Password
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Connection timeout
        /// </summary>
        int TimeOut { get; set; }
        /// <summary>
        /// Determine if transactions are enabled
        /// </summary>
        bool IsAutoCommit { get; set; }
    }
}
