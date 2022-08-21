using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces.Configs
{
    /// <summary>
    /// Specifies additional fields required by document-style databases
    /// </summary>
    public interface IDocumentConnection : IDataConnection
    {
        /// <summary>
        /// The name of the database
        /// </summary>
        public string DbName { get; set; }
    }
}
