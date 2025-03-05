using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaReportesAzureBlobStorage.BaseDatos
{
    public class DAOReporteFondeo
    {
        public DataTable DBGetObtieneReporteMovimientosDiario(string colectiva, string fecha, string conexion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteMovimientosDiario")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@fecha", fecha));
                        command.Parameters.Add(new SqlParameter("@IdColectiva", colectiva));
                        conLocal.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(command);
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);
                        retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                retorno = null;
            }

            return retorno;
        }

        public DataTable DBGetObtieneReporteFondeo(string conexion, string fecha, string colectiva )
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteFondeo")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@fecha", fecha));
                        command.Parameters.Add(new SqlParameter("@IdColectiva", colectiva));
                        conLocal.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(command);
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);
                        retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                retorno = null;
            }

            return retorno;
        }

        public DataTable DBGetObtieneReporteActividadDiaria(string colectiva, string fecha, string conexion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteDA")))
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@fecha", fecha));
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", colectiva));
                        conLocal.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(command);
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);
                        retorno = opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                retorno = null;
            }

            return retorno;
        }


    }
}
