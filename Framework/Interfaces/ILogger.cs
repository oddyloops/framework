using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface specifying the requirements for a logger implementation 
    /// </summary>
    public interface ILogger
    {

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the logger
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="message">Log message</param>
        void Info(string message);

        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Info(object messageObj);

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Debug(string message);

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Debug(object messageObj);

        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Warn(string message);

        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Warn(object messageObj);

        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Error(string message);

        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Error(object messageObj);

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Fatal(string message);

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Fatal(object messageObj);

        /// <summary>
        /// Logs message regardless of log level, except OFF
        /// </summary>
        /// <param name="message">Log messaage</param>
        void Log(string message);

        /// <summary>
        /// Logs message regardless of log level, except OFF
        /// </summary>
        /// <param name="messageObj">Object encapsualting log message</param>
        void Log(object messageObj);
    }
}
