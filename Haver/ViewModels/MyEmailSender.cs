using Haver.ViewModels;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Org.BouncyCastle.Crypto.Modes;

namespace Haver.Utilities
{
    public class MyEmailSender : IMyEmailSender
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public MyEmailSender(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public async Task SendOneAsync(string name, string email, string subject, string htmlMessage)
        {
            try
            {
                if (String.IsNullOrEmpty(name))
                {
                    name = email;
                }
                var message = new MimeMessage();
                message.To.Add(new MailboxAddress(name, email));
                message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUserName));

                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = htmlMessage
                };

                using var emailClient = new SmtpClient();
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_emailConfiguration.SmtpUserName, _emailConfiguration.SmtpPassword);
                await emailClient.SendAsync(message);
                emailClient.Disconnect(true);
            }
            catch(Exception)
            {

            }
        }

        public async Task SendToManyAsync(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.Add(new MailboxAddress(_emailConfiguration.SmtpFromName, _emailConfiguration.SmtpUserName));

            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            using var emailClient = new SmtpClient();
            emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);
            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            emailClient.Authenticate(_emailConfiguration.SmtpUserName, _emailConfiguration.SmtpPassword);
            await emailClient.SendAsync(message);
            emailClient.Disconnect(true);
        }
    }
}
