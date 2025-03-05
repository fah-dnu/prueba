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
    public class AsignacionSaldos : IProcesoNocturno
    {
        public string path;
        public string cnna;
        public string tipoColectiva;

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ASIGNACIONSALDOS", "DirAsigSaldos");
                tipoColectiva = PNConfig.Get("ASIGNACIONSALDOS", "ClaveTipoColectiva");
                LNAsignacionSaldos _AsignacionSaldos = new LNAsignacionSaldos(path, tipoColectiva);
                _AsignacionSaldos.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio  ");
                return _AsignacionSaldos.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar Archivo de Asignacion de Saldos] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ASIGNACIONSALDOS", "DirAsigSaldos");
                tipoColectiva = PNConfig.Get("ASIGNACIONSALDOS", "ClaveTipoColectiva");
                LNAsignacionSaldos _AsignacionSaldos = new LNAsignacionSaldos(path, tipoColectiva);
                _AsignacionSaldos.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_AsignacionSaldos.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_AsignacionSaldos.OcurrioError);
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
                path = PNConfig.Get("ASIGNACIONSALDOS", "DirAsigSaldos");
                tipoColectiva = PNConfig.Get("ASIGNACIONSALDOS", "ClaveTipoColectiva");
                LNAsignacionSaldos _AsignacionSaldos = new LNAsignacionSaldos(path, tipoColectiva);
                _AsignacionSaldos.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                _AsignacionSaldos.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }
    }
}
