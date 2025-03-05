using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumReportes.BaseDatos
{
    public class ExecuteSp////
    {
        public static DataTable ejecutarSp(string nomSP, Hashtable parametros, string conexion)
        {
            SqlConnection conn = new SqlConnection(conexion);
            DataSet retorno = new DataSet();
            Dictionary<string, string> valuesHt = new Dictionary<string, string>();

            try
            {
                conn.Open();
                SqlCommand query = new SqlCommand(nomSP, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };

                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(query);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

        }


        public static DataTable ejecutarSpReporteV2(string nomSP, Hashtable parametros
                                    , string conexion, string timeOut)
        {
            SqlConnection conn = new SqlConnection(conexion);
            DataSet retorno = new DataSet();
            Dictionary<string, string> valuesHt = new Dictionary<string, string>();

            try
            {
                conn.Open();
                SqlCommand query = new SqlCommand(nomSP, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = Convert.ToInt32(timeOut)
                };

                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(query);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

        }


        public static DataTable updateColectivaMUC(string nomSP, Hashtable parametros
                                    , string conexion, string timeOut)
        {
            SqlConnection conn = new SqlConnection(conexion);
            DataSet retorno = new DataSet();
            Dictionary<string, string> valuesHt = new Dictionary<string, string>();

            try
            {
                conn.Open();
                SqlCommand query = new SqlCommand(nomSP, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = Convert.ToInt32(timeOut)
                };

                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(query);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

        }


        //Obtiene el ultimo id fihero detalle registrado y el ultimo id poliza registrado
        public static string obtenerIdFicheroDetalle_IDPoliza(string connT112, string tipoProceso)
        {
            SqlConnection conn = new SqlConnection(connT112);
            DataSet retorno = new DataSet();
            string idRespuesta = null;
            try
            {
                conn.Open();
                SqlCommand query = new SqlCommand("ProcNoct_COMP_ObtenerUltimo_ID_FicheroDetalle_ID_Poliza", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                };
                query.Parameters.AddWithValue("@tipoProceso", tipoProceso);

                SqlDataAdapter da = new SqlDataAdapter(query);
                DataTable dt = new DataTable();
                da.Fill(dt);

                idRespuesta = dt.Rows[0]["IdRespuesta"].ToString();

                return idRespuesta;
            }
            catch (Exception ex)
            {
                idRespuesta = "0";

                return idRespuesta;
            }
            finally
            {
                conn.Close();
            }

        }
    }
}
