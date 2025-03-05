using DNU_AppConnectValidacionUsuarios.BaseDatos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_AppConnectValidacionUsuarios.LogicaNegocio
{
    public class LNLimpiarUsuarios
    {

        public static void ProcesarRegistrosUsuarios()
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDAppConnect.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOAppConnect.LimpiarUsuariosNoValidados();
                        transaccionSQL.Commit();
                        Logueo.Evento("Se realizo la Depuración de los Usuarios no Validados.");
                    }
                    catch (Exception ex)
                    {
                        transaccionSQL.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("ProcesarRegistrosUsuarios(): " + ex.Message);

                return false;
            }
        }
    }
}
