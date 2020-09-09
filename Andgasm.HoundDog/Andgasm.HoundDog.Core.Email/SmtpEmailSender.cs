using Andgasm.HoundDog.Core.Email.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.Core.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private const string SmtpPortConfigName = "SmtpConfig:SmtpPortConfigName";
        private const string SmtpHostConfigName = "SmtpConfig:SmtpHostConfigName";
        private const string SmtpUseSSLConfigName = "SmtpConfig:SmtpUseSSLConfigName";
        private const string SmtpHostAccountNameConfigName = "SmtpConfig:SmtpHostAccountNameConfigName";
        private const string SmtpHostAccountPasswordConfigName = "SmtpConfig:SmtpHostAccountPasswordConfigName";

        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;

        private readonly SmtpClient _client;

        public SmtpEmailSender(IConfiguration config, 
                               ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;

            _client = new SmtpClient()
            {
                Port = Convert.ToInt32(_config.GetSection(SmtpPortConfigName)?.Value),
                EnableSsl = Convert.ToBoolean(_config.GetSection(SmtpUseSSLConfigName)?.Value),
                Host = _config.GetSection(SmtpHostConfigName)?.Value,

                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_config.GetSection(SmtpHostAccountNameConfigName)?.Value,
                                                    _config.GetSection(SmtpHostAccountPasswordConfigName)?.Value)
            };
        }

        public async Task SendEmailAsync(string from, string to, string subject, string body, bool fireandforget)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body
            };
            mail.IsBodyHtml = true;
            mail.To.Add(new MailAddress(to));

            if (fireandforget) _client.SendAsync(mail, null);
            else await _client.SendMailAsync(mail);
        }
    }
}
