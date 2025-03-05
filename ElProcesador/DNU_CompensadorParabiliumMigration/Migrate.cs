#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumMigration.LogicaNegocio;
using DNU_CompensadorParabiliumMigration.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumMigration
{
    public class Migrate : IProcesoNocturno
    {
        public void Detener()
        {
            
        }

        public void Iniciar()
        {
        }

        bool IProcesoNocturno.Procesar()
        {

            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            string etiquetaProcesoHomologacionLog = "[" + ip + "] [" + log + "] [" + Processes.PROCESA_HOMOLOGACION.ToString() + "] ";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                LogueoProcesaMigracion.Info(etiquetaProcesoHomologacionLog + "[INICIA PROGRAMADO AT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                Start(etiquetaProcesoHomologacionLog);

                return true;

            }
            catch(Exception ex)
            {
                LogueoProcesaMigracion.Error(etiquetaProcesoHomologacionLog + "[" + ex.Message + "]");
                return false;
            }
        }


        public static void Start(string etiquetaLogueo)
        {
            ManageMigration.DoHomologacion(etiquetaLogueo);
        }
    }
}
