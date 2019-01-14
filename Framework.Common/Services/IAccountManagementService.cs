using Framework.Interfaces;
using Framework.Common.Data;
using System.Threading.Tasks;


namespace Framework.Common.Services
{
    /// <summary>
    /// Contracts that all account management services must satisfy
    /// </summary>
    public interface IAccountManagementService : IDataService
    {
       
        

        /// <summary>
        /// Creates a new user account for the application within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus<int>> CreateUserAccountAsync(IUser user);


        /// <summary>
        /// Permanently closes a user account within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus> CloseUserAccountAsync(IUser user);


        /// <summary>
        /// Suspends user account until it is reopened within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus<int>> SuspendUserAccountAsync(IUser user);

        /// <summary>
        /// Reopens a previously suspended user account within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus<int>> ReopenUserAccountAsync(IUser user);

        /// <summary>
        /// Initiates password recovery by username within a non-blocking context
        /// </summary>
        /// <param name="username">Account username</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus> PasswordRecoveryByUsernameAsync(string username);

        /// <summary>
        /// Initiates password recovery by email within a non-blocking context
        /// </summary>
        /// <param name="email">Account email</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus> PasswordRecoveryByEmailAsync(string email);

        /// <summary>
        /// Resets password for user account  within a non-blocking context
        /// </summary>
        /// <param name="user">User account containing new password</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus<int>> ResetPasswordAsync(IUser user);

    }
}
