using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Dnu.Batch.Utilities
{
    public class MailUtils
    {
        private string smtpServer;
        private int smtpPort;
        private string smtpUser;
        private string smtpPass;
        private string smtpEmail;

        public MailUtils()
        {
            var section = ConfigurationManager.GetSection("BatchBonificacion") as NameValueCollection;

            smtpServer = section["SMTPServer"].ToString();
            smtpPort = int.Parse(section["SmtpPort"].ToString());
            smtpUser = section["SmtpUser"].ToString();
            smtpPass = section["SmtpPass"].ToString();
            smtpEmail = section["SmtpEmail"].ToString();
        }
        public void SendMail(string ToMail, string subject, string body, bool isHtml)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(smtpServer);

                mail.From = new MailAddress(smtpEmail);
                mail.To.Add(ToMail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;
                SmtpServer.Port = smtpPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                SmtpServer.EnableSsl = false;

                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
    }
}
