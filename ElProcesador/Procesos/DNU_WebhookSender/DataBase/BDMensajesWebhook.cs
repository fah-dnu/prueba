using CommonProcesador;
using DNU_WebhookSender.LogicaNegocio;
using DNU_WebhookSender.Utilidades;
using log4net;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace DNU_WebhookSender.DataBase
{
    public class BDMensajesWebhook
    {
        SqlConnection DBLectura;
        SqlConnection DBEscritura;

        public SqlConnection GetLecturaConnection(int i,string ip,string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            if (DBLectura == null)
            {
                DBLectura = new SqlConnection(String.Format(strBDLectura, i.ToString()));

            }

            if (DBLectura.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    DBLectura.Open();
                }
                catch (SqlException exsql)
                {
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION LECTURA  - ERROR AL REALIZA LA CONEXION {0}", exsql.Message) + "]");
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION LECTURA  - SE INTENTA NUEVAMENTE REALIZAR LA CONEXION") + "]");
                    Thread.Sleep(1000);
                    try
                    {
                        DBLectura.Open();
                    }
                    catch (SqlException ex)
                    {
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION LECTURA  - ERROR AL REALIZA LA SEGUNDA CONEXION {0}", ex.Message) + "]");
                        throw ex;
                    }
                }
            }


            return DBLectura;

        }

        public SqlConnection GetEscrituraConnection(int i, string ip, string log)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            if (DBEscritura == null)
            {
                DBEscritura = new SqlConnection(String.Format(strBDEscritura, i.ToString()));

            }

            if (DBEscritura.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    DBEscritura.Open();
                }
                catch (SqlException exsql)
                {
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION ESCRITURA - ERROR AL REALIZA LA CONEXION {0}", exsql.Message) + "]");
                    LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION ESCRITURA - SE INTENTA NUEVAMENTE REALIZAR LA CONEXION") + "]");
                   Thread.Sleep(1000);
                    try
                    {
                        DBEscritura.Open();
                    }
                    catch (SqlException ex)
                    {
                        LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("CONEXION ESCRITURA - ERROR AL REALIZA LA SEGUNDA CONEXION {0}", ex.Message) + "]");
                       throw ex;
                    }
                }

            }


            return DBEscritura;

        }

        public static String strBDLectura
        {
            get
            {
                return PNConfig.Get("WEBHOOK", "BDReadMensajes");
            }
        }

        public static String strBDEscritura
        {
            get
            {
                return PNConfig.Get("WEBHOOK", "BDWriteMensajes");
            }
        }
    }
}
