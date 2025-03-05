using CommonProcesador;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Utilities
{
    class LogueoAltaEmpleadoV2
    {
        private log4net.ILog _loggerError = log4net.LogManager.GetLogger(PNConfig.Get("ALTAEMPLEADODNU", "LogError"));
        private log4net.ILog _loggerInfo = log4net.LogManager.GetLogger(PNConfig.Get("ALTAEMPLEADODNU", "LogInfo"));
        private log4net.ILog _loggerDebug = log4net.LogManager.GetLogger(PNConfig.Get("ALTAEMPLEADODNU", "LogDebug"));

        //static LogueoAltaEmpleadoV2()
        //{
        //    // XmlConfigurator.Configure();//
        //    XmlConfigurator.Configure(new Uri(PNConfig.Get("ALTAEMPLEADODNU", "LogConfig")));
        //}

        public LogueoAltaEmpleadoV2(string idLog, string appID, string user)
        {
            // XmlConfigurator.Configure();//
            XmlConfigurator.Configure(new Uri(PNConfig.Get("ALTAEMPLEADODNU", "LogConfig")));

            ThreadContext.Properties["ts"] = DateTime.Now.ToString("yyyyMMddHHmmss");
            ThreadContext.Properties["IpAddress"] = System.Net.Dns.GetHostName();
            ThreadContext.Properties["AppId"] = appID;
            ThreadContext.Properties["User"] = user;
            ThreadContext.Properties["TraceId"] = idLog is null ? "" : idLog;

        }

        public void Error(String Error)
        {
            _loggerError.Error(Error.Replace("\n", " ").Replace("\r", " "));
        }

        public void Warn(String Error)
        {
            _loggerError.Warn(Error.Replace("\n", " ").Replace("\r", " "));

        }

        public void Info(String Error)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));

        }

        public void Info(String Error, string mensaje)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));
        }

        public void Info(String Error, string mensaje, string mensaje2)
        {
            _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));
        }

        public void Debug(String Error)
        {
            _loggerDebug.Debug(Error.Replace("\n", " ").Replace("\r", " "));

        }

        //public static void Error(String Error)
        //{
        //    _loggerError.Error(Error.Replace("\n", " ").Replace("\r", " "));
        //}

        //public static void Warn(String Error)
        //{
        //    _loggerError.Warn(Error.Replace("\n", " ").Replace("\r", " "));

        //}

        //public static void Info(String Error)
        //{
        //    _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));

        //}

        //public static void Info(String Error, string mensaje)
        //{
        //    _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));
        //}

        //public static void Info(String Error, string mensaje, string mensaje2)
        //{
        //    _loggerInfo.Info(Error.Replace("\n", " ").Replace("\r", " "));
        //}

        //public static void Debug(String Error)
        //{
        //    _loggerDebug.Debug(Error.Replace("\n", " ").Replace("\r", " "));

        //}

    }
}
