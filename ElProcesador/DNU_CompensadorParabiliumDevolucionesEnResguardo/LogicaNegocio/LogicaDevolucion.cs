using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumDevolucionesEnResguardo.LogicaNegocio
{
    public class LogicaDevolucion
    {

        public static void ActualizaEstatusDevolucion(DevolucionModel dev, SqlConnection conn, SqlTransaction tran)
        {
            DAOAutorizador.ActualizaEstatusDevolucion(dev,conn,tran);
        }

        public static List<int> ObtieneTiposIntegracion(SqlConnection conn)
        {
            return DAOAutorizador.ObtieneTiposIntegracion(conn);
        }


        public static List<DevolucionModel> ObtieneDevoluicionesEnResguardoPorTipoIntegracion(int ID_TipoIntegracion, SqlConnection conn)
        {
            return DAOAutorizador.ObtieneDevoluciones(ID_TipoIntegracion, conn);
        }


        public static bool RealizaDevolucionDeRecurso(DevolucionModel devolucion, SqlConnection con, SqlTransaction transaccionSQL)
        {
            Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();
            try
            {

                String LaCadenaComercial = devolucion.ClaveColectiva;
                //@Importe
                try
                {

                    obtieneParametros(ref losParametros,devolucion, "", con, transaccionSQL);

                    #region
                    Poliza laPoliza = null;

                    Dictionary<String, Parametro> losParametrosContrato = 
                        Executer
                        .BaseDatos
                        .DAOEvento
                        .ListaDeParamentrosContrato(devolucion.ClaveColectiva, devolucion.claveMa, devolucion.ClaveEvento , "PROCNOCT", devolucion.moneda, float.Parse(devolucion.importe.ToString()), con, transaccionSQL);


                    try
                    {

                        foreach (String Parametro in losParametrosContrato.Keys)
                        {

                            if (losParametros.ContainsKey(Parametro))
                            {
                                losParametros[Parametro] = losParametrosContrato[Parametro];
                            }
                            else
                            {
                                losParametros.Add(Parametro, losParametrosContrato[Parametro]);
                            }

                        }
                    }
                    catch (Exception err)
                    {
                        Log.Error("No se pudo obtener los parametros de contrato para la CADENA: " + LaCadenaComercial);
                    }



                    if (int.Parse(losParametros["@ID_Evento"].toString()) == 0)
                    {
                        Log.Error("No Hay Evento con la clave seleccionada: " + devolucion.ClaveEvento);
                        throw new Exception("No Hay Evento con la clave seleccionada: " + devolucion.ClaveEvento);
                    }


                    //Genera y Aplica la Poliza @DescEvento
                    Executer.EventoManual aplicador = new Executer.EventoManual(int.Parse(losParametros["@ID_Evento"].toString()),
                            losParametros["@DescEvento"].toString(),
                            false,
                            devolucion.ID_Operacion,
                            //pMovimiento.IdColectiva,
                            losParametros,
                            "DEVOLUCION DE RESGUARDO DE RECURSOS ",
                            con, transaccionSQL);

                    laPoliza = aplicador.AplicaContablilidad();

                    Log.Evento("[EventoManual] Poliza generada: " + laPoliza.ID_Poliza);


                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else if (laPoliza.CodigoRespuesta == 0)
                    {
                        devolucion.ID_Poliza = laPoliza.ID_Poliza;
                        return true;
                    }
                    else
                    {
                        Log.Error("Error al realizar la devolucion de resguardo de recursos codigoRespuesta = " + laPoliza.CodigoRespuesta);
                        return false;
                    }
                    #endregion
                }
                catch (Exception elerr)
                {
                    Log.Error(String.Format("NO SE GENERO LA POLIZA: {0} CON EL IMPORTE {1}", devolucion.claveMa.ToMaskedField(), devolucion.importe.ToString()));
                    return false;
                }


            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return false;
            }

        }

        public static void obtieneParametros(ref Dictionary<String, Parametro> losParametros,
            DevolucionModel dev, 
            String DrafCaptureFlag, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            //Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();
            try
            {

                //Se consultan los parámetros del contrato
                losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato("", dev.claveMa, "", "PROCNOCT", dev.moneda, float.Parse(dev.importe.ToString()), connOperacion, transaccionSQL);


                losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = dev.importe.ToString(), Descripcion = "Importe" };
                losParametros["@MedioAcceso"] = new Parametro() { Nombre = "@MedioAcceso", Valor = dev.claveMa };
                losParametros["@TipoMedioAcceso"] = new Parametro() { Nombre = "@TipoMedioAcceso", Valor = "TAR" };
                losParametros["@Tarjeta"] = new Parametro() { Nombre = "@Tarjeta", Valor = dev.claveMa };
                losParametros["@ReferenciaNumerica"] = new Parametro() { Nombre = "@ReferenciaNumerica", Valor = dev.referencia };
                losParametros["@Autorizacion"] = new Parametro() { Nombre = "@Autorizacion", Valor = dev.autorizacion };
                losParametros["@FechaOperacion"] = new Parametro() { Nombre = "@FechaOperacion", Valor = dev.FechaOperacion };
                losParametros["@FechaAplicacion"] = new Parametro() { Nombre = "@FechaAplicacion", Valor = dev.FechaOperacion };
                losParametros["@Ticket"] = new Parametro() { Nombre = "@Ticket", Valor = "" };

                losParametros["@ProcessingCode"] = new Parametro() { Nombre = "@ProcessingCode", Valor = dev.tipo};

                if (dev.moneda.Equals("840"))
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = dev.importe.ToString() };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = "0" };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = dev.importe.ToString()};
                }
                else if (dev.moneda.Equals("484")) //pesos 
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = "0" };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = dev.importe.ToString() };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = dev.importe.ToString() };
                }

                //nuevos paramentros T112 
                //losParametros["@T112_CodigoMonedaLocal"] = new Parametro() { Nombre = "@T112_CodigoMonedaLocal", Valor = dev.moneda };
                //losParametros["@T112_CuotaIntercambio"] = new Parametro() { Nombre = "@T112_CuotaIntercambio", Valor = elMovimiento.T112_CuotaIntercambio };
                //losParametros["@T112_ImporteCompensadoDolar"] = new Parametro() { Nombre = "@T112_ImporteCompensadoDolar", Valor = elMovimiento.T112_ImporteCompensadoDolar };
                //losParametros["@T112_ImporteCompensadoLocal"] = new Parametro() { Nombre = "@T112_ImporteCompensadoLocal", Valor = elMovimiento.T112_ImporteCompensadoLocal };
                //losParametros["@T112_ImporteCompensadoPesos"] = new Parametro() { Nombre = "@T112_ImporteCompensadoPesos", Valor = elMovimiento.T112_ImporteCompensadoPesos };
                //losParametros["@T112_IVA"] = new Parametro() { Nombre = "@T112_IVA", Valor = elMovimiento.T112_IVA };
                //losParametros["@T112_NombreArchivo"] = new Parametro() { Nombre = "@T112_NombreArchivo", Valor = elMovimiento.T112_NombreArchivo };
                //losParametros["@T112_FechaPresentacion"] = new Parametro() { Nombre = "@T112_FechaPresentacion", Valor = elMovimiento.T112_FechaPresentacion };

                //PARA INDICAR SI ES CHECKoUT O SOLO ES UNA COMPENSACION NORMAL  
                losParametros["@DrafCaptureFlag"] = new Parametro() { Nombre = "@DrafCaptureFlag", Valor = DrafCaptureFlag };
                losParametros["@ID_OperacionOriginal"] = new Parametro() { Nombre = "@ID_OperacionOriginal", Valor = dev.ID_Operacion.ToString() };

            }
            catch (Exception err)
            {

            }


        }


    }
}
