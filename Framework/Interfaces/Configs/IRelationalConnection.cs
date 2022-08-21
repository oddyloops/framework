using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces.Configs
{
    /// <summary>
    /// Specifies additional fields needed by a relational DB Connection
    /// </summary>
    public interface IRelationalConnection : IDataConnection
    {
        /// <summary>
        /// An integer used for specifying which DB Platform
        /// </summary>
        int DBType { get; set; }
        /// <summary>
        /// Specifies default schema for connection if any
        /// </summary>
        string DefaultSchema { get; set; }
    }
}
