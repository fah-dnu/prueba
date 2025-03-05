using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ParabiliaAsignarLimiteCredito.Entidades;
using DNU_ParabiliaAsignarLimiteCredito.Utilidades;
using log4net;

namespace DNU_ParabiliaAsignarLimiteCredito.BaseDatos
{
    public class DAOArchivo
    {

        internal static bool ColectivaValida(string claveColectiva, SqlConnection conn)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                var result = false;
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP proc_ValidaColectiva]");
                using (SqlCommand command = new SqlCommand("proc_ValidaColectiva", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    int resp = -1;

                    command.Parameters.Add(new SqlParameter("@ClaveColectiva", claveColectiva));
                    var param = new SqlParameter("@Resultado", result);
                    param.Direction = ParameterDirection.Output;

                    command.Parameters.Add(param);



                    resp = command.ExecuteNonQuery();

                    result = Convert.ToBoolean(command.Parameters["@Resultado"].Value);
                    return result;
                }

            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
               return false;
            }
        }

    }
}
