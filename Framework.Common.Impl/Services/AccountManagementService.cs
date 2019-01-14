using System;
using System.Composition;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Framework.Common.Services;
using Framework.Interfaces;
using Framework.Common.Data;
using Framework.Enums;
using Framework.Utils;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A concrete implementation of the IAccountManagementService
    /// </summary>
    [Export(typeof(IAccountManagementService))]
    public class AccountManagementService : IAccountManagementService
    {
        /// <summary>
        /// A reference to the component for accessing the data source
        /// </summary>
        [Import("SqlAzureContext")]
        public IDataContext DataContext { get; set; }

        /// <summary>
        /// A reference to the component for data protection
        /// </summary>
        [Import]
        public ISecurityService SecurityService { get; set; }

        /// <summary>
        /// A reference to the component for communication services
        /// </summary>
        [Import]
        public ICommunicationService CommunicationService { get; set; }

        /// <summary>
        /// A reference to the component for accessing configuration values
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }

        /// <summary>
        /// A helper method for initiating password recovery
        /// </summary>
        /// <param name="user">User account whose password is being recovered</param>
        /// <returns>A completion token encapsulating the status indicating result of the operation</returns>
        #region Helpers
        private async Task<IStatus> RecoveryHelperAsync(IUser user)
        {

            byte[] hash = SecurityService.Hash(user.Id.ToByteArray());
            user.RecoveryHash = hash;

            await DataContext.UpdateAsync(user, true);

            string linkUrl = ConfigConstants.RECOVERY_LINK_PREFIX + Convert.ToBase64String(hash);
            IDictionary<string, string> templatePair = new Dictionary<string, string>();
            templatePair.Add("#{link}", linkUrl);

            string emailBody = Util.BuildStringTemplateFromFile(Config.GetValue(ConfigConstants.RECOVERY_MAIL_TEMPLATE_PATH), templatePair);
            string emailSubject = Config.GetValue(ConfigConstants.RECOVERY_MAIL_SUBJECT);
            string emailSender = Config.GetValue(ConfigConstants.RECOVERY_SENDER_ALIAS);

            return await CommunicationService.SendEmailAsync(emailSender, user.Email, emailSubject, emailBody);
        }
        #endregion

        /// <summary>
        /// Permanently closes a user account within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus> CloseUserAccountAsync(IUser user)
        {
            user.Status = (int)DataStatus.Closed;
            var result = await DataContext.UpdateAsync(user, true);
            var status = Util.Container.CreateInstance<IStatus>();
            status.IsSuccess = result.IsSuccess;
            return status;

        }

        /// <summary>
        /// Creates a new user account for the application within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus<int>> CreateUserAccountAsync(IUser user)
        {
            var status = Util.Container.CreateInstance<IStatus<int>>();
            var existing = await DataContext.SelectMatchingAsync<IUser>(u => u.UserName.ToLower() == user.UserName.ToLower() || u.Email.ToLower() == user.Email.ToLower());
           
            if (existing != null)
            {
                status.IsSuccess = false;
                status.StatusMessage = "User already exists";
                status.StatusInfo = (int)DataStatus.Exists;
                return status;
            }
            user.Status = (int)DataStatus.NeedsVerification; //email needs to be verified


            byte[] salt = null;
            byte[] saltedPwd = SecurityService.Salt(user.Password, out salt);
            byte[] hash = SecurityService.Hash(saltedPwd);
            user.Password = hash; //hash password before storage
            user.Salt = salt;
            var result  = await DataContext.InsertAsync(user);
            if(result.StatusInfo > 0)
            {
                status.IsSuccess = true;
                status.StatusInfo = (int)DataStatus.OK;
                status.StatusMessage = "Successful";
            }
            return status;
        }

        /// <summary>
        /// Initiates password recovery by email within a non-blocking context
        /// </summary>
        /// <param name="email">Account email</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus> PasswordRecoveryByEmailAsync(string email)
        {
           
            var users = (await DataContext.SelectMatchingAsync<IUser>(u => u.Email.ToLower() == email.ToLower())).ToList();
            if (users == null || users.Count != 1)
            {
                var status = Util.Container.CreateInstance<IStatus>();
                status.IsSuccess = false;
                status.StatusMessage = "Email not found";
                return status;
            }
            IUser user = users[0];
            return await RecoveryHelperAsync(user);
        }


        /// <summary>
        /// Initiates password recovery by username within a non-blocking context
        /// </summary>
        /// <param name="username">Account username</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus> PasswordRecoveryByUsernameAsync(string username)
        {
            var users = (await DataContext.SelectMatchingAsync<IUser>(u => u.UserName.ToLower() == username.ToLower())).ToList();
            if (users == null || users.Count != 1)
            {
                var status = Util.Container.CreateInstance<IStatus>();
                status.IsSuccess = false;
                status.StatusMessage = "Username not found";
                return status;
            }
            IUser user = users[0];
            return await RecoveryHelperAsync(user);
        }

        /// <summary>
        /// Reopens a previously suspended user account within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus<int>> ReopenUserAccountAsync(IUser user)
        {
            var status = Util.Container.CreateInstance<IStatus<int>>();
            if (user.Status != (int)DataStatus.Suspended)
            {
                status.IsSuccess = false;
                status.StatusInfo = (int)DataStatus.InvalidOp;
                status.StatusMessage = "User account is not currently suspended";
                return status;
            }
            user.Status = (int)DataStatus.OK;
            var result = await DataContext.UpdateAsync(user, true);
            if(result.StatusInfo > 0)
            {
                status.IsSuccess = true;
                status.StatusInfo = (int)DataStatus.OK;
                status.StatusMessage = "Successful";
            }
            else
            {
                status.IsSuccess = false;
                status.StatusInfo = (int)DataStatus.NotFound;
                status.StatusMessage = "User account not found";
            }
            return status;
        }

        /// <summary>
        /// Resets password for user account  within a non-blocking context
        /// </summary>
        /// <param name="user">User account containing new password</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus<int>> ResetPasswordAsync(IUser user)
        {
            var status = Util.Container.CreateInstance<IStatus<int>>();
            byte[] salt = null;
            byte[] saltedPwd = SecurityService.Salt(user.Password, out salt);
            byte[] hash = SecurityService.Hash(saltedPwd);
            

            user.Password = hash;
            user.Salt = salt;
            var result = await DataContext.UpdateAsync(user, true);
            if(result.StatusInfo > 0)
            {
                status.IsSuccess = true;
                status.StatusInfo = (int)DataStatus.OK;
                status.StatusMessage = "Successful";
            }
            else
            {
                status.IsSuccess = false;
                status.StatusInfo = (int)DataStatus.NotFound;
                status.StatusMessage = "User account not found";
            }
            return status;

        }

        /// <summary>
        /// Suspends user account until it is reopened within a non-blocking context
        /// </summary>
        /// <param name="user">User account details</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus<int>> SuspendUserAccountAsync(IUser user)
        {
            var status = Util.Container.CreateInstance<IStatus<int>>();
            user.Status = (int)DataStatus.Suspended;
            var result = await DataContext.UpdateAsync(user, true);
            if (result.StatusInfo > 0)
            {
                status.IsSuccess = true;
                status.StatusInfo = (int)DataStatus.OK;
                status.StatusMessage = "Successful";
            }
            else
            {
                status.IsSuccess = false;
                status.StatusInfo = (int)DataStatus.NotFound;
                status.StatusMessage = "User account not found";
            }
            return status;
        }
    }
}
