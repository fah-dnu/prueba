using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Utilities
{
    public class Email
    {
        SmtpClient server = new SmtpClient(PNConfig.Get("ALTAEMPLEADOCACAO", "SMTPServer"), Convert.ToInt32(PNConfig.Get("ALTAEMPLEADOCACAO", "SMTPPort")));
        public Email()
        {
            server.Credentials = new System.Net.NetworkCredential(PNConfig.Get("ALTAEMPLEADOCACAO", "SMTPUser"), PNConfig.Get("ALTAEMPLEADOCACAO", "SMTPPass"));
            server.EnableSsl = true;
        }
        public void Send(string strTo, string strMessage, string strSubject)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.Subject = strSubject;
                msg.To.Add(new MailAddress(strTo));
                msg.From = new MailAddress(PNConfig.Get("ALTAEMPLEADOCACAO", "SMTPFrom"));
                msg.IsBodyHtml = true;
                msg.Body = strMessage;
                server.Send(msg);
                Logueo.Evento("[Se envío correctamente el correo:" + strTo + "]");
            }
            catch (Exception Ex)
            {
                Logueo.Error("[Send][Error al enviar correo electronico:" + strTo + "] " + Ex.StackTrace);
            }

        }
    }
}
