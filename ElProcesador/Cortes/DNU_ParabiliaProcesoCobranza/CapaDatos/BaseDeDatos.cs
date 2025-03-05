using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCobranza.CapaDatos
{
    public class BaseDeDatos
    {
        private readonly SqlConnection conexion;
        public BaseDeDatos(string cadenaConexion)
        {
            conexion = new SqlConnection(cadenaConexion);
        }

        public DataTable ConstruirConsulta(string procedimiento, List<ParametrosProcedimiento> parametros, string nombreMetodo, string timeOut = null, SqlConnection conn = null, SqlTransaction transaccionSQL = null, string idLog = null, List<ParametrosSalidaProcedimiento> parametrosDeSalida = null, DataSet conjuntoDatos = null)
        {
            DataTable retorno = new DataTable();

            try
            {
                if (conn is null)
                {
                    conexion.Open();
                }
                else
                {

                }
                SqlCommand query;
                if (conn is null)
                {

                    query = new SqlCommand(procedimiento, conexion);
                }
                else if (transaccionSQL != null)
                {

                    query = new SqlCommand(procedimiento, conn, transaccionSQL);

                }
                else
                {
                    query = new SqlCommand(procedimiento, conn);
                }
                query.CommandType = CommandType.StoredProcedure;
                query.CommandTimeout = string.IsNullOrEmpty(timeOut)
                                       ? 30
                                       : Convert.ToInt32(timeOut);
                if (parametros != null && parametros.Count > 0)
                {
                    foreach (ParametrosProcedimiento parametro in parametros)
                    {
                        // query.Parameters.AddWithValue(parametro.Nombre, parametro.parametro);
                        if (parametro.encriptado == false)
                        {
                            query.Parameters.AddWithValue(parametro.Nombre, parametro.parametro);
                        }
                        else
                        {
                            SqlParameter paramSSN = query.CreateParameter();
                            paramSSN.ParameterName = parametro.Nombre;
                            paramSSN.DbType = DbType.AnsiStringFixedLength;
                            paramSSN.Direction = ParameterDirection.Input;
                            paramSSN.Value = parametro.parametro;
                            paramSSN.Size = parametro.longitud;
                            query.Parameters.Add(paramSSN);
                        }
                    }
                }
                if (parametrosDeSalida != null && parametrosDeSalida.Count > 0)
                {
                    foreach (ParametrosSalidaProcedimiento parametro in parametrosDeSalida)
                    {
                        query.Parameters.AddWithValue(parametro.Nombre, parametro.tipo).Direction = ParameterDirection.Output;
                    }
                }

                SqlDataAdapter sda = new SqlDataAdapter(query);
                DataSet opcional = new DataSet();
                if (conjuntoDatos is null)
                {
                    sda.Fill(opcional);
                }
                else
                {
                    sda.Fill(conjuntoDatos);
                }
                if (parametrosDeSalida != null)
                {
                    if (parametrosDeSalida.Count > 0)
                    {
                        foreach (ParametrosSalidaProcedimiento parametro in parametrosDeSalida)
                        {
                            parametro.parametro = query.Parameters[parametro.Nombre].Value;
                        }
                    }
                }
                if (conjuntoDatos is null)
                {
                    retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                }
                else
                {
                    retorno = conjuntoDatos.Tables.Count > 0 ? conjuntoDatos.Tables[1] : null;
                }
            }
            catch (Exception ex)
            {
                DataTable table = new DataTable();
                table.Columns.Add("tipo", typeof(string));
                table.Columns.Add("Mesaje", typeof(string));
                table.Columns.Add("MesajeReal", typeof(string));
                table.Columns.Add("Codigo", typeof(string));
                table.Rows.Add("error", "Error en base de datos", ex.Message, "9999");
                // System.Diagnostics.Debug.WriteLine("error" + ex.Message);
                Logueo.Error(nombreMetodo + " " + ex.Message + " " + ex.StackTrace + ' ' + (string.IsNullOrEmpty(idLog) ? "" : "[" + idLog + "]"));

                retorno = table;
            }
            finally
            {
                if (conn is null)
                {
                    conexion.Close();
                }

            }
            return retorno;
        }

        public void verificarParametrosNulosString(List<ParametrosProcedimiento> parametros, string nombre, string parametroString)
        {
            if (!string.IsNullOrEmpty(parametroString))
            {
                parametros.Add(new ParametrosProcedimiento { Nombre = nombre, parametro = parametroString });
            }
        }
        public void verificarParametrosNulosObjetos(List<ParametrosProcedimiento> parametros, string nombre, object parametroObjeto)
        {
            //este lo uso porque no puedo hacer un cast to string para validar los valores que no son string ya que si hay un null marcaria error
            if (parametroObjeto != null)
            {
                parametros.Add(new ParametrosProcedimiento { Nombre = nombre, parametro = parametroObjeto });
            }
        }


    }
    public class ParametrosProcedimiento
    {
        public string Nombre;
        public object parametro;
        public bool encriptado = false;
        public int longitud;

    }
    public class ParametrosSalidaProcedimiento
    {
        public string Nombre;
        public object parametro;
        public SqlDbType tipo;

    }

}
