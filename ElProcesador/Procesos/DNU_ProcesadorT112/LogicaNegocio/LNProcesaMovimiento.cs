using CommonProcesador;
using DNU_ProcesadorT112.BaseDatos;
using DNU_ProcesadorT112.Entidades;
using Executer.BaseDatos;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ProcesadorT112.LogicaNegocio
{
    class LNProcesaMovimiento
    {
        public static int ProcesarMovimiento(Movimiento elMovimiento, String ClaveEvento, String CadenaComercial, String CodigoOperacion, String DrafCaptureFlag, Dictionary<String, Parametro> losParametros, SqlConnection conn, SqlTransaction transaccionSQL, SqlConnection FicheroConn)
        {

            //using (SqlTransaction transaccionSQL = conn.BeginTransaction())
            //{
            try
            {
                #region
                Poliza laPoliza = null;

                Dictionary<String, Parametro> losParametrosContrato = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato(CadenaComercial, elMovimiento.ClaveMA, ClaveEvento, "PROCNOCT", elMovimiento.MonedaOriginal, float.Parse(elMovimiento.ImporteMonedaOriginal), conn, transaccionSQL);


                try
                {

                    foreach (String Parametro in losParametrosContrato.Keys)
                    {

                        if (losParametros.ContainsKey(Parametro))
                        {
                            losParametros[Parametro]= losParametrosContrato[Parametro];
                        }
                        else
                        {
                            losParametros.Add(Parametro, losParametrosContrato[Parametro]);
                        }

                    }
                } catch (Exception err)
                {
                    Logueo.Error("No se pudo obtener los parametros de contrato para la CADENA: " + CadenaComercial);
                }



                if (int.Parse(losParametros["@ID_Evento"].toString()) == 0)
                {
                    Logueo.Error("No Hay Evento con la clave seleccionada: " + ClaveEvento);
                    return -1;
                }
                

                //Genera y Aplica la Poliza @DescEvento
                Executer.EventoManual aplicador = new Executer.EventoManual(int.Parse(losParametros["@ID_Evento"].toString()),
                        losParametros["@DescEvento"].toString(), 
                        false, 
                        long.Parse(elMovimiento.ReferenciaNumerica), 
                        losParametros, 
                        elMovimiento.Observaciones, 
                        conn, transaccionSQL);

                laPoliza = aplicador.AplicaContablilidad();


                //VALIDAR QUE SEA DE EVENTOS QUE NO GENERAN MOVIMIENOTS EN T112 PARA SOLO CAMBIARLO A COMPENSADA

              


                if (laPoliza.CodigoRespuesta != 0)
                {

                    // transaccionSQL.Rollback();
                   
                    try
                    {
                        DAOArchivo.ActualizaFicheroDetalleEnBD(int.Parse(elMovimiento.ReferenciaNumerica), laPoliza.CodigoRespuesta.ToString(), laPoliza.ID_Poliza, FicheroConn);

                    }
                    catch (Exception err)
                    {
                       // throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                        throw new Exception("[EventoManual.ActualizaFicheroDetalleEnBD] No se generó la Póliza: " +  laPoliza.DescripcionRespuesta + ", ID_Operacion:"  + laPoliza.ID_Operacion + ", ID_Poliza " + laPoliza.ID_Poliza);
                    }
                }

                else if (laPoliza.CodigoRespuesta == 0)
                {

                    try
                    {
                        if (laPoliza.losParametros.ContainsKey("@ID_OperacionOriginal"))
                        {
                            float ImportePesos = 0;

                            if (elMovimiento.MonedaOriginal.Equals("840")) //son dolares
                            {
                                ImportePesos = float.Parse(elMovimiento.Importe) * float.Parse(losParametros["@TipoCambio_USD"].toString());
                            }
                            else
                            {
                                ImportePesos = float.Parse(elMovimiento.Importe);
                                //elMovimiento.Importe = "0";
                            }


                            DAOUtilerias lasUtilerias = new DAOUtilerias();

                            lasUtilerias.ActualizaOperacionOriginalACompensada(Int32.Parse(laPoliza.losParametros["@ID_OperacionOriginal"].toString()), ImportePesos, float.Parse(elMovimiento.Importe), losParametros, conn, transaccionSQL);
                        }

                        try
                        {


                            //Sending notification async 
                            new Task(() => {
                                LNWebhook.enviaNotificacion(elMovimiento,
                                    losParametros,
                                    laPoliza.ID_Poliza,
                                    int.Parse(elMovimiento.ReferenciaNumerica),
                                    FicheroConn);
                            }).Start();

                            //Thread.Sleep(240000);

                            DAOArchivo.ActualizaFicheroDetalleEnBD(int.Parse(elMovimiento.ReferenciaNumerica), laPoliza.CodigoRespuesta.ToString(), laPoliza.ID_Poliza, FicheroConn);

                        }
                        catch (Exception err)
                        {
                             throw new Exception("[EventoManual.ActualizaFicheroDetalleEnBD] NO SE ACTUALIZO NINGUNA OPERACION: ID_Operacion:" + laPoliza.ID_Operacion + ", ID_Poliza " + laPoliza.ID_Poliza);
                        }

                    }
                    catch (Exception ERR)
                    {
                        Logueo.Evento("[EventoManual] NO SE ACTUALIZO LA OPERACION COMO COMPENSADA: " + laPoliza.ID_Operacion + "," + ERR.Message);
                        throw new Exception("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION:" + laPoliza.ID_Operacion);
                    }
                    // transaccionSQL.Commit();
                    Logueo.Evento("Se Realizó la afectacion a la Cuenta de la Tarjeta: " + elMovimiento.ClaveMA);
                    return 0;
                }
                else
                {
                    return laPoliza.CodigoRespuesta;
                }

                return laPoliza.CodigoRespuesta;
                #endregion
            }
            catch (Exception elerr)
            {
                Logueo.Error(String.Format("NO SE GENERO LA POLIZA: {0} CON EL IMPORTE {1}", elMovimiento.ClaveMA, elMovimiento.Importe));
                Logueo.Error(String.Format("NO SE GENERO LA POLIZA: {0} ERROR {1}", elMovimiento.ClaveMA, elerr.Message));
                //  transaccionSQL.Rollback();
                return 8;
            }
            //   }


        }


    }
}
