#define Azure
using DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio;
using System;
using CommonProcesador;
using System.IO;
using log4net;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Dnu.AutorizadorParabiliaAzure.Services;
using Dnu.AutorizadorParabiliaAzure.Models;
using System.Configuration;

namespace DNU_ParabiliaAltaTarjetasNominales
{
    public class AltaTarjetasNominativas : IProcesoNocturno
    {
        public string path;
        public string cnna;
        public string tipoColectiva;

        bool IProcesoNocturno.Procesar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADOCACAO", "ClaveTipoColectiva");
                LNAltaTarjetaNominal _AltaTarjetas = new LNAltaTarjetaNominal(path, tipoColectiva);
                _AltaTarjetas.crearDirectorio();
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Iniciando Escucha en Directorio]");
                return _AltaTarjetas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADOCACAO", "ClaveTipoColectiva");
                LNAltaTarjetaNominal _AltaTarjetas = new LNAltaTarjetaNominal(path, tipoColectiva);
                _AltaTarjetas.crearDirectorio();
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Iniciando Escucha en Directorio " + path + "]");
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_AltaTarjetas.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_AltaTarjetas.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

        public void Iniciar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            try
            {
              
#if Azure
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);
#endif
                ConfiguracionContexto.InicializarContexto();
#if DEBUG
                path = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada");
#else
                path = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada");
#endif
                LogueoAltaEmpleadoCacao.Info("[" + direccionIP + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + idLog + "] [Inicia Escucha en Directorio " + path + "]");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADOCACAO", "ClaveTipoColectiva");
                LNAltaTarjetaNominal _AltaTarjetas = new LNAltaTarjetaNominal(path, tipoColectiva);
                _AltaTarjetas.crearDirectorio();
                LogueoAltaEmpleadoCacao.Info("[" + direccionIP + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + idLog + "] [Inicia Escucha en Directorio]");
                _AltaTarjetas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + direccionIP + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + idLog + "] [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

    }
}
