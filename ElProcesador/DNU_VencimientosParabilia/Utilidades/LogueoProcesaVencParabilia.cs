using CommonProcesador;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.Utilidades
{
    public class LogueoProcesaVencParabilia
    {
       // private static readonly log4net.ILog _loggerError = log4net.LogManager.GetLogger(PNConfig.Get("PROCESAVENCPARABILIA", "LogError"));
       // private static readonly log4net.ILog _loggerInfo = log4net.LogManager.GetLogger(PNConfig.Get("PROCESAVENCPARABILIA", "LogInfo"));
       // private static readonly log4net.ILog _loggerDebug = log4net.LogManager.GetLogger(PNConfig.Get("PROCESAVENCPARABILIA", "LogDebug"));

        private static readonly log4net.ILog _loggerError = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerErrorProcesador"]);
        private static readonly log4net.ILog _loggerEventos = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerInfoProcesador"]);
        private static readonly log4net.ILog _loggerRecibidos = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerDebugProcesador"]);


        static LogueoProcesaVencParabilia()
        {
            XmlConfigurator.Configure();//    XmlConfigurator.Configure(new Uri(PNConfig.Get("PROCESAVENCPARABILIA", "LogConfig")));
        }

        public static void Error(String Error)
        {
            _loggerError.Error(Error.Replace("\n", " ").Replace("\r", " "));

        }

        public static void Warn(String Error)
        {
            _loggerError.Warn(Error.Replace("\n", " ").Replace("\r", " "));

        }

        public static void Info(String Error)
        {
            _loggerEventos.Info(Error.Replace("\n", " ").Replace("\r", " "));

        }
        public static void Info(String Error, string mensaje)
        {
            _loggerEventos.Info(Error.Replace("\n", " ").Replace("\r", " "));

        }

       
        public static void Debug(String Error)
        {
          //  _loggerDebug.Debug(Error.Replace("\n", " ").Replace("\r", " "));

        }
    }
}
