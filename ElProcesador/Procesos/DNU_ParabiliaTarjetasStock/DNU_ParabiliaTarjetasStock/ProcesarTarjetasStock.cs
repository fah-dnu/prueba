using CommonProcesador;
using DNU_ParabiliaTarjetasStock.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DNU_ParabiliaTarjetasStock
{
    public class ProcesarTarjetasStock :IProcesoNocturno
    {

        private const string METODO = "Iniciar";
        private const string CLASE = "ProcesarTarjetasStock";
        
        private string m_path;



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
                    m_path = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada");

                LNArchivoStocks tarjetas = new LNArchivoStocks(m_path);
                tarjetas.crearDirectorio();
                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Inicia Proceso.. Validacion de archivos en el directorio", CLASE, METODO));
                Logueo.Evento("Inicia Escucha en Directorio " + m_path);
                return tarjetas.validarArchivos(false);

            }
            catch (Exception ex)
            {
                Logueo.Error("[PRPROCESARTARSTOCK] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }
#endif


        void IProcesoNocturno.Detener()
        {
        }
        void IProcesoNocturno.Iniciar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                m_path = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada");

                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Directorio Obtenio" + m_path, CLASE, METODO));
                if (!string.IsNullOrEmpty(m_path))
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()   Obteniendo ruta Origen  PROCESO STOCKCARDS", CLASE, METODO));


                    LNArchivoStocks tarjetas = new LNArchivoStocks(m_path);
                    tarjetas.crearDirectorio();

                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()   Inicia el  proceso se escucha directorio: ", CLASE, METODO));

                    FileSystemWatcher watcher = new FileSystemWatcher(m_path);
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                    //// Add event handlers.
                    watcher.Created += new FileSystemEventHandler(tarjetas.NuevoArchivo);
                    watcher.Error += new ErrorEventHandler(tarjetas.OcurrioError);
                    watcher.EnableRaisingEvents = true;

                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  No se pudo  Obtener la ruta Origen ", CLASE, METODO));

                }
            }
            catch (Exception ex)
            {
                Logueo.Error(string.Format("[PRPROCESARTARSTOCK]{0}.{1}()  [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]", CLASE, METODO));

            }
        }

        bool IProcesoNocturno.Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                
                {
                    try
                    {
                        ConfiguracionContexto.InicializarContexto();
                        m_path = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada");
                        LNArchivoStocks tarjetas1 = new LNArchivoStocks(m_path);
                        tarjetas1.crearDirectorio();
                        Logueo.Evento("Inicia Escucha en Directorio " + m_path + "PRALTATARJPARABILIA");
                        return tarjetas1.validarArchivos(false);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logueo.Error("[Iniciar] [Error al Iniciar Procesar de Archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }




    }
}
