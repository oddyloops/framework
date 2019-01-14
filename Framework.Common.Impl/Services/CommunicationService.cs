
using System.Collections.Generic;
using System.Text;
using System.Composition;

using System.Threading.Tasks;
using System.IO;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Framework.Common.Services;
using Framework.Interfaces;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A concrete implementation for the ICommunicationService interface 
    /// </summary>
    [Export(typeof(ICommunicationService))]
    public class CommunicationService : ICommunicationService
    {
        /// <summary>
        /// A reference to the configuration component for accessing setting values
        /// </summary>
        [Import("JsonConfig")]
        public IConfiguration Config { get; set; }


        /// <summary>
        /// A reference to a security component for encrypting/decrypting email credentials
        /// </summary>
        [Import]
        public ISecurityService SecurityService { get; set; }

        /// <summary>
        /// A reference to a key store component for retrieving keys used in encryption/decryption
        /// </summary>
        [Import]
        public IKeyStoreService KeyStoreService { get; set; }

        /// <summary>
        /// Sends out an email to a single recipient within a non-blocking context
        /// </summary>
        /// <param name="sender">Sender alias</param>
        /// <param name="recipient">Recipient email</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email content</param>
        /// <returns>A callback handle providing access to the status code indicating the result of the operation</returns>
        public async Task<IStatus> SendEmailAsync(string sender, string recipient, string subject, string body)
        {
            return await SendEmailAsync(sender, new List<string>() { recipient }, null, null, subject, body);
        }


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
        public async Task<IStatus> SendEmailAsync(string sender, IList<string> recipients, IList<string> ccs, IList<string> bccs, string subject, string body)
        {

            byte[] encryptedCredentials = File.ReadAllBytes(Config.GetValue(ConfigConstants.SEND_GRID_ENCRYPTED));
            byte[] symmKey = KeyStoreService.GetKey(Config.GetValue(ConfigConstants.SYMMETRIC_KEY_INDEX));
            string apiKey = Encoding.UTF8.GetString(SecurityService.Decrypt(encryptedCredentials, symmKey));

            List<EmailAddress> receivers = (from r in recipients select new EmailAddress(r)).ToList();

            List<EmailAddress> copies = (ccs == null ? new List<EmailAddress>() :
                (from c in ccs select new EmailAddress(c)).ToList());
            List<EmailAddress> blindCopies = (bccs == null ? new List<EmailAddress>() :
                (from b in bccs select new EmailAddress(b)).ToList());

            EmailAddress senderAddress = new EmailAddress(sender);

            SendGridMessage message = new SendGridMessage();
            message.Subject = subject;
            message.SetFrom(senderAddress);
            message.AddTos(receivers);
            message.AddCcs(copies);
            message.AddBccs(blindCopies);
            message.AddContent(MimeType.Html, body);

            SendGridClient client = new SendGridClient(apiKey);
            await client.SendEmailAsync(message);
            return new Status() { IsSuccess = true, StatusMessage = "Sent" };

        }
    }
}
