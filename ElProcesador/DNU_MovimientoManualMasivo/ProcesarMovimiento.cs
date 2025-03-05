using CommonProcesador;
using DNU_MovimientoManualMasivo.LogicaNegocio;
using DNU_MovimientoManualMasivo.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_MovimientoManualMasivo
{
    public class ProcesarMovimiento : IProcesoNocturno
    {
        public string path;
        string log = "";
        string ip = "";

        public ProcesarMovimiento() 
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }


        public void Detener()
        {
            throw new NotImplementedException();
        }

        public bool Iniciar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            string idLog = Guid.NewGuid().ToString();
            ConfiguracionContexto.InicializarContexto();
            try
            {

                path = PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada");
                LNArchivo _AsignaLC = new LNArchivo(path);
                _AsignaLC.crearDirectorio();
                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio MOVIMIENTO_MASIVO: " + path);
                return _AsignaLC.validarArchivos(false, direccionIP, idLog);
            }
            catch (Exception ex)
            {
                Logueo.Error(" [MovimientoMasivo] [PROCESADORNOCTURNO] [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        public bool Procesar()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            string idLog = Guid.NewGuid().ToString();
            ConfiguracionContexto.InicializarContexto();
            try
            {

                path = PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada");
                LNArchivo _AsignaLC = new LNArchivo(path);
                _AsignaLC.crearDirectorio();
                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio MOVIMIENTO_MASIVO: " + path);
                return _AsignaLC.validarArchivos(false, direccionIP, idLog);
            }
            catch (Exception ex)
            {
                Logueo.Error(" [MovimientoMasivo] [PROCESADORNOCTURNO] [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
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
                path = PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada");
                LNArchivo _lnArchivoLimiteCred = new LNArchivo(path);
                _lnArchivoLimiteCred.crearDirectorio();

                _lnArchivoLimiteCred.EscucharDirectorio(direccionIP, idLog);
            }
            catch (Exception ex)
            {
                LogueoMovimientoMasivo.Error("[MovimientoMasivo] [PROCESADORNOCTURNO]  [Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }
    }
}
