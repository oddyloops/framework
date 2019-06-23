using Framework.Enums;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;

namespace Logger.MSImpl
{
    /// <summary>
    /// An implementation of ILogListener for logging to the console
    /// </summary>
    [Export("Console",typeof(ILogListener))]
    public class ConsoleLogListener : ILogListener
    {
        /// <summary>
        /// A reference to a configuration component used to access config settings required by the listener
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        /// <summary>
        /// Log level for listener
        /// </summary>
        public LogLevels LogLevel { get; set; }

        public ConsoleLogListener()
        {
            //check if configuration for log level exists
            string logLevelConf = Config.GetValue(ConfigConstants.CONSOLE_LOG_LEVEL);
            if (logLevelConf == null)
            {
                //debug by default
                LogLevel = LogLevels.Debug;
            }
            else
            {
                try
                {
                    LogLevel = (LogLevels)Enum.Parse(typeof(LogLevels), logLevelConf);
                }
                catch(Exception)
                {
                    throw new Exception("Valid log levels are None, Warn, Debug, Info, Fatal, and Error");
                }
            }
        }

        /// <summary>
        /// Writes a string to the listener's source
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            Console.Write(message);
        }
    }
}
