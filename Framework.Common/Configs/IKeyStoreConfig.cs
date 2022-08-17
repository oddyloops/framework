using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies fields needed to configure a cryptographic key store
    /// </summary>
    public interface IKeyStoreConfig : ICryptoConfig
    {
        /// <summary>
        /// Path to where key store cache is located
        /// </summary>
        string KeyStorePath { get; set; }
    }
}
