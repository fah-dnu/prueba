using CommonProcesador;
using DNU_ParabiliumProcesoMSI.Modelos.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.Servicios.EjecutorPolizas
{
    class ServicioExecuter
    {
        public static RespuestaExecuter EjecutarEvento( Dictionary<String, Parametro> todosLosParametros, Bonificacion elAbono, SqlConnection conn, SqlTransaction transaccionSQL)
        {

            RespuestaExecuter unaRespo2 = new RespuestaExecuter();
            try
            {


                int Resp = -1;

               
                try
                {
                    Poliza laPoliza = null;


                    ////Genera y Aplica la Poliza
                    //Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, Concepto, false,1234, TodosLosParametros, Observaciones, conn, transaccionSQL);
                    ////llama al metodo AplicaContabilidad. 
                    //laPoliza = aplicador.AplicaContablilidad();
                    //Genera y Aplica la Poliza
                    Executer.EventoManual aplicadorEvento = new Executer.EventoManual(elAbono.IdEvento,
                    elAbono.Concepto, false, Convert.ToInt64(elAbono.RefNumerica), todosLosParametros, elAbono.Observaciones, conn, transaccionSQL);
                    laPoliza = aplicadorEvento.AplicaContablilidad();
                    //fechaPoliza = laPoliza.FechaCreacion;
                    //IdPoliza = laPoliza.ID_Poliza;

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        //errores.CodRespuesta = laPoliza.CodigoRespuesta.ToString();
                        //errores.DescRespuesta = laPoliza.DescripcionRespuesta.Replace("[EventoManual]", "").Replace("NO", "No").Replace(laPoliza.CodigoRespuesta.ToString(), "").Replace("Error:", "").Trim();
                        throw new Exception("[ProcesoMSI]No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }



                    //Genera y Aplica la Poliza
                    //EventoManual aplicador = new EventoManual(ID_Evento, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, Consecutivo);
                    //llama al metodo AplicaContabilidad. 
                    //laPoliza = aplicador.AplicaContablilidad();


                    if (laPoliza.ID_Poliza <= 0)
                    {
                        //   transaccionSQL.Rollback();
                        throw new Exception("[ProcesoMSI]No se Genero la Póliza del Evento Seleccionado");
                    }

                    //Guardar el Lote
                    if (laPoliza.ID_Poliza <= 0)
                    {
                        throw new Exception("[ProcesoMSI]No se Genero la Póliza del Evento Seleccionado");
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
                        unaRespo2.Descripcion = "AUTORIZADA " + (todosLosParametros.ContainsKey("@DescEvento") ? todosLosParametros["@DescEvento"].Valor : "").ToUpper();
                        //  transaccionSQL.Commit();

                    }

                }

                catch (Exception err)
                {
                    //  transaccionSQL.Rollback();
                    Logueo.Error("[ProcesoMSI]AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message);
                    unaRespo2.CodigoRespuesta = 87;
                    unaRespo2.Autorizacion = "000000";
                    //unaRespo2.Descripcion = "DECLINADO";
                    unaRespo2.Descripcion = "DECLINADA " + (todosLosParametros.ContainsKey("@DescEvento") ? todosLosParametros["@DescEvento"].Valor : "").ToUpper();

                }
                //   }

            }


            catch (Exception err)
            {
                Logueo.Error("[ProcesoMSI]Cortador.LNEventos.EjecuarEvento()" + err.Message);
                unaRespo2.CodigoRespuesta = 87;
                unaRespo2.Autorizacion = "000000";
                unaRespo2.Descripcion = "DECLINADO";

            }

            return unaRespo2;
        }

    }
}
