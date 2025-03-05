using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbarConexionAzure
{
    class ConexionBase
    {


        private String _cadenaConexion { get; set; }

        public ConexionBase(string _cadenaConexion) {
            this._cadenaConexion = _cadenaConexion;
        }

        public static DataTable ejecutarSP(string sp) {


            return new DataTable();
        }


        public DataTable ConstruirConsulta(string procedimiento, List<ParametrosProcedimiento> parametros
                               , string nombreMetodo, string timeOut = null
                               , SqlConnection conn = null, SqlTransaction transaccionSQL = null
                               , List<ParametrosSalidaProcedimiento> parametrosDeSalida = null)
        {

            if (conn == null)
            {
                return GetResult(procedimiento, parametros, nombreMetodo, timeOut, parametrosDeSalida);
            }
            else
            {
                return GetResult(procedimiento, parametros, nombreMetodo, timeOut, parametrosDeSalida, conn, transaccionSQL);
            }


        }

        private DataTable GetResult(string procedimiento, List<ParametrosProcedimiento> parametros,
            string nombreMetodo, string timeOut, List<ParametrosSalidaProcedimiento> parametrosDeSalida,
            SqlConnection conn, SqlTransaction transaccionSQL)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (transaccionSQL != null)
                        cmd.Transaction = transaccionSQL;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = string.IsNullOrEmpty(timeOut)
                                           ? 30
                                           : Convert.ToInt32(timeOut);
                    cmd.CommandText = procedimiento;
                    //Deberia estar abierta , pero se corrobora que se abra la conexion
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    return GetExecutionResponse(cmd, parametros, parametrosDeSalida);


                }//Dispose SqlCommand
            }
            catch (Exception ex)
            {
                return DefaultResponse(nombreMetodo, ex);
            }
        }

        private DataTable GetResult(string procedimiento, List<ParametrosProcedimiento> parametros, string nombreMetodo,
            string timeOut, List<ParametrosSalidaProcedimiento> parametrosDeSalida)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_cadenaConexion))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        cmd.Connection = conn;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = string.IsNullOrEmpty(timeOut)
                                               ? 30
                                               : Convert.ToInt32(timeOut);
                        cmd.CommandText = procedimiento;
                        conn.Open();
                        return GetExecutionResponse(cmd, parametros, parametrosDeSalida);
                    }// Dispose SqlCommand
                }//Dispose SqlConnection
            }
            catch (Exception ex)
            {
                return DefaultResponse(nombreMetodo, ex);
            }
        }

        private DataTable GetExecutionResponse(SqlCommand cmd, List<ParametrosProcedimiento> parametros,
            List<ParametrosSalidaProcedimiento> parametrosDeSalida)
        {
            if (parametros != null && parametros.Count > 0)
            {
               
                foreach (ParametrosProcedimiento parametro in parametros)
                {
                    //cmd.Parameters.AddWithValue(parametro.Nombre, parametro.parametro);

                  
                 
                  
                        SqlParameter paramSSN = cmd.CreateParameter();
                        paramSSN.ParameterName = parametro.Nombre;
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = parametro.parametro;
                        paramSSN.Size = parametro.longitud;
                        cmd.Parameters.Add(paramSSN);
                    
                }
            }
            if (parametrosDeSalida != null && parametrosDeSalida.Count > 0)
            {
                foreach (ParametrosSalidaProcedimiento parametro in parametrosDeSalida)
                {
                    if (parametro.tipo == SqlDbType.VarChar)
                    {
                        cmd.Parameters.Add(parametro.Nombre, parametro.tipo, 1000).Direction = ParameterDirection.Output;

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(parametro.Nombre, parametro.tipo).Direction = ParameterDirection.Output;
                    }
                }
            }

            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                DataSet opcional = new DataSet();
                sda.Fill(opcional);
                if (parametrosDeSalida != null)
                {
                    if (parametrosDeSalida.Count > 0)
                    {
                        foreach (ParametrosSalidaProcedimiento parametro in parametrosDeSalida)
                        {
                            parametro.parametro = cmd.Parameters[parametro.Nombre].Value;
                        }
                    }
                }

                return opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
            }//Dispose SqlDataAdapter
        }

        private DataTable DefaultResponse(string nombreMetodo, Exception ex)
        {
            DataTable table = new DataTable();
            table.Columns.Add("tipo", typeof(string));
            table.Columns.Add("Mesaje", typeof(string));
            table.Columns.Add("MesajeReal", typeof(string));
            table.Columns.Add("Codigo", typeof(string));
            table.Rows.Add("error", "Error en base de datos", ex.Message, "9999");
            // System.Diagnostics.Debug.WriteLine("error" + ex.Message);
           

            return table;
        }

        public void verificarParametrosNulosString(List<ParametrosProcedimiento> parametros, string nombre, string parametroString)
        {
            if (!string.IsNullOrEmpty(parametroString))
            {
                parametros.Add(new ParametrosProcedimiento { Nombre = nombre, parametro = parametroString });
            }
        }
        public void verificarParametrosNulosString(List<ParametrosProcedimiento> parametros, string nombre, string parametroString, bool encriptado, int longitud)
        {
            if (!string.IsNullOrEmpty(parametroString))
            {
                parametros.Add(new ParametrosProcedimiento { Nombre = nombre, parametro = parametroString, encriptado = encriptado, longitud = longitud });
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

