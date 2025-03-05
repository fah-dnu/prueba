using CommonProcesador;
using Dnu.AutorizadorParabilia_NCliente.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente
{
    public class BajaEmpleado : IProcesoNocturno
    {
        public string path;
        public string cnna;
        public string tipoColectiva;

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("BAJAEMPLEADO", "DirBajaEmp");
                tipoColectiva = PNConfig.Get("BAJAEMPLEADO", "ClaveTipoColectiva");
                LNBajaEmpleado _BajaEmpleado = new LNBajaEmpleado(path, tipoColectiva);
                _BajaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                return _BajaEmpleado.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("BAJAEMPLEADO", "DirBajaEmp");
                tipoColectiva = PNConfig.Get("BAJAEMPLEADO", "ClaveTipoColectiva");
                LNBajaEmpleado _BajaEmpleado = new LNBajaEmpleado(path, tipoColectiva);
                _BajaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio " + path);
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_BajaEmpleado.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_BajaEmpleado.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        void IProcesoNocturno.Detener()
        {
        }

        public void Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("BAJAEMPLEADO", "DirBajaEmp");
                tipoColectiva = PNConfig.Get("BAJAEMPLEADO", "ClaveTipoColectiva");
                LNBajaEmpleado _BajaEmpleado = new LNBajaEmpleado(path, tipoColectiva);
                _BajaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                _BajaEmpleado.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }
    }
}
