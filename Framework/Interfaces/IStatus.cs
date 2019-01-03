using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that provides specifications for operational status implementations
    /// </summary>
    public interface IStatus
    {

        /// <summary>
        /// A flag indicating the result of an operation
        /// </summary>
        bool IsSuccess { get; set; }


        /// <summary>
        /// A string containing messages related to the operational status being returned
        /// </summary>
        string StatusMessage { get; set; }

    }

    /// <summary>
    /// An interface that provides specifications for operational status implementations
    /// </summary>
    public interface IStatus<T> : IStatus
    {
        /// <summary>
        /// Additional information regarding the operational status
        /// </summary>
        T StatusInfo { get; set; }
    }
}
