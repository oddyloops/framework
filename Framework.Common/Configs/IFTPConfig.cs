using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies fields needed for setting up an FTP service
    /// </summary>
    public interface IFTPConfig : IConfig
    {
        /// <summary>
        /// (S)FTP Server Address
        /// </summary>
        string FTPServerUrl { get; set; }
        /// <summary>
        /// (S)FTP Server Username
        /// </summary>
        string FTPUsername { get; set; }
        /// <summary>
        /// (S)FTP Server Password
        /// </summary>
        string FTPPassword { get; set; }


    }
}
