using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumReportes.BaseDatos;
using DNU_CompensadorParabiliumReportes.LogicaNegocio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using CommonProcesador;
using System.Threading;

namespace DNU_CompensadorParabiliumReportes
{
    public class ParabiliumReportes
    {
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROC_REPORTES";
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


        /// <summary>
        /// Reportes Preliminar MXN, HNL, SNL; Reporte preliminar detallado;
        /// Reporte preliminar detallado v2
        /// </summary>
        /// <param name="connWriteAuto"></param>
        /// <param name="tipoReporte"></param>
        /// <param name="connReadT112"></param>
        /// <param name="connReadAuto"></param>
        /// <returns></returns>
        public bool generarReporte(string connWriteAuto, string tipoReporte, string connReadT112,
                                    string connReadAuto, string etiquetaLogueo)
        {
            LNProcesoReporte reportingProcess = new LNProcesoReporte();

            try
            {
                return reportingProcess.ObtenerYGenerarReports(connWriteAuto, etiquetaLogueo, tipoReporte, connReadT112, connReadAuto);
            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[GenerarReporte] [" + ex.Message + "]");
                return false;
            }
        }


        public void generarReporteTimeout(string connAutoRead, string fechaCompensacion, string conwriteAuto, string connT112Read, string etiquetaLogueo, string timeoutReportes)
        {
            try
            {

                DNU_CompensadorParabiliumReportes.ParabiliumReportes
                                          .registrarReporte("5"
                                              , fechaCompensacion
                                              , conwriteAuto
                                              , fechaCompensacion, "RTNPT");

                Log.Evento(etiquetaLogueo + "[Inicia Obtención de Reporte para Reproceso Timeout 1 de 2 ... ]");
                IEnumerable<DataRow> IereporteTimeuout = new ParabiliumReportes().obtenerReportesV2(connAutoRead).AsEnumerable().Where(Row => Row.Field<string>("ClaveReporte").Equals("RTNPT"));
                if (IereporteTimeuout.Count() > 0)
                {
                    DataTable reporteTimeout = IereporteTimeuout.CopyToDataTable<DataRow>();
                    Log.Evento(etiquetaLogueo + "[Reporte Reproceso Timeout 1 de 2 .... Obtenido :" + reporteTimeout.Rows.Count + "]");

                    new ParabiliumReportes().generacionReportesV2_NA(reporteTimeout, etiquetaLogueo, connAutoRead
                                                                  , connT112Read, timeoutReportes, conwriteAuto);
                }
                else
                    Log.Evento(etiquetaLogueo + "[ Reportes reproceso timeout 1 de 2.. : 0 No configurado]");
            }
            catch (Exception Ex)
            {
                Log.Evento(etiquetaLogueo + "[No se pudo obtener el reporte de  reproceso timeout 1 de 2.. revise configuracion]");
            }


        }

        /// <summary>
        /// Reporte dashboard, Liquidación
        /// </summary>
        /// <param name="reportesAgenerar"></param>
        /// <param name="idLog"></param>
        /// <param name="connReadAuto"></param>
        /// <param name="connReadT112"></param>
        /// <param name="timeOut"></param>
        /// <param name="connWriteAuto"></param>
        public void generacionReportesV2(DataTable reportesAgenerar, string etiquetaLogueo
                                            , string timeOut, string connWriteAuto)
        {
            Hashtable ht;
            string conexion;
            LNProcesoReporte lnPR = new LNProcesoReporte();
            try
            {
                foreach (DataRow dr in reportesAgenerar.Rows)
                {
                    try
                    {
                        conexion = connWriteAuto;

                        ht = new Hashtable();

                        ht.Add("@fecha", dr["fechaPresentacion"]);
                        ht.Add("@ClavePlugIn", dr["ClavePlugin"]);

                        DataTable respReporte = ExecuteSp.ejecutarSpReporteV2(dr["spExecute"].ToString(), ht, conexion, timeOut);
                        bool respGenerarFile = false;

                        if (dr["claveReporte"].ToString().Equals("NLQ"))
                        {
                            respGenerarFile = lnPR.generarFileAES(dr["dirSalida"].ToString(), dr["ClavePlugin"].ToString().Replace("|","")
                                , dr["fechaPresentacion"], dr["fechaCompensacion"]
                                , respReporte, etiquetaLogueo, dr["identificador"].ToString(), null, null, dr["urlSFTP"].ToString(), dr["portSFTP"].ToString(), dr["userSFTP"].ToString(), dr["pwstSFTP"].ToString(), dr["tipoServicio"].ToString());
                        }
                        else
                        {

                            respGenerarFile = lnPR.generarFile(dr["dirSalida"].ToString(), dr["ClavePlugin"].ToString().Replace("|", "")
                                , dr["fechaPresentacion"], dr["fechaCompensacion"]
                                , respReporte, etiquetaLogueo, dr["identificador"].ToString(), null, null, dr["urlSFTP"].ToString(), dr["portSFTP"].ToString(), dr["userSFTP"].ToString(), dr["pwstSFTP"].ToString(), dr["tipoServicio"].ToString());
                        }

                        if (respGenerarFile)
                        {
                            ht = new Hashtable();

                            ht.Add("@idReporte", dr["idReporte"]);
                            ht.Add("@cveReporte", dr["claveReporte"]);

                            ExecuteSp.ejecutarSp("Procnoc_ActualizarEstatusReporte", ht, connWriteAuto);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueo + "[generacionReportesV2] " + "[ClavePlugin: " + dr["ClavePlugin"].ToString()
                                    + "] [FecPresentacion: " + dr["fechaPresentacion"] + "] [FecCompensacion: " + dr["fechaCompensacion"] +
                                    "] [ClaveReporte: " + dr["claveReporte"].ToString() + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        /// <summary>
        /// Reporte dashboard, Liquidación No actualizacion para parte 1 de 2 en reporceso Timeout....
        /// </summary>
        /// <param name="reportesAgenerar"></param>
        /// <param name="idLog"></param>
        /// <param name="connReadAuto"></param>
        /// <param name="connReadT112"></param>
        /// <param name="timeOut"></param>
        /// <param name="connWriteAuto"></param>
        public void generacionReportesV2_NA(DataTable reportesAgenerar, string etiquetaLogueo
                                        , string connReadAuto, string connReadT112
                                        , string timeOut, string connWriteAuto)
        {
            Hashtable ht;
            string conexion;
            LNProcesoReporte lnPR = new LNProcesoReporte();
            try
            {
                foreach (DataRow dr in reportesAgenerar.Rows)
                {
                    try
                    {
                        //if (dr["claveReporte"].ToString().Equals("LIQ"))
                        //    conexion = connReadAuto;
                        //else
                        //    conexion = connReadT112;

                        // if (dr["claveReporte"].ToString().Equals("DASH"))
                        conexion = connReadT112;
                        //else
                        //  conexion = connReadAuto;//Para reportes Nuevos de liquidacion 20220107

                        ht = new Hashtable();

                        ht.Add("@fecha", dr["fechaPresentacion"]);
                        ht.Add("@ClavePlugIn", dr["ClavePlugin"]);

                        DataTable respReporte = ExecuteSp.ejecutarSpReporteV2(dr["spExecute"].ToString(), ht, conexion, timeOut);

                        lnPR.generarFileTxnNoProcesasadas(dr["dirSalida"].ToString(), dr["ClavePlugin"].ToString()
                                    , dr["fechaPresentacion"], dr["fechaCompensacion"]
                                    , respReporte, etiquetaLogueo, dr["identificador"].ToString(), true, null, null, dr["urlSFTP"].ToString(), dr["portSFTP"].ToString(), dr["userSFTP"].ToString(), dr["pwstSFTP"].ToString(), dr["tipoServicio"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueo + "[generacionReportesV2] " + "[ClavePlugin: " + dr["ClavePlugin"].ToString()
                                    + "] [FecPresentacion: " + dr["fechaPresentacion"] + "] [FecCompensacion: " + dr["fechaCompensacion"] +
                                    "] [ClaveReporte: " + dr["claveReporte"].ToString() + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reporte MUC
        /// </summary>
        /// <param name="reportesAgenerar"></param>
        /// <param name="idLog"></param>
        /// <param name="connWriteAuto"></param>
        /// <param name="timeOut"></param>
        /// <param name="connReadAuto"></param>
        public void generacionReportesMUC(DataTable reportesAgenerar, string etiquetaLogueo
                                            , string connWriteAuto, string timeOut, string connReadAuto)
        {
            Hashtable ht, htUpdatMuc;
            LNProcesoReporte lnPR = new LNProcesoReporte();
            try
            {
                foreach (DataRow dr in reportesAgenerar.Rows)
                {
                    try
                    {
                        ht = new Hashtable();
                        htUpdatMuc = new Hashtable();

                        ht.Add("@fecha", dr["fechaPresentacion"]);
                        ht.Add("@ClavePlugIn", dr["ClavePlugin"]);

                        DataTable respReporte = ExecuteSp.ejecutarSpReporteV2(dr["spExecute"].ToString(), ht, connWriteAuto, timeOut);

                        //htUpdatMuc.Add("@fechaGeneracion", dr["fechaPresentacion"]);
                        //htUpdatMuc.Add("@idColectiva", dr["ClavePlugin"]);
                        //htUpdatMuc.Add("@idPoliza", dr["ClavePlugin"]);
                        //ExecuteSp.updateColectivaMUC("Procnoc_Comp_ActualizaColectivaMuc", htUpdatMuc, connWriteAuto, timeOut);

                        lnPR.generarFileAES(dr["dirSalida"].ToString(), dr["ClavePlugin"].ToString()
                                    , dr["fechaPresentacion"], dr["fechaCompensacion"]
                                    , respReporte, etiquetaLogueo, dr["identificador"].ToString(), null, null, dr["urlSFTP"].ToString(), dr["portSFTP"].ToString(), dr["userSFTP"].ToString(), dr["pwstSFTP"].ToString(), dr["tipoServicio"].ToString());

                        ht = new Hashtable();

                        ht.Add("@idReporte", dr["idReporte"]);
                        ht.Add("@cveReporte", dr["claveReporte"]);

                        ExecuteSp.ejecutarSp("Procnoc_ActualizarEstatusReporte", ht, connWriteAuto);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(etiquetaLogueo + "[generacionReportesMUC] " + "[ClavePlugin: " + dr["ClavePlugin"].ToString()
                                    + "] [FecPresentacion: " + dr["fechaPresentacion"] + "] [FecCompensacion: " + dr["fechaCompensacion"] +
                                    "] [ClaveReporte: " + dr["claveReporte"].ToString() + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable obtenerReportesV2(string connReadAuto)
        {
            try
            {
                Hashtable ht;
                ht = new Hashtable();
                ht.Add("@cveReporte", null);
                DataTable reportesAgenerar = ExecuteSp.ejecutarSp("Procnoc_ObtenerReportesAgenerar_v7", ht, connReadAuto);

                return reportesAgenerar;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void procesarReportesMUC(string connWriteAut, string etiquetaLogueo, string timeOut, string connReadAuto)
        {
            try
            {
                Hashtable ht;
                ht = new Hashtable();
                Log.Evento(etiquetaLogueo + "[Inicia Obtención de Reportes MUC]");
                DataTable dtReportesAgenerar = ExecuteSp.ejecutarSp("ProcNoct_COMP_ObtieneReportesAGenerarMUC", ht, connWriteAut);
                Log.Evento(etiquetaLogueo + "[Finaliza Obtención de Reportes MUC totales: " + dtReportesAgenerar.Rows.Count + "]");

                Log.Evento(etiquetaLogueo + "[Inicia Generación de Reportes MUC]");
                generacionReportesMUC(dtReportesAgenerar, etiquetaLogueo, connWriteAut, timeOut, connReadAuto);
                Log.Evento(etiquetaLogueo + "[Finaliza Generación de Reportes totales MUC: " + dtReportesAgenerar.Rows.Count + "]");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Transaction]
        public void procesadorReportesMUC(string connWriteAut, string etiquetaLogueo, string timeOut, string connReadAuto)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("procesadorReportesMUC");

            try
            {

                Hashtable ht;
                ht = new Hashtable();
                Log.Evento(etiquetaLogueo + "[Inicia Obtención de Reportes MUC]");
                DataTable dtReportesAgenerar = ExecuteSp.ejecutarSp("ProcNoct_COMP_Proceso_ObtieneReportesAGenerarMUC", ht, connReadAuto);
                Log.Evento(etiquetaLogueo + "[Finaliza Obtención de Reportes MUC totales: " + dtReportesAgenerar.Rows.Count + "]");

                Log.Evento(etiquetaLogueo + "[Inicia Generación de Reportes MUC]");
                generacionReportesMUC(dtReportesAgenerar, etiquetaLogueo, connWriteAut, timeOut, connReadAuto);
                Log.Evento(etiquetaLogueo + "[Finaliza Generación de Reportes totales MUC: " + dtReportesAgenerar.Rows.Count + "]");

            }
            catch (Exception ex)
            {
                ApmNoticeWrapper.NoticeException(ex);
                throw ex;
            }
        }

        public static void registrarReporte(string emisor, string fechaPresentacion
                                                        , string connWriteAuto, string fechaComp, string tipoReporte)
        {
            try
            {
                if (!emisor.Equals("0"))
                    LNProcesoReporte.insertarReporte(emisor, fechaPresentacion, connWriteAuto, fechaComp, tipoReporte);
            }
            catch (Exception ex)
            {
                Log.Error("[registrarReporte " + emisor + ", " + fechaPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        public static void registrarReportePRED(string fechaPresentacion
                                                        , string connWriteAuto, string fechaComp, string tipoReporte
                                                        , string ma)
        {
            try
            {
                LNProcesoReporte.insertarReportePRED(fechaPresentacion, connWriteAuto, fechaComp, tipoReporte, ma);
            }
            catch (Exception ex)
            {
                Log.Error("[registrarReporte, " + fechaPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }


        public static void registrarReportePREDV2(string fechaPresentacion
                                                        , string connWriteAuto, string fechaComp, string tipoReporte
                                                        , string ma)
        {
            try
            {
                LNProcesoReporte.insertarReportePREDV2(fechaPresentacion, connWriteAuto, fechaComp, tipoReporte, ma);
            }
            catch (Exception ex)
            {
                Log.Error("[registrarReporte, " + fechaPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }


        //Genera un delay antes de generar los reportes
        //tipoProceso = 0 => ProcesoMigracion
        //tipoProceos = 1 => ProcesoCompensación
        public void DelayReportes(string connT112Write, string connT112Read, int timeDelay, string tipoProceso, string etiquetaLogueo)
        {
            long idFicheroDetalleWrite = Convert.ToInt64(ExecuteSp.obtenerIdFicheroDetalle_IDPoliza(connT112Write, tipoProceso)[0]);
            long idFicheroDetalleRead = 0;
            int contador = 0;


            while (contador < 10)
            {
                Thread.Sleep(timeDelay);

                idFicheroDetalleRead = Convert.ToInt64(ExecuteSp.obtenerIdFicheroDetalle_IDPoliza(connT112Read, tipoProceso)[0]);

                if (idFicheroDetalleWrite <= idFicheroDetalleRead)
                {
                    Log.Evento(etiquetaLogueo + "[DelayReportesHomologacion] [NumeroIteraciones: " + (contador + 1) + "]");
                    break;
                }

                contador += 1;
            }
        }
    }
}