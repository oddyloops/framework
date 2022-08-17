using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Common.Impl
{
    /// <summary>
    /// A concrete implementation of the IStatus interface
    /// </summary>
    public class Status : IStatus
    {
        /// <summary>
        /// A flag indicating the result of an operation
        /// </summary>
        public bool IsSuccess { get; set; }


        /// <summary>
        /// A string containing messages related to the operational status being returned
        /// </summary>
        public string StatusMessage { get; set; }
    }

  
    public class Status<T> : IStatus<T>
    {
        /// <summary>
        /// A flag indicating the result of an operation
        /// </summary>
        public bool IsSuccess { get; set; }


        /// <summary>
        /// A string containing messages related to the operational status being returned
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Additional information regarding the operational status
        /// </summary>
        public T StatusInfo { get; set; }
    }
}
