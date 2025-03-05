using CommonProcesador;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientoCompensacion.BaseDatos
{
    public class BDsps
    {
        public static DataSet EjecutarSP(string procedimiento, Hashtable parametros, string pConexion)
        {
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
                Logueo.Error("[EjecutarSP] [Error al ejecutar sp: " + procedimiento + "] [Conexion: " + connection + "] [Mensaje: " + e.Message + " TRACE: " + e.StackTrace + "]");
                return null;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }
    }
}
