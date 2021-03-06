﻿
using Framework.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Common.Services
{
    /// <summary>
    /// Contracts that all communication services must satisfy
    /// </summary>
    public interface ICommunicationService : IService
    {

        /// <summary>
        /// A reference to a configuration component used to access config settings required by the communication service
        /// </summary>
        IConfiguration Config { get; set; }

        /// <summary>
        /// Sends out an email to a single recipient within a non-blocking context
        /// </summary>
        /// <param name="sender">Sender alias</param>
        /// <param name="recipient">Recipient email</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email content</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus> SendEmailAsync(string sender,string recipient, string subject, string body);

        /// <summary>
        /// Sends out an email to multiple recipients, with a list of copies and blind copies within a non-blocking context
        /// </summary>
        /// <param name="sender">Sender alias</param>
        /// <param name="recipients">Primary email recipients</param>
        /// <param name="ccs">Copy emails</param>
        /// <param name="bccs">Blind copy emails</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email content</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        Task<IStatus> SendEmailAsync(string sender, IList<string> recipients, IList<string> ccs, IList<string> bccs, string subject, string body);
    }
}
