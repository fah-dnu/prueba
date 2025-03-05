using CommonProcesador;
using DNU_MensajesWebhook.BaseDatos;
using DNU_MensajesWebhook.Entidades;
using DNU_MensajesWebhook.Utilidades;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_MensajesWebhook.LogicaNegocio
{
    /// <summary>
    /// Establece la lógica de negocio para el envío de mensajes Webhook
    /// </summary>
    public class LNEnvio
    {
        /// <summary>
        /// Realiza el proceso de envío de mensajes webhook pendientes de procesar
        /// </summary>
        /// <returns>TRUE en caso de éxito</returns>
        ///REMSJSWEBHOOK 
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "REMSJSWEBHOOK";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        [Transaction]
        public static bool LanzaEnvioDeMensajes()
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("LanzaEnvioDeMensajes");
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                //Consultamos los mensajes sin procesar
                List<Mensaje> losMensajes = DAOMensajes.ListaMensajesSinProcesar( log,  ip);

                foreach (Mensaje unMensaje in losMensajes)
                {
                    try
                    {
                        //Arma y envía el mensaje
                        PostMessage(unMensaje,  log,  ip);

                        //Actualiza el estatus de procesamiento del mensaje
                        ModificaEstatusProcesadoMensaje(unMensaje.ID_Mensaje,  log,  ip);
                    }

                    catch (Exception err)
                    {
                        LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][ERROR: Mensaje ID: " + unMensaje.ID_Mensaje.ToString() + ": " + err.Message + "]");
                        ApmNoticeWrapper.NoticeException(err);
                    }
                }

                return true;
            }

            catch (Exception err)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][LanzaEnvioDeMensajes():" + err.Message + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }
        }


        /// <summary>
        /// Genera y envía el mensaje webhook como POST de HTTP para la operación 
        /// </summary>
        /// <param name="elMensaje">Entidad de tipo Mensaje con los datos para el envío.</param>
        /// <returns>Respuesta del envío</returns>
        public static bool PostMessage(Mensaje elMensaje, string log, string ip)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string statusCode, claveEstatus;

                //Se genera el POST
                var clientRS = new RestClient(elMensaje.URL);
                clientRS.Timeout = Convert.ToInt32(PNConfig.Get("REMSJSWEBHOOK", "ClientTimeout"));
                clientRS.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("REMSJSWEBHOOK", "ClientReadWriteTimeout"));

                var request = new RestRequest("", Method.POST);
                request.Timeout = Convert.ToInt32(PNConfig.Get("REMSJSWEBHOOK", "RequestTimeout"));
                request.ReadWriteTimeout = Convert.ToInt32(PNConfig.Get("REMSJSWEBHOOK", "RequestReadWriteTimeout"));

                //Body
                request.AddParameter("application/json", elMensaje.MensajeJSON, ParameterType.RequestBody);

                //Headers HTTP
                request.AddHeader("Content-Type", "application/json");

                //Control de reintentos
                for (int MsgCounter = 1; MsgCounter <= elMensaje.Reintentos; MsgCounter++)
                {
                    //Se loguea la salida
                    LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][POST_Notificacion|ID_MENSAJE: " + elMensaje.ID_Mensaje + "|INTENTO: " +
                    MsgCounter.ToString() + "|URL: " + elMensaje.URL + "|BODY: " + elMensaje.MensajeJSON, "", false + "]");
                  
                    //Ejecuta la petición
                    IRestResponse response = clientRS.Execute(request);
                    statusCode = response.StatusCode.ToString();

                    sb = new StringBuilder();
                    foreach (Parameter p in response.Headers)
                    {
                        sb.AppendFormat("{0}={1};", p.Name, p.Value);
                    }

                    //Se loguea la entrada
                    LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][RE:POST_Notificacion|STATUS_CODE: " + statusCode + "|RESPONSE_HEADERS: " +
                        sb.ToString() + "|RESPONSE_CONTENT: " + response.Content, "", true + "]");
                   

                    if (statusCode == "OK")
                    {
                        var content = response.Content;

                        //Procesa la respuesta
                        HTTPResponses.NotificationResponse notificationResponse =
                            JsonConvert.DeserializeObject<HTTPResponses.NotificationResponse>(content);

                        if (notificationResponse.CodigoRespuesta != 0)
                        {
                            sb = new StringBuilder();
                            sb.AppendFormat("Notificación NO exitosa. Código de Respuesta: ({0}). Descripción: {1}. " +
                                "ID_Mensaje: {2}", notificationResponse.CodigoRespuesta, notificationResponse.Descripcion,
                                elMensaje.ID_Mensaje.ToString());
                            LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][" + sb.ToString() + "]");
                            
                            if (MsgCounter == elMensaje.Reintentos)
                            {
                                //Registra en bitácora
                                claveEstatus = PNConfig.Get("REMSJSWEBHOOK", "EstatusNoOK");
                                RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter, log, ip);
                            }
                        }
                        else
                        {
                            sb = new StringBuilder();
                            sb.AppendFormat("Notificación exitosa. Código de Respuesta: ({0}). Descripción: {1}. " +
                                "ID_Mensaje: {2}", notificationResponse.CodigoRespuesta, notificationResponse.Descripcion,
                                elMensaje.ID_Mensaje.ToString());
                            LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][" + sb.ToString() + "]");
                         
                            //Registra en bitácora
                            claveEstatus = PNConfig.Get("REMSJSWEBHOOK", "EstatusOK");
                            RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter, log, ip);

                            MsgCounter = elMensaje.Reintentos + 1;
                        }
                    }
                    else
                    {
                        if (MsgCounter == elMensaje.Reintentos)
                        {
                            claveEstatus = statusCode == "0" ? PNConfig.Get("REMSJSWEBHOOK", "EstatusNoEnv") :
                                PNConfig.Get("REMSJSWEBHOOK", "EstatusSinResp");

                            //Registra en bitácora
                            RegistraEnvioEnBitacora(elMensaje.ID_Mensaje, claveEstatus, MsgCounter,  log,  ip);
                        }
                    }
                }

                return true;
            }

            catch (Exception ex)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][PostMessage(): " + ex.Message + "]");
                throw new Exception("PostMessage(): " + ex.Message);
            }
        }

        /// <summary>
        /// Establece las condiciones de validación en la conexión a base de datos para registrar
        /// en bitácora el estatus de envío de un mensaje
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        /// <param name="ClaveEstatus">Clave del estatus de envío</param>
        /// <param name="NumIntentos">Número de intentos de envío del mensaje</param>
        public static void RegistraEnvioEnBitacora(Int32 IdMensaje, String ClaveEstatus, Int32 NumIntentos, string log, string ip)
        {
            SqlConnection conn = null;
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                conn = new SqlConnection(BDMensajesWebhook.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOMensajes.InsertaEnvioEnBitacora(IdMensaje, ClaveEstatus, NumIntentos, conn, transaccionSQL,  log,  ip);
                        transaccionSQL.Commit();

                        LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][Se insertó en bitácora el estatus de envío del mensaje con ID: " + IdMensaje.ToString() + "]");
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("RegistraEnvioEnBitacora() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Establece las condiciones de validación en la conexión a base de datos para 
        /// actualizar el estatus de procesamiento de un mensaje
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        public static void ModificaEstatusProcesadoMensaje(Int32 IdMensaje, string log, string ip)
        {
            SqlConnection conn = null;
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                conn = new SqlConnection(BDMensajesWebhook.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOMensajes.ActualizaEstatusMensaje(IdMensaje, conn, transaccionSQL,  log,  ip);
                        transaccionSQL.Commit();

                        LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][Se Actualizó el estatus de procesamiento del mensaje con ID: " + IdMensaje.ToString() + "]");
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][ModificaEstatusProcesadoMensaje() " + err.Message + "]");
                throw new Exception("ModificaEstatusProcesadoMensaje() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }
    }
}
