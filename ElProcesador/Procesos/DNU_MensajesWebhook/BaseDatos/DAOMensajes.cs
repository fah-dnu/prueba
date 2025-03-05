using DNU_MensajesWebhook.Entidades;
using DNU_MensajesWebhook.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DNU_MensajesWebhook.BaseDatos
{
    class DAOMensajes
    {
        /// <summary>
        /// Consulta el listado de mensajes sin procesar en base de datos
        /// </summary>
        /// <returns>Lista de tipo Mensaje con los registros</returns>
        public static List<Mensaje> ListaMensajesSinProcesar(string log, string ip)
        {
           
            try
            {
                List<Mensaje> response = new List<Mensaje>();

                SqlDatabase database = new SqlDatabase(BDMensajesWebhook.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ListaMensajesSinProcesar");
                LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][Se ejcecuto SP ProcNoct_ListaMensajesSinProcesar]");
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

            catch (Exception ex)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][ListaMensajesSinProcesar(): " + ex.Message + "]");
                throw new Exception("ListaMensajesSinProcesar(): " + ex.Message);
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
            SqlConnection connection, SqlTransaction transaccionSQL, string log, string ip)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_InsertaMensajeEnBitacora", connection);
                LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][Se ejcecuto SP ProcNoct_InsertaMensajeEnBitacora" + idMsj, claveEst, numIntentos + "]");

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@IdMensaje", idMsj));
                command.Parameters.Add(new SqlParameter("@ClaveEstatus", claveEst));
                command.Parameters.Add(new SqlParameter("@NumIntentos", numIntentos));

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][InsertaEnvioEnBitacora() ERROR:" + ex.Message + "]");
                throw new Exception("InsertaEnvioEnBitacora() ERROR:" + ex.Message);
            }
        }

        /// <summary>
        /// Realiza la actualización del estatus de procesamiento del mensaje en base de datos
        /// </summary>
        /// <param name="IdMensaje">Identificador del mensaje</param>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void ActualizaEstatusMensaje(Int32 IdMensaje, SqlConnection connection,
            SqlTransaction transaccionSQL, string log, string ip)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_ActualizaEstatusMensaje", connection);
                LogueoReMsjsWebHook.Info("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][Se ejcecuto SP ProcNoct_ActualizaEstatusMensaje" + IdMensaje + "]");

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@IdMensaje", IdMensaje));

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][ActualizaEstatusMensaje() ERROR:" + ex.Message + "]");
                throw new Exception("ActualizaEstatusMensaje() ERROR:" + ex.Message);
            }
        }
    }
}
