using CommonProcesador;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoreoCacaoPN.CapaDatos
{
    public class DAOOperaciones
    {
        public DataTable executeSPDT(string nomSP, Hashtable parametros)
        {
            SqlConnection conexion = new SqlConnection(PNConfig.Get("MONCACAO", "SettingDatabase"));
            DataSet retorno = new DataSet();
            Dictionary<string, string> valuesHt = new Dictionary<string, string>();

            try
            {
                conexion.Open();
                SqlCommand query = new SqlCommand(nomSP, conexion)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };

                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                        valuesHt.Add(parametro.Key.ToString(), parametro.Value == null ? "" : parametro.Value.ToString());
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(query);
                DataTable dt = new DataTable();
                Logueo.EntradaSalida("[executeSPDT] [" + nomSP + "] " + JsonConvert.SerializeObject(valuesHt), "", false);
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }

        }

        public string executeSP(string nomSP, Hashtable parametros)
        {
            SqlConnection conexion = new SqlConnection(PNConfig.Get("MONCACAO", "SettingDatabase"));
            DataSet retorno = new DataSet();
            Dictionary<string, string> valuesHt = new Dictionary<string, string>();
            string _respuesta;

            try
            {
                conexion.Open();
                SqlCommand query = new SqlCommand(nomSP, conexion)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };

                if (parametros != null && parametros.Count > 0)
                {
                    foreach (DictionaryEntry parametro in parametros)
                    {
                        query.Parameters.AddWithValue(parametro.Key.ToString(), parametro.Value);
                        valuesHt.Add(parametro.Key.ToString(), parametro.Value == null ? "" : parametro.Value.ToString());
                    }
                }

                SqlParameter outparamMensaje = query.Parameters.Add("@mensajeRespuesta", SqlDbType.VarChar, -1);
                outparamMensaje.Direction = ParameterDirection.Output;

                Logueo.EntradaSalida("[executeSP] [" + nomSP + "] " + JsonConvert.SerializeObject(valuesHt), "", false);
                query.ExecuteNonQuery();

                _respuesta = query.Parameters["@mensajeRespuesta"].Value.ToString();

                Logueo.EntradaSalida("[executeSP] [" + nomSP + "] " + JsonConvert.SerializeObject(_respuesta), "", true);
                return _respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }

        }
    }
}
