using CommonProcesador;
using DNU_VencimientosParabilia.BaseDatos;
using DNU_VencimientosParabilia.Entidades;
using DNU_VencimientosParabilia.Utilidades;
using Executer.BaseDatos;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewRelic.Api.Agent;
using System.Globalization;

namespace DNU_VencimientosParabilia.LogicaNegocio
{
    public class LNProcesaVencimeintos
    {

        public static readonly string DEVOLUCIONSALDOSINTERNOS = "DEVSALINTERNOS";
        public static readonly string DEVOLUCIONSALDOSEXTERNOS = "DEVSALEXTERNOS";
        //
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROCESAVENCPARABILIA";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        [Transaction]
        public static Boolean ejecutaCambioEstatus()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            string log = ""; //ThreadContext.Properties["log"].ToString();
            string ip = "";//ThreadContext.Properties["ip"].ToString();
            Boolean estatusOperaciones = true;
            try
            {
                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Inicio de Cambio de estatus de operaciones]");
                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Obteniendo colectivas a procesar]");

               
                List<ColectivaContrato> lstColectivasAProcesar = LNDBvencimientos.obtenerColectivas().ToList();

                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, # Colectivas a procesar " + lstColectivasAProcesar.Count + "]");
              
                foreach (ColectivaContrato loColectiva in lstColectivasAProcesar)
                {
                    try
                    {
                        var nac = loColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Nacionales")).FirstOrDefault();
                        var internac = loColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Internacionales")).FirstOrDefault();

                        if (nac == null && internac == null)
                            continue;

                        LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Actualizando Colectiva ... : " + loColectiva.ClaveColectiva + "]");
                        LNDBvencimientos.ActualizarMovimientosPorColectivaDB(loColectiva);
                        LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, ... Colectiva Actualizada : " + loColectiva.ClaveColectiva + "]");
                    }
                    catch (Exception e)
                    {
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Error en la actualización de cambio de estatus para la colectiva: " + loColectiva.ClaveColectiva + " " + e.Message + "]");
                    
                    }
                }

                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Termino de Cambio de estatus de operaciones]");
             }

            catch (Exception ex)
            {
                estatusOperaciones = false;
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaCambioEstatus, Error en la actualización de cambio de estatus : " + ex.Message + "]");
             
            }

            return estatusOperaciones;
        }

        [Transaction]
        public static Boolean ejecutaAplicacionDevoluciones()
        {
           
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            //ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            //ApmNoticeWrapper.SetTransactionName("ejecutaAplicacionDevoluciones");
            string log = "";//ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();

            Boolean estatusOperacion = true;
            try
            {

                char[] delimiter1 = new char[] { ',' };



                List<Movimiento> lstMovimientosColectiva =
                    LNProcesaVencimeintos.ObtieneMovimientos(
                            LNDBvencimientos.obtieneMovimientosPorVencer());
                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, Movimeintos a procesar : " + lstMovimientosColectiva.Count + "]");
               

                StringBuilder sb = new StringBuilder();
                foreach (Movimiento mov in lstMovimientosColectiva)
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, ForeachMoviemientos, Operacion a procesar : " + mov.Id_Operacion + "]");
                   
                    using (SqlConnection conn = new SqlConnection(BDAutorizador.strBDEscritura))
                    {
                        conn.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {

                            try
                            {
                                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();


                                LNProcesaVencimeintos.obtieneParametros(ref losParametros, mov, string.Empty, conn, transaccionSQL);

                                Regla laPertenencia = DAOPertenencia.getParamentrosStoredPertenencia(conn, transaccionSQL);

                                int ID_Pertenencia = DAOPertenencia.getPertenencia(laPertenencia, ref losParametros, conn, transaccionSQL);


                                if (ID_Pertenencia == 0)
                                {
                                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones][ForeachMoviemientos] No hay pertenencia definida para el movimiento : " + mov.Id_Operacion);
                                    throw new Exception($"No hay pertenencia definida para el movimiento :  {mov.Id_Operacion}");
                                }

                                List<String> losEventosAEjecutar = DAOPertenencia.getEventosAEjecutarPorPertenencia(ID_Pertenencia, conn, transaccionSQL);

                                if (losEventosAEjecutar.Count == 0)
                                {
                                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones][ForeachMoviemientos] No hay eventos definidos para la pertenencia : " + ID_Pertenencia);
                                    throw new Exception($"No hay eventos definidos para la pertenencia : {ID_Pertenencia}");
                                }

                                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones][ForeachMoviemientos] Numero de Eventos : " + losEventosAEjecutar.Count());

                                foreach (String elEventoToEjecutar in losEventosAEjecutar)
                                {
                                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones][ForeachMoviemientos] Ejecutando evento Eventos : " + elEventoToEjecutar);
                                    int respuesta = LNProcesaVencimeintos.ProcesaMovimiento(mov, elEventoToEjecutar, losParametros, conn, transaccionSQL);

                                }

                                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Procesar] Commit(): ");
                                transaccionSQL.Commit();
                                GuardaEnStreamParaArchivo(sb,mov);
                            }
                            catch (Exception ex)
                            {
                                estatusOperacion = false;
                                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Procesar, Error En eventos Rollback(): " + ex.Message + " - " + ex.StackTrace + "]");
                             
                                transaccionSQL.Rollback();
                            }
                        }
                    }
                }

                GuardaArchivoEnRuta(sb);

            }

            catch (Exception ex)
            {
                estatusOperacion = false;
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Procesar, ejecutaAplicacionDevoluciones Error en proceso " + ex.Message + " - " + ex.StackTrace + "]");
              
            }

            return estatusOperacion;
        }

        private static void GuardaArchivoEnRuta(StringBuilder sb)
        {
            string log = "";//ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();

            String ruta = PNConfig.Get("PROCESAVENCPARABILIA", "PATHSALIDA");
            if (String.IsNullOrEmpty(ruta))
                throw new Exception("La ruta para almacenar el archivo es nula o vacia");
           
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);

            String fileName = String.Format(@"{0}\PVA{1}.txt", ruta, DateTime.Now.ToString("yyyyMMdd"));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                file.WriteLine(sb.ToString());
            }

        }

        private static void GuardaEnStreamParaArchivo(StringBuilder pSb, Movimiento pMov)
        {
            String patron = "{0}{1}{2}{3}{4}{5}{6}{7}";
            String emisor = pMov.ClaveColectiva.PadLeft(8);
            String tipoTransaccion = "D";
            String fechaOperacion = DateTime.Parse(pMov.FechaOperacion).ToString("yyyyMMdd");
            String horaOperacion = DateTime.Parse(pMov.FechaOperacion).ToString("HHMMss");
            String numeroAutorizacion = pMov.Autorizacion.PadLeft(6);
            String Importe = Convert.ToDecimal(pMov.Importe).ToString("0000000000.00");
            String ImporteRellenado = Importe.Replace(".", string.Empty);



            String moneda = pMov.MonedaOriginal;

            pSb.AppendLine(String.Format(patron, pMov.ClaveMA.ToMaskedField(), emisor, tipoTransaccion, fechaOperacion, horaOperacion, numeroAutorizacion, ImporteRellenado, moneda));
        }

        public static List<Movimiento> ObtieneMovimientos(DataTable losMovimientos)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                List<Movimiento> losResultados = new List<Movimiento>();
                int regi = 0;

                foreach (DataRow row in losMovimientos.Rows)
                {
                    try
                    {
                        regi++;
                        Movimiento dato = new Movimiento();
                        dato.ClaveColectiva = row["ClaveColectiva"].ToString();
                        dato.ClaveMA = row["ClaveMA"].ToString();
                        dato.IdColectiva = Convert.ToInt64(row["ID_ColectivaCadComercial"]);
                        dato.Id_Operacion = Convert.ToInt64(row["ID_Operacion"]);
                        dato.TipoMA = row["TipoMA"].ToString();
                        dato.ClaveEvento = row["C000"].ToString();
                        dato.Observaciones = "PROCESAMIENTO AUTOMATICO VENCIMIENTOS";
                        dato.Autorizacion = row["Autorizacion"].ToString();
                        dato.FechaOperacion = row["FechaRegistro"].ToString();

                        //dato.ReferenciaNumerica = row["ID_FicheroDetalle"].ToString();
                        dato.Ticket = "";
                        dato.MonedaOriginal = row["C049"].ToString();

                        dato.T112_CodigoMonedaLocal = row["C049"].ToString();
                        try
                        {
                            dato.T112_ImporteCompensadoLocal = row["Importe"].ToString(); //(float.Parse(row["C006"].ToString()) / 100).ToString();
                        }
                        catch (Exception ex)
                        {
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, ObtieneMovimientos(): C006 invalido" + ex.Message + "]");
                            throw ex;
                        }

                        //dato.T112_CuotaIntercambio = (float.Parse(row["C020"].ToString()) / 100).ToString();

                        //if (row["C049"].ToString().Equals("484")) //es Pesos
                        //{
                        dato.T112_ImporteCompensadoDolar = "0";
                        dato.T112_ImporteCompensadoPesos = row["Importe"].ToString(); //(float.Parse(row["C006"].ToString()) / 100).ToString();
                        dato.Importe = row["Importe"].ToString(); //(float.Parse(row["C006"].ToString()) / 100).ToString();
                        dato.ImporteMonedaOriginal = row["Importe"].ToString();//(float.Parse(row["C006"].ToString()) / 100).ToString();
                        dato.MonedaOriginal = row["C049"].ToString();
                        //}
                        //else
                        //{
                        //    if (row["C049"].ToString().Equals("840")) //es Dolar
                        //    {
                        //        dato.T112_ImporteCompensadoDolar = (float.Parse(row["C008"].ToString()) / 100).ToString();
                        //        dato.T112_ImporteCompensadoPesos = "0";

                        //    }
                        //    else if (row["C009"].ToString().Equals("484")) //pesos
                        //    {
                        //        dato.T112_ImporteCompensadoDolar = "0";
                        //        dato.T112_ImporteCompensadoPesos = (float.Parse(row["C008"].ToString()) / 100).ToString();
                        //    }

                        //    dato.Importe = (float.Parse(row["C006"].ToString()) / 100).ToString();
                        //    dato.ImporteMonedaOriginal = (float.Parse(row["C006"].ToString()) / 100).ToString();
                        //    dato.MonedaOriginal = row["C009"].ToString();
                        //}

                        try
                        {
                            dato.T112_IVA = (float.Parse(row["C050"].ToString().Substring(3, 12)) / 100).ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_IVA = "0";
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, ObtieneMovimientos(): error al Obtenr el Campo IVA " + regi + " Valor en T112: " + row["C050"].ToString() + ";" + err.Message + "]");
                       
                        }

                        try
                        {
                            dato.T112_FechaPresentacion = row["FechaPresentacion"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_FechaPresentacion = DateTime.Now.ToString("yyyy-MM-dd");
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, ObtieneMovimientos(): error al Obtenr el Campo Fecha Presentacion" + err.Message + "]");
                        }

                        try
                        {
                            dato.T112_NombreArchivo = row["NombreArchivo"].ToString();

                        }
                        catch (Exception err)
                        {
                            dato.T112_NombreArchivo = "";
                            LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones, ObtieneMovimientos(): error al Obtenr el Campo Nombre de Archivo" + err.Message + "]");

                        }


                        dato.ID_EstatusOperacion = row["ID_Estatus"].ToString();

                        dato.ID_EstatusPostOperacion = string.IsNullOrEmpty(row["ID_EstatusPostOperacion"].ToString())
                                                        ? "0"
                                                        : row["ID_EstatusPostOperacion"].ToString();


                        losResultados.Add(dato);

                    }
                    catch (Exception err)
                    {
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones]ObtieneMovimientos(): Formato de Registro no valido [" + regi + "] " + row.ToString() + ";" + err.Message + "]");
                        //throw err;
                    }
                }

                return losResultados;

            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ejecutaAplicacionDevoluciones]ObtieneMovimientos(): " + err.Message + "]");
                 throw new Exception("[ejecutaAplicacionDevoluciones]Error al Obtener los Movimientos");
            }
        }

      
        public static void obtieneParametros(ref Dictionary<String, Parametro> losParametros, Movimiento elMovimiento, String DrafCaptureFlag, SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            //Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();
            try
            {

                //Se consultan los parámetros del contrato
                losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato("", elMovimiento.ClaveMA, "", "PROCNOCT", elMovimiento.MonedaOriginal, float.Parse(elMovimiento.ImporteMonedaOriginal), connOperacion, transaccionSQL);


                losParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = elMovimiento.Importe, Descripcion = "Importe" };
                losParametros["@MedioAcceso"] = new Parametro() { Nombre = "@MedioAcceso", Valor = elMovimiento.ClaveMA };
                losParametros["@TipoMedioAcceso"] = new Parametro() { Nombre = "@TipoMedioAcceso", Valor = "TAR" };
                losParametros["@Tarjeta"] = new Parametro() { Nombre = "@Tarjeta", Valor = elMovimiento.ClaveMA };
                losParametros["@ReferenciaNumerica"] = new Parametro() { Nombre = "@ReferenciaNumerica", Valor = elMovimiento.ReferenciaNumerica };
                losParametros["@Autorizacion"] = new Parametro() { Nombre = "@Autorizacion", Valor = elMovimiento.Autorizacion };
                losParametros["@FechaOperacion"] = new Parametro() { Nombre = "@FechaOperacion", Valor = elMovimiento.FechaOperacion };
                losParametros["@FechaAplicacion"] = new Parametro() { Nombre = "@FechaAplicacion", Valor = elMovimiento.FechaOperacion };
                losParametros["@Ticket"] = new Parametro() { Nombre = "@Ticket", Valor = elMovimiento.Ticket };

                losParametros["@ProcessingCode"] = new Parametro() { Nombre = "@ProcessingCode", Valor = elMovimiento.ClaveEvento };

                if (elMovimiento.MonedaOriginal.Equals("840"))
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = elMovimiento.ImporteMonedaOriginal };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = "0" };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = elMovimiento.ImporteMonedaOriginal };
                }
                else if (elMovimiento.MonedaOriginal.Equals("484")) //pesos 
                {
                    losParametros["@ImporteOriginal_USD"] = new Parametro() { Nombre = "@ImporteOriginal_USD", Valor = "0" };
                    losParametros["@ImporteOriginal_MXN"] = new Parametro() { Nombre = "@ImporteOriginal_MXN", Valor = elMovimiento.ImporteMonedaOriginal };
                    losParametros["@Imp_08_T112"] = new Parametro() { Nombre = "@Imp_08_T112", Valor = elMovimiento.ImporteMonedaOriginal };
                }

                //nuevos paramentros T112 
                losParametros["@T112_CodigoMonedaLocal"] = new Parametro() { Nombre = "@T112_CodigoMonedaLocal", Valor = elMovimiento.T112_CodigoMonedaLocal };
                losParametros["@T112_CuotaIntercambio"] = new Parametro() { Nombre = "@T112_CuotaIntercambio", Valor = elMovimiento.T112_CuotaIntercambio };
                losParametros["@T112_ImporteCompensadoDolar"] = new Parametro() { Nombre = "@T112_ImporteCompensadoDolar", Valor = elMovimiento.T112_ImporteCompensadoDolar };
                losParametros["@T112_ImporteCompensadoLocal"] = new Parametro() { Nombre = "@T112_ImporteCompensadoLocal", Valor = elMovimiento.T112_ImporteCompensadoLocal };
                losParametros["@T112_ImporteCompensadoPesos"] = new Parametro() { Nombre = "@T112_ImporteCompensadoPesos", Valor = elMovimiento.T112_ImporteCompensadoPesos };
                losParametros["@T112_IVA"] = new Parametro() { Nombre = "@T112_IVA", Valor = elMovimiento.T112_IVA };
                losParametros["@T112_NombreArchivo"] = new Parametro() { Nombre = "@T112_NombreArchivo", Valor = elMovimiento.T112_NombreArchivo };
                losParametros["@T112_FechaPresentacion"] = new Parametro() { Nombre = "@T112_FechaPresentacion", Valor = elMovimiento.T112_FechaPresentacion };

                //PARA INDICAR SI ES CHECKoUT O SOLO ES UNA COMPENSACION NORMAL  
                losParametros["@DrafCaptureFlag"] = new Parametro() { Nombre = "@DrafCaptureFlag", Valor = DrafCaptureFlag };
                losParametros["@ID_OperacionOriginal"] = new Parametro() { Nombre = "@ID_OperacionOriginal", Valor = elMovimiento.Id_Operacion.ToString() };
                losParametros["@ID_EstatusOperacion"] = new Parametro() { Nombre = "@ID_EstatusOperacion", Valor = elMovimiento.ID_EstatusOperacion};
                losParametros["@ID_EstatusPostOperacion"] = new Parametro() { Nombre = "@ID_EstatusPostOperacion", Valor = elMovimiento.ID_EstatusPostOperacion };
            }
            catch (Exception err)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }


        }


        public static int ProcesaMovimiento(Movimiento pMovimiento, String ClaveEvento, Dictionary<String, Parametro> losParametros, SqlConnection conn, SqlTransaction transaccionSQL)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            string tarjeta = "";
            try
            {

                String LaCadenaComercial = pMovimiento.ClaveColectiva;

                try
                {
                    #region
                    Poliza laPoliza = null;

                    Dictionary<String, Parametro> losParametrosContrato = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato(pMovimiento.ClaveColectiva, pMovimiento.ClaveMA, ClaveEvento, "PROCNOCT", pMovimiento.MonedaOriginal, float.Parse(pMovimiento.ImporteMonedaOriginal), conn, transaccionSQL);


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
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [No se pudo obtener los parametros de contrato para la CADENA: " + LaCadenaComercial + "]");
                    }



                    if (int.Parse(losParametros["@ID_Evento"].toString()) == 0)
                    {
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [No Hay Evento con la clave seleccionada: " + ClaveEvento + "]");
                        throw new Exception("No Hay Evento con la clave seleccionada: " + ClaveEvento);
                    }

                    try {
                        DateTime fecha = Convert.ToDateTime(losParametros["@FechaAplicacion"].toString());
                    } catch (Exception ex) {

                    }

                    //Genera y Aplica la Poliza @DescEvento
                    Executer.EventoManual aplicador = new Executer.EventoManual(int.Parse(losParametros["@ID_Evento"].toString()),
                            losParametros["@DescEvento"].toString(),
                            false,
                            pMovimiento.Id_Operacion,
                            //pMovimiento.IdColectiva,
                            losParametros,
                            pMovimiento.Observaciones,
                            conn, transaccionSQL);

                    laPoliza = aplicador.AplicaContablilidad();

                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EventoManual, Poliza generada: " + laPoliza.ID_Poliza + "]");
                    

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [No se generó la Póliza: " + laPoliza.DescripcionRespuesta + "]");
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else if (laPoliza.CodigoRespuesta == 0)
                    {

                        try
                        {

                            if (laPoliza.losParametros.ContainsKey("@ID_OperacionOriginal"))
                            {
                                float ImportePesos = 0;

                                if (pMovimiento.MonedaOriginal.Equals("840")) //son dolares
                                {
                                    ImportePesos = float.Parse(pMovimiento.Importe) * float.Parse(losParametros["@TipoCambio_USD"].toString());
                                }
                                else
                                {
                                    ImportePesos = float.Parse(pMovimiento.Importe);
                                }

                                new Task(() => {
                                    LNWebhook.enviaNotificacion(pMovimiento,
                                        losParametros);
                                }).Start();

                                //Thread.Sleep(240000);


                                DAOUtilerias lasUtilerias = new DAOUtilerias();
                                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EventoManual, ACTUALIZANDO OPERACION COMO DEVUELTA: " + laPoliza.ID_Operacion + "]");
                                lasUtilerias.ActualizaOperacionOriginalADevuelta(Int32.Parse(laPoliza.losParametros["@ID_OperacionOriginal"].toString()), conn, transaccionSQL);

                                LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EventoManual, SE ACTUALIZO OPERACION COMO DEVUELTA: " + laPoliza.ID_Operacion + "]");
                            }

                        }
                        catch (Exception ERR)
                        {
                            LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [EventoManual, NO SE ACTUALIZO LA OPERACION COMO COMPENSADA: " + laPoliza.ID_Operacion + "," + ERR.Message + "]");
                           throw new Exception("[EventoManual] NO SE ACTUALIZO NINGUNA OPERACION:" + laPoliza.ID_Operacion);
                        }
                        tarjeta = pMovimiento.ClaveMA.Substring(0, 6) + "******" + pMovimiento.ClaveMA.Substring(12, 4);
                        LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Se Realizó la afectacion a la Cuenta de la Tarjeta: " + tarjeta + "]");
                        return 0;
                    }
                    else
                    {
                        return laPoliza.CodigoRespuesta;
                    }
                    #endregion
                }
                catch (Exception elerr)
                {
                    LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("NO SE GENERO LA POLIZA: {0} CON EL IMPORTE {1}", pMovimiento.ClaveMA.ToMaskedField(), pMovimiento.Importe) + "]");
                     throw elerr;
                }
            }
            catch (Exception ERR)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): ERROR AL GENERAR MOVIMIENTO " + pMovimiento.ClaveMA.ToMaskedField() + " " + ERR.Message + "]");
                throw ERR;
            }

        }
    }
}
