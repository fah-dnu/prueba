using CommonProcesador;
using Dnu.ConsumosServiciosCacao.DataContract.Entities;
using Dnu.ConsumosServiciosCacao.DataContract.Request;
using Dnu.MonitoreoServiciosCacao.DataContract.Entities;
using Dnu.MonitoreoServiciosCacao.DataContract.Request;
using Dnu.MonitoreoServiciosCacao.DataContract.Response;
using MonitoreoCacaoPN.CapaDatos;
using MonitoreoCacaoPN.WebReferenceSMS;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MonitoreoCacaoPN.LogicaNegocio
{
    public class LNOperaciones
    {
        DAOOperaciones DAOOperaciones;

        public LNOperaciones()
        {
            DAOOperaciones = new DAOOperaciones();
        }

        public void iniciarProcesoTarjetaDebito()
        {
            Response _respuesta = new Response { StatusCode = 99, DescResponse = "Error inesperado. Consulte al administrador" };

            try
            {
                _respuesta = consultarTarjetaDebito();
                validarNotificacion(_respuesta);
                Logueo.EntradaSalida("[POST: api/ConsultarTarjetaDebito/] " + JsonConvert.SerializeObject(_respuesta), "", false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST: api/ConsultarTarjetaDebito/] " + "[" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }


        public void iniciarProcesoTestingMethod()
        {
            Response _respuesta = new Response { StatusCode = 99, DescResponse = "Error inesperado. Consulte al administrador" };
            
            try
            {
                _respuesta = testMetodo();
                validarNotificacion(_respuesta);
                Logueo.EntradaSalida("[POST: api/testing_method/] " + JsonConvert.SerializeObject(_respuesta), "", false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST: api/testing_method/] " + "[" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        public void iniciarProcesoLineaCredito()
        {
            Response _respuesta = new Response { StatusCode = 99, DescResponse = "Error inesperado. Consulte al administrador" };

            try
            {
                _respuesta = lineaCredito();
                validarNotificacion(_respuesta);
                Logueo.EntradaSalida("[POST: api/linea_credito/] " + JsonConvert.SerializeObject(_respuesta), "", false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST: api/linea_credito] " + "[" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }


        private Response consultarTarjetaDebito()
        {
            int status = 99;
            Response _respuesta = new Response();
            Hashtable htRequest = new Hashtable();
            
            string IdRequestInsert = null;
            try
            {
                if (validarConexionVPN())
                {
                    var jsonInputConsultarTarjeta = JsonConvert.SerializeObject(BodyRequestConsultarTarjeta());
                    htRequest.Add("@descripcion", jsonInputConsultarTarjeta);
                    htRequest.Add("@metodo", "/api/v1/consulta_gral_tarjeta_debito");

                    IdRequestInsert = DAOOperaciones.executeSP("ws_RequestWs_insertar", htRequest);

                    Logueo.EntradaSalida("[POST api/ConsultarTarjetaDebito] [consultarTarjetaDebito] " + jsonInputConsultarTarjeta, "", false);

                    HttpWebRequest clientRequest = (HttpWebRequest)WebRequest.Create(PNConfig.Get("MONCACAO", "wsUrlConsultarTarjetaDebito"));
                    clientRequest.AutomaticDecompression = DecompressionMethods.None;
                    clientRequest.ContentType = "application/json; charset= utf-8";
                    clientRequest.Headers.Add("X-Api-Key", "femcThoWilWmv952ldCjngtt");
                    clientRequest.Headers.Add("user", "Cacao");
                    clientRequest.Headers.Add("role", "1");

                    clientRequest.KeepAlive = false;
                    clientRequest.Method = "Post";
                    clientRequest.Timeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));

                    clientRequest.ServicePoint.ConnectionLeaseTimeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));
                    clientRequest.ServicePoint.MaxIdleTime = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));


                    byte[] postData = new UTF8Encoding().GetBytes(jsonInputConsultarTarjeta);
                    clientRequest.ContentLength = postData.Length;

                    try
                    {
                        using (Stream stream = clientRequest.GetRequestStream())
                            stream.Write(postData, 0, postData.Length);


                        HttpStatusCode statusCode;

                        HttpWebResponse response = (HttpWebResponse)clientRequest.GetResponse();  //fail
                        StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        var temp = streamReader.ReadToEnd();
                        statusCode = response.StatusCode;
                        response.Close();
                        streamReader.Close();

                        status = Convert.ToInt32(statusCode);   //codigo de respuesta del ws
                        if (status == 200)
                        {
                            Logueo.EntradaSalida("[POST api/ConsultarTarjetaDebito] [consultarTarjetaDebito] " + temp, "", true);
                            _respuesta.StatusCode = status;
                            _respuesta.DescResponse = temp;
                        }
                        else
                        {
                            Logueo.EntradaSalida("[POST api/ConsultarTarjetaDebito] [consultarTarjetaDebito] " + temp, "", true);
                            _respuesta = new Response { StatusCode = status, DescResponse = temp };
                        }
                        _respuesta.IdRequest = IdRequestInsert;

                        return _respuesta;
                    }
                    catch (WebException wex)
                    {
                        Logueo.Error("[POST api/ConsultarTarjetaDebito] [consultarTarjetaDebito] " +
                                "[" + wex.Message + "] [" + wex.StackTrace + "]");

                        if (wex.Response != null)
                        {
                            using (var errorResponse = (HttpWebResponse)wex.Response)
                            {
                                status = (int)errorResponse.StatusCode;
                                _respuesta = new Response
                                {
                                    StatusCode = status,
                                    DescResponse = wex.Message,
                                    IdRequest = IdRequestInsert
                                };
                            }
                        }
                        else
                        {
                            _respuesta = new Response
                            {
                                StatusCode = 80,
                                DescResponse = wex.Message,
                                IdRequest = IdRequestInsert
                            };
                        }
                    }
                    clientRequest.Abort();
                    GC.Collect();
                }
                else
                {
                    _respuesta = new Response
                    {
                        StatusCode = 81,
                        DescResponse = "VPN desconectada por Time Out",
                        IdRequest = IdRequestInsert
                    };
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST api/ConsultarTarjetaDebito] [consultarTarjetaDebito] " +
                            "[" + ex.Message + "] [" + ex.StackTrace + "]");
                _respuesta = new Response
                {
                    StatusCode = status,
                    DescResponse = ex.Message,
                    IdRequest = IdRequestInsert
                };

            }

            return _respuesta;
        }
        
        private RequestConsultarTarjeta BodyRequestConsultarTarjeta()
        {
            RequestConsultarTarjeta requestTarjeta = new RequestConsultarTarjeta();
            DContentEncabezado contentEncabezado = new DContentEncabezado();
            DEncabezado encabezado = new DEncabezado();
            DDetalleTarjeta detalle = new DDetalleTarjeta();

            detalle.numero_tarjeta = PNConfig.Get("MONCACAO", "numTarjeta");

            contentEncabezado.Fecha = DateTime.Now.ToString("yyyyMMdd");
            contentEncabezado.Hora = DateTime.Now.ToString("HHmmss");
            contentEncabezado.Detalle = detalle;

            encabezado.Encabezado = contentEncabezado;

            requestTarjeta.Entrada = encabezado;

            return requestTarjeta;
        }

        private Response testMetodo()
        {
            int status = 99;
            Response _respuesta = new Response();
            Hashtable htRequest = new Hashtable();
            string IdRequestInsert = null;

            try
            {
                if (validarConexionVPN())
                {
                    var jsonInputTestMethod = JsonConvert.SerializeObject(BodyTestingMethod());
                    htRequest.Add("@descripcion", jsonInputTestMethod);
                    htRequest.Add("@metodo", "/api/v1/testing_method");
                    IdRequestInsert = DAOOperaciones.executeSP("ws_RequestWs_insertar", htRequest);
                    
                    Logueo.EntradaSalida("[POST api/TestMetodo] [testMetodo] " + jsonInputTestMethod, "", false);
                    HttpWebRequest clientRequest = (HttpWebRequest)WebRequest.Create(PNConfig.Get("MONCACAO", "wsUrlTestingMethod"));
                    clientRequest.AutomaticDecompression = DecompressionMethods.None;
                    clientRequest.ContentType = "application/json; charset= utf-8";
                    clientRequest.Headers.Add("X-Api-Key", "femcThoWilWmv952ldCjngtt");
                    clientRequest.Headers.Add("user", "Cacao");
                    clientRequest.Headers.Add("role", "1");

                    clientRequest.KeepAlive = false;
                    clientRequest.Method = "Post";
                    clientRequest.Timeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));

                    clientRequest.ServicePoint.ConnectionLeaseTimeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));
                    clientRequest.ServicePoint.MaxIdleTime = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));


                    byte[] postData = new UTF8Encoding().GetBytes(jsonInputTestMethod);
                    clientRequest.ContentLength = postData.Length;

                    try
                    {
                        using (Stream stream = clientRequest.GetRequestStream())
                            stream.Write(postData, 0, postData.Length);


                        HttpStatusCode statusCode;

                        HttpWebResponse response = (HttpWebResponse)clientRequest.GetResponse();  //fail
                        StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        var temp = streamReader.ReadToEnd();
                        statusCode = response.StatusCode;
                        response.Close();
                        streamReader.Close();

                        status = Convert.ToInt32(statusCode);   //codigo de respuesta del ws
                        if (status == 200)
                        {
                            Logueo.EntradaSalida("[POST api/TestMetodo] [testing_method] " + temp, "", true);
                            _respuesta.StatusCode = status;
                            _respuesta.DescResponse = temp;
                        }
                        else
                        {
                            Logueo.EntradaSalida("[POST api/TestMetodo] [testing_method] " + temp, "", true);
                            _respuesta = new Response { StatusCode = status, DescResponse = temp };
                        }
                        _respuesta.IdRequest = IdRequestInsert;

                        return _respuesta;
                    }
                    catch (WebException wex)
                    {
                        Logueo.Error("[POST api/TestMetodo] [testing_method] " +
                                "[" + wex.Message + "] [" + wex.StackTrace + "]");

                        if (wex.Response != null)
                        {
                            using (var errorResponse = (HttpWebResponse)wex.Response)
                            {
                                status = (int)errorResponse.StatusCode;
                                _respuesta = new Response
                                {
                                    StatusCode = status,
                                    DescResponse = wex.Message,
                                    IdRequest = IdRequestInsert
                                };
                            }
                        }
                        else
                        {
                            _respuesta = new Response
                            {
                                StatusCode = 80,
                                DescResponse = wex.Message,
                                IdRequest = IdRequestInsert
                            };
                        }
                    }

                    clientRequest.Abort();
                    GC.Collect();
                }
                else
                {
                    _respuesta = new Response
                    {
                        StatusCode = 81,
                        DescResponse = "VPN desconectada por Time Out",
                        IdRequest = IdRequestInsert
                    };
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST api/TestMetodo] [testing_method] " +
                            "[" + ex.Message + "] [" + ex.StackTrace + "]");
                _respuesta = new Response
                {
                    StatusCode = status,
                    DescResponse = ex.Message,
                    IdRequest = IdRequestInsert
                };
            }

            return _respuesta;
        }

        private RequestTestMetodo BodyTestingMethod()
        {
            RequestTestMetodo requestTestMethod = new RequestTestMetodo();

            DContentEncabezadoMetod contentEncabezado = new DContentEncabezadoMetod();
            DEncabezadoMetod encabezado = new DEncabezadoMetod();
            DDetalleTestMetodo detalle = new DDetalleTestMetodo();

            detalle.cuenta = PNConfig.Get("MONCACAO", "cuentaTest");
            detalle.emisor = PNConfig.Get("MONCACAO", "emisorTest");

            contentEncabezado.Fecha = DateTime.Now.ToString("yyyyMMdd");
            contentEncabezado.Hora = DateTime.Now.ToString("HHmmss");
            contentEncabezado.Detalle = detalle;

            encabezado.Encabezado = contentEncabezado;

            requestTestMethod.Entrada = encabezado;

            return requestTestMethod;
        }

        private Response lineaCredito()
        {
            int status = 99;
            Response _respuesta = new Response();
            Hashtable htRequest = new Hashtable();
            string IdRequestInsert = null;

            try
            {
                if (validarConexionVPN())
                {
                    var jsonInputLimite = JsonConvert.SerializeObject(BodyLineaCredito());
                    htRequest.Add("@descripcion", jsonInputLimite);
                    htRequest.Add("@metodo", "/api/v1/linea_credito");
                    IdRequestInsert = DAOOperaciones.executeSP("ws_RequestWs_insertar", htRequest);

                    Logueo.EntradaSalida("[POST api/linea_credito] [lineaCredito] " + jsonInputLimite, "", false);
                    HttpWebRequest clientRequest = (HttpWebRequest)WebRequest.Create(PNConfig.Get("MONCACAO", "wsUrlLineaCredito"));
                    clientRequest.AutomaticDecompression = DecompressionMethods.None;
                    clientRequest.ContentType = "application/json; charset= utf-8";
                    clientRequest.Headers.Add("X-Api-Key", "femcThoWilWmv952ldCjngtt");
                    clientRequest.Headers.Add("user", "Cacao");
                    clientRequest.Headers.Add("role", "1");

                    clientRequest.KeepAlive = false;
                    clientRequest.Method = "Post";
                    clientRequest.Timeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));

                    clientRequest.ServicePoint.ConnectionLeaseTimeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));
                    clientRequest.ServicePoint.MaxIdleTime = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));


                    byte[] postData = new UTF8Encoding().GetBytes(jsonInputLimite);
                    clientRequest.ContentLength = postData.Length;

                    try
                    {
                        using (Stream stream = clientRequest.GetRequestStream())
                            stream.Write(postData, 0, postData.Length);


                        HttpStatusCode statusCode;

                        HttpWebResponse response = (HttpWebResponse)clientRequest.GetResponse();  //fail
                        StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        var temp = streamReader.ReadToEnd();
                        statusCode = response.StatusCode;
                        response.Close();
                        streamReader.Close();

                        status = Convert.ToInt32(statusCode);   //codigo de respuesta del ws
                        if (status == 200)
                        {
                            Logueo.EntradaSalida("[POST api/linea_credito] [lineaCredito] " + temp, "", true);
                            _respuesta.StatusCode = status;
                            _respuesta.DescResponse = temp;
                        }
                        else
                        {
                            Logueo.EntradaSalida("[POST api/linea_credito] [lineaCredito] " + temp, "", true);
                            _respuesta = new Response { StatusCode = status, DescResponse = temp };
                        }
                        _respuesta.IdRequest = IdRequestInsert;

                        return _respuesta;
                    }
                    catch (WebException wex)
                    {
                        Logueo.Error("[POST api/linea_credito] [lineaCredito] " +
                                "[" + wex.Message + "] [" + wex.StackTrace + "]");

                        if (wex.Response != null)
                        {
                            using (var errorResponse = (HttpWebResponse)wex.Response)
                            {
                                status = (int)errorResponse.StatusCode;
                                _respuesta = new Response
                                {
                                    StatusCode = status,
                                    DescResponse = wex.Message,
                                    IdRequest = IdRequestInsert
                                };
                            }
                        }
                        else
                        {
                            _respuesta = new Response
                            {
                                StatusCode = 80,
                                DescResponse = wex.Message,
                                IdRequest = IdRequestInsert
                            };
                        }
                    }

                    clientRequest.Abort();
                    GC.Collect();
                }
                else
                {
                    _respuesta = new Response
                    {
                        StatusCode = 81,
                        DescResponse = "VPN desconectada por Time Out",
                        IdRequest = IdRequestInsert
                    };
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST api/linea_credito] [lineaCredito] " +
                            "[" + ex.Message + "] [" + ex.StackTrace + "]");
                _respuesta = new Response
                {
                    StatusCode = status,
                    DescResponse = ex.Message,
                    IdRequest = IdRequestInsert
                };
            }

            return _respuesta;
        }


        private RequestLimiteCredito BodyLineaCredito()
        {
            RequestLimiteCredito requestLimite = new RequestLimiteCredito();

            requestLimite.name = PNConfig.Get("MONCACAO", "nameLimite");

            return requestLimite;
        }

        private void validarNotificacion(Response _respuesta)
        {
            Hashtable htRequest = new Hashtable();
            string notificacion = "0";

            htRequest.Add("@estatus", _respuesta.StatusCode);
            htRequest.Add("@descripcion", _respuesta.DescResponse);
            htRequest.Add("@idRequest", _respuesta.IdRequest);

            if (_respuesta.StatusCode != 200)
            {
                if (_respuesta.StatusCode == 80)
                {
                    htRequest.Add("@error", 2);
                    notificacion = DAOOperaciones.executeSP("ws_ResponseWs_insertar", htRequest);
                }
                else
                {
                    htRequest.Add("@error", 1);
                    notificacion = DAOOperaciones.executeSP("ws_ResponseWs_insertar", htRequest);
                }
            }
            else
            {
                htRequest.Add("@error", 0);
                notificacion = DAOOperaciones.executeSP("ws_ResponseWs_insertar", htRequest);
            }

            if (notificacion.Equals("1"))
                enviarNotificaciones(_respuesta.StatusCode, _respuesta.DescResponse);
        }

        private void enviarNotificaciones(int pStatusServer, string pDesc)
        {
            enviarSMS(pStatusServer, pDesc);
            enviarEmail(pStatusServer, pDesc);
        }

        private void enviarSMS(int pStatusServer, string desc)
        {
            try
            {
                wsMensaje paramMensaje = new wsMensaje();
                DataTable dtTelefonos = DAOOperaciones.executeSPDT("ws_telefonos_ObtenerTelefonos", null);

                if (dtTelefonos != null && dtTelefonos.Rows.Count != 0)
                {
                    foreach (DataRow numeroTel in dtTelefonos.Rows)
                    {
                        paramMensaje.Destinatario = numeroTel["Telefono"].ToString();
                        paramMensaje.MensajeSMS = desc.Length > 150 ? desc.Substring(0, 150) : desc;
                        paramMensaje.wsPassword = PNConfig.Get("MONCACAO", "pwdSMS");
                        paramMensaje.wsUsuario = PNConfig.Get("MONCACAO", "userSMS");


                        var operSMS = new ServicioMensajes();
                        Logueo.EntradaSalida("[WSenviarSMS] [enviarSMS] " + JsonConvert.SerializeObject(paramMensaje), "", false);
                        var wsrespuestaSMS = operSMS.MENSAJERO_EnviarMensaje(paramMensaje);
                        Logueo.EntradaSalida("[WSenviarSMS] [enviarSMS] " + JsonConvert.SerializeObject(wsrespuestaSMS), "", true);
                    }
                }
                else
                {
                    Logueo.Evento("[enviarSMS] [No hay ningun número teléfonico]");
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST: api/ConsultarTarjetaDebito/] [enviarSMS]" + "[" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        private void enviarEmail(int pStatusServer, string descripcion)
        {
            SmtpClient clienteSmtp = null;
            try
            {
                DataTable dtMails = DAOOperaciones.executeSPDT("ws_Mail_ObtenerMails", null);
                if (dtMails != null && dtMails.Rows.Count != 0)
                {
                    string strTo = null;
                    string strFrom = PNConfig.Get("MONCACAO", "fromMail");
                    string strMessage = descripcion;
                    string strSubject = PNConfig.Get("MONCACAO", "subjectMail");
                    MailMessage msg = new MailMessage();

                    foreach (DataRow dr in dtMails.Rows)
                    {
                        msg.To.Add(new MailAddress(dr["correo"].ToString()));
                        strTo = strTo + dr["correo"].ToString() + ";";
                    }
                    Logueo.EntradaSalida("[SendEmail] [" + strTo + ";" + strFrom + ";" + strMessage + ";" +
                                strSubject + "]", "", false);

                    msg.From = new MailAddress(strFrom);
                    msg.Subject = strSubject;
                    msg.IsBodyHtml = true;
                    msg.Body = strMessage;

                    clienteSmtp = new SmtpClient(PNConfig.Get("MONCACAO", "serverSMTP"), Convert.ToInt32(PNConfig.Get("MONCACAO", "portSMTP")));
                    clienteSmtp.Credentials = new NetworkCredential(PNConfig.Get("MONCACAO", "userSMTP"), PNConfig.Get("MONCACAO", "pwdSMTP"));

                    clienteSmtp.Send(msg);
                    Logueo.EntradaSalida("[enviarEmail] [" + strTo + ";" + strFrom + ";" + strMessage + ";" +
                                strSubject + "] ", "", false);
                }
                else
                {
                    Logueo.Evento("[enviarEmail] [No hay ningún número email]");
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[POST: api/ConsultarTarjetaDebito/] [enviarEmail]" + "[" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }


        private bool validarConexionVPN()
        {
            bool respuesta = false;
            Ping Pings = new Ping();
            int timeout = Convert.ToInt32(PNConfig.Get("MONCACAO", "timeOut"));
            int contador = 0;
            int contadorPing = 1;

            while (contador != 5)
            {
                if (Pings.Send(PNConfig.Get("MONCACAO", "IPVPN"), timeout).Status != IPStatus.Success)
                    contadorPing++;
                contador++;
            }

            if (contadorPing != 5)
                respuesta = true;

            return respuesta;
        }
    }
}
