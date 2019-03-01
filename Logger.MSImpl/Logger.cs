using Framework.Enums;
using Framework.Interfaces;
using System;
using System.Composition;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Logger.MSImpl
{
    [Export(typeof(ILogger))]
    public class Logger : ILogger
    {
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }


        private bool _includeTimestamp;
        private bool _includeCaller;
        private bool _autoFlush;

        public IList<ILogListener> LogListeners { get; set; }


        #region Message
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
    

        public void Debug(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach(var listener in LogListeners)
            {
                if (((int)listener.LogLevel) >= ((int)LogLevels.Debug))
                {
                    listener.Write("DBG: " + BuildLogString(message,caller));
                }
            }
        }

        public void Debug(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) >= ((int)LogLevels.Debug))
                {
                    listener.Write("DBG: " + BuildLogString(messageObj.ToString(), caller));
                }
            }
        }

        public void Error(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Error))
                {
                    listener.Write("ERR: " + BuildLogString(message, caller));
                }
            }
        }

        public void Error(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Error))
                {
                    listener.Write("ERR: " + BuildLogString(messageObj.ToString(), caller));
                }
            }
        }

        public void Fatal(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Fatal))
                {
                    listener.Write("FTL: " + BuildLogString(message, caller));
                }
            }
        }

        public void Fatal(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Fatal))
                {
                    listener.Write("FTL: " + BuildLogString(messageObj.ToString(), caller));
                }
            }
        }

        public void Info(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Info))
                {
                    listener.Write("INF: " + BuildLogString(message, caller));
                }
            }
        }

        public void Info(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Info))
                {
                    listener.Write("INF: " + BuildLogString(messageObj.ToString(), caller));
                }
            }
        }

        public void Log(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
               listener.Write("LOG: " + BuildLogString(message, caller));
               
            }
        }

        public void Log(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                listener.Write("LOG: " + BuildLogString(messageObj.ToString(), caller));

            }
        }

        public void Warn(string message)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Warning))
                {
                    listener.Write("WRN: " + BuildLogString(message, caller));
                }
            }
        }

        public void Warn(object messageObj)
        {
            StackTrace stackTrace = new StackTrace();
            string caller = stackTrace.GetFrame(1).GetMethod().Name;
            foreach (var listener in LogListeners)
            {
                if (((int)listener.LogLevel) <= ((int)LogLevels.Warning))
                {
                    listener.Write("WRN: " + BuildLogString(messageObj.ToString(), caller));
                }
            }
        }
    }
}
