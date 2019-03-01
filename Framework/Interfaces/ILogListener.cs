

using Framework.Enums;

namespace Framework.Interfaces
{
    /// <summary>
    /// Provides specifications required for a logger lister
    /// </summary>
    public interface ILogListener
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the listener
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Writes a string to the listener's source
        /// </summary>
        /// <param name="message"></param>
        void Write(string message);

        /// <summary>
        /// Log level for listener
        /// </summary>
        LogLevels LogLevel { get; set; }

       
    }
}
