using CommonProcesador;
using Dnu.AutorizadorParabilia_NCliente.LogicaNegocio;
using System;
using System.IO;

namespace Dnu.AutorizadorParabilia_NCliente
{
    public class AltaEmpleado : IProcesoNocturno
    {
        public string path;
        public string cnna;
        public string tipoColectiva;

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ALTAEMPLEADO", "DirAltaEmp");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADO", "ClaveTipoColectiva");
                LNAltaEmpleado _AltaEmpleado = new LNAltaEmpleado(path, tipoColectiva);
                _AltaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                return _AltaEmpleado.validarArchivos(false);
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
                path = PNConfig.Get("ALTAEMPLEADO", "DirAltaEmp");
                tipoColectiva = PNConfig.Get("ALTAEMPLEADO", "ClaveTipoColectiva");
                LNAltaEmpleado _AltaEmpleado = new LNAltaEmpleado(path, tipoColectiva);
                _AltaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio " + path);
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(_AltaEmpleado.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(_AltaEmpleado.OcurrioError);
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
                Logueo.Evento("Inicia Escucha en Directorio " + path);
                ConfiguracionContexto.InicializarContexto();
                path = PNConfig.Get("ALTAEMPLEADO", "DirAltaEmp");
                Logueo.Evento("Inicia Escucha en Directorio " + path);
                tipoColectiva = PNConfig.Get("ALTAEMPLEADO", "ClaveTipoColectiva");
                LNAltaEmpleado _AltaEmpleado = new LNAltaEmpleado(path, tipoColectiva);
                _AltaEmpleado.crearDirectorio();
                Logueo.Evento("Inicia Escucha en Directorio ");
                _AltaEmpleado.validarArchivos(false);
            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

    }
}
