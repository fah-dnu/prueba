using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_WebhookSender.LogicaNegocio
{
    public static class Log
    {
        static readonly string app = "[WEBHOOK_SENDER]";
        public static void LogEvento(String msg)
        {
            Logueo.Evento(String.Format("{0} | {1}", app,msg));
        }
        public static void LogError(String msg)
        {
            Logueo.Error(String.Format("{0} | {1}", app, msg));
        }

        public static void LogError(Exception ex)
        {
            Logueo.Error(String.Format("{0} | {1}", app, ex.Message));
        }

        public static void LogEntradaSalida(String msg, string user, bool esEntrada)
        {
            Logueo.EntradaSalida(String.Format("{0} | {1}", app, msg), user, esEntrada);
        }
    }
}
