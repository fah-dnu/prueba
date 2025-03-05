using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;
using System.Configuration;
using DNU.ConexionLogSockets.LNConexion;
using System.Net;
using System.Net.Sockets;
using DNU.ConexionLogSocket.Modelos;

namespace CommonProcesador
{
    public class Logueo
    {
        //private static readonly ILog _loggerError = LogManager.GetLogger("");
        private static readonly log4net.ILog _loggerError = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerErrorProcesador"]);
        private static readonly log4net.ILog _loggerEventos = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerInfoProcesador"]);
        private static readonly log4net.ILog _loggerRecibidos = log4net.LogManager.GetLogger(ConfigurationManager.AppSettings["LogggerDebugProcesador"]);
        
        private static string ippaddress = null;

        static Logueo()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            ippaddress = host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

            XmlConfigurator.Configure();
        }

        public static void Error(String Error)
        {
            _loggerError.Error( Error.Replace("\n", " ").Replace("\r", " ") );

        }

        public static void EntradaSalida(string message, String User, Boolean esEntrada)
        {
            String leyenda = esEntrada ? "ENTRADA <<" : "SALIDA >>";
            _loggerRecibidos.Debug("[ " + User + "]  " + leyenda + message.Replace("\n", " ").Replace("\r", " ").Replace((char)13,(char)' '));
        }
       

        public static void Evento(String Evento)
        {
            _loggerEventos.Info(Evento.Replace("\n", " ").Replace("\r", " "));
        }

        public static void EventoInfo(String Evento)
        {
            _loggerEventos.Info(Evento.Replace("\n", " ").Replace("\r", " "));
        }

        public static void EventoDebug(String Evento)
        {
            var log = LNServerSocket.envioLogAsync(Evento.Replace("\n", " ").Replace("\r", " "), ippaddress, "", "", "PN");
        }
    }
}
