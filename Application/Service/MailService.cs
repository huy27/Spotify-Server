﻿using Application.IService;
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

        public void SendMail(string toEmail, string bodyRequest)
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
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmailAddress, fromEmailDisplayName);
                message.To.Add(new MailAddress(toEmail));
                message.CC.Add(new MailAddress(ccEmailAddress));
                message.Subject = "Notification";
                message.IsBodyHtml = true;
                message.Body = body;


                Attachment attachment1, attachment2;
                attachment1 = new System.Net.Mail.Attachment(FileService.GetUrl("Music.json"));
                attachment2 = new System.Net.Mail.Attachment(FileService.GetUrl("Album.json"));
                message.Attachments.Add(attachment1);
                message.Attachments.Add(attachment2);
                

                var client = new SmtpClient();
                client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
                client.Host = smtpHost;
                client.EnableSsl = true;
                client.Port = int.Parse(smtpPort);
                client.Send(message);
                attachment1.Dispose();
                attachment2.Dispose();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
