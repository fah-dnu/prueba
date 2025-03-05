using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaReportesAzureBlobStorage.BaseDatos
{
    public class DAOReporteABU
    {
        public DataTable ObtieneIcasReporteABU(string conexion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPIcasReporteABU")))
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

        public DataTable ObtieneDatosReporteABU(string conexion, string Ica, int opcion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteABU")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@fecha", DateTime.Now.ToString("yyyy-MM-dd")));
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", Ica));
                        command.Parameters.Add(new SqlParameter("@Opcion", opcion));
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

        public DataTable ObtieneDatosDepositoArchivo(string conexion, string ID_colectiva, string NombreSP)
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
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", ID_colectiva));
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
