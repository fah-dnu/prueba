#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_FechasClientes.LogicaNegocio;
using DNU_FechasClientes.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_FechasClientes
{
    public class AltaFechas : IProcesoNocturno
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
                path = PNConfig.Get("FECHASCLIENTES", "DirectorioEntrada");
                LNAltaFechas _Altafechas = new LNAltaFechas(path);
                _Altafechas.crearDirectorio();
                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Inicia Escucha en Directorio]");
                return _Altafechas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
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
                path = PNConfig.Get("FECHASCLIENTES", "DirectorioEntrada");
                LogueoFechasClientes.Info("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Inicia Escucha en Directorio " + path + "]");
                LNAltaFechas _Altafechas = new LNAltaFechas(path);
                _Altafechas.crearDirectorio();
                FileSystemWatcher watcher = new FileSystemWatcher(_Altafechas.directorio);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_Altafechas.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_Altafechas.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                LogueoFechasClientes.Error("[" + ip + "] [AltaFechas] [PROCESADORNOCTURNO] [" + log + "] [FECHASCLIENTES, Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
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
                path = PNConfig.Get("FECHASCLIENTES", "DirectorioEntrada");
                LNAltaFechas _Altafechas = new LNAltaFechas(path);
                _Altafechas.crearDirectorio();
                LogueoFechasClientes.Info("[" + direccionIP + "] [AltaFechas] [PROCESADORNOCTURNO] [" + idLog + "] [FECHASCLIENTES Inicia Escucha en Directorio]");
                _Altafechas.validarArchivos(false);
            }
            catch (Exception ex)
            {
                LogueoFechasClientes.Error("[" + direccionIP + "] [AltaFechas] [PROCESADORNOCTURNO] [" + idLog + "] [FECHASCLIENTES, Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

            }
        }
    }
}
