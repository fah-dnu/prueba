using DNU.ConexionLogSockets.LNConexion;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonProcesador
{
    public class AppLogger
    {
        //private static readonly ILog _loggerError = LogManager.GetLogger("");
        private static readonly log4net.ILog _loggerError = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerErrorProcesador"]);
        private static readonly log4net.ILog _loggerInfo = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerInfoProcesador"]);
        private static readonly log4net.ILog _loggerDebug = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerDebugProcesador"]);
       
        static AppLogger()
        {
            XmlConfigurator.Configure();
        }

        public static void Error(String Error)
        {
            _loggerError.Error(Error.Replace("\n", " ").Replace("\r", " "));

        }

        public static void Info(String Error)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));

        }

       
        public static void Debug(String Error, string idLog, string ip)
        {
            var log = LNServerSocket.envioLogAsync(Error, ip, idLog, "", "PN");
        }             
        
    }
}

