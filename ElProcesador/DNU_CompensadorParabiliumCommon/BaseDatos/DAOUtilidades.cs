using DNU_CompensadorParabiliumCommon.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    public class DAOUtilidades
    {
        /// <summary>
        /// Metodo que obtiene los contratos por cliente.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ID_GrupoCuenta"></param>
        /// <param name="ID_Colectiva"></param>
        /// <returns></returns>
        public static DataTable ObtieneParametrosCliente(string conexion, string ID_GrupoCuenta, string ID_Colectiva)
        {
            DataTable contratos = new DataTable();
            try
            {
                SqlConnection conn = new SqlConnection(conexion);

                string SP_CONTRATOS = "[Procnoc_Comp_ObtenerDatosContratos]";
                conn.Open();
                using (SqlCommand command = new SqlCommand(SP_CONTRATOS, conn))
                {
                    command.Parameters.Add(new SqlParameter("@ID_Colectiva", ID_Colectiva));
                    command.Parameters.Add(new SqlParameter("@ID_GrupoCuenta", ID_GrupoCuenta));
                    command.CommandType = CommandType.StoredProcedure;
                    contratos.Load(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            {

            }
            return contratos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtDetalladoPreliminar"></param>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static DataTable ObtenerCuentaInterna(DataTable dtDetalladoPreliminar, string conexion)
        {
            
            DataTable dtConfiguracion = new DataTable();
            try
            {

                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.BulkCopyTimeout = 60;
                        bulkCopy.DestinationTableName = "dbo.TblRegistrosMATemp";
                        try
                        {
                            bulkCopy.WriteToServer(dtDetalladoPreliminar);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("GuardarTblRegistrosMATemp_Bulk " + ex.Message);
                        }
                    }

                    string SP_CI = "[Procnoc_Comp_ObtenerCuentaInterna]";

                    SqlCommand command = new SqlCommand(SP_CI, conn);
                    command.CommandType = CommandType.StoredProcedure;

                    dtConfiguracion.Load(command.ExecuteReader());
                }
            }
            catch (Exception ex)
            { 
                
            }
            return dtConfiguracion;
        }
    }
}
