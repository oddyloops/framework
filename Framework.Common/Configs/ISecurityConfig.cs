using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies the fields needed to configure a security service
    /// </summary>
    public interface ISecurityConfig : ICryptoConfig
    {
        /// <summary>
        /// The block size in bytes for symmetric encryption algorithm
        /// </summary>
        string EncryptionBlockSizeBytes { get; set; }
    }
}
