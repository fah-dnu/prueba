using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.BaseDatos
{
    public class DAOTrackOrder
    {
        public static bool InsertTrackOrder(string consecutivo, string cuenta, string folio, string noGuia, string userID, SqlConnection conn, SqlTransaction transaccionSQL)
        {
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_InsertaTrackOrder", conn);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@Consecutivo", consecutivo));
                command.Parameters.Add(new SqlParameter("@Cuenta", cuenta));
                command.Parameters.Add(new SqlParameter("@Folio", folio));
                command.Parameters.Add(new SqlParameter("@NoGuia", noGuia));
                command.Parameters.Add(new SqlParameter("@UserID", userID));

                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception err)
            {
                Logueo.Error(("InsertTrackOrder(): " + err.Message));
                throw new Exception(err.Message);
            }
        }
    }
}
