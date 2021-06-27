using Framework.Enums;
using Framework.Interfaces;
using System;
using System.Composition;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Framework.Utils;
using Newtonsoft.Json;

namespace Logger.MSImpl
{
    /// <summary>
    /// A concrete implementation of the ILogger Interface
    /// </summary>
    [Export(typeof(ILogger))]
    public class Logger : ILogger
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the logger
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }


        private bool _includeTimestamp;
        private bool _includeCaller;
        private bool _autoFlush;

        /// <summary>
        /// List of listeners for this logger
        /// </summary>
        public IList<ILogListener> LogListeners { get; set; }
        public Type ClassType { get; set; }


        #region Message
        /// <summary>
        /// Helper method for formatting log messages based on options specified for this logger instance
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="caller">Name of method logging the message</param>
        /// <returns>Formatted log message</returns>
        private string BuildLogString(string message,string caller)
        {
            StringBuilder finalMsg = new StringBuilder();

            if(_includeTimestamp)
            {
                finalMsg.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\t");
            }
            if(_includeCaller)
            {
                finalMsg.Append(caller + "\t");
            }

            finalMsg.Append(message);
            if(_autoFlush)
            {
                finalMsg.Append(Environment.NewLine);
            }

            return finalMsg.ToString();
        }
        #endregion
        public Logger()
        {
            LogListeners = new List<ILogListener>();
            //true by default
            _includeTimestamp = Config.GetValue(ConfigConstants.INCLUDE_TIMESTAMP) == null ? true : Config.GetValue(ConfigConstants.INCLUDE_TIMESTAMP) == "1";
            _autoFlush = Config.GetValue(ConfigConstants.AUTO_FLUSH) == null ? true : Config.GetValue(ConfigConstants.AUTO_FLUSH) == "1";
            //false by default
            _includeCaller = Config.GetValue(ConfigConstants.INCLUDE_CALLER) == null ? false : Config.GetValue(ConfigConstants.INCLUDE_CALLER) == "1";
        }

        /// <summary>
        /// Loads listener list by scanning all ILogListener implementations within MEF container
        /// </summary>
        public void LoadListenersFromContainer()
        {
            LogListeners = Util.Container.CreateInstances<ILogListener>();
        }

        /// <summary>
        /// Helper method for writing log message to listener
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        /// <param name="level"></param>

        private void LogWriter(string message,string tag,LogLevels level)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(2).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)level))
                {
                    listener.Write(tag + ": " + BuildLogString(message, caller));
                }
            }
        }

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="message">Log message</param>
        public void Debug(string message)
        {
            LogWriter(message, "DBG", LogLevels.Debug);
        }

        /// <summary>
        /// Logs debug messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Debug(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "DBG", LogLevels.Debug);
        }

        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="message">Log message</param>
        public void Error(string message)
        {
            LogWriter(message, "ERR", LogLevels.Error);
        }


        /// <summary>
        /// Logs error messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Error(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "ERR", LogLevels.Error);
        }

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="message">Log message</param>
        public void Fatal(string message)
        {
            LogWriter(message, "FTL", LogLevels.Fatal);
        }

        /// <summary>
        /// Logs crash messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Fatal(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "FTL", LogLevels.Fatal);
        }


        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="message">Log message</param>
        public void Info(string message)
        {
            LogWriter(message, "INF", LogLevels.Info);
        }

        /// <summary>
        /// Informational Logging
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Info(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "INF", LogLevels.Info);
        }

        /// <summary>
        /// Logs message regardless of log level
        /// </summary>
        /// <param name="message">Log messaage</param>
        public void Log(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
               listener.Write("LOG: " + BuildLogString(message, caller));
               
            }
        }

        /// <summary>
        /// Logs message regardless of log level
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Log(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                listener.Write("LOG: " + BuildLogString(JsonConvert.SerializeObject(messageObj), caller));

            }
        }


        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="message">Log message</param>
        public void Warn(string message)
        {
            LogWriter(message, "WRN", LogLevels.Warn);
        }

        /// <summary>
        /// Logs warning messages
        /// </summary>
        /// <param name="messageObj">Object encapsulating log message</param>
        public void Warn(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "WRN", LogLevels.Warn);
        }

        public void Trace(string message)
        {
            LogWriter(message, "TRC", LogLevels.Trace);
        }

        public void Trace(object messageObj)
        {
            LogWriter(JsonConvert.SerializeObject(messageObj), "TRC", LogLevels.Trace);
        }
    }
}
