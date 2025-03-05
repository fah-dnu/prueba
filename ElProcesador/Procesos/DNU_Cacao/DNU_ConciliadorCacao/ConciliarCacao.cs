#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ConciliadorCacao.BaseDatos;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.LogicaNegocio;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_ConciliadorCacao
{

    public class ConciliarCacao : IProcesoNocturno
    { 
        public bool Procesar()
        {

            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            ConfiguracionContexto.InicializarContexto();
#if DEBUG
            var path = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var path = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif
            LNArchivo _conciliacion = new LNArchivo(path, string.Empty);
            _conciliacion.crearDirectorio();
            return _conciliacion.validarArchivos(false);

        }

        void IProcesoNocturno.Iniciar()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                ConfiguracionContexto.InicializarContexto();
#if DEBUG
                var path = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var path = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif

                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Inicia la escucha de la carpeta: " + PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada") + " en espera de archivos. PROCONCILIACACAO]");
                LNArchivo _conciliacion = new LNArchivo(path, string.Empty);
                _conciliacion.crearDirectorio();
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.

                 LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Escuchando la carpeta: " + PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada") + " en espera de archivos. PROCONCILIACACAO]");



                watcher.Created += new FileSystemEventHandler(_conciliacion.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_conciliacion.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
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
                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(aplication, cliente, "CACAO-DESA-PN-PN-W");

#endif
                ConfiguracionContexto.InicializarContexto();
#if DEBUG
                var path = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var path = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif
                LNArchivo _conciliacion = new LNArchivo(path, string.Empty);
                _conciliacion.crearDirectorio();
                _conciliacion.validarArchivos(false);
                
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + direccionIP + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + idLog + "] [Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }
    }
}
