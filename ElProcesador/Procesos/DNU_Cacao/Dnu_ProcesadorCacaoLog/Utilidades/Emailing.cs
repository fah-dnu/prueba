using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using CommonProcesador;

namespace Dnu_ProcesadorCaCao.Utilidades
{
    public class Emailing
    {

        static SmtpClient clienteSmtp = null;

        static Emailing()
        {
            clienteSmtp = new SmtpClient(PNConfig.Get("PROCARCH", "SMTPServer"), Int16.Parse(PNConfig.Get("PROCARCH", "SMTPPort") ));

            //if (ConfigurationManager.AppSettings["SMTPUser"].Contains('@'))
            //   {

            clienteSmtp.Credentials = new NetworkCredential(PNConfig.Get("PROCARCH", "SMTPUser") , PNConfig.Get("PROCARCH", "SMTPPass"));
            //clienteSmtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], ConfigurationManager.AppSettings["SMTPPass"]);
            //}
            //else
            //{
            //     clienteSmtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"] + "@" + ConfigurationManager.AppSettings["SMTPServer"].Replace("mail.",""), ConfigurationManager.AppSettings["SMTPPass"]);

            //}
        }

        public Emailing()
        {

        }

        public static int Send(string strTo, string strMessage, string strSubject, Attachment Atach, Attachment Atach2)
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
                    if (toSend.Contains("@"))
                    {
                        msg.To.Add(new MailAddress(toSend));
                    }
                }


                //  msg.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"]);

                String smtpUser = PNConfig.Get("PROCARCH", "SMTPUser");// Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "SMTPUser").Valor;
                String smtpServer = PNConfig.Get("PROCARCH", "SMTPServer"); //Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "SMTPServer").Valor;


                if (smtpUser.Contains("@"))
                {
                    msg.From = new MailAddress(smtpUser);
                }
                else
                {
                    msg.From = new MailAddress(smtpUser + "@" + smtpServer.Replace("mail.", ""));
                }

                msg.Subject = strSubject;
                msg.IsBodyHtml = true;
                msg.Body = strMessage;
                msg.Attachments.Add(Atach);
                try
                {
                    msg.Attachments.Add(Atach2);
                }
                catch (Exception err)
                {
                    Logueo.Error("No se Encontro el XML:" + err.Message);
                }

                clienteSmtp.Send(msg);
                return 0;

            }
            catch (Exception Ex)
            {


            }
            return 5121;

        }

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
