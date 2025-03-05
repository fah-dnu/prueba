using CommonProcesador;
using DNU_ParabiliaReportesAzureBlobStorage.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaReportesAzureBlobStorage
{
    public class ProcesaReportes : IProcesoNocturno
    {
        void IProcesoNocturno.Iniciar()
        {
            
        }

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                string Con = PNConfig.Get("REPORTES_AZUREBS", "BDReadAuto");
                string Plugin = PNConfig.Get("REPORTES_AZUREBS", "Colectivas");

                string nombreArchivoMovtosDiario = PNConfig.Get("REPORTES_AZUREBS", "RepMovDiarios");
                string nombreArchivoFondeo = PNConfig.Get("REPORTES_AZUREBS", "RepFondeo");
                string nombreArchivoMovsCtaEje = PNConfig.Get("REPORTES_AZUREBS", "RepMovsCtaEje") + DateTime.Now.ToString("yyyyMMdd");
                string nombreArchivoActividadDiaria = PNConfig.Get("REPORTES_AZUREBS", "RepActividadDiaria") + DateTime.Now.ToString("yyyyMMdd");
                string nombreArchivoNuevoLQ = PNConfig.Get("REPORTES_AZUREBS", "RepNuevoLQ") + Plugin + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("yyyyMMdd");
                string nombreArchivoDA_Monge = PNConfig.Get("REPORTES_AZUREBS", "RepActividadDiaria") + "_" + DateTime.Now.ToString("yyyyMMdd");
                
                
                int generaReporteMonge = Convert.ToInt32(PNConfig.Get("REPORTES_AZUREBS", "GeneraReporteMonge"));
                int generaReporteABU = Convert.ToInt32(PNConfig.Get("REPORTES_AZUREBS", "GeneraReporteABU"));
                int generaReporteEstadoTarjetas = Convert.ToInt32(PNConfig.Get("REPORTES_AZUREBS", "GeneraReporteEstadoTarjetas"));
                int generaReporteTransfronterizo = Convert.ToInt32(PNConfig.Get("REPORTES_AZUREBS", "GeneraReporteTransfronterizo"));
                int GeneraReporteAsientosContables = Convert.ToInt32(PNConfig.Get("REPORTES_AZUREBS", "GeneraReporteAsientosContables"));

                string rutaArchivoMovsCtaEje = PNConfig.Get("REPORTES_AZUREBS", "RutaMovsCtaEje");
                string rutaArchivoActividadDiaria = PNConfig.Get("REPORTES_AZUREBS", "RutaActividadDiaria");
                string rutaArchivoNuevoLQ = PNConfig.Get("REPORTES_AZUREBS", "RutaNuevoLQ");

                LNReportes _reportes = new LNReportes();
                _reportes.CreaDirectorios();


                if (generaReporteMonge == 1)
                {
                    #region ReporteMovimientosDiario
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte de Movimientos Diario.");
                        bool ban = _reportes.GeneraReporteMovimientosDiario(Con, Plugin, nombreArchivoMovtosDiario);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Movimientos Diario ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteMovimientosDiario

                    #region ReporteFondeo
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte de Fondeo.");
                        bool banFondeo = _reportes.GeneraReporteFondeo(Con, Plugin, nombreArchivoFondeo);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Fondeo ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteFondeo

                    #region ReporteMovsCtaEje
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte de Cuenta Eje.");
                        bool ban = _reportes.GeneraReporteMovsCtaEje(Con, Plugin, nombreArchivoMovsCtaEje, rutaArchivoMovsCtaEje);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Cuenta Eje ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteMovsCtaEje

                    #region ReporteActividadDiaria
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte de Actividad Diaria.");
                        bool ban = _reportes.GeneraReporteActividadDiaria(Con, Plugin, nombreArchivoActividadDiaria, rutaArchivoActividadDiaria);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Actividad Diaria] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteActividadDiaria

                    #region ReporteNuevoLQ
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte Nuevo LQ.");
                        bool ban = _reportes.GeneraReporteNuevoLQ(Con, Plugin, nombreArchivoNuevoLQ, rutaArchivoNuevoLQ);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte Nuevo LQ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteNuevoLQ

                    #region ReporteActividadDiariaMonge
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] Inicia Reporte de Actividad Diaria.");
                        bool ban = _reportes.GeneraReporteActividadDiariaMonge(Con, Plugin, nombreArchivoDA_Monge);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Actividad Diaria ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion ReporteActividadDiariaMonge
                }

                if (generaReporteTransfronterizo == 1)
                {
                    #region Reporte TRXIVA
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] [Inicia Reporte Transfronterizo]");
                        bool ban = _reportes.GeneraReporteTransaccionesTransfronterizo(Con);
                        Logueo.EventoInfo("[generacionReporteAzure] [Termina Reporte Transfronterizo]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte Transfronterizo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion

                    #region Reporte ACUMIVA
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] [Inicia Reporte Transfronterizo]");
                        bool ban = _reportes.GeneraReporteAcumuladoTransfronterizo(Con);
                        Logueo.EventoInfo("[generacionReporteAzure] [Termina Reporte Transfronterizo]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte Transfronterizo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion

                    #region Reporte ECOMTRX
                    try
                    {
                        Logueo.EventoInfo("[generacionReporteAzure] [Inicia Reporte Transfronterizo]");
                        bool ban = _reportes.GeneraReporteTransfronterizo(Con);
                        Logueo.EventoInfo("[generacionReporteAzure] [Termina Reporte Transfronterizo]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte Transfronterizo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion
                }

                if (generaReporteABU == 1)
                {
                    #region Reporte ABU
                    try
                    {
                        Logueo.EventoInfo("[GeneraReporteABU] [Inicia Reporte ABU]");
                        bool ban = _reportes.GeneraReporteABU(Con);
                        Logueo.EventoInfo("[GeneraReporteABU] [Termina Reporte ABU]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteABU] [Error al Generar Reporte de Movimientos Diario ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion Reporte ABU
                }


                if (generaReporteEstadoTarjetas == 1)
                {
                    #region Reporte EstadoTarjetas Optima
                    try
                    {
                        Logueo.EventoInfo("[GeneraReporteEstadoTarjetas] [Inicia Reporte Estado de Tarjetas]");
                        bool ban = _reportes.GeneraReporteEstatusTarjetas(Con);
                        Logueo.EventoInfo("[GeneraReporteEstadoTarjetas] [Termina Reporte Estado de Tarjetas]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteEstadoTarjetas] [Error al Generar Reporte de Estado de Tarjetas ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion Reporte EstadoTarjetas Optima
                }

                if (GeneraReporteAsientosContables == 1)
                {
                    #region Reporte Asientos Contables
                    try
                    {
                        Logueo.EventoInfo("[GeneraReporteAsientosContablesDetallado] [Inicia Reporte Asientos Contables Detallado]");
                        bool ban = _reportes.GeneraReporteAsientosContablesDetallado(Con);
                        Logueo.EventoInfo("[GeneraReporteAsientosContablesDetallado] [Termina Reporte Asientos Contables Detallado]");
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteAsientosContablesDetallado] [Error al Generar Reporte de Asientos Contables Detallado ] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    #endregion Reporte Asientos Contables
                }

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[generacionReporteAzure] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            return false;
        }

        void IProcesoNocturno.Detener()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al detener Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }
    }
}
