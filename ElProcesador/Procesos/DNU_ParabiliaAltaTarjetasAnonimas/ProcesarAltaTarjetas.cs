#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ParabiliaAltaTarjetasAnonimas.Entidades;
using DNU_ParabiliaAltaTarjetasAnonimas.LogicaNegocio;
using DNU_ParabiliaAltaTarjetasAnonimas.Utilidades;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasAnonimas
{
    //
    public class ProcesarAltaTarjetasAnonimas : IProcesoNocturno

    {
        public string path;
        public string cnna;
        public string tipoColectiva;
        string log="";
        string ip="";
        public ProcesarAltaTarjetasAnonimas() {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }

//#if DEBUG
        public bool Iniciar()
        {
            Logueo.Error("inicioo");
           // ThreadContext.Properties["log"] = DateTime.Now.ToString("yyyyMMddHHmmss");
           // ThreadContext.Properties["ip"] = DateTime.Now.ToString("yyyyMMddHHmmss");
           // LogueoPrAltaTarjParabilia.Error("[ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje:");

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
               
#if Azure
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                //string BDAutorizadorRead = ConfigurationManager.AppSettings["BDAutorizadorRead"].ToString();

                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);
                Logueo.EventoDebug(JsonConvert.SerializeObject(respuesta));
                //responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(aplication, cliente, BDAutorizadorRead);

#endif

                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada");
                LNArchivo _AltaTarjeta = new LNArchivo(path, String.Empty);
                _AltaTarjeta.crearDirectorio();
                LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia el renombramiento del Archivo procesado: ]");

                // LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio " + path + "]");
                return _AltaTarjeta.validarArchivos(false);
            }
            catch (Exception ex)
            {
              //  LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }
//#endif

        bool IProcesoNocturno.Procesar()
        {
           Logueo.Evento("inicio alta tarjeta");
           try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada");
                LNArchivo _AltaTarjeta = new LNArchivo(path, String.Empty);
                _AltaTarjeta.crearDirectorio();
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio " + path + "PRALTATARJPARABILIA]");
                return _AltaTarjeta.validarArchivos(false);
            }
            catch (Exception ex)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            
            Logueo.Evento("inicio alta tarjeta");
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada");
                LNArchivo _AltaTarjeta = new LNArchivo(path, String.Empty);
                _AltaTarjeta.crearDirectorio();
                LogueoPrAltaTarjParabilia.Info("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio " + path + "PRALTATARJPARABILIA]");
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_AltaTarjeta.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_AltaTarjeta.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                LogueoPrAltaTarjParabilia.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

    }
}
