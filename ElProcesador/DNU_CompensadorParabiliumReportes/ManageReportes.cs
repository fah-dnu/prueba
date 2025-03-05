using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumReportes
{
    public class ManageReportes : IProcesoNocturno
    {
        public void Detener()
        {
            throw new NotImplementedException();
        }

        public void Iniciar()
        {
            throw new NotImplementedException();
        }

        public bool Procesar()
        {
            try
            {
                string direccionIP = System.Net.Dns.GetHostName();
                Guid idLog = Guid.NewGuid();

                string etiquetaProcesoReportLog = "[" + direccionIP + "] [" + idLog + "] [REPORTES] ";

                Log.Evento("[" + idLog + "] [REPORTESMUC] INICIA PROGRAMADO AT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                ConfiguracionContexto.InicializarContexto();
                ParabiliumReportes pr = new ParabiliumReportes();

                pr.procesadorReportesMUC(PNConfig.Get("PROC_REPORTES", "BDWriteAutorizador"), etiquetaProcesoReportLog
                                    , PNConfig.Get("PROC_REPORTES", "TimeOutReportes")
                                    , PNConfig.Get("PROC_REPORTES", "BDReadAutorizador"));

                return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }
    }
}