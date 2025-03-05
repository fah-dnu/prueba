#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_WebhookSender.LogicaNegocio;
using DNU_WebhookSender.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DNU_WebhookSender
{
    public class Manager : IProcesoNocturno
    {
        private static System.Timers.Timer aTimer;
        string log = "";
        string ip = "";
        public Manager()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }

        public static void Start()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            ConfiguracionContexto.InicializarContexto();

            SetTimer(direccionIP, idLog.ToString());
        }


        private static void SetTimer(string ip="",string log="")
        {////
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            var span = PNConfig.Get("WEBHOOK", "TIMER").ToString();

            LogueoWebHook.Info("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[WEBHOOK_SENDER, Timer : {0}", span) + "]");
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(Convert.ToInt32(span));
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            //timer.Elapsed += (sender, e) => MyElapsedMethod(sender, e, ip, log);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

           // Thread.Sleep(1080000);
        }


        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;

            LNThreadManager.startThreadingProcess(direccionIP,idLog.ToString());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IProcesoNocturno.Procesar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                //Start();
                return true;
            }

            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IProcesoNocturno.Iniciar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {

                Start();
            }
            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void IProcesoNocturno.Detener()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
                aTimer.Stop();
                aTimer.Dispose();
                LNThreadManager.Stop(ip,log);
            }
            catch (Exception err)
            {
                LogueoWebHook.Error("[" + ip + "] [WebHook] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
            }
        }
    }
}
