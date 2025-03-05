using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace CommonProcesador.Utilidades
{
    public class LNEmail
    {
        static SmtpClient clienteSmtp = null;

        static LNEmail()
        {
            clienteSmtp = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], Int16.Parse(ConfigurationManager.AppSettings["SMTPPort"]));

            //if (ConfigurationManager.AppSettings["SMTPUser"].Contains('@'))
            //{
                    clienteSmtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], ConfigurationManager.AppSettings["SMTPPass"]);
            //}
            //else
            //{
            //     clienteSmtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"] + "@" + ConfigurationManager.AppSettings["SMTPServer"].Replace("mail.",""), ConfigurationManager.AppSettings["SMTPPass"]);
            //}
        }

        public LNEmail()
        {

        }

        public static int Send(string strTo, string strMessage, string strSubject, Attachment Atach)
        {

            try
            {

                MailMessage msg = new MailMessage();

                string[] listaDist = strTo.Split(';');

                if (listaDist.Length == 0)
                {
                    Logueo.Error("Lista de distribucion vacia");
                    return 0;
                }

                foreach (String toSend in listaDist)
                {
                    msg.To.Add(new MailAddress(toSend));
                }


              //  msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"]);

                if (ConfigurationManager.AppSettings["SMTPUser"].Contains('@'))
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"]);
                }
                else
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"] + "@" + ConfigurationManager.AppSettings["SMTPServer"].Replace("mail.", ""));
                }
                msg.Subject = strSubject;
                msg.IsBodyHtml = true;
                msg.Body = strMessage;
                msg.Attachments.Add(Atach);

                clienteSmtp.Send(msg);
                return 0;

            }
            catch (Exception Ex)
            {


            }
            return 5121;

        }
        /*
        public static int Send(string strTo, string strMessage, string strSubject)
        {

            try
            {
                
               MailMessage msg = new MailMessage();

                string[] listaDist = strTo.Split(';');

                if (listaDist.Length == 0)
                {
                    Logueo.Error("Lista de distribucion vacia");
                    return 0;
                }

                foreach (String toSend in listaDist)
                {
                    try
                    {
                        msg.To.Add(new MailAddress(toSend));
                    }
                    catch
                    {
                    }
                }

                if (ConfigurationManager.AppSettings["SMTPUser"].Contains('@'))
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"]);
                }
                else
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"] + "@" + ConfigurationManager.AppSettings["SMTPServer"].Replace("mail.",""));
                }

                msg.Subject = strSubject;
                msg.IsBodyHtml = true;
                msg.Body = strMessage;
                                
                clienteSmtp.Send(msg);

                return 0;

            }
            catch (Exception Ex)
            {
                Logueo.Error(Ex.Message);

            }
            return 5121;

        }
        */
        public static int Send(string strTo, string strFrom, string strMessage, string strSubject)
        {

            try
            {

                MailMessage msg = new MailMessage();

                string[] listaDist = strTo.Split(';');

                if (listaDist.Length == 0)
                {
                    Logueo.Error("Lista de distribucion vacia");
                    return 0;
                }

                foreach (String toSend in listaDist)
                {
                    try
                    {
                        msg.To.Add(new MailAddress(toSend));
                    }
                    catch
                    {
                    }
                }

                msg.From = new MailAddress(strFrom);
              
                msg.Subject = strSubject;
                msg.IsBodyHtml = true;
                msg.Body = strMessage;

                clienteSmtp.Send(msg);

                return 0;

            }
            catch (Exception Ex)
            {
                Logueo.Error(Ex.Message);

            }
            return 5121;

        }
    
    }
}
