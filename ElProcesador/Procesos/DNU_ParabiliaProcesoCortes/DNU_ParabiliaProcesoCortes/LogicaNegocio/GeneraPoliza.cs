using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaNegocio
{
    class GeneraPoliza
    {
        static Thread unTimer;
        //static SqlConnection conn = new SqlConnection(BDCortes.strBDEscritura);






        public static Respuesta EjecutarEvento(int ID_Evento, int DiasVigencia, String IDCliente, String Tarjeta, String Concepto, String Observaciones, Int64 Consecutivo, Dictionary<String, Parametro> TodosLosParametros, Evento unEvento, Int64 ID_Corte, SqlConnection conn, SqlTransaction transaccionSQL, Bonificacion elAbono, Dictionary<String, Parametro> losParametros)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            Respuesta unaRespo2 = new Respuesta();

            // DAOCortes UnBDCorte = new DAOCortes();


            unaRespo2.Tarjeta = Tarjeta;

            try
            {


                int Resp = -1;

                //try
                //{
                //    conn.Open();

                //}
                //catch (Exception err)
                //{
                //    // Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento.AbrirConexion()" + err.Message);
                //}

                //using (SqlTransaction transaccionSQL = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                //{

                try
                {
                    Poliza laPoliza = null;


                    ////Genera y Aplica la Poliza
                    //Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, Concepto, false,1234, TodosLosParametros, Observaciones, conn, transaccionSQL);
                    ////llama al metodo AplicaContabilidad. 
                    //laPoliza = aplicador.AplicaContablilidad();
                    //Genera y Aplica la Poliza
                    Executer.EventoManual aplicadorEvento = new Executer.EventoManual(elAbono.IdEvento,
                    elAbono.Concepto, false, Convert.ToInt64(elAbono.RefNumerica), losParametros, elAbono.Observaciones, conn, transaccionSQL);
                    laPoliza = aplicadorEvento.AplicaContablilidad();
                    //fechaPoliza = laPoliza.FechaCreacion;
                    //IdPoliza = laPoliza.ID_Poliza;

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        //errores.CodRespuesta = laPoliza.CodigoRespuesta.ToString();
                        //errores.DescRespuesta = laPoliza.DescripcionRespuesta.Replace("[EventoManual]", "").Replace("NO", "No").Replace(laPoliza.CodigoRespuesta.ToString(), "").Replace("Error:", "").Trim();
                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [No se generó la Póliza: " + laPoliza.DescripcionRespuesta + "]");
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }



                    //Genera y Aplica la Poliza
                    //EventoManual aplicador = new EventoManual(ID_Evento, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, Consecutivo);
                    //llama al metodo AplicaContabilidad. 
                    //laPoliza = aplicador.AplicaContablilidad();


                    if (laPoliza.ID_Poliza <= 0)
                    {
                        //   transaccionSQL.Rollback();
                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [No se Genero la Póliza del Evento Seleccionado]");
                        throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                    }

                    //Guardar el Lote
                    if (laPoliza.ID_Poliza <= 0)
                    {
                        LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [No se Genero la Póliza del Evento Seleccionado]");
                        throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                    }
                    else
                    {
                        //Actualzia LotePoliza
                        //no entiendo para que agrega sibre otroa tabla, si se va agregar sobre una tabla saliendo de este metodo
                        //new DAOPoliza(conn, transaccionSQL).insertaLotePoliza(laPoliza.ID_Poliza, ID_Corte, unEvento.ID_AgrupadorEvento, conn, transaccionSQL);

                        unaRespo2.CodigoRespuesta = 0;
                        unaRespo2.Autorizacion = DateTime.Now.ToString("HHmmss");
                        //unaRespo2.Descripcion = "AUTORIZADA
                        unaRespo2.ID_Poliza = laPoliza.ID_Poliza;
                        unaRespo2.Descripcion = "AUTORIZADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();
                        //  transaccionSQL.Commit();

                    }

                }

                catch (Exception err)
                {
                    //  transaccionSQL.Rollback();
                    LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message + "]");
                    unaRespo2.CodigoRespuesta = 87;
                    unaRespo2.Autorizacion = "000000";
                    //unaRespo2.Descripcion = "DECLINADO";
                    unaRespo2.Descripcion = "DECLINADA " + (TodosLosParametros.ContainsKey("@DescEvento") ? TodosLosParametros["@DescEvento"].Valor : "").ToUpper();

                }
                //   }

            }


            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Cortador.LNEventos.EjecuarEvento()" + err.Message + "]");
                 unaRespo2.CodigoRespuesta = 87;
                unaRespo2.Autorizacion = "000000";
                unaRespo2.Descripcion = "DECLINADO";

            }

            return unaRespo2;
        }

    }
}
