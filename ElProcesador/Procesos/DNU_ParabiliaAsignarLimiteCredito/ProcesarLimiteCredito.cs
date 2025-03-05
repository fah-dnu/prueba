#define Azure
using CommonProcesador;
using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ParabiliaAsignarLimiteCredito.LogicaNegocio;
using DNU_ParabiliaAsignarLimiteCredito.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_ParabiliaAsignarLimiteCredito
{
    public class ProcesarLimiteCredito : IProcesoNocturno

    {
        public string path;
        public string cnna;
        public string tipoColectiva;

        string log = "";
        string ip = "";
        public ProcesarLimiteCredito()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }

        public bool Iniciar()
        {
            try
            {
                //    ConfiguracionContexto.InicializarContexto();
                //    path = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada");
                //    LNArchivo _lnArchivoLimiteCred = new LNArchivo(path, String.Empty);
                //    _lnArchivoLimiteCred.crearDirectorio();

                //    _lnArchivoLimiteCred.EscucharDirectorio(log, ip);
                string direccionIP = System.Net.Dns.GetHostName();
                string idLog = Guid.NewGuid().ToString();
                string aplication = ConfigurationManager.AppSettings["applicationId"].ToString();
                string cliente = ConfigurationManager.AppSettings["clientKey"].ToString();
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(aplication, cliente);

                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada");
                LNArchivo _AsignaLC = new LNArchivo(path);
                _AsignaLC.crearDirectorio();
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio Asigna Limite" + path + "]");
                return _AsignaLC.validarArchivos(false, direccionIP, idLog);
            }
            catch (Exception ex)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Iniciar, Error al Iniciar Procesar de Archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }


        bool IProcesoNocturno.Procesar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            string idLog = Guid.NewGuid().ToString();
            ConfiguracionContexto.InicializarContexto();
            try
            {

                path = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada");
                LNArchivo _AsignaLC = new LNArchivo(path);
                _AsignaLC.crearDirectorio();
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio ASIGNALIMITECREDITO: " + path);
                return _AsignaLC.validarArchivos(false, direccionIP, idLog);
            }
            catch (Exception ex)
            {
                Logueo.Error(" [LimiteCredito] [PROCESADORNOCTURNO] [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            string idLog = Guid.NewGuid().ToString();
            ConfiguracionContexto.InicializarContexto();
            try
            {
                path = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada");
                LNArchivo _lnArchivoLimiteCred = new LNArchivo(path);
                _lnArchivoLimiteCred.crearDirectorio();

                _lnArchivoLimiteCred.EscucharDirectorio(direccionIP, idLog);
            }
            catch (Exception ex)
            {
                Logueo.Error("[LimiteCredito] [PROCESADORNOCTURNO]  [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

    }
}
