using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using DNU_CompensadorParabiliumReportes;
using log4net;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using DNU_CompensadorParabiliumCommon.Constants;

namespace DNU_CompensadorParabiliumProcesador.LogicaNegocio
{

    public static class Compensacion
    {
        private static string _NombreNewRelic;
        private static string etiquetaProcesoComp = Processes.PROCESA_COMPENSACION.ToString();
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROCESA_COMPENSACION";
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
        public static bool EjecutaCompensacion(string etiquetaProcesoCompLog)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("EjecutaCompensacion");

            try
            {
                string connAutoRead = PNConfig.Get(etiquetaProcesoComp, "BDReadAutorizador");
                string connAutoWrite = PNConfig.Get(etiquetaProcesoComp, "BDWriteAutorizador");
                string connCompRead = PNConfig.Get(etiquetaProcesoComp, "BDReadCompensador");
                string timeout = PNConfig.Get(etiquetaProcesoComp, "TimeOutReportes");
                string connCompWrite = PNConfig.Get(etiquetaProcesoComp, "BDWriteCompensador");
                int timeDelay = Convert.ToInt32(PNConfig.Get(etiquetaProcesoComp, "TimeDelayReport"));

                RespuestaProceso respuestaProceso;
                try
                {
                    respuestaProceso = LNArchivoProcesador.ProcesaArchivosDesdeBD(etiquetaProcesoCompLog);
                }
                catch (Exception ex)
                {
                    LogueoCompensador.Error(etiquetaProcesoCompLog + "[Error en la ejecucion del proceso: " + ex.Message);
                    ApmNoticeWrapper.NoticeException(ex);
                    return false;
                }

               
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[INICIA PROCESO DE REPORTES]");
                
                try
                {
                    ParabiliumReportes comReporte = new ParabiliumReportes();

                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Inicia DELAY de Reportes]");
                    comReporte.DelayReportes(connCompWrite, connCompRead, timeDelay, "1", etiquetaProcesoCompLog);
                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Finaliza DELAY de Reportes]");


                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Inicia Obtención de Reportes]");
                    DataTable dtReportes = comReporte.obtenerReportesV2(connAutoRead);
                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Finaliza Obtención de Reportes totales: " + dtReportes.Rows.Count + "]");

                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Inicia Generación de Reportes]");
                    comReporte.generacionReportesV2(dtReportes, etiquetaProcesoCompLog, timeout, connAutoWrite);
                    LogueoCompensador.Info(etiquetaProcesoCompLog + "[Finaliza Generación de Reportes totales: " + dtReportes.Rows.Count + "]");

                }
                catch (Exception ex)
                {
                    LogueoCompensador.Error(etiquetaProcesoCompLog + "[" + ex.Message + "] [" + ex.StackTrace + "]");
                    ApmNoticeWrapper.NoticeException(ex);
                }

                LogueoCompensador.Info(etiquetaProcesoCompLog + "[FINALIZA PROCESO DE REPORTES]");

                return true;

            }
            catch (Exception err)
            {
                LogueoCompensador.Error(etiquetaProcesoCompLog + "[" + err.Message + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }
        }

    }


}
