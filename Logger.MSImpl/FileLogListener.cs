using Framework.Enums;
using Framework.Interfaces;
using System;
using System.Composition;
using System.IO;
using System.Text;

namespace Logger.MSImpl
{
    [Export(typeof(ILogger))]
    public class FileLogListener : ILogListener
    {
        private bool _isRolling;
        private uint _maxRollingSizeBytes;
        private string _logPath;
        private string _filePrefix;

        private const uint DEFAULT_ROLL_SIZE = 1024 * 1024; // 1MB
        private uint _currRollFileSize;
        private string _currFile;

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the listener
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        /// <summary>
        /// Log level for listener
        /// </summary>
        public LogLevels LogLevel { get; set; }


        #region Helpers
        /// <summary>
        /// Helper method for creating a new log file according to configuration specifications
        /// </summary>
        private void CreateNewLogFile()
        {
            string path = $"{_logPath}\\{_filePrefix}";

            if(_isRolling)
            {
                path = $"{path}{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.log";
                File.Create(path).Close();
                _currRollFileSize = 0;
            }
            else
            {
                path = $"{path}.log";
                if(!File.Exists(path))
                {
                    File.Create(path).Close();
                }
            }

            _currFile = path;
        }
        #endregion

        public FileLogListener()
        {
            string isRollingConf = Config.GetValue(ConfigConstants.IS_ROLLING_FILE);
            _isRolling = (isRollingConf == null) ? false : isRollingConf == "1";

            string maxRollSizeConf = Config.GetValue(ConfigConstants.MAX_ROLL_FILE_SIZE);
            _maxRollingSizeBytes = (maxRollSizeConf == null) ? DEFAULT_ROLL_SIZE : Convert.ToUInt32(maxRollSizeConf);

            string logPathConf = Config.GetValue(ConfigConstants.LOG_FOLDER_PATH);
            _logPath = (logPathConf == null) ? Directory.GetCurrentDirectory() : logPathConf;

            _filePrefix = AppDomain.CurrentDomain.FriendlyName;

            CreateNewLogFile();

            
        }

        /// <summary>
        /// Writes a string to the listener's source
        /// </summary>
        /// <param name="message">Log message</param>
        public void Write(string message)
        {
            using (FileStream fs = File.OpenWrite(_currFile))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                fs.Seek(0, SeekOrigin.End);
                fs.Write(messageBytes, 0, messageBytes.Length);
                if (_isRolling)
                {
                    _currRollFileSize += (uint)fs.Length;

                    if (_currRollFileSize >= _maxRollingSizeBytes)
                    {
                        CreateNewLogFile();
                    }
                }
            }
        }
    }
}
