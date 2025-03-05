using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;
using System.Data.Common;
using CommonProcesador;
using QUANTUM_BloquearDesbloquearCliente.Entidades;
using System.Data.SqlClient;

namespace QUANTUM_BloquearDesbloquearCliente.BaseDatos
{
    public class DAOCliente
    {

        public static Respuesta BloquearTarjeta(Cliente elCliente, Boolean SolicitaReemplazo, String Usuario)
        {
            SqlDataReader Reader=null;
            Respuesta laRespuesta = new Respuesta();
            SqlConnection conn = BDBloqueoDesbloqueo.BDEscritura;
            try
            {


                SqlParameter param = null;

               // conn.Open();

                SqlCommand comando = new SqlCommand("ws_Quantum_BloquearTarjeta", conn);
                comando.CommandType = CommandType.StoredProcedure;

                int resp = -1;

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = elCliente.Tarjeta;
                comando.Parameters.Add(param);

                param = new SqlParameter("@CodigoMotivo", SqlDbType.VarChar);
                param.Value = elCliente.CodigoMotivo;
                comando.Parameters.Add(param);

                param = new SqlParameter("@Sucursal", SqlDbType.VarChar);
                param.Value = elCliente.Sucursal;
                comando.Parameters.Add(param);


                param = new SqlParameter("@SolicitarReposicion", SqlDbType.Bit);
                param.Value = SolicitaReemplazo;
                comando.Parameters.Add(param);


                Reader = comando.ExecuteReader();

                if (null != Reader)
                {
                    while (Reader.Read())
                    {

                        laRespuesta.Saldo = 0;
                        laRespuesta.Tarjeta = elCliente.Tarjeta;
                        laRespuesta.Autorizacion = Reader["Autorizacion"].ToString();
                        laRespuesta.CodigoRespuesta = Int32.Parse(Reader["CodigoRespuesta"].ToString());
                        laRespuesta.Descripcion = "BLOQUEO AUTORIZADO";

                    }
                }


                Logueo.Evento("Se bloqueo la Tarjeta :" + elCliente.Tarjeta);
                return laRespuesta;

            }
            catch (Exception err)
            {
                Logueo.Error("Error :" + elCliente.Tarjeta + ", " + err.Message);
                
                throw err;
            }
            finally
            {
                try
                {
                    Reader.Close();
                    conn.Close();
                }
                catch (Exception err)
                {
                }
            }
        }


        public static Respuesta BloquearCuenta(Cliente elCliente, String Usuario)
        {
            SqlDataReader Reader=null;
            Respuesta laRespuesta = new Respuesta();
            SqlConnection conn = BDBloqueoDesbloqueo.BDEscritura;
            try
            {


                SqlParameter param = null;
               // conn.Open();

                SqlCommand comando = new SqlCommand("ws_Quantum_BloquearCuenta", conn);
                comando.CommandType = CommandType.StoredProcedure;

                int resp = -1;

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = elCliente.Tarjeta;
                comando.Parameters.Add(param);




                Reader = comando.ExecuteReader();

                if (null != Reader)
                {
                    while (Reader.Read())
                    {

                        laRespuesta.Saldo = 0;
                        laRespuesta.Tarjeta = elCliente.Tarjeta;
                        laRespuesta.Autorizacion = Reader["Autorizacion"].ToString();
                        laRespuesta.CodigoRespuesta = Int32.Parse(Reader["CodigoRespuesta"].ToString());
                        laRespuesta.Descripcion = "BLOQUEO AUTORIZADO";

                    }
                }



                Logueo.Evento("Se bloquearon las cuentas de la tarjeta :" + elCliente.Tarjeta);
                return laRespuesta;

            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                try
                {
                    Reader.Close();
                    conn.Close();
                }
                catch (Exception err)
                {
                }
            }
        }


        public static Respuesta ActivarTarjetaCuenta(Cliente elCliente, String Usuario)
        {

            SqlDataReader Reader=null;
            Respuesta laRespuesta = new Respuesta();
            SqlConnection conn = BDBloqueoDesbloqueo.BDEscritura;
            try
            {


                SqlParameter param = null;
               // conn.Open();

                SqlCommand comando = new SqlCommand("ws_Quantum_ActivarTarjetaBloqueada", conn);
                comando.CommandType = CommandType.StoredProcedure;

                int resp = -1;

                param = new SqlParameter("@Tarjeta", SqlDbType.VarChar);
                param.Value = elCliente.Tarjeta;
                comando.Parameters.Add(param);



                Reader = comando.ExecuteReader();

                if (null != Reader)
                {
                    while (Reader.Read())
                    {

                        laRespuesta.Saldo = Decimal.Parse(Reader["Saldo"].ToString());
                        laRespuesta.Tarjeta = elCliente.Tarjeta;
                        laRespuesta.Autorizacion = Reader["Autorizacion"].ToString();
                        laRespuesta.CodigoRespuesta = Int32.Parse(Reader["CodigoRespuesta"].ToString());
                        laRespuesta.Descripcion = Reader["DescripcionRespuesta"].ToString();

                    }
                }


                Logueo.Evento("Se Activaron la Tarjeta y sus cuentas:" + elCliente.Tarjeta);
                return laRespuesta;

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                throw err;
            }
            finally
            {
                try
                {
                    Reader.Close();
                    conn.Close();
                }
                catch (Exception err)
                {
                }
            }
        }

    }
}
