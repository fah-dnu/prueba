using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Entidades.Response;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumMigration.Utilidades;
using DNU_CompensadorParabiliumReportes;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using System.Runtime.CompilerServices;

namespace DNU_CompensadorParabiliumMigration.LogicaNegocio
{
    public class ManageMigration
    {
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = Processes.PROCESA_HOMOLOGACION.ToString();
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
        public static bool DoHomologacion(string etiquetaLogueo)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("DoHomologacion");

          
            string connAutoRead = PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "BDReadAutorizador");
            string connAutoWrite = PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "BDWriteAutorizador");
            int timeoutHomologacion = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "TimeOutHomologacion") ?? "240");
            
            string connCompRead = PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "BDReadCompensador");
            string connCompWrite = PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "BDWriteCompensador");
            int timeDelay = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_HOMOLOGACION.ToString(), "TimeDelayReport"));

            //Sub Modulo => Sincronizar Catalogos
            List<Catalogo> lstCatalogos = DAOMigracion.ObtenerCatalogos(connAutoRead, etiquetaLogueo);

            DAOMigracion.ActualizarCatalogos(connCompWrite, lstCatalogos, etiquetaLogueo);

            //Sub Modulo => Restablecimiento de Registros con Error
            DAOMigracion.RestablecerRegistros(connCompWrite, timeoutHomologacion, etiquetaLogueo);

            //Sub Modulo => Consolidación de Registros
            ConsolidacionRegistros(connCompWrite, timeoutHomologacion, etiquetaLogueo);

            //Sub Modulo => Homologacion de Registros
            HomologacionRegistros(connCompWrite, timeoutHomologacion, etiquetaLogueo, connAutoRead);

            //Sub Modulo => - Solicitud de Reporte
            ParabiliumReportes procRreportes = new ParabiliumReportes();
            try
            {
                procRreportes.DelayReportes(connCompWrite, connCompRead, timeDelay, "0", etiquetaLogueo);
            }
            catch (Exception ex)
            {
                LogueoProcesaMigracion.Error(etiquetaLogueo + "[DELAY] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }

            SolicitudReportes(connCompWrite, connAutoWrite, timeoutHomologacion, etiquetaLogueo);


            //GENERAR REPORTES PRELIMINAR
            
            try
            {
                procRreportes.generarReporte(connAutoWrite, "RPRE", connCompWrite
                                    , connAutoRead, etiquetaLogueo);
            }
            catch (Exception ex)
            {
                LogueoProcesaMigracion.Error(etiquetaLogueo + "[" + ex.Message + "] [" + ex.StackTrace + "]");
                ApmNoticeWrapper.NoticeException(ex);
            }
            

            //GENERAR REPORTE PRELIMINAR DETALLADO
            try
            {
                procRreportes.generarReporte(connAutoWrite, "DPRE", connCompWrite
                                            , connAutoWrite, etiquetaLogueo);
            }
            catch (Exception ex)
            {
                LogueoProcesaMigracion.Error(etiquetaLogueo + "[" + ex.Message + "] [" + ex.StackTrace + "]");
                ApmNoticeWrapper.NoticeException(ex);
            }

            return true;

        }

        public static void ConsolidacionRegistros(string conn, int timeoutHomologacion, string etiquetaLogueo)
        {

            List<RegistroConsolidar> lstRegConsolidar = DAOMigracion.ObtenerRegistrosConsolidar(conn, timeoutHomologacion, etiquetaLogueo);

            foreach (RegistroConsolidar reg in lstRegConsolidar) 
            {
                DAOMigracion.ConsolidarRegistro(conn, reg, timeoutHomologacion, etiquetaLogueo);
            }

            DAOMigracion.RestablecerRegistrosPorConsolidar(conn, timeoutHomologacion, etiquetaLogueo);
        }

        public static void HomologacionRegistros(string conn, int timeoutHomologacion, string etiquetaLogueo, string connAuto)
        {
            bool homologarRegistro = true;
            List<RegistroHomologar> lstRegHomologar = DAOMigracion.ObtenerRegistrosHomologar(conn, timeoutHomologacion, etiquetaLogueo);
            string descRespuesta = null;
          
            foreach (RegistroHomologar reg in lstRegHomologar)
            {
                homologarRegistro = true;
                if (!string.IsNullOrEmpty(reg.CveTipoAlias)) 
                {
                    string medioAcceso = null, bin = null;

                    DAOMigracion.ObtenerTarjeta(connAuto, reg.CveTipoAlias, reg.Alias, etiquetaLogueo, ref medioAcceso, ref bin);
                    if (string.IsNullOrEmpty(medioAcceso))
                    {
                        DAOMigracion.ActualizarEstatusFicheroDetalleTemp(reg.IdFicheroDetalleTemp, EstatusFicheroDetalleTemp.INVALID.ToString(), conn, etiquetaLogueo, "1802");
                        homologarRegistro = false;
                    }
                    else
                    {
                        bool respSustituir = DAOMigracion.SustituirAlias(conn, reg.IdFicheroDetalleTemp, medioAcceso, etiquetaLogueo, bin, ref descRespuesta);

                        if (!respSustituir) 
                        {
                            DAOMigracion.ActualizarEstatusFicheroDetalleTemp(reg.IdFicheroDetalleTemp, EstatusFicheroDetalleTemp.ERROR.ToString(), conn, etiquetaLogueo, null, descRespuesta);
                            homologarRegistro = false;
                        }
                    }

                }

                if(homologarRegistro)
                    DAOMigracion.HomologarRegistro(conn, reg, timeoutHomologacion, etiquetaLogueo);
            }
            //Pendiente de validar
            //DAOMigracion.RestablecerRegistrosPorHomologar(conn, timeoutHomologacion, etiquetaLogueo);
        }

        public static void SolicitudReportes(string connComp, string connAuto, int timeoutHomologacion, string etiquetaLogueo)
        {

            List<SolicitudReporte> lstSolicitudReporte = DAOMigracion.ObtenerSolicitudesReportes(connComp, timeoutHomologacion, etiquetaLogueo);
            string folioReporte = null;
            foreach (SolicitudReporte reg in lstSolicitudReporte)
            {
                RespuestaGral resp = DAOMigracion.InsertarReporte(connAuto, reg, ref folioReporte, etiquetaLogueo);

                if (string.IsNullOrEmpty(resp.CodRespuesta))
                {
                    DAOMigracion.ActualizarEstatusReporte(connComp, "ERROR", folioReporte, reg.ID_SolicitudReporte, etiquetaLogueo);
                }
                else
                {
                    int valCodigo = Convert.ToInt32(resp.CodRespuesta);
                    if (valCodigo >= 500 && valCodigo <= 599)
                        DAOMigracion.ActualizarEstatusReporte(connComp, "ERROR", folioReporte, reg.ID_SolicitudReporte, etiquetaLogueo);
                    else
                        DAOMigracion.ActualizarEstatusReporte(connComp, "OK", folioReporte, reg.ID_SolicitudReporte, etiquetaLogueo);
                }
            }

            DAOMigracion.RestablecerRegistrosReportes(connComp, timeoutHomologacion, etiquetaLogueo);
        }
    }
}
