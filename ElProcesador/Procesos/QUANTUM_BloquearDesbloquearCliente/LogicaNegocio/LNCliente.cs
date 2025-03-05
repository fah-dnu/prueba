using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using QUANTUM_AplicarMovimientos.Entidades;
using Interfases.Entidades;
using QUANTUM_BloquearDesbloquearCliente.Entidades;
using QUANTUM_BloquearDesbloquearCliente.BaseDatos;
using System.Data.SqlClient;
using CommonProcesador;
using System.Data;

namespace QUANTUM_BloquearDesbloquearCliente.LogicaNegocio
{
    public class LNCliente
    {
        static wsQuantumBloquearDesbloquear.Quantum unaOperacion = new wsQuantumBloquearDesbloquear.Quantum();
        private static Respuesta ModificarCliente(Cliente elCliente)
        {
            Respuesta laRespuesta = new Respuesta();
            int Resp = -1;
            //  Parametro[] losParamentros = null;
            try
            {


              
                        try
                        {

                            if (elCliente.TipoMovimiento.Equals("01")) //bloqueo de cuenta
                            {
                               laRespuesta= DAOCliente.BloquearCuenta(elCliente, "ProcesadorNocturno");
                               laRespuesta= DAOCliente.BloquearTarjeta(elCliente,false, "ProcesadorNocturno");
                            }
                            else if (elCliente.TipoMovimiento.Equals("02")) // desbloqueo de cuenta
                            {
                               laRespuesta= DAOCliente.ActivarTarjetaCuenta(elCliente, "ProcesadorNocturno");
                            }
                            else if (elCliente.TipoMovimiento.Equals("03")) //cancelar y reponer tarjeta
                            {
                               laRespuesta= DAOCliente.BloquearTarjeta(elCliente, true, "ProcesadorNocturno");
                                
                            }

                        }

                        catch (Exception err)
                        {
                            laRespuesta.CodigoRespuesta = 12;
                            Logueo.Error(err.Message + " BloquearDesbloquear.LNEventos.EjecuarEvento()");
                        }
                  
            }
            catch (Exception err)
            {
                laRespuesta.CodigoRespuesta = 13;
                Logueo.Error(err.Message + " BloquearDesbloquear.LNEventos.EjecuarEvento()");
            }

            return laRespuesta;

        }


        public static Boolean Modificar()
        {


            List<Respuesta> lasRespuestas = new List<Respuesta>();


            foreach (Cliente elCliente in obtieneClientesBloquear())
            {
                try
                {

                    Logueo.EntradaSalida("PETICION: BLOQUEO DE TARJETA: Consecutivo:" + elCliente.Consecutivo + ", ID_Cliente: " + elCliente.ID_Cliente + ", Tarjeta: " + elCliente.Tarjeta + ", TIpoMovimiento:" + elCliente.TipoMovimiento + "CodigoMotivo:" + elCliente.CodigoMotivo, "PROCESADOR", false);
                    Respuesta unaRespuesta = ModificarCliente(elCliente);

                    lasRespuestas.Add(unaRespuesta);

                    unaOperacion.BloqueoDesbloqueo_respuesta(int.Parse(elCliente.Consecutivo),
                        elCliente.ID_Cliente, elCliente.Tarjeta, unaRespuesta.CodigoRespuesta.ToString(), unaRespuesta.Descripcion);

                    Logueo.EntradaSalida("RESPUESTA: BLOQUEO DE TARJETA: Consecutivo:" + elCliente.Consecutivo + ", ID_Cliente: " + elCliente.ID_Cliente + ", Tarjeta: " + elCliente.Tarjeta + ", Codigo Respuesta: " + unaRespuesta.CodigoRespuesta.ToString() + ", Descripcion:" + unaRespuesta.Descripcion, "PROCESADOR", false);
                }
                catch (Exception err)
                {
                    Logueo.Error("LNCliente.Modificar():" + elCliente.ID_Cliente +" -" + err.Message);
                }
            }


            // Generar Archivo de Respuestas.


            return true;
        }



        private static List<Cliente> obtieneClientesBloquear()
        {
            List<Cliente> losClientes = new List<Cliente>();

            //  Cliente unClienteNuevo1 = new Cliente();

            //        unClienteNuevo1.Consecutivo = "01";//renglon["CONSECUTIVO"].ToString();
            //        unClienteNuevo1.ID_Cliente = "00000000000001"; //renglon["REFERENCIAPAGO"].ToString();
            //        unClienteNuevo1.Tarjeta = "2001010000002069"; //renglon["TARJETA"].ToString();
            //        unClienteNuevo1.TipoMovimiento = "03";//renglon["TIPOMOVIMIENTO"].ToString();
            //        unClienteNuevo1.Sucursal = "SPBR001"; //renglon["SUCURSAL"].ToString();
            //        unClienteNuevo1.CodigoMotivo ="03"; //renglon["CODIGOMOTIVO"].ToString();

            //        losClientes.Add(unClienteNuevo1);

            //return losClientes;

            try
            {

                //obtiene el dataset del ws
                //wsQuantumBloquearDesbloquear.Quantum unaOperacion = new wsQuantumBloquearDesbloquear.Quantum();

                DataSet losUsuarios = unaOperacion.BloqueoDesbloqueo(DateTime.Now.ToString("yyyy-MM-dd"));


                foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                {
                    Cliente unClienteNuevo = new Cliente();

                    unClienteNuevo.Consecutivo = renglon["CONSECUTIVO"].ToString();
                    unClienteNuevo.ID_Cliente = renglon["REFERENCIAPAGO"].ToString();
                    unClienteNuevo.Tarjeta = renglon["TARJETA"].ToString();
                    unClienteNuevo.TipoMovimiento = renglon["TIPOMOVIMIENTO"].ToString();
                    unClienteNuevo.Sucursal = renglon["SUCURSAL"].ToString();
                    unClienteNuevo.CodigoMotivo = renglon["CODIGOMOTIVO"].ToString();

                    losClientes.Add(unClienteNuevo);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("obtieneNuevosClientes()" + err.Message);
            }

            return losClientes;
        }
    }
}
