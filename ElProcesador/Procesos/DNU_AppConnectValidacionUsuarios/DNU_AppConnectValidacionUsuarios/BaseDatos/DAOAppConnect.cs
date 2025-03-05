using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_AppConnectValidacionUsuarios.BaseDatos
{
    class DAOAppConnect
    {
        public static void LimpiarUsuariosNoValidados()
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAppConnect.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_LimpiarUsuariosNoValidados");

                database.ExecuteNonQuery(command);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
