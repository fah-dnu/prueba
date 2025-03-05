using CommonProcesador;
using DNU_ParabiliaBloqDesbloqueoTDC.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaBloqDesbloqueoTDC.CapaDatos
{
    public class DAOBloqueoDesbloqueo
    {
        private BaseDeDatos consultaBDRead;
        private DataTable dtResultado;
        public SqlConnection _connConsulta;
        public SqlTransaction _transaccionSQL;


        public List<Resultado> BloqueoTarjetasTDC(string usuarioServicio, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);

            List<Resultado> lstResultado = new List<Resultado>();

            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@usuarioServicio", parametro = usuarioServicio });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Ejecutor_BloqueoTDC]", parametros, "Procnoc_Ejecutor_BloqueoTDC", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "Bloqueo";

                foreach (DataRow row in dtResultado.Rows)
                {
                    Resultado resultado = new Resultado();
                    resultado.ID_Cuenta = Convert.ToInt64(row["ID_Cuenta"].ToString());
                    resultado.Tipo = row["Tipo"].ToString();
                    resultado.Mensaje = row["Mensaje"].ToString();
                    resultado.Bitacora = row["Bitacora"].ToString();

                    lstResultado.Add(resultado);
                }

                return lstResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al bloquear tarjetas:" + ex.Message + ex.StackTrace);
                return lstResultado;
            }
        }

        public List<Resultado> DesbloqueoTarjetasTDC(string usuarioServicio, string cadenaConexion = null, SqlConnection conn = null, DataSet conjuntoDatos = null)
        {
            if (cadenaConexion != null)
                consultaBDRead = new BaseDeDatos(cadenaConexion);

            List<Resultado> lstResultado = new List<Resultado>();

            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@usuarioServicio", parametro = usuarioServicio });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Ejecutor_DesBloqueoTDC]", parametros, "Procnoc_Ejecutor_DesBloqueoTDC", "", conn, null, null, null, conjuntoDatos);
                dtResultado.TableName = "Desbloqueo";

                foreach (DataRow row in dtResultado.Rows)
                {
                    Resultado resultado = new Resultado();
                    resultado.ID_Cuenta = Convert.ToInt64(row["ID_Cuenta"].ToString());
                    resultado.Tipo = row["Tipo"].ToString();
                    resultado.Mensaje = row["Mensaje"].ToString();
                    resultado.Bitacora = row["Bitacora"].ToString();

                    lstResultado.Add(resultado);
                }

                return lstResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("error al desbloquear tarjetas:" + ex.Message + ex.StackTrace);
                return lstResultado;
            }
        }


    }
}
