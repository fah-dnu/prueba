using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente.Utilities
{
    public class Email
    {
        SmtpClient server = new SmtpClient(PNConfig.Get("ALTAEMPLEADO", "SMTPServer"), Convert.ToInt32(PNConfig.Get("ALTAEMPLEADO", "SMTPPort")));
        public Email()
        {
            server.Credentials = new System.Net.NetworkCredential(PNConfig.Get("ALTAEMPLEADO", "SMTPUser"), PNConfig.Get("ALTAEMPLEADO", "SMTPPass"));
            server.EnableSsl = true;
        }
        public void Send(string strTo, string strMessage, string strSubject)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.Subject = strSubject;
                msg.To.Add(new MailAddress(strTo));
                msg.From = new MailAddress(PNConfig.Get("ALTAEMPLEADO", "SMTPFrom"));
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
