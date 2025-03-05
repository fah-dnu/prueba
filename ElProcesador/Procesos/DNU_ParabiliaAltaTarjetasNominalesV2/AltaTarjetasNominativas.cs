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
          
            Guid idLog = Guid.NewGuid();
            String app = ConfigurationManager.AppSettings["ApplicationId"].ToString();
             try
            {
         
                ConfiguracionContexto.InicializarContexto();
                LogueoAltaEmpleadoV2 logEmpelado = new LogueoAltaEmpleadoV2(idLog.ToString(), app, "");

                path = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioEntrada");
                string directorioSalida = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioSalida");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADODNU", "ClaveTipoColectiva");
                LNAltaTarjetaNominalV2 _AltaTarjetas = new LNAltaTarjetaNominalV2(path, tipoColectiva, logEmpelado, directorioSalida);
                LNOperacionesArchivos.crearDirectorio(path, directorioSalida);
                logEmpelado.Info("[TarjetasNominativas] [PROCESADORNOCTURNO] [Iniciando Escucha en Directorio]");
                return _AltaTarjetas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[TarjetasNominativas] [PROCESADORNOCTURNO] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            Guid idLog = Guid.NewGuid();
            String app = ConfigurationManager.AppSettings["ApplicationId"].ToString();
            try
            {
            

                ConfiguracionContexto.InicializarContexto();
                LogueoAltaEmpleadoV2 logEmpelado = new LogueoAltaEmpleadoV2(idLog.ToString(), app, "");

                path = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioEntrada");
                string directorioSalida = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioSalida");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADODNU", "ClaveTipoColectiva");
                LNAltaTarjetaNominalV2 _AltaTarjetas = new LNAltaTarjetaNominalV2(path, tipoColectiva, logEmpelado, directorioSalida);
                LNOperacionesArchivos.crearDirectorio(path, directorioSalida);
                logEmpelado.Info("[TarjetasNominativas] [PROCESADORNOCTURNO][Iniciando Escucha en Directorio " + path + "]");
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_AltaTarjetas.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_AltaTarjetas.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[TarjetasNominativas] [PROCESADORNOCTURNO]  [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

        public void Iniciar()
        {
            Guid idLog = Guid.NewGuid();
            String app = ConfigurationManager.AppSettings["ApplicationId"].ToString();
         
            try
            {
              
#if Azure
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);
#endif
                ConfiguracionContexto.InicializarContexto();
                LogueoAltaEmpleadoV2 logEmpelado = new LogueoAltaEmpleadoV2(idLog.ToString(), app, "");
                path = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioEntrada");
                string directorioSalida = PNConfig.Get("ALTAEMPLEADODNU", "DirectorioSalida");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADODNU", "ClaveTipoColectiva");
                LNAltaTarjetaNominalV2 _AltaTarjetas = new LNAltaTarjetaNominalV2(path, tipoColectiva, logEmpelado, directorioSalida);
                LNOperacionesArchivos.crearDirectorio(path, directorioSalida);
                logEmpelado.Info("[TarjetasNominativas] [PROCESADORNOCTURNO] [" + idLog + "] [Inicia Escucha en Directorio " + path + "]");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADODNU", "ClaveTipoColectiva");
                _AltaTarjetas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error(" [TarjetasNominativas] [PROCESADORNOCTURNO]  [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

    }
}
