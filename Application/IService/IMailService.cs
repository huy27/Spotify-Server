using System;
using System.Collections.Generic;
using System.Text;

namespace Application.IService
{
    public interface IMailService
    {
        void SendMail(string toEmail, string bodyRequest, string subject);
        void SendMail(string toEmail, string bodyRequest, string subject, List<string> filePaths);
    }
}
