using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Services
{
    /// <summary>
    /// A service used for user authentication
    /// </summary>
    public interface IUserAuthService : IService
    {
        /// <summary>
        /// Creates a new user credential record
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="password">Password</param>
        /// <returns>Operation result</returns>
        Task<IStatus<int>> AddNewUserAsync(Guid id,string password);

        /// <summary>
        /// Validates the correctness of user password
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="password">Password</param>
        /// <returns>Password.IsValid flag</returns>
        Task<bool> ValidatePasswordAsync(Guid id, string password);

        /// <summary>
        /// Resets password
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="newPassword">New Password</param>
        /// <returns>Operation result</returns>
        Task<IStatus<int>> ResetPasswordAsync(Guid id, string newPassword);

        /// <summary>
        /// Deletes user credentials
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Operation result</returns>
        Task<IStatus<int>> DeleteUserAsync(Guid id);

        /// <summary>
        /// Generates token which user can authenticate subsequent requests 
        /// to application
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>Token</returns>
        string AcquireToken(Guid id);

        /// <summary>
        /// Verifies the authenticity of user token
        /// </summary>
        /// <param name="token">Token string</param>
        /// <returns>Token.IsValid flag</returns>
        bool VerifyToken(string token);
    }
}
