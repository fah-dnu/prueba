#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumProcesador.LogicaNegocio;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumProcesador
{
    public class ParabiliumProcesador : IProcesoNocturno
    {
        string log = "";
        string ip = "";
        public ParabiliumProcesador()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }
        void IProcesoNocturno.Detener()
        {
            throw new NotImplementedException();
        }

        void IProcesoNocturno.Iniciar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [" + Processes.PROCESA_COMPENSACION.ToString() + "] ";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[INICIA EscucharDirectorios]");
                LNArchivoListener.EscucharDirectorio(etiquetaProcesoCompLog);
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[TERMINA EscucharDirectorios]");

            }
            catch(Exception ex)
            {
                LogueoCompensador.Error(etiquetaProcesoCompLog +"[" + ex.Message + "]");
            }
        }

        bool IProcesoNocturno.Procesar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [" + Processes.PROCESA_COMPENSACION.ToString() + "] ";

            try
            {
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[INICIA EjecutaCompensacion]");
                ConfiguracionContexto.InicializarContexto();
                var res = Compensacion.EjecutaCompensacion(etiquetaProcesoCompLog);
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[TERMINA EjecutaCompensacion]");

                return res;

            }
            catch(Exception ex)
            {
                LogueoCompensador.Error(etiquetaProcesoCompLog + "[" + ex.Message + "]");
                return false;
            }
        }



        public static void Start()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [" + Processes.PROCESA_COMPENSACION.ToString() + "] ";
            Compensacion.EjecutaCompensacion(etiquetaProcesoCompLog);
            LogueoCompensador.Info("[" + direccionIP + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + idLog + "] [TERMINA EjecutaCompensacion]");
        }


        public static void Listening()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [" + Processes.PROCESA_COMPENSACION.ToString() + "] ";
            ConfiguracionContexto.InicializarContexto();
            LogueoCompensador.Info(etiquetaProcesoCompLog + "[INICIA EscucharDirectorios]");
            LNArchivoListener.EscucharDirectorio(etiquetaProcesoCompLog);
            LogueoCompensador.Info(etiquetaProcesoCompLog + "[TERMINA EscucharDirectorios]");
            Thread.Sleep(440000);
        }
    }
}
