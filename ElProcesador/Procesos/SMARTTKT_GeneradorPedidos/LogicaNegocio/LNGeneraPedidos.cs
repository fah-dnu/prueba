using CommonProcesador;
using SMARTTKT_GeneradorPedidos.BaseDatos;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SMARTTKT_GeneradorPedidos.LogicaNegocio
{
    public class LNGeneraPedidos
    {
        public static bool GeneraPedidosFaltantes()
        {
            try
            {
                string idPedido = "";
                int minutos = int.Parse(PNConfig.Get("GENERAPEDIDOS", "MinutosReintento"));
                int maxIntentos = int.Parse(PNConfig.Get("GENERAPEDIDOS", "NumMaxIntentos"));

                //Se consultan los pedidos incompletos
                DataSet dsPedidosIncompletos = DAOEcommerce.ListaPedidosIncompletos(minutos, maxIntentos);
                Logueo.Evento("Se Consultaron los Pedidos Smart-Ticket Incompletos");

                //Se solicitan sus códigos
                foreach (DataRow pedido in dsPedidosIncompletos.Tables[0].Rows)
                {
                    idPedido = pedido["ID_Pedido"].ToString();

                    try
                    {
                        //Se realiza la solicitud de códigos al Web Service
                        WebService.WS_Promociones(idPedido);

                        //Se actualizan los datos de la solicitud del pedido
                        MarcaSolicitudCodigosPedido(int.Parse(idPedido));

                        Logueo.Evento("Se solicitaron los códigos del pedido no. " + idPedido);
                    }

                    catch (Exception err)
                    {
                        Logueo.Error("ERROR: Generación de Códigos del Pedido: " + idPedido + ": " + err.Message);
                    }
                }


                //Se consultan los pedidos completos
                DataSet dsPedidosCompletos = DAOEcommerce.ListaPedidosCompletos(maxIntentos);

                //Se marcan como completos
                foreach (DataRow _Pedido in dsPedidosCompletos.Tables[0].Rows)
                {
                    idPedido = _Pedido["ID_Pedido"].ToString();

                    try
                    {
                        MarcaPedidoCompletoOK(int.Parse(idPedido));

                        Logueo.Evento("Se marcó el pedido como completo. Pedido no. " + idPedido);
                    }

                    catch (Exception err)
                    {
                        Logueo.Error("ERROR: No se marcó pedido como completo. Pedido no. : " + idPedido + ": " + err.Message);
                    }
                }

                return true;
            }

            catch (Exception err)
            {
                Logueo.Error("GeneraPedidosFaltantes():" + err.Message);
                return false;
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la marca de un pedido
        /// con solicitud de códigos OK en base de datos
        /// </summary>
        /// <param name="IdPedido">Identificador del pedido</param>
        public static void MarcaSolicitudCodigosPedido(int IdPedido)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDEcommerce.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOEcommerce.ActualizaSolicitudCodigosPedido(conn, transaccionSQL, IdPedido);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se actualizó la solicitud de códigos del pedido " + IdPedido.ToString() + " en base de datos");
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("MarcaSolicitudCodigosPedido() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para la marca de un pedido
        /// como completo en base de datos
        /// </summary>
        /// <param name="IdPedido">Identificador del pedido</param>
        public static void MarcaPedidoCompletoOK(int IdPedido)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDEcommerce.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOEcommerce.ActualizaPedidoCompleto(conn, transaccionSQL, IdPedido);
                        transaccionSQL.Commit();
                        Logueo.Evento("Se marcó el pedido " + IdPedido.ToString() + " como completo en base de datos");
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("MarcaPedidoCompletoOK() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        
    }
}
