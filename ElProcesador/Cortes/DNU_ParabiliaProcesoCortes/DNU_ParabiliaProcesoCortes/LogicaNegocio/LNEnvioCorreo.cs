using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
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

        public bool envioCorreo(Correo _correo, Cuentas cuenta, RespuestaSolicitud _respuesta=null)
        {
            string nombreMes = "";
            if (cuenta != null)
            {
                DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
                nombreMes = formatoFecha.GetMonthName(cuenta.Fecha_Corte.Month);
            }
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
                DateTime Hoy = DateTime.Now;
                string fechaCreacion = Hoy.ToString("dd/MM/yyyy");
                linea.AppendLine("");
                linea.AppendLine("YA LLEGÓ,\n\n" +
                    "Aquí está tu estado de cuenta de "+ nombreMes + " de tu tarjeta "+ LNOperaciones.EnmascararTarjeta(cuenta.Tarjeta) + ". \n\n" +
                    "¿Tienes dudas o comentarios ?\n\n" +
                    "Llámanos estamos para ti. \n" +
                    "MEX - 800 953 2020\n" +
                    "EUA - 011(52) 55 4163 8160\n" +
                    "www.jaguaryucard.com");
                //linea.AppendLine("Fecha: " + fechaCreacion/*DateTime.Now.ToShortDateString()*/ + " Hora: " + DateTime.Now.ToShortTimeString());


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
                    Logueo.Error("error al enviar correo:" + e.Message + " " + e.StackTrace);

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
                Logueo.Error("error al enviar correo:" + ex.Message + " " + ex.StackTrace);
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

        public bool envioCorreoConImagen(Correo _correo, Cuentas cuenta,RespuestaSolicitud _respuesta = null)
        {
            string nombreMes = "";
            if (cuenta != null)
            {
                DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
                nombreMes = formatoFecha.GetMonthName(cuenta.Fecha_Corte.Month);
            }
            StringBuilder linea = new StringBuilder();
            bool envioCorrecto = true;
            //Creamos un nuevo Objeto de mensaje
            MailMessage mmsg = new MailMessage();

            try
            {
                /*-------------------------MENSAJE DE CORREO----------------------*/
                //Creamos el body
                linea.AppendLine("");
                linea.AppendLine("YA LLEGÓ,<br/><br/>" +
                    "Aquí está tu estado de cuenta de " + nombreMes.ToUpper() + " de tu tarjeta " + LNOperaciones.EnmascararTarjeta(cuenta.Tarjeta) + ". <br/><br/>" +
                    "¿Tienes dudas o comentarios ?<br/><br/>" +
                    "Llámanos estamos para ti. <br/>" +
                    "MEX - 800 953 2020<br/>" +
                    "EUA - 011(52) 55 4163 8160<br/><br/>");
                    //"www.jaguaryucard.com<br/>");
                //linea.AppendLine("Fecha: " + fechaCreacion/*DateTime.Now.ToShortDateString()*/ + " Hora: " + DateTime.Now.ToShortTimeString());
                linea.AppendLine("<br/><img src=\"http://www.jaguaryucard.com/IMAGE/FIRMA_PIEK.jpg\" />");
               // linea.AppendLine("<br/><img src=\"cid:companyLogo\" />");
                //width=\"104\" height=\"27\" 
                //byte[] reader = File.ReadAllBytes(_correo.rutaimagen);
                //MemoryStream image1 = new MemoryStream(reader);
                //AlternateView av = AlternateView.CreateAlternateViewFromString(linea.ToString(), null, System.Net.Mime.MediaTypeNames.Text.Html);

                ////LinkedResource headerImage = new LinkedResource(image1, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                //headerImage.ContentId = "companyLogo";
                //headerImage.ContentType = new ContentType("image/jpg");
                //av.LinkedResources.Add(headerImage);
                //Agregando vista
                //mmsg.AlternateViews.Add(av);

                ////otra vista(esta no se poque)
                //ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                //AlternateView alternate = AlternateView.CreateAlternateViewFromString(linea.ToString(), mimeType);
                //mmsg.AlternateViews.Add(alternate);

                //Direccion de correo electronico a la que queremos enviar el mensaje
                mmsg.To.Add(_correo.correoReceptor);

                //Nota: La propiedad To es una colección que permite enviar el mensaje a más de un destinatario

                //Asunto
                mmsg.Subject = _correo.titulo;
                mmsg.SubjectEncoding = System.Text.Encoding.UTF8;

                //Direccion de correo electronico que queremos que reciba una copia del mensaje
                // mmsg.Bcc.Add("pruebaPagoPinPad@outlook.com"); //Opcional

                //Cuerpo del Mensaje


                mmsg.Body = linea.ToString();
                //body
                mmsg.BodyEncoding = System.Text.Encoding.UTF8;
                mmsg.IsBodyHtml = true; //Si no queremos que se envíe como HTML

                //Correo electronico desde la que enviamos el mensaje
                mmsg.From = new MailAddress(_correo.correoEmisor, _correo.sobrenombre);
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
                    mmsg.Dispose();
                }
                catch (System.Net.Mail.SmtpException e)
                {
                    mmsg.Dispose();
                  
                    //El servidor SMTP requiere una conexión segura o el cliente no se autenticó. La respuesta del servidor fue: 5.7.57 SMTP; Client was not authenticated to send anonymous mail during MAIL FROM [BN4PR12CA0013.namprd12.prod.outlook.com]
                    //requiere ssl

                    //Aquí gestionamos los errores al intentar enviar el correo
                    //  LogsPinPad.generaLog(rutaLogError, "Mensaje:" + e.Message + " " + e.StackTrace);
                    if (_respuesta != null)
                    {
                        _respuesta.codigoRespuesta = "9999";
                        _respuesta.descripcionRespuesta = "Error al enviar correo,consulte al administrador";

                    }
                    Logueo.Error("error al enviar correo:" + e.Message + " " + e.StackTrace);

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
                Logueo.Error("error al enviar correo:" + ex.Message + " " + ex.StackTrace);
                envioCorrecto = false;
                mmsg.Dispose();

            }
            return envioCorrecto;
        }

        public bool envioCorreoConImagenv2(Correo _correo, Cuentas cuenta, RespuestaSolicitud _respuesta = null)
        {
            string nombreMes = "";
            if (cuenta != null)
            {
                DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
                nombreMes = formatoFecha.GetMonthName(cuenta.Fecha_Corte.Month);
            }
            StringBuilder linea = new StringBuilder();
            bool envioCorrecto = true;
            //Creamos un nuevo Objeto de mensaje
            MailMessage mmsg = new MailMessage();

            try
            {
                /*-------------------------MENSAJE DE CORREO----------------------*/
                //Creamos el body
                linea.AppendLine("");
                linea.AppendLine("YA LLEGÓ,<br/><br/>" +
                    "Aquí está tu estado de cuenta de " + nombreMes.ToUpper() + " de tu tarjeta " + LNOperaciones.EnmascararTarjeta(cuenta.Tarjeta) + ". <br/><br/>" +
                    "¿Tienes dudas o comentarios ?<br/><br/>" +
                    "Llámanos estamos para ti. <br/>" +
                    "MEX - 800 953 2020<br/>" +
                    "EUA - 011(52) 55 4163 8160<br/><br/>");
                //"www.jaguaryucard.com<br/>");
                //linea.AppendLine("Fecha: " + fechaCreacion/*DateTime.Now.ToShortDateString()*/ + " Hora: " + DateTime.Now.ToShortTimeString());
                //  linea.AppendLine("<br/><img src=\"http://www.jaguaryucard.com/IMAGE/FIRMA_PIEK.jpg\" />");
                linea.AppendLine("<br/><img src=\"cid:companyLogo\" />");
                //width=\"104\" height=\"27\" 
                byte[] reader = File.ReadAllBytes(_correo.rutaimagen);
                MemoryStream image1 = new MemoryStream(reader);
                AlternateView av = AlternateView.CreateAlternateViewFromString(linea.ToString(), null, System.Net.Mime.MediaTypeNames.Text.Html);

                LinkedResource headerImage = new LinkedResource(image1, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                headerImage.ContentId = "companyLogo";
                headerImage.ContentType = new ContentType("image/jpg");
                av.LinkedResources.Add(headerImage);



                //Agregando vista
                mmsg.AlternateViews.Add(av);

                //otra vista(esta no se poque)
                ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                AlternateView alternate = AlternateView.CreateAlternateViewFromString(linea.ToString(), mimeType);
                mmsg.AlternateViews.Add(alternate);

                //Direccion de correo electronico a la que queremos enviar el mensaje
                mmsg.To.Add(_correo.correoReceptor);

                //Nota: La propiedad To es una colección que permite enviar el mensaje a más de un destinatario

                //Asunto
                mmsg.Subject = _correo.titulo;
                mmsg.SubjectEncoding = System.Text.Encoding.UTF8;

                //Direccion de correo electronico que queremos que reciba una copia del mensaje
                // mmsg.Bcc.Add("pruebaPagoPinPad@outlook.com"); //Opcional

                //Cuerpo del Mensaje


                //mmsg.Body = linea.ToString();
                ////body
                //mmsg.BodyEncoding = System.Text.Encoding.UTF8;
                //mmsg.IsBodyHtml = false; //Si no queremos que se envíe como HTML

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
                    mmsg.Dispose();
                }
                catch (System.Net.Mail.SmtpException e)
                {
                    mmsg.Dispose();

                    //El servidor SMTP requiere una conexión segura o el cliente no se autenticó. La respuesta del servidor fue: 5.7.57 SMTP; Client was not authenticated to send anonymous mail during MAIL FROM [BN4PR12CA0013.namprd12.prod.outlook.com]
                    //requiere ssl

                    //Aquí gestionamos los errores al intentar enviar el correo
                    //  LogsPinPad.generaLog(rutaLogError, "Mensaje:" + e.Message + " " + e.StackTrace);
                    if (_respuesta != null)
                    {
                        _respuesta.codigoRespuesta = "9999";
                        _respuesta.descripcionRespuesta = "Error al enviar correo,consulte al administrador";

                    }
                    Logueo.Error("error al enviar correo:" + e.Message + " " + e.StackTrace);

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
                Logueo.Error("error al enviar correo:" + ex.Message + " " + ex.StackTrace);
                envioCorrecto = false;
                mmsg.Dispose();

            }
            return envioCorrecto;
        }


    }
}
