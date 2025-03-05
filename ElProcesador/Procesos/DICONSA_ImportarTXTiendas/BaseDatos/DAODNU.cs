using CommonProcesador;
using DICONSA_ImportarTXTiendas.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DICONSA_ImportarTXTiendas.BaseDatos
{
    class DAODNU
    {
        /// <summary>
        /// Consulta el listado de tiendas Diconsa activas en base de datos
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static List<TiendaDiconsa> ListaTiendasActivas()
        {
            try
            {
                DataSet dsTiendas = new DataSet();
                List<TiendaDiconsa> response = new List<TiendaDiconsa>();


                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ListaTiendasDiconsa");

                database.AddInParameter(command, "@TipoProceso", DbType.Int32, 1);

                dsTiendas = database.ExecuteDataSet(command);

                if (null != dsTiendas)
                {
                    for (int counter = 0; counter < dsTiendas.Tables[0].Rows.Count; counter++)
                    {
                        response.Add(new TiendaDiconsa(Convert.ToInt32(dsTiendas.Tables[0].Rows[counter]["ID_Colectiva"].ToString()), Convert.ToBoolean(dsTiendas.Tables[0].Rows[counter]["Procesada"].ToString())));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("ListaTiendasActivas(): " + ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las credenciales para la conexión al Web Service de Sr Pago en base de datos
        /// </summary>
        /// <param name="IdColectiva">Identificador de la tienda</param>
        /// <returns>DataSet con las credenciales</returns>
        public static DataSet ObtenerCredencialesWS(Int32 IdColectiva)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDServicios.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ObtieneCredencialesSrPago");

                database.AddInParameter(command, "@IdColectiva", DbType.Int32, IdColectiva);

                return database.ExecuteDataSet(command);
            }
            catch (Exception ex)
            {
                throw new Exception("ObtenerCredencialesWS(): " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta un registro de resumen de operación Tienda Diconsa en base de datos
        /// </summary>
        /// <param name="dtResumenOp">Datos del resumen de operación</param>
        /// <param name="connection">Objeto con la conexión SQL prestablecida</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        /// <returns>Identificador del resumen de operación insertado</returns>
        public static int InsertaResumenOperaciones(DataTable dtResumenOp, SqlConnection connection, SqlTransaction transaccionSQL)
        {
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_InsertaResumenOperacionesTiendaDiconsa", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@tablaTmpResumen", dtResumenOp));
                command.Parameters.Add("@IdResumen", SqlDbType.Int).Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();

                return Convert.ToInt32(command.Parameters["@IdResumen"].Value);
            }

            catch (Exception ex)
            {
                throw new Exception("InsertaResumenOperaciones(): " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta los registros de las operaciones de la Tienda Diconsa correspondiente en base de datos
        /// </summary>
        /// <param name="dtDetalleOp">Datos de las operaciones</param>
        /// <param name="connection">Objeto con la conexión SQL prestablecida</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        public static void InsertaOperaciones(DataTable dtDetalleOp, SqlConnection connection, SqlTransaction transaccionSQL)
        {
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_InsertaOperacionesTiendaDiconsa", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@tablaTmpDetalle", dtDetalleOp));

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("InsertaOperaciones(): " + ex.Message);
            }
        }

        /// <summary>
        /// Actualiza en base de datos el estatus de procesado de la tienda
        /// </summary>
        /// <param name="IdColectiva">Identificador de la colectiva</param>
        public static void ActualizaEstatusTienda(Int32 IdColectiva)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDAutorizador.strBDEscritura);
                DbCommand command = database.GetStoredProcCommand("ProcNoct_ActualizaEstatusTiendaDiconsa");

                database.AddInParameter(command, "@IdColectiva", DbType.Int32, IdColectiva);
                database.AddInParameter(command, "@TipoProceso", DbType.Int32, 1);

                database.ExecuteNonQuery(command);
            }

            catch (Exception ex)
            {
                throw new Exception("DAODNU.ActualizaEstatusTienda(): " + ex.Message);
            }
        }
    }
}
