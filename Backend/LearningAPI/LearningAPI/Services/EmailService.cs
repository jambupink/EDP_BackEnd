using System.Net.Mail;
using System.Net;

namespace LearningAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendConfirmationEmail(string email, string confirmationLink)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
            using var smtpClient = new SmtpClient(smtpSettings.Host)
            {
                Port = smtpSettings.Port,
                Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                EnableSsl = smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.FromEmail),
                Subject = "Confirm your email",
                Body = $"Please confirm your email by clicking this link: {confirmationLink}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
        }
    }
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }

}
