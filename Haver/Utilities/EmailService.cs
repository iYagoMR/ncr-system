using Haver.ViewModels;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Haver.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IEmailConfiguration emailConfiguration, ILogger<EmailSender> logger)
        {
            _emailConfiguration = emailConfiguration;
            _logger = logger;
        }

        /// <summary>
        /// Asynchronously builds and sends a message to a single recipient
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns></returns>

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(email, email));
            message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUserName));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };
            try
            {
                using var emailClient = new SmtpClient();

                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_emailConfiguration.SmtpUserName, _emailConfiguration.SmtpPassword);

                await emailClient.SendAsync(message);
                emailClient.Disconnect(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException().Message);    
            }
        }
    }
}
