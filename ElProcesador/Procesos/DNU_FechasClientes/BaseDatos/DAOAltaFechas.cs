using CommonProcesador;
using DNU_FechasClientes.Utilidades;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_FechasClientes.BaseDatos
{
    public class DAOAltaFechas
    {
        public static DataSet EjecutarSP(string procedimiento, Hashtable parametros, string pConexion)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            DataSet retorno = new DataSet();
            SqlConnection connection = new SqlConnection(pConexion);
            try
            {

                connection.Open();
                SqlCommand query = new SqlCommand(procedimiento, connection);
                query.CommandType = CommandType.StoredProcedure;
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }

                }

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                retorno = opcional.Tables.Count > 0 ? opcional : null;
            }
            catch (Exception e)
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES] [EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        public static bool insertaDatos(string idFile, string fecha, string idValor, string valorD1
                                  , string valorD2, string valorD3, string connFechas)
        {
            Hashtable htFile = new Hashtable();
            htFile.Add("@idFile", idFile);
            htFile.Add("@fecha", fecha);
            htFile.Add("@idValor", idValor);
            htFile.Add("@valorD1", valorD1);
            htFile.Add("@valorD2", valorD2);
            htFile.Add("@valorD3", valorD3);

            DataTable datosInsertFechas = DAOAltaFechas.EjecutarSP("procnoc_travel_InsertaFechaCliente", htFile, connFechas).Tables["Table"];

            if (datosInsertFechas != null)
            {
                return true;
            }

            return false;
        }

        public static bool insertaDatosDigitales(string idFile, string fecha, string valorD1
                                  , string valorD2, string pEmisor, string connFechas)
        {
            Hashtable htFile = new Hashtable();
            htFile.Add("@idFile", idFile);
            htFile.Add("@valorD3", fecha);
            htFile.Add("@valorD1", valorD1);
            htFile.Add("@valorD2", valorD2);
            htFile.Add("@emisor", pEmisor);

            DataTable datosInsertFechas = DAOAltaFechas.EjecutarSP("procnoc_travel_InsertaDatDigitales", htFile, connFechas).Tables["Table"];

            if (datosInsertFechas != null)
            {
                return true;
            }

            return false;
        }
    }
}
