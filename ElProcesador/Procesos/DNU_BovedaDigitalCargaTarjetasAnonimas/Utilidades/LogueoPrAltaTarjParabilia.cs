using CommonProcesador;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_BovedaDigitalCargaTarjetasAnonimas.Utilidades
{
    public class LogueoPrAltaTarjParabilia
    {
        private static readonly log4net.ILog _loggerError = log4net.LogManager.GetLogger(PNConfig.Get("CARGABOVEDADIGITAL", "LogError"));
        private static readonly log4net.ILog _loggerInfo = log4net.LogManager.GetLogger(PNConfig.Get("CARGABOVEDADIGITAL", "LogInfo"));
        private static readonly log4net.ILog _loggerDebug = log4net.LogManager.GetLogger(PNConfig.Get("CARGABOVEDADIGITAL", "LogDebug"));

        static LogueoPrAltaTarjParabilia()
        {
            XmlConfigurator.Configure(new Uri(PNConfig.Get("CARGABOVEDADIGITAL", "LogConfig")));
        }

        public static void Error(String Error)
        {
            _loggerError.Error(Error.Replace("\n", " ").Replace("\r", " "));// + ". ORIGINAL: " + Error.InnerException == null ? "" : Error.InnerException.Message);

        }

        public static void Warn(String Error)
        {
            _loggerError.Warn(Error.Replace("\n", " ").Replace("\r", " "));// + ". ORIGINAL: " + Error.InnerException == null ? "" : Error.InnerException.Message);

        }

        public static void Info(String Error)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));// + ". ORIGINAL: " + Error.InnerException == null ? "" : Error.InnerException.Message);

        }
        public static void Info(String Error, string mensaje)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));// + ". ORIGINAL: " + Error.InnerException == null ? "" : Error.InnerException.Message);

        }
        
        public static void Debug(String Error)
        {
            _loggerDebug.Debug(Error.Replace("\n", " ").Replace("\r", " "));// + ". ORIGINAL: " + Error.InnerException == null ? "" : Error.InnerException.Message);

        }
    }
}
