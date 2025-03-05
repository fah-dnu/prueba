using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNEnvioCorreo
    {
        public LNEnvioCorreo() {
        //    ServicePointManager.ServerCertificateValidationCallback = RemoteServerCertificateValidationCallback;
        }

        public bool envioCorreo(Correo _correo, RespuestaSolicitud _respuesta=null)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            StringBuilder linea = new StringBuilder();
            bool envioCorrecto = true;
            try
            {
                /*-------------------------MENSAJE DE CORREO----------------------*/

                //Creamos un nuevo Objeto de mensaje
                MailMessage mmsg = new MailMessage();

                //Direccion de correo electronico a la que queremos enviar el mensaje
                mmsg.To.Add(_correo.correoReceptor);

                //Nota: La propiedad To es una colección que permite enviar el mensaje a más de un destinatario

                //Asunto
                mmsg.Subject =_correo.titulo;
                mmsg.SubjectEncoding = System.Text.Encoding.UTF8;

                //Direccion de correo electronico que queremos que reciba una copia del mensaje
                // mmsg.Bcc.Add("pruebaPagoPinPad@outlook.com"); //Opcional

                //Cuerpo del Mensaje

                linea.AppendLine("");
                linea.AppendLine("Estado de cuenta");
                linea.AppendLine("Fecha: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToShortTimeString());


                mmsg.Body = linea.ToString();
                //body
                mmsg.BodyEncoding = System.Text.Encoding.UTF8;
                mmsg.IsBodyHtml = false; //Si no queremos que se envíe como HTML

                //Correo electronico desde la que enviamos el mensaje
                mmsg.From = new MailAddress(_correo.correoEmisor);
              //  mmsg.ReplyToList.Add(new MailAddress("foo@bar.net"));
                //archivos adjuntos

                //la lista se llena con direcciones fisicas, por ejemplo: c:/pato.txt
                if (_correo.archivos != null)
                {
                    //agregado de archivo
                    foreach (string archivo in _correo.archivos)
                    {
                        //comprobamos si existe el archivo y lo agregamos a los adjuntos
                        if (System.IO.File.Exists(@archivo))
                            mmsg.Attachments.Add(new Attachment(@archivo));

                    }
                }

                /*-------------------------CLIENTE DE CORREO----------------------*/

                //Creamos un objeto de cliente de correo
                SmtpClient cliente = new SmtpClient();

                //Hay que crear las credenciales del correo emisor
                cliente.UseDefaultCredentials = false;
                cliente.Credentials =
                    new System.Net.NetworkCredential(_correo.correoEmisor,
                     _correo.password);

                //Lo siguiente es obligatorio si enviamos el mensaje desde Gmail
                /*
                cliente.Port = 587;
                cliente.EnableSsl = true;
                */

                cliente.Host = _correo.host;//"smtp-mail.outlook.com"; //"smtp.live.com"; //Para Gmail "smtp.gmail.com";
                cliente.Port = Convert.ToInt32(_correo.puerto);//587;
                cliente.EnableSsl = true;// true;
                cliente.DeliveryMethod = SmtpDeliveryMethod.Network;
               // cliente.UseDefaultCredentials = false;

                /*-------------------------ENVIO DE CORREO----------------------*/

                try
                {
                    //Enviamos el mensaje      
                    cliente.Send(mmsg);
                }
                catch (System.Net.Mail.SmtpException e)
                {
                    //El servidor SMTP requiere una conexión segura o el cliente no se autenticó. La respuesta del servidor fue: 5.7.57 SMTP; Client was not authenticated to send anonymous mail during MAIL FROM [BN4PR12CA0013.namprd12.prod.outlook.com]
                    //requiere ssl

                    //Aquí gestionamos los errores al intentar enviar el correo
                    //  LogsPinPad.generaLog(rutaLogError, "Mensaje:" + e.Message + " " + e.StackTrace);
                    if (_respuesta != null) {
                        _respuesta.codigoRespuesta = "9999";
                        _respuesta.descripcionRespuesta = "Error al enviar correo,consulte al administrador";

                    }
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al enviar correo:" + e.Message + " " + e.StackTrace + "]");
                   
                    envioCorrecto = false;

                }
            }
            catch (Exception ex)
            {
                if (_respuesta != null)
                {
                    _respuesta.codigoRespuesta = "9999";
                    _respuesta.descripcionRespuesta = "Error al enviar correo,consulte al administrador";

                }
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al enviar correo:" + ex.Message + " " + ex.StackTrace + "]");
                envioCorrecto = false;

            }
            return envioCorrecto;
        }
        public static bool RemoteServerCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // if got an cert auth error
            if (sslPolicyErrors != SslPolicyErrors.RemoteCertificateNameMismatch) return false;
            const string sertFileName = "smpthost.cer";

            // check if cert file exists
            if (File.Exists(sertFileName))
            {
                var actualCertificate = X509Certificate.CreateFromCertFile(sertFileName);
                return certificate.Equals(actualCertificate);
            }

            // export and check if cert not exists
            using (var file = File.Create(sertFileName))
            {
                var cert = certificate.Export(X509ContentType.Cert);
                file.Write(cert, 0, cert.Length);
            }
            var createdCertificate = X509Certificate.CreateFromCertFile(sertFileName);
            return certificate.Equals(createdCertificate);
        }

    }
}
