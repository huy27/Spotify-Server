using Application.IService;
using Application.Ultilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Application.Service
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendMail(string toEmail, string bodyRequest, string subject, List<string> filePaths)
        {
            SendMailUltilities(toEmail, bodyRequest, subject, filePaths);
        }

        public void SendMail(string toEmail, string bodyRequest, string subject)
        {
            SendMailUltilities(toEmail, bodyRequest, subject, new List<string>());
        }

        private void SendMailUltilities(string toEmail, string bodyRequest, string subject, List<string> filePaths)
        {
            try
            {
                var fromEmailAddress = _configuration["FromEmailAddress"].ToString();
                var ccEmailAddress = _configuration["CCEmailAddress"].ToString();
                var fromEmailDisplayName = _configuration["FromEmailDisplayName"].ToString();
                var fromEmailPassword = _configuration["FromEmailPassword"].ToString();
                var smtpHost = _configuration["SMTPHost"].ToString();
                var smtpPort = _configuration["SMTPPort"].ToString();

                string body = bodyRequest;
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmailAddress, fromEmailDisplayName);
                    message.To.Add(new MailAddress(toEmail));
                    message.CC.Add(new MailAddress(ccEmailAddress));
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = body;

                    foreach (var filePath in filePaths)
                    {
                        message.Attachments.Add(new Attachment(filePath));
                    }

                    var client = new SmtpClient();
                    client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
                    client.Host = smtpHost;
                    client.EnableSsl = true;
                    client.Port = int.Parse(smtpPort);
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
