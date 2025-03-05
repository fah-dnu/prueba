using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QUANTUM_AplicarMovimientos.Entidades;
using Interfases.Entidades;
using System.Data;
using CommonProcesador;
using QUANTUM_AplicarMovimientos.BaseDatos;
using System.Data.SqlClient;
using Executer.BaseDatos;

namespace QUANTUM_AplicarMovimientos.LogicaNegocio
{


    class LNAplicarMovimientos
    {
        delegate void AplicaMovi(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();


        static wsQuantumApMov.Quantum unaOperacion = new wsQuantumApMov.Quantum();

        public static Respuesta AplicarMovimiento(int ID_Evento, int ID_Contrato, int DiasVigencia, String IDCliente, String Tarjeta, String Concepto, String Observaciones, Int64 Consecutivo, Dictionary<String, Parametro> TodosLosParametros)
        {

            Respuesta unaRespo2 = new Respuesta();


            unaRespo2.Tarjeta = Tarjeta;

            try
            {


                int Resp = -1;
                //if (ID_Contrato == 0 | ID_Evento == 0)
                //{
                //    throw new Exception("No se pudo obtener un ID_Evento o Contraro valido");
                //}



                using (SqlConnection conn = new SqlConnection(BDAplicarMov.strBDEscritura))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {


                            //Genera y Aplica la Poliza
                            Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, ID_Contrato, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, Consecutivo);
                            Resp = aplicador.AplicaContablilidad();

                            //Guardar el Lote
                            if (Resp != 0)
                            {
                                throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                            }
                            else
                            {
                                unaRespo2.CodigoRespuesta = Resp;
                                unaRespo2.Autorizacion = DateTime.Now.ToString("HHmmss");
                                //unaRespo2.Descripcion = "AUTORIZADA
                                unaRespo2.Descripcion = "AUTORIZADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();
                                transaccionSQL.Commit();
                            }

                        }

                        catch (Exception err)
                        {
                            Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message);
                            unaRespo2.CodigoRespuesta = 87;
                            unaRespo2.Autorizacion = "000000";
                            //unaRespo2.Descripcion = "DECLINADO";
                            unaRespo2.Descripcion = "DECLINADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();
                            transaccionSQL.Rollback();
                        }

                    }
                }


            }
            catch (Exception err)
            {
                Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message);
                unaRespo2.CodigoRespuesta = 87;
                unaRespo2.Autorizacion = "000000";
                unaRespo2.Descripcion = "DECLINADO";

            }

            return unaRespo2;
        }


        public static bool AplicarMovimiento()
        {
            List<Respuesta> lasRespuestas = new List<Respuesta>();
            try
            {

                String ClaveCadenaComercial = PNConfig.Get("APLICARMOV", "ClaveCadenaComercial");

                String IDTipoColectiva = PNConfig.Get("APLICARMOV", "IDTipoColectivaCCH");

                /*
                 *   Parametro unParametroCH = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = elCliente.ID_Colectiva.ToString(), Descripcion = "Colectiva Cliente CuentaHabiente", ID_TipoColectiva = int.Parse(PNConfig.Get("NUEVOCLNT", "IDTipoColectivaCCH")) };

                              TodosLosParametros.Add(unParametroCH.Nombre, unParametroCH);ObtieneMovimientos()
                 */

                foreach (Movimiento elMov in  DAOUtilerias.ObtieneMomivientosNuevos())
                {

                    try
                    {

                        Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(elMov.ClaveCadenaComercial, elMov.Tarjeta, elMov.ID_Concepto_Evento, "Procesador");

                        int ID_Evento = TodosLosParametros.ContainsKey("@ID_Evento") ? Int32.Parse(TodosLosParametros["@ID_Evento"].Valor) : 0;
                        int ID_Contrato = TodosLosParametros.ContainsKey("@ID_Contrato") ? Int32.Parse(TodosLosParametros["@ID_Contrato"].Valor) : 0;
                        String descEvento = TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : elMov.ID_Concepto_Evento + ": Movimientos por Procesador Nocturno";

                        if (int.Parse(elMov.ID_Cliente) != 0)
                        {
                            TodosLosParametros["@ID_Cuentahabiente"] = new Parametro() { Nombre = "@ID_CuentaHabiente", Valor = elMov.ID_Cliente, EsClave = false, ID_TipoColectiva = Int32.Parse(IDTipoColectiva), Descripcion = "ID_CuentaHabiente" };
                            
                        }
                        else
                        {
                            elMov.ID_Cliente = TodosLosParametros["@ID_Cuentahabiente"].Valor;
                        }

                       

                        TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = elMov.Importe.ToString(), Descripcion = "Importe Reportado" };

                        //OBTIENE LOS PARAMENTROS DE LA OPERACION ORIGINAL
                        Dictionary<String, Parametro> losParametrosOperacion = DAOEvento.ObtieneDatosOperacion(elMov.Tarjeta, elMov.Autorizacion);

                        foreach (Parametro elParamentro in losParametrosOperacion.Values)
                        {
                            TodosLosParametros[elParamentro.Nombre] = elParamentro;
                        }

                     
                        Logueo.EntradaSalida("El movimiento: Consecutivo:" + elMov.Consecutivo + ", ID_Cliente: " + elMov.ID_Cliente + ", Tarjeta: " + elMov.Tarjeta + ", Importe:" + elMov.Importe + ", ClaveEvento:" + elMov.ID_Concepto_Evento + ", Factura" + elMov.Factura + ", FechaAutorizacion:" + elMov.FechaAutorizacion + ", Autorizacion:" + elMov.Autorizacion, "", false);
                        Respuesta unaRespo = AplicarMovimiento(ID_Evento, ID_Contrato, 0, elMov.ID_Cliente, elMov.Tarjeta, descEvento, elMov.Observaciones, Int64.Parse(elMov.Consecutivo), TodosLosParametros);

                        Logueo.EntradaSalida("Respuesta de El movimiento: Consecutivo:" + elMov.Consecutivo + ", CodigoRespuesta:" + unaRespo.CodigoRespuesta + ", Autorizacion: " + unaRespo.Autorizacion +  ", Descripcion:" +  unaRespo.Descripcion +" , Tarjeta: " + unaRespo.Tarjeta + ", Importe:" +  elMov.Importe + ", XML:" + unaRespo.XmlExtras, "", false);
                        lasRespuestas.Add(unaRespo);

                        DAOUtilerias.setMovimientoProcesado(Int64.Parse(elMov.Consecutivo), elMov.Tarjeta);

                       // DataSet laresp = unaOperacion.MovimientoCuenta_respuesta(int.Parse(elMov.Consecutivo),  elMov.ID_Cliente, elMov.Tarjeta, elMov.Importe, unaRespo.CodigoRespuesta.ToString(), unaRespo.Descripcion);

                    }
                    catch (Exception err)
                    {
                        Logueo.Error("AplicarMovimiento():" + err.Message);
                    }
                }


                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("AplicarMovimiento():" + err.Message);
                return false;
            }
        }



        private static List<Movimiento> ObtieneMovimientos()
        {
            List<Movimiento> losMovimientos = new List<Movimiento>();
            try
            {

                //Movimiento unMovimiento = new Movimiento();


                //unMovimiento.Consecutivo = "141625";// renglon["CONSECUTIVO"].ToString();
                //unMovimiento.ID_Cliente = "90000000070308";// renglon["REFERENCIAPAGO"].ToString();
                //unMovimiento.Autorizacion = "957846";// renglon["AUTORIZACION"].ToString();
                //unMovimiento.Factura = "1845";// renglon["FACTURA"].ToString();
                //unMovimiento.FechaAutorizacion = DateTime.Now;// (DateTime)renglon["FECHAAUTORIZACION"];
                //unMovimiento.ID_Concepto_Evento = "01"; // ((int)renglon["IDCONCEPTO"]).ToString();
                //unMovimiento.Importe = 15632.32M;// (Decimal)renglon["IMPORTE"];
                //unMovimiento.Observaciones = "SIN OBSERVACIONES";// renglon["OBSERVACIONES"].ToString();

                //losMovimientos.Add(unMovimiento);

                // return losMovimientos;
                //  obtiene el dataset del ws


                DataSet losUsuarios = unaOperacion.MovimientoCuenta(DateTime.Now.ToString("yyyy-MM-dd"));


                foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                {
                    Movimiento unMovimiento = new Movimiento();

                    unMovimiento.Consecutivo = renglon["CONSECUTIVO"].ToString();
                    unMovimiento.ID_Cliente = renglon["ID_CLIENTE"].ToString();
                    unMovimiento.Tarjeta = renglon["TARJETA"].ToString();
                    unMovimiento.Autorizacion = renglon["AUTORIZACION"].ToString();
                    unMovimiento.Factura = renglon["FACTURA"].ToString();
                    unMovimiento.FechaAutorizacion = (DateTime)renglon["FECHAAUTORIZACION"];
                    unMovimiento.ID_Concepto_Evento = renglon["ID_CONCEPTO"].ToString();
                    unMovimiento.Importe = (Decimal)renglon["IMPORTE"];
                    unMovimiento.Observaciones = renglon["OBSERVACIONES"].ToString();

                    losMovimientos.Add(unMovimiento);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("obtieneMovimientos()" + err.Message);
            }

            return losMovimientos;
        }

    }
}
