using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Common.Aspects
{
    /// <summary>
    /// A specification for aspect classes used for logging
    /// </summary>
    public interface ILoggerAspect
    {
        /// <summary>
        /// Logger
        /// </summary>
        ILogger Logger { get; set; }
    }
}
