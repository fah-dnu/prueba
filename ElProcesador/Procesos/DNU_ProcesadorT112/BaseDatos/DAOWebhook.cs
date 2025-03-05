using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorT112.BaseDatos
{
    public class DAOWebhook
    {
        public static void actualizaEstatusEnvioNotificacion(int idFicheroDetalle)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    using (SqlCommand command = new SqlCommand("ProcNoct_ActualizaEstatusEnvioNotificacionFichero"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conn;
                        int resp = -1;

                        command.Parameters.Add(new SqlParameter("@ID_FicheroDetalle", idFicheroDetalle));
                        conn.Open();

                        resp = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error("[PROCESAT112] Error al actualizar el estatus del envío de la notificacion "+err.Message);
            }
        }
    }
}
