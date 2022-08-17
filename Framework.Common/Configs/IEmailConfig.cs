using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Configs
{
    /// <summary>
    /// Specifies fields needed for setting up an email service
    /// </summary>
    public interface IEmailConfig : IConfig
    {
        /// <summary>
        /// The username part of the email credential
        /// </summary>
        string Username { get; set; }
        /// <summary>
        /// The password part of the email credential
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Path to the encrypted credential file (if-any)
        /// </summary>
        string EncryptedCredentialPath { get; set; }
        /// <summary>
        /// Path to encryption key in key-store
        /// </summary>
        string EncryptionKeyPath { get; set; }
        /// <summary>
        /// URL for SMTP Mail Servr 
        /// </summary>
        string MailServer { get; set; }
        /// <summary>
        /// Port number for mail server
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// What name to display at the sender field
        /// </summary>
        string DisplayName { get; set; }



    }
}
