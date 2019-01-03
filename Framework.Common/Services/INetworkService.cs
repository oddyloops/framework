using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Common.Services
{
    /// <summary>
    /// An interface that specifies the requirements for a networking framework implementation
    /// </summary>
    public interface INetworkService : IService
    {
            IConfiguration Config { get; set; }
    }
}
