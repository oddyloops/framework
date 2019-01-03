using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{

    /// <summary>
    /// An interface that is required by services accessing a data provider
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// A handle to the data provider
        /// </summary>
        IDataContext DataContext { get; set; }
    }
}
