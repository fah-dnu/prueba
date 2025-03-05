
// -----------------------------------------------------------------------
// <copyright file="ProcesarTarjetasPCI.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasPCI
{
    using CommonProcesador;
    using System;
    using LogicaNegocio;
    using System.IO;

    public class ProcesarTarjetasPCI : IProcesoNocturno
    {

        private  string m_path;
        

        private const string METODO = "Iniciar";
        private const  string CLASE = "ProcesarTarjetasPCI";

#if DEBUG

        /// <summary>
        /// Metodo que emula al warch de archivos para poder 
        /// </summary>
        /// <returns></returns>
        public bool Iniciar()
        {
            const string METODO = "Iniciar";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                m_path = PNConfig.Get("PRPROCESARTARPCI", "DirectorioEntrada");
               
                LNArchivoPCI  tarjetas = new LNArchivoPCI(m_path);
                tarjetas.crearDirectorio(m_path);
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicia Proceso.. Validacion de archivos en el directorio", CLASE, METODO));
                Logueo.Evento("Inicia Escucha en Directorio " + m_path);
                return tarjetas.validarArchivos(false);

            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }
#endif

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                m_path = PNConfig.Get("PRPROCESARTARPCI", "DirectorioEntrada");
                LNArchivoPCI tarjetas = new LNArchivoPCI(m_path);
                tarjetas.crearDirectorio(m_path);
              Logueo.Evento("Inicia Escucha en Directorio " + m_path + "PRALTATARJPARABILIA");
                return tarjetas.validarArchivos(false);
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
                m_path = PNConfig.Get("PRPROCESARTARPCI", "DirectorioEntrada");

                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Directorio Obtenio"+m_path, CLASE, METODO));
                if (!string.IsNullOrEmpty(m_path))
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   Obteniendo ruta Origen  PROCESO PCI", CLASE, METODO));


                    LNArchivoPCI tarjetas = new LNArchivoPCI(m_path);
                    tarjetas.crearDirectorio(m_path);

                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   Inicia el  proceso se escucha directorio: ", CLASE, METODO));

                    FileSystemWatcher watcher = new FileSystemWatcher(m_path);
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                    //// Add event handlers.
                    watcher.Created += new FileSystemEventHandler(tarjetas.NuevoArchivo);
                    watcher.Error += new ErrorEventHandler(tarjetas.OcurrioError);
                    watcher.EnableRaisingEvents = true;

                }
                else 
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  No se pudo  Obtener la ruta Origen ", CLASE, METODO));

                }
            }
            catch (Exception ex)
            {
                Logueo.Error(string.Format("{0}.{1}()  [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", CLASE, METODO));
          
            }
        }

        void IProcesoNocturno.Detener()
        {
        }


    }
}
