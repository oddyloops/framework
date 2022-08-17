using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies fields needed for all cryptographic configurations
    /// </summary>
    public interface ICryptoConfig : IConfig
    {
        /// <summary>
        /// Path to where encryption key used for assymetric cryptography is located
        /// </summary>
        string AssymetricKeyPath { get; set; }

        /// <summary>
        /// Number of bits in assymetric encryption key
        /// </summary>
        string AssymetricKeySizeBits { get; set; }
    }
}
