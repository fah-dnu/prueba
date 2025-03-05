using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SMARTTKT_GeneradorPedidos.BaseDatos
{
    /// <summary>
    /// Clase de control de objetos de acceso a datos a Ecommerce
    /// </summary>
    class DAOEcommerce
    {
        /// <summary>
        /// Consulta el listado de pedidos incompletos en base de datos
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static DataSet ListaPedidosIncompletos(int minutos, int maxIntentos)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDEcommerce.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("web_ProcNoct_ObtienePedidosIncompletos");

                database.AddInParameter(command, "@Minutos", DbType.Int32, minutos);
                database.AddInParameter(command, "@MaxNumIntentos", DbType.Int32, maxIntentos);

                return database.ExecuteDataSet(command);
            }
            catch (Exception ex)
            {
                throw new Exception("ListaPedidosIncompletos(): " + ex.Message);
            }
        }

        /// <summary>
        /// Actualiza la fecha de solicitud de códigos y el número de intentos
        /// del pedido en base de datos
        /// </summary>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        /// <param name="IdPedido">ID del pedido</param>
        public static void ActualizaSolicitudCodigosPedido(SqlConnection connection, SqlTransaction transaccionSQL, int IdPedido)
        {
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_ActualizaSolicitudPedidos", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@IdPedido", IdPedido));

                command.ExecuteNonQuery();
            }

            catch (Exception Ex)
            {
                throw new Exception("ActualizaPedidoIncompleto()", Ex);
            }
        }

        /// <summary>
        /// Consulta el listado de pedidos completos en base de datos
        /// </summary>
        /// <returns>DataSet con los registros</returns>
        public static DataSet ListaPedidosCompletos(int maxIntentos)
        {
            try
            {
                SqlDatabase database = new SqlDatabase(BDEcommerce.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("web_ProcNoct_ObtienePedidosCompletos");

                database.AddInParameter(command, "@MaxNumIntentos", DbType.Int32, maxIntentos);

                return database.ExecuteDataSet(command);
            }
            catch (Exception ex)
            {
                throw new Exception("ListaPedidosCompletos(): " + ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el estatus de pedido a completo en base de datos
        /// </summary>
        /// <param name="connection">Conexión SQL prestablecida a la BD</param>
        /// <param name="transaccionSQL">Transacción SQL prestablecida</param>
        /// <param name="IdPedido">ID del pedido</param>
        public static void ActualizaPedidoCompleto(SqlConnection connection, SqlTransaction transaccionSQL, int IdPedido)
        {
            try
            {
                SqlCommand command = new SqlCommand("ProcNoct_ActualizaPedidoCompletoOK", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaccionSQL;

                command.Parameters.Add(new SqlParameter("@IdPedido", IdPedido));

                command.ExecuteNonQuery();
            }

            catch (Exception Ex)
            {
                throw new Exception("ActualizaPedidoCompleto()", Ex);
            }
        }

        
    }
}
