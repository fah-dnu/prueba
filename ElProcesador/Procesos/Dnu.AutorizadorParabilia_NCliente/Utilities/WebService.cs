using CommonProcesador;
using Dnu.AutorizadorParabilia_NCliente.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Dnu.AutorizadorParabilia_NCliente.Utilities
{
    public class WebService
    {
        public static string WSAltaEmpleadoEvertec(EntradaWSEvertecAlta datosEnvio)
        {
            string strResponseValue = string.Empty;
            Entrada entrada = new Entrada();
            EncabezadoAlta encabezado = new EncabezadoAlta();

            encabezado.Fecha = DateTime.Today.ToString("yyyyMMdd");
            encabezado.Hora = DateTime.Now.ToString("HHmmss");
            encabezado.Detalle = datosEnvio;
            entrada.Encabezado = encabezado;

            string serializerBody = Serializar(entrada);
            string datosEnvioWS = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"http://ws.prospectacion.evertec.com/\">" +
                            "<soapenv:Header/><soapenv:Body><ws:prospectarCuenta><!--Optional:-->" +
                            "<datosProspectarXML><![CDATA[" + serializerBody + "]]></datosProspectarXML>" +
                            "</ws:prospectarCuenta></soapenv:Body></soapenv:Envelope>";

            try
            {
                //Se genera el POST para el ws
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("ALTAEMPLEADO", "UrlWSEvertecA"));
                WebHeaderCollection myWebHeaderCollection = request.Headers;


                request.Method = "POST";
                request.ContentType = "text/xml";

                //header autorization
                String encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(PNConfig.Get("ALTAEMPLEADO", "UsuarioWS") + ":" +
                PNConfig.Get("ALTAEMPLEADO", "PasswordWS")));
                //request.Headers.Add("Authorization", "Basic " + encoded);
                myWebHeaderCollection.Add("Authorization: Basic " + encoded);

                byte[] bytes = Encoding.UTF8.GetBytes(datosEnvioWS);

                using (Stream requestStream = request.GetRequestStream()) // generar la petición mandando al Body el XML a enviar
                {
                    Logueo.EntradaSalida("[WS_AltaEmpleado.Evertec.Prospectación] [ENVIADO] [WSAltaEmpleadoEvertec] [datos XML ha enviar: " + datosEnvioWS.Replace("\n", " ").Replace("\r", " ") + "]", "PROCNOC", false);
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new ApplicationException("Codigo Error : " + response.StatusCode);
                        }

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    strResponseValue = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                }


                return strResponseValue;
            }

            catch (Exception ex)
            {
                Logueo.Error("[WSAltaEmpleadoEvertec][Error al invokar WS Evertec con datos: " + datosEnvioWS.Replace("\n", " ").Replace("\r", " ") + "][" + ex.Message + "]");
                return null;
            }
        }

        public static string WSObtenerFechaVencimiento(WSObtenerFechaVencimiento datosEnvio)
        {
            string strResponseValue = string.Empty;
            string serializerBody = Serializar(datosEnvio);

            serializerBody = serializerBody.Replace("WSObtenerFechaVencimiento", "web:consultaGeneralTarjeta").Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");


            string datosEnvioWS = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                "xmlns:web=\"http://www.athservices.net/servicios/WebServiceSitioWeb\">" +
                "<soapenv:Header/><soapenv:Body>" + serializerBody + "</soapenv:Body></soapenv:Envelope>";

            try
            {
                //Se genera el POST para el ws
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("ALTAEMPLEADO", "UrlWSEvertecAVencimiento"));
                WebHeaderCollection myWebHeaderCollection = request.Headers;
                request.Method = "POST";
                request.ContentType = "text/xml";

                //header autorization
                String encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(PNConfig.Get("ALTAEMPLEADO", "UsuarioWS") + ":" +
                PNConfig.Get("ALTAEMPLEADO", "PasswordWS")));
                myWebHeaderCollection.Add("Authorization: Basic " + encoded);
                byte[] bytes = Encoding.UTF8.GetBytes(datosEnvioWS);

                using (Stream requestStream = request.GetRequestStream()) // generar la petición mandando al Body el XML a enviar
                {
                    Logueo.EntradaSalida("[WS_AltaEmpleado.Evertec.ConsultaVigencia] [ENVIADO] [WSObtenerFechaVencimiento] [datos ha enviar: " + datosEnvioWS.Replace("\n", " ").Replace("\r", " ") + "]", "PROCNOC", false);
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new ApplicationException("Codigo Error : " + response.StatusCode);
                        }

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    strResponseValue = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                }


                return strResponseValue;
            }

            catch (Exception ex)
            {
                Logueo.Error("[WSObtenerFechaVencimiento][Error al invokar WS Evertec con datos: " + datosEnvioWS.Replace("\n", " ").Replace("\r", " ") + "][" + ex.Message + "]");
                return null;
            }
        }

        public static string WSBajaEmpleadoEvertec(EntradaWSEvertecBaja datosEnvio)
        {
            try
            {
                string strResponseValue = string.Empty;
                string serializerBody = Serializar(datosEnvio);

                //Se genera el POST para el ws
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("BAJAEMPLEADO", "UrlWSEvertecB"));
                request.Method = "POST";
                request.ContentType = "text/xml";
                byte[] bytes = Encoding.UTF8.GetBytes(serializerBody);

                using (Stream requestStream = request.GetRequestStream()) // generar la petición mandando al Body el XML a enviar
                {
                    datosEnvio.numeroTarjeta = SeguridadCifrado.cifrar(datosEnvio.numeroTarjeta);
                    var datosSalida = new JavaScriptSerializer().Serialize(datosEnvio);
                    Logueo.EntradaSalida("[WS_BajaEmpleado.Evertec.Bloqueo_Y_DesbloqueoTarjetas] [ENVIADO] [WSBajaEmpleadoEvertec] [datos ha enviar: " + datosSalida + "]", "PROCNOC", false);
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new ApplicationException("Codigo Error : " + response.StatusCode);
                        }

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    strResponseValue = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                }


                return strResponseValue;
            }

            catch (Exception ex)
            {
                datosEnvio.numeroTarjeta = datosEnvio.numeroTarjeta.Substring(12).PadLeft(16, '*');
                var datos = new JavaScriptSerializer().Serialize(datosEnvio);
                Logueo.Error("[WSBajaEmpleadoEvertec][Error al invokar WS Evertec: " + datos + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return null;
            }
        }

        public static string Serializar(object objeto)
        {
            try
            {
                StringBuilder writer = new StringBuilder();
                XmlSerializer serializer = new XmlSerializer(objeto.GetType());

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "datosAlta");

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;

                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
                    serializer.Serialize(xmlWriter, objeto, ns);

                    return writer.ToString();
                }
            }
            catch (Exception E)
            {
                throw E;
            }
        }

        public static RWSMail WSEnviaCorreoBienvenida(Correo pCorreoBienvenida)
        {
            var datosProcesar = new JavaScriptSerializer().Serialize(pCorreoBienvenida);

            RWSMail respuestaEvertec = new RWSMail();
            respuestaEvertec.responseCode = 9;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("ALTAEMPLEADO", "URLWSMail"));
                WebHeaderCollection myWebHeaderCollection = request.Headers;

                request.Method = "POST";
                request.ContentType = "application/json";
               

                Stream webStream = null;
                using (webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.UTF8))
                {
                    Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.SendMail] [ENVIADO] [WSEnviaCorreoBienvenida] [datos ha enviar: " + datosProcesar + "]", "PROCNOC", false);
                    requestWriter.Write(datosProcesar);
                }

                WebResponse webResponse = null;

                StreamReader responseReader = null;
                try
                {
                    webResponse = request.GetResponse();
                    using (webStream = webResponse.GetResponseStream())
                    {
                        if (webStream != null)
                        {
                            using (responseReader = new StreamReader(webStream))
                            {
                                var responseEvertec = responseReader.ReadToEnd();
                                respuestaEvertec = JsonConvert.DeserializeObject<RWSMail>(responseEvertec);
                                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoBienvenida] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)wex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                Logueo.Error("[WSEnviaCorreoBienvenida] [" + datosProcesar + "][Mensaje: " + wex.Message + " TRACE: " + wex.StackTrace + "]");
                                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoBienvenida] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("[WSEnviaCorreoBienvenida][Error al invokar WS senMail Armit: " + datosProcesar + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoBienvenida] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
            }
            return respuestaEvertec;
        }

        public static RWSMail WSEnviaCorreoSaldo(Correo pCorreo)
        {
            var datosProcesar = new JavaScriptSerializer().Serialize(pCorreo);

            RWSMail respuestaEvertec = new RWSMail();
            respuestaEvertec.responseCode = 9;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("ASIGNACIONSALDOS", "URLWSMail"));
                WebHeaderCollection myWebHeaderCollection = request.Headers;

                request.Method = "POST";
                request.ContentType = "application/json";

                Stream webStream = null;
                using (webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.UTF8))
                {
                    Logueo.EntradaSalida("[WS_AsignacionSaldos.Armit.SendMail] [ENVIADO] [WSEnviaCorreoSaldo] [datos ha enviar: " + datosProcesar + "]", "PROCNOC", false);
                    requestWriter.Write(datosProcesar);
                }

                WebResponse webResponse = null;

                StreamReader responseReader = null;
                try
                {
                    webResponse = request.GetResponse();
                    using (webStream = webResponse.GetResponseStream())
                    {
                        if (webStream != null)
                        {
                            using (responseReader = new StreamReader(webStream))
                            {
                                var responseEvertec = responseReader.ReadToEnd();
                                respuestaEvertec = JsonConvert.DeserializeObject<RWSMail>(responseEvertec);
                                Logueo.EntradaSalida("[WS_AsignacionSaldos.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoSaldo] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)wex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                Logueo.Error("[WSEnviaCorreoSaldo] [" + datosProcesar + "][Mensaje: " + wex.Message + " TRACE: " + wex.StackTrace + "]");
                                Logueo.EntradaSalida("[WS_AsignacionSaldos.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoSaldo] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("[WSEnviaCorreoSaldo][Error al invokar WS senMail Armit: " + datosProcesar + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                Logueo.EntradaSalida("[WSEnviaCorreoSaldo.Armit.SendMail] [RECIBIDO] [WSEnviaCorreoSaldo] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
            }
            return respuestaEvertec;
        }


        public static RWSMail WSCrearCorreoEmployee(EmployeeAccount pEmployeeAccount)
        {
            var datosProcesar = new JavaScriptSerializer().Serialize(pEmployeeAccount);
            RWSMail respuestaEvertec = new RWSMail();
            respuestaEvertec.responseCode = 9;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PNConfig.Get("ALTAEMPLEADO", "URLWSEmployee"));
                WebHeaderCollection myWebHeaderCollection = request.Headers;

                request.Method = "POST";
                request.ContentType = "application/json";

                Stream webStream = null;
                using (webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.UTF8))
                {
                    Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.AccountEmployee] [ENVIADO] [WSCrearCorreoEmployee] [datos a procesar: " + datosProcesar + "]", "PROCNOC", false);
                    requestWriter.Write(datosProcesar);
                }

                WebResponse webResponse = null;

                StreamReader responseReader = null;
                try
                {
                    webResponse = request.GetResponse();
                    using (webStream = webResponse.GetResponseStream())
                    {
                        if (webStream != null)
                        {
                            using (responseReader = new StreamReader(webStream))
                            {
                                var responseEvertec = responseReader.ReadToEnd();
                                respuestaEvertec = JsonConvert.DeserializeObject<RWSMail>(responseEvertec);
                                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.AccountEmployee] [RECIBIDO] [WSCrearCorreoEmployee] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)wex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                Logueo.Error("[WSCrearCorreoEmployee] [" + datosProcesar + "][Mensaje: " + wex.Message + " TRACE: " + wex.StackTrace + "]");
                                respuestaEvertec.responseCode = 9;
                                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.AccountEmployee] [RECIBIDO] [WSCrearCorreoEmployee] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("[WSCrearCorreoEmployee][Error al invokar WS Armit de correo crear cuentas Employee: " + datosProcesar + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                respuestaEvertec.responseCode = 9;
                Logueo.EntradaSalida("[WS_AltaEmpleado.Armit.AccountEmployee] [RECIBIDO] [WSCrearCorreoEmployee] [CODIGO:" + respuestaEvertec.responseCode + ";RESPUESTA:" + respuestaEvertec.message + "]", "PROCNOC", true);
            }
            return respuestaEvertec;
        }
    }
}
