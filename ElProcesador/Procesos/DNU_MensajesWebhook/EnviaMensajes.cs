#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_MensajesWebhook.LogicaNegocio;
using DNU_MensajesWebhook.Utilidades;
using log4net;
using System;
using System.Configuration;

namespace DNU_MensajesWebhook
{
    public class EnviaMensajes : IProcesoNocturno
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            try
            {
             
                ConfiguracionContexto.InicializarContexto();
                return LNEnvio.LanzaEnvioDeMensajes(direccionIP, idLog.ToString());
            }

            catch (Exception err)
            {
                LogueoReMsjsWebHook.Error("[" + direccionIP + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + idLog + "][" + err.Message + "]");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IProcesoNocturno.Iniciar()
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
            }
            catch (Exception err)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][" + err.Message + "]");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IProcesoNocturno.Detener()
        {
            string log = "";//ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
            }
            catch (Exception err)
            {
                LogueoReMsjsWebHook.Error("[" + ip + "] [EnviaMensajes] [PROCESADORNOCTURNO] [" + log + "][" + err.Message + "]");
             }
        }
    }
}
