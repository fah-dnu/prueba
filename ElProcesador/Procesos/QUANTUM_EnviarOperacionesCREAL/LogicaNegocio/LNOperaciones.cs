using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QUANTUM_EnviarOperacionesCREAL.Entidades;
using CommonProcesador;
using DALAutorizador.BaseDatos;
using System.Data;

namespace QUANTUM_EnviarOperacionesCREAL.LogicaNegocio
{
    public class LNOperaciones
    {
        // static
        delegate void EnviarDetalle(Operacion laOperacion);

        public static List<Operacion> ObtenerOperacionesPorFecha(DateTime laFecha)
        {
            List<Operacion> lasOperaciones = new List<Operacion>();
            try
            {
                DataSet losUsuarios = DAOOperacion.ListarOperacionesDelDia(laFecha);


                foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                {
                    Operacion unMovimiento = new Operacion();

                    try
                    {
                        unMovimiento.Afiliacion = renglon["Afiliacion"].ToString();
                        unMovimiento.Autorizacion = renglon["Autorizacion"].ToString();
                        unMovimiento.Beneficiario = renglon["Beneficiario"].ToString();
                        unMovimiento.CodigoMoneda = renglon["CodigoMoneda"].ToString();
                        unMovimiento.CodigoProceso = renglon["CodigoProceso"].ToString(); ;
                        unMovimiento.FechaAlta = (DateTime)renglon["FechaAlta"];
                        unMovimiento.FechaRegistro = (DateTime)renglon["FechaRegistro"];
                        unMovimiento.ID_Cliente = renglon["ID_Cliente"].ToString();
                        unMovimiento.ID_Operacion = (Int64)renglon["ID_Operacion"];
                        unMovimiento.ID_OperacionActualizar = (Int64)renglon["ID_OperacionOriginal"];
                        unMovimiento.Importe = float.Parse(renglon["Importe"].ToString());
                        unMovimiento.Mensualidades = renglon["Mensualidades"].ToString();
                        unMovimiento.Operador = renglon["Operador"].ToString();
                        unMovimiento.Sucursal = renglon["Sucursal"].ToString();
                        unMovimiento.Tarjeta = renglon["Tarjeta"].ToString();
                        unMovimiento.Terminal = renglon["Terminal"].ToString();
                        unMovimiento.Ticket = renglon["Ticket"].ToString(); 
                        unMovimiento.TipoOperacion = renglon["TipoOperacion"].ToString();
                        unMovimiento.PagoInicial = renglon["PagoInicial"].ToString();


                        lasOperaciones.Add(unMovimiento);
                    }
                    catch (Exception err)
                    {
                        Logueo.Error("ObtenerOperacionesPorFecha Un Detalle ()" + err.Message);
                    }

                }
            }
            catch (Exception err)
            {
                Logueo.Error("ObtenerOperacionesPorFecha()" + err.Message);
            }


            return lasOperaciones;
        }

        public static Boolean EnviarOperacionesProcesadas()
        {
            try
            {
                foreach (Operacion laOpera in ObtenerOperacionesPorFecha(DateTime.Now))
                {
                    /*INICIA MULTIPROCESO*/
                    /********MultiHilo***************

                    //ActualizaOper(elDetalle);

                    LNOperaciones EjetutaActualizar = new LNOperaciones();
                    //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                    EnviarDetalle ProcesoAEjecutar = EjetutaActualizar.EnviaLaOperacion;


                    //Ejecuta el metodo asincrono

                    ProcesoAEjecutar.BeginInvoke(laOpera,
                    delegate(IAsyncResult ar1)
                    {
                        try
                        {
                            ProcesoAEjecutar.EndInvoke(ar1);
                        }
                        catch (Exception ex)
                        {

                        }
                    }, null);*/
                    EnviaLaOperacion(laOpera);
                }

                /*TERMINAL MULTIPROCESO*/
                Logueo.Evento("Se enviaron todas las operaciones");
                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("EnviarOperacionesProcesadas()" + err.Message);
                return false;
            }
        }

        public static void EnviaLaOperacion(Operacion laOperacion)
        {
            try
            {

                try
                {
                    mx.com.creditoreal.quantum.Quantum unaOperacion = new mx.com.creditoreal.quantum.Quantum();


                    Logueo.EntradaSalida("ID_Operacion: " + laOperacion.ID_Operacion.ToString() + ",  Beneficiario:" +
                        laOperacion.Beneficiario + ",  Tarjeta:" +
                        laOperacion.Tarjeta + ",  ID_Cliente:" +
                        laOperacion.ID_Cliente + ",  FechaRegistro:" +
                        laOperacion.FechaRegistro.ToString("yyyy-MM-dd") + ",  Importe:" +
                        laOperacion.Importe.ToString() + ",  CodigoMoneda:" +
                        laOperacion.CodigoMoneda + ",  Sucursal:" +
                        laOperacion.Sucursal + ",  Afiliacion:" +
                        laOperacion.Afiliacion + ",  Terminal:" +
                        laOperacion.Terminal + ",  Ticket:" +
                        laOperacion.Ticket + ",  Operador:" +
                        laOperacion.Operador + ",  Autorizacion:" +
                        laOperacion.Autorizacion + ",  Mensualidades:" +
                        laOperacion.Mensualidades + ",  TipoOperacion:" +
                        laOperacion.TipoOperacion + ",  CodigoProceso:" +
                        laOperacion.CodigoProceso + ",  PagoInicial:" +
                        laOperacion.PagoInicial, "Procesador Nocturno", true);

                    DataSet laRespuesta = unaOperacion.Transaccion(
                           Int32.Parse(laOperacion.ID_Operacion.ToString()),
                           laOperacion.Beneficiario,
                           laOperacion.Tarjeta,
                           laOperacion.ID_Cliente,
                           laOperacion.FechaRegistro.ToString("yyyy-MM-dd"),
                           Decimal.Parse(laOperacion.Importe.ToString()),
                           laOperacion.CodigoMoneda,
                           laOperacion.Sucursal,
                           laOperacion.Afiliacion,
                           laOperacion.Terminal,
                           laOperacion.Ticket,
                           laOperacion.Operador,
                           laOperacion.Autorizacion,
                           laOperacion.Mensualidades,
                           laOperacion.TipoOperacion,
                           laOperacion.CodigoProceso, 
                           laOperacion.PagoInicial);

                    Int32 laResp = -1;
                    try
                    {
                        laResp = Int32.Parse(laRespuesta.Tables[0].Rows[0][0].ToString());

                    }
                    catch (Exception err)
                    {
                        Logueo.Error("EnviarOperaciones.Ciclo " + err.ToString());
                        Logueo.Error("La operación [" + laOperacion.ID_Operacion + "] [" + laOperacion.ID_Cliente + "] GENERO UN ERROR AL RECIBIR LA RESPUESTA");

                        laResp = -1;
                    }

                    Logueo.EntradaSalida("Respuesta: " + laResp, "Procesador Nocturno", false);

                    if (laResp == 1)
                    {
                        Logueo.Evento("La operación [" + laOperacion.ID_Operacion + "] [" + laOperacion.ID_Cliente + "]  ha sido Informada a Credito Real sin Problemas reportados");

                        try
                        {
                            DAOOperacion.IndicarOperacionEnviada(laOperacion.ID_OperacionActualizar, "1", "CR_ENVOPER", "Procesador");
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("EnviarOperaciones.Ciclo " + err.ToString());
                            Logueo.Error("La operación [" + laOperacion.ID_Operacion + "] [" + laOperacion.ID_Cliente + "] GENERO UN ERROR AL CAMBIAR ESTATUS A ENVIADA");
                            laResp = -1;

                        }
                    }
                    else
                    {
                        Logueo.Evento("La operación [" + laOperacion.ID_Operacion + "] [" + laOperacion.ID_Cliente + "]  ha sido Informada a Credito Real, pero la respuesta fue no OK");
                    }



                }
                catch (Exception err)
                {
                    Logueo.Evento("La operación [" + laOperacion.ID_Operacion + "] GENERO UN ERROR EN EL ENVIO");
                    Logueo.Error("La operación [" + laOperacion.ID_Operacion + "] GENERO UN ERROR EN EL ENVIO");
                    Logueo.Error("EnviarOperaciones.Ciclo " + err.ToString());
                }

                //  return true;
            }
            catch (Exception err)
            {
                Logueo.Error("EnviarOperaciones()" + err.Message);
                // return false;
            }
        }
    }
}
