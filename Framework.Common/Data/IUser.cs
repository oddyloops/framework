using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Common.Data
{
    /// <summary>
    /// An interface that specifies the basic requirements for a user record type
    /// </summary>
    public interface IUser
    {
     
        Guid Id { get; set; }

        string UserName { get; set; }

        byte[] Password { get; set; }

        byte[] Salt { get; set; }
      
        string Email { get; set; }
      
        int Status { get; set; }

        byte[] RecoveryHash { get; set; }
    }
}
