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
    public class DAOTransfronterizo
    {
        public DataTable ObtieneReporteTransaccionesIvaTransfronterizo(string conexion, string SP, string fecha, string ID_colectiva)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(SP))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@ClavePlugIn", ID_colectiva));
                        command.Parameters.Add(new SqlParameter("@fecha", fecha));
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

        public DataTable ObtienerReporteTransfronterizoTotalidadTransacciones(string conexion, string fechaIni, string fechaFin, string ID_colectiva)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteIVAT")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@ID_Colectiva", ID_colectiva));
                        command.Parameters.Add(new SqlParameter("@FechaInicio", fechaIni));
                        command.Parameters.Add(new SqlParameter("@FechaFinal", fechaFin));
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
