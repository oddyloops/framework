using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Services
{
    /// <summary>
    /// An interface that specifies the contract to be 
    /// fulfiiled by an authentication service
    /// </summary>
    public interface IApiAuthService : IService
    {
        /// <summary>
        /// Creates a new record for an client application
        /// </summary>
        /// <param name="email">Email of the app owner</param>
        /// <param name="password">Password to log in to their account</param>
        /// <param name="appName">Name of app</param>
        /// <param name="type">0-IOS,1-Android,2-Web,3-Others</param>
        /// <returns>Operation Status embedding AppId and Key Pair in Json</returns>
        Task<IStatus<string>> AddNewAppAsync(string email, string password, string appName, short type);

        /// <summary>
        /// Validates credentials for logging in to API account
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="password">Password</param>
        /// <returns>AppId, Public Key in Json</returns>
        Task<IStatus<string>> ValidateCredentialsAsync(string email, string password);

        /// <summary>
        /// Verifies app for test environment
        /// </summary>
        /// <param name="appId">App Id</param>
        /// <returns>Operation Status</returns>
        Task<IStatus<int>> VerifyEmailForTestAsync(Guid appId);

        /// <summary>
        /// Verifies app for production environment
        /// </summary>
        /// <param name="appId">App Id</param>
        /// <returns>Operation Status</returns>
        Task<IStatus<int>> VerifyForProductionAsync(Guid appId);


        /// <summary>
        /// Resets API account password
        /// </summary>
        /// <param name="appId">App Id</param>
        /// <param name="newPassword">New Password</param>
        /// <returns>Operation Status</returns>
        Task<IStatus<int>> ResetPasswordAsync(Guid appId, string newPassword);

        /// <summary>
        /// Retrieves App Id based on API account email
        /// </summary>
        /// <param name="email">API Account Email</param>
        /// <returns>Corresponding App Id</returns>
        Task<Guid> GetAppIdByEmailAsync(string email);

        /// <summary>
        /// Closes API Account
        /// </summary>
        /// <param name="appId">App Id</param>
        /// <returns>Operation Status</returns>
        Task<IStatus<int>> CloseApiAccountAsync(Guid appId);

        /// <summary>
        /// Generates a token
        /// </summary>
        /// <param name="appId">The ID of the app being authenticated</param>
        /// <param name="appSignature">A digital signature created by app private key</param>
        /// <returns>A token string</returns>
        Task<IStatus<string>> GenerateTokenAsync(Guid appId,string appSignature);


        /// <summary>
        /// Checks the authenticity and integrity of a token
        /// </summary>
        /// <param name="token">Token string</param>
        /// <returns>Is token valid?</returns>
        bool ValidateTokenSignature(string token);

        /// <summary>
        /// Checks the authenticity and integrity of a 
        /// one-time secure token allowing for a time window
        /// </summary>
        /// <param name="token">Token string</param>
        /// <param name="timeWindowSecs">Time window in seconds for validity of token</param>
        /// <returns>Is token valid?</returns>
        bool ValidateOnetimeToken(string token,int timeWindowSecs);
    }
}
