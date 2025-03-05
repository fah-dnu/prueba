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
    public class DAOReporteAsientosContables
    {
        public DataTable ObtieneColectivasReporteAsientosContables(string conexion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteAsientosContables_Colectivas")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
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

        public DataTable ObtieneDatosReporteAsientosContablesDetalle(string conexion, string ID_colectiva)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteAsientosContables_Detallado")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@fecha", DateTime.Now.AddDays(-1).ToString("yyyyMMdd")));
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", ID_colectiva));
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

        public DataTable ObtenerClaveReporte(string conexion, string idColectiva, string NombreSP)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteObtieneFTP")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", idColectiva));
                        command.Parameters.Add(new SqlParameter("@NombreSP", NombreSP));
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
