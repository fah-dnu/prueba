using CommonProcesador;
using DNU_WebhookSender.DataBase;
using DNU_WebhookSender.Entities;
using DNU_WebhookSender.Utilidades;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace DNU_WebhookSender.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para el envío de mensajes Webhook
    /// </summary>
    public class LNEnvio
    {


        public static bool HayMensajesPendientes(string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("Verificando si hay mensajes por procesar. ") + "]");
                return DAOMensajes.HayMensajesPorProcesar( ip, log);
            }
            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [ERROR: " + ex.Message + "]");
                return false;
            }

        }

        /// <summary>
        /// Realiza el proceso de envío de mensajes webhook pendientes de procesar
        /// </summary>
        /// <returns>TRUE en caso de éxito</returns>
        public static bool LanzaEnvioDeMensajes(List<Mensaje> messages, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                //Consultamos los mensajes sin procesar
                foreach (Mensaje unMensaje in messages)
                {
                    try
                    {
                        //Arma y envía el mensaje
                        PostMessage(unMensaje, dbWebhookConnections, process, ip, log);

                        //Actualiza el estatus de procesamiento del mensaje
                        ModificaEstatusProcesadoMensaje(unMensaje.ID_Mensaje, dbWebhookConnections, process, ip,  log);
                    }

                    catch (Exception err)
                    {
                        LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [ERROR: Mensaje ID: " + unMensaje.ID_Mensaje.ToString() + ": " + err.Message + "]");
                   }
                }

                return true;
            }

            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [LanzaEnvioDeMensajes():" + err.Message + "]");
                return false;
            }
        }


        /// <summary>
        /// Genera y envía el mensaje webhook como POST de HTTP para la operación 
        /// </summary>
        /// <param name="elMensaje">Entidad de tipo Mensaje con los datos para el envío.</param>
        /// <returns>Respuesta del envío</returns>
        public static bool PostMessage(Mensaje elMensaje, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            int MsgCounter = 1;
            try
            {
                StringBuilder sb = new StringBuilder();
                string statusCode, claveEstatus;

                //Se genera el POST
                var clientRS = new RestClient(elMensaje.URL);
                clientRS.Timeout = Convert.ToInt32(PNConfig.Get("WEBHOOK", "ClientTimeout"));
                clientRS.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("WEBHOOK", "ClientReadWriteTimeout"));

                var request = new RestRequest("", Method.POST);
                request.Timeout = Convert.ToInt32(PNConfig.Get("WEBHOOK", "RequestTimeout"));
                request.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("WEBHOOK", "RequestReadWriteTimeout"));

                //Body
                request.AddParameter("application/json", elMensaje.MensajeJSON, ParameterType.RequestBody);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");

                if (elMensaje.Reintentos == 0)
                    elMensaje.Reintentos = 1;


                foreach (Header item in GetHeadersByMessageId(elMensaje.ID_Mensaje, dbWebhookConnections, process,  ip, log))
                {
                    request.AddHeader(item.header, item.value);
                 }



                //Control de reintentos
                for (MsgCounter = 1; MsgCounter <= elMensaje.Reintentos; MsgCounter++)
                {
                    
                    //Ejecuta la petición
                    IRestResponse response = clientRS.Execute(request);

                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Newtonsoft.Json.JsonConvert.SerializeObject(response.Headers) | " + Newtonsoft.Json.JsonConvert.SerializeObject(response.Content) + " | "
                        + response.StatusCode + " | "
                        + response.StatusDescription + " | "
                        + response.ErrorException + "]");

                    statusCode = response.StatusCode.ToString();



                    var content = String.Format("{0} | {1}", response.Content, response.ErrorException);
                    var errorMessage = response.ErrorMessage;

                    sb = new StringBuilder();
                    foreach (Parameter p in response.Headers)
                    {
                        sb.AppendFormat("{0}={1};", p.Name, p.Value);
                    }

                    //Se loguea la entrada


                    if (statusCode == "OK")
                    {
                        

                        if (ContentOk(content, elMensaje, ip, log))
                        {
                            //Procesa la respuesta
                            sb = new StringBuilder();
                            sb.AppendFormat("Notificación exitosa. Código de Respuesta: ({0}). Descripción: {1}. " +
                                "ID_Mensaje: {2}", statusCode, content,
                                elMensaje.ID_Mensaje.ToString());
                            LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + sb.ToString() + "]");
                         
                            //Registra en bitácora
                            claveEstatus = PNConfig.Get("WEBHOOK", "EstatusOK");
                            RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter, content, response.StatusCode, dbWebhookConnections, process, ip, log);

                            break;
                        }
                        else
                        {
                            LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [WEBHOOK_SENDER, RE:POST_Notificacion|STATUS_CODE: " + statusCode + "|RESPONSE_HEADERS: " +
                                sb.ToString() + "|RESPONSE_CONTENT: " + content + "|ERROR_MESSAGE " + errorMessage, "", true + "]");

                             //if (MsgCounter == elMensaje.Reintentos)
                            // {
                            claveEstatus = statusCode == "0" ? PNConfig.Get("WEBHOOK", "EstatusNoEnv") :
                                PNConfig.Get("WEBHOOK", "EstatusSinResp");

                            //Registra en bitácora
                            RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter, content, response.StatusCode, dbWebhookConnections, process,  ip, log);


                            //}
                        }




                    }
                    else
                    {
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [RE:POST_Notificacion|STATUS_CODE: " + statusCode + "|RESPONSE_HEADERS: " +
                        sb.ToString() + "|RESPONSE_CONTENT: " + content + "|ERROR_MESSAGE " + errorMessage, "", true + "]");
                       
                        //if (MsgCounter == elMensaje.Reintentos)
                        //{
                        claveEstatus = statusCode == "0" ? PNConfig.Get("WEBHOOK", "EstatusNoEnv") :
                            PNConfig.Get("WEBHOOK", "EstatusSinResp");

                        //Registra en bitácora
                        RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter, content, response.StatusCode, dbWebhookConnections, process, ip,  log);


                        //}
                    }
                }

                return true;
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [PostMessage()" + ex.Message + "] [" + ex.StackTrace + "]");
                var cveError = PNConfig.Get("WEBHOOK", "EstatusNoEnv");
                RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, cveError, MsgCounter, ex.Message, 0, dbWebhookConnections, process, ip,  log);
                return false;
            }
        }

        private static IEnumerable<Header> GetHeadersByMessageId(int iD_Mensaje, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            return GetDAOHeaders(iD_Mensaje, dbWebhookConnections, process, ip,log);
        }

        private static bool ContentOk(string content, Mensaje elMensaje, string ip, string log)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                if (String.IsNullOrEmpty(content))
                {
                    sb.AppendFormat("Notificación exitosa. " +
                            "ID_Mensaje: {0}", elMensaje.ID_Mensaje.ToString());
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + sb.ToString() + "]");
                  
                    return true;
                }

                //Procesa la respuesta
                HTTPResponses.NotificationResponse notificationResponse =
                    JsonConvert.DeserializeObject<HTTPResponses.NotificationResponse>(content);



                if (notificationResponse.CodigoRespuesta != 0)
                {

                    sb.AppendFormat("Notificación NO exitosa. Código de Respuesta: ({0}). Descripción: {1}. " +
                        "ID_Mensaje: {2}", notificationResponse.CodigoRespuesta, notificationResponse.Descripcion,
                        elMensaje.ID_Mensaje.ToString());

                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + sb.ToString() + "]");
                  
                    return false;

                }
                else
                {
                    sb.AppendFormat("Notificación exitosa. Código de Respuesta: ({0}). Descripción: {1}. " +
                               "ID_Mensaje: {2}", notificationResponse.CodigoRespuesta, notificationResponse.Descripcion,
                               elMensaje.ID_Mensaje.ToString());
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + sb.ToString() + "]");
                  
                    return true;

                }

            }
            catch (Exception ex)
            {
                return true;
            }
        }


        public static IEnumerable<Header> GetDAOHeaders(Int32 IdMensaje, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            SqlConnection conn = dbWebhookConnections.GetLecturaConnection(process,  ip,  log);

            try
            {
                try
                {
                    return DAOMensajes.GetDAOHeaders(IdMensaje, conn, ip, log);

                }

                catch (Exception err)
                {
                    throw err;
                }
            }

            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [GetDAOHeaders()" + err.Message + "]");
                throw new Exception("GetDAOHeaders() " + err.Message);
            }


        }



        /// <summary>
        /// Establece las condiciones de validación en la conexión a base de datos para registrar
        /// en bitácora el estatus de envío de un mensaje
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        /// <param name="ClaveEstatus">Clave del estatus de envío</param>
        /// <param name="NumIntentos">Número de intentos de envío del mensaje</param>
        public static void RegistraEnvioEnBitacora(Int32 IdMensaje, String ClaveEstatus, Int32 NumIntentos,
            string content, System.Net.HttpStatusCode statusCode, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            SqlConnection conn = dbWebhookConnections.GetEscrituraConnection(process, ip,log);

            try
            {
                //using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                //{
                try
                {
                    DAOMensajes.InsertaEnvioEnBitacora(IdMensaje, ClaveEstatus, NumIntentos,
                        content, statusCode,
                        conn,  ip,  log);
                    //transaccionSQL.Commit();

                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("Se insertó en bitácora el estatus de envío del mensaje con ID: {0}" , IdMensaje.ToString()) + "]");
                  
                }

                catch (Exception err)
                {
                    //transaccionSQL.Rollback();
                    throw err;
                }
                //}
            }

            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [RegistraEnvioEnBitacora() " + err.Message + "]");
                throw new Exception("RegistraEnvioEnBitacora() " + err.Message);
            }


        }

        /// <summary>
        /// Establece las condiciones de validación en la conexión a base de datos para 
        /// actualizar el estatus de procesamiento de un mensaje
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        public static void ModificaEstatusProcesadoMensaje(Int32 IdMensaje, BDMensajesWebhook dbWebhookConnections, int process, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            SqlConnection conn = dbWebhookConnections.GetEscrituraConnection(process,  ip,  log);

            try
            {
                //using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                //{
                try
                {
                    DAOMensajes.ActualizaEstatusMensaje(IdMensaje, conn, ip, log);
                    //transaccionSQL.Commit();
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Se Actualizó el estatus de procesamiento del mensaje con ID: " + IdMensaje.ToString() + "]");
                }

                catch (Exception err)
                {
                    //transaccionSQL.Rollback();
                    throw err;
                }
                //}
            }

            catch (Exception err)
            {
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [ModificaEstatusProcesadoMensaje() " + err.Message + "]");
                throw new Exception("ModificaEstatusProcesadoMensaje() " + err.Message);
            }

        }
    }
}
