using DNU_WebhookSender.Entities;
using DNU_WebhookSender.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DNU_WebhookSender.DataBase
{
    class DAOMensajes
    {
        /// <summary>
        /// Consulta el listado de mensajes sin procesar en base de datos
        /// </summary>
        /// <returns>Lista de tipo Mensaje con los registros</returns>
        public static List<Mensaje> ListaMensajesSinProcesar(int top, string ip, string log,string tiempoEspera)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                List<Mensaje> response = new List<Mensaje>();

                SqlDatabase database = new SqlDatabase(BDMensajesWebhook.strBDEscritura);
                using (DbCommand command = database.GetStoredProcCommand("ProcNoct_ListaMensajesSinProcesarPCI"))
                {

                    command.Parameters.Add(new SqlParameter("@top", top));
                    if(!string.IsNullOrEmpty(tiempoEspera))
                    command.Parameters.Add(new SqlParameter("@tiempoDeEspera", tiempoEspera));

                    DataTable dtMsjs = database.ExecuteDataSet(command).Tables[0];

                    if (null != dtMsjs && dtMsjs.Rows.Count != 0)
                    {
                        foreach (DataRow mensaje in dtMsjs.Rows)
                        {
                            Mensaje unMensaje = new Mensaje();

                            unMensaje.ID_Mensaje = int.Parse(mensaje["ID_Mensaje"].ToString());
                            unMensaje.MensajeJSON = mensaje["Mensaje"].ToString();
                            unMensaje.URL = mensaje["URL"].ToString();
                            unMensaje.Reintentos = int.Parse(mensaje["Reintentos"].ToString());
                            unMensaje.ID_Operacion = Convert.ToInt64(mensaje["ID_Operacion"]);
                            unMensaje.ID_Colectiva = Convert.ToInt64(mensaje["ID_Colectiva"]);
                            unMensaje.Procesado = false;

                            response.Add(unMensaje);
                        }
                    }



                    return response;
                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [WEBHOOK_SENDER, ListaMensajesSinProcesar(): " + ex.Message + "]");
                throw new Exception("[WEBHOOK_SENDER]  ListaMensajesSinProcesar(): " + ex.Message);
            }
        }

        internal static void CambiaMensajesEnProceso(List<Mensaje> mensajes, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                foreach (var mensaje in mensajes)
                {
                    using (SqlConnection con = new SqlConnection(BDMensajesWebhook.strBDEscritura))
                    {
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ProcNoct_CambiaMensajesEnProceso" + mensaje.ID_Mensaje + "]");
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Parameters.Add(new SqlParameter("@ID_mensaje", mensaje.ID_Mensaje));
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "ProcNoct_CambiaMensajesEnProceso";
                            cmd.Connection = con;
                            con.Open();

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [WEBHOOK_SENDER]  HayMensajesPorProcesar(): " + ex.Message + "]");
                throw new Exception("[WEBHOOK_SENDER]  HayMensajesPorProcesar(): " + ex.Message);
            }
        }

        internal static bool HayMensajesPorProcesar(string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                using (SqlConnection con = new SqlConnection(BDMensajesWebhook.strBDLectura))
                {
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ProcNoct_HayMensajesPorProcesar]");
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "ProcNoct_HayMensajesPorProcesar";
                        cmd.Connection = con;
                        con.Open();


                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            return reader[0].ToString().Equals("1");
                        }

                        return false;
                    }

                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [HayMensajesPorProcesar(): " + ex.Message + "]");
                throw new Exception("HayMensajesPorProcesar(): " + ex.Message);
            }
        }

        /// <summary>
        /// Realiza la inserció del estatus de envío del mensaje en la bitácora de base de datos
        /// </summary>
        /// <param name="idMsj">Identificador del mensaje</param>
        /// <param name="claveEst">Clave del estatus de envío del mensaje</param>
        /// <param name="numIntentos">Número de intentos de envío del mensaje</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void InsertaEnvioEnBitacora(Int32 idMsj, String claveEst, Int32 numIntentos,
                    string content, System.Net.HttpStatusCode statusCode, SqlConnection connection, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            int codigoRespuesta = 0;
            try
            {
                codigoRespuesta = Convert.ToInt32(statusCode);
            }
            catch (Exception ex) {
                codigoRespuesta = 0;
            }
            try
            {
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ProcNoct_InsertaMensajeEnBitacora]");
                using (SqlCommand command = new SqlCommand("ProcNoct_InsertaMensajeEnBitacora", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;
                    //command.Transaction = transaccionSQL;

                    command.Parameters.Add(new SqlParameter("@IdMensaje", idMsj));
                    command.Parameters.Add(new SqlParameter("@ClaveEstatus", claveEst));
                    command.Parameters.Add(new SqlParameter("@NumIntentos", numIntentos));
                    command.Parameters.Add(new SqlParameter("@CodigoRespuesta", codigoRespuesta/*Convert.ToInt32(statusCode)*/));
                    command.Parameters.Add(new SqlParameter("@Respuesta", content));

                    command.ExecuteNonQuery();
                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [InsertaEnvioEnBitacora() ERROR:" + ex.Message + "]");
                throw new Exception("InsertaEnvioEnBitacora() ERROR:" + ex.Message);
            }
        }

        /// <summary>
        /// Realiza la actualización del estatus de procesamiento del mensaje en base de datos
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void ActualizaEstatusMensaje(Int32 IdMensaje, SqlConnection connection, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ProcNoct_ActualizaEstatusMensaje" + IdMensaje + "]");
                using (SqlCommand command = new SqlCommand("ProcNoct_ActualizaEstatusMensaje", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdMensaje", IdMensaje));

                    command.ExecuteNonQuery();
                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [ActualizaEstatusMensaje() ERROR:" + ex.Message + "]");
                throw new Exception("ActualizaEstatusMensaje() ERROR:" + ex.Message);
            }
        }

        internal static IEnumerable<Header> GetDAOHeaders(int idMensaje, SqlConnection conn, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                List<Header> lstHeaders = new List<Header>();
                LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ProcNoct_ObtieneHeadersMensaje" + idMensaje + "]");

                using (SqlCommand command = new SqlCommand("ProcNoct_ObtieneHeadersMensaje", conn))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdMensaje", idMensaje));

                    using (var read = command.ExecuteReader())
                    {

                        while (read.Read())
                        {
                            lstHeaders.Add(new Header
                            {
                                header = read["header"].ToString(),
                                value = read["valor"].ToString(),
                            });
                        }
                    }

                    return lstHeaders;
                }
            }

            catch (Exception ex)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [GetDAOHeaders() ERROR:" + ex.Message + "]");
                throw new Exception("GetDAOHeaders() ERROR:" + ex.Message);
            }


        }
    }
}
