using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Quickrent.Model;
using Quickrent.Service.Interface;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net;
using System.Net.Sockets;

namespace Quickrent.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> options)
        {
            this.emailSettings = options.Value;
        }

        public void SendEmail(Mailrequest mailrequest)
        {
            if (mailrequest == null)
                throw new ArgumentNullException(nameof(mailrequest));

            if (string.IsNullOrWhiteSpace(emailSettings.Email))
                throw new InvalidOperationException("EmailSettings.Email is not configured.");

            if (string.IsNullOrWhiteSpace(mailrequest.Email))
                throw new ArgumentException("Recipient email address is required.", nameof(mailrequest.Email));

            var email = new MimeMessage();
            if (string.IsNullOrWhiteSpace(emailSettings.DisplayName))
            {
                email.Sender = MailboxAddress.Parse(emailSettings.Email);
            }
            else
            {
                email.Sender = MailboxAddress.Parse($"{emailSettings.DisplayName} <{emailSettings.Email}>");
            }

            email.To.Add(MailboxAddress.Parse(mailrequest.Email));
            email.Subject = mailrequest.Subject ?? string.Empty;

            var builder = new BodyBuilder();
            builder.HtmlBody = mailrequest.EmailBody ?? string.Empty;
            email.Body = builder.ToMessageBody();

            if (string.IsNullOrWhiteSpace(emailSettings.Host))
                throw new InvalidOperationException("EmailSettings.Host is not configured.");

            if (emailSettings.Port <= 0)
                throw new InvalidOperationException("EmailSettings.Port must be a valid port number.");

            using var smtp = new SmtpClient();
            try
            {
                // Pre-resolve host to provide clearer DNS errors
                try {
                    Dns.GetHostAddresses(emailSettings.Host);
                } catch (SocketException dnsEx) {
                    throw new InvalidOperationException($"Unable to resolve SMTP host '{emailSettings.Host}': {dnsEx.Message}", dnsEx);
                }

                smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSettings.Email, emailSettings.Password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
            catch (SocketException sockEx)
            {
                throw new InvalidOperationException($"Network error connecting to SMTP host '{emailSettings.Host}': {sockEx.Message}", sockEx);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}