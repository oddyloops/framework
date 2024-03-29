﻿using Framework.Enums;
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
        /// Type of class consuming logger
        /// </summary>
        Type ClassType { get; set; }

        /// <summary>
        /// List of listeners for this logger
        /// </summary>
        IList<ILogListener> LogListeners { get; set; }

        /// <summary>
        /// Loads listener list by scanning all ILogListener implementations within MEF container
        /// </summary>
        void LoadListenersFromContainer();

        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="message">Log message</param>
        void Info(string message);

        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Info(object messageObj);

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Debug(string message);

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Debug(object messageObj);

        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Warn(string message);

        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Warn(object messageObj);

        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Error(string message);

        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Error(object messageObj);

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="message">Log message</param>
        void Fatal(string message);

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Fatal(object messageObj);

        /// <summary>
        /// Logs trace messages
        /// </summary>
        /// <param name="message">Message string</param>
        void Trace(string message);

        /// <summary>
        /// Logs trace messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Trace(object messageObj);

        /// <summary>
        /// Logs message regardless of log level
        /// </summary>
        /// <param name="message">Log messaage</param>
        void Log(string message);

        /// <summary>
        /// Logs message regardless of log level
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        void Log(object messageObj);
    }
}
