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
    public class DAOReporteEstatusTarjeta
    {
        public DataTable ObtieneColectivasReporteEstatusTarjetas(string conexion)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPColectivasEstadoTarjetas")))
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

        public DataTable ObtieneDatosReporteEstatusTarjetas(string conexion, string ID_colectiva, string ID_TipoCuenta)
        {
            DataTable retorno = new DataTable();
            try
            {
                using (SqlConnection conLocal = new SqlConnection(conexion))
                {
                    using (SqlCommand command = new SqlCommand(PNConfig.Get("REPORTES_AZUREBS", "SPReporteEstadoTarjetas")))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = conLocal;
                        command.Parameters.Add(new SqlParameter("@Id_Colectiva", ID_colectiva));
                        command.Parameters.Add(new SqlParameter("@Id_tipocta", ID_TipoCuenta));
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
