using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNTrafalgar;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNCargaArchivo
    {

        private string directorioSalida;
        private string directorioEntrada;
        private string tipoColectiva;
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        string log; string ip;
        private static DataTable dtContenidoFile;
        private static DataTable dtContenidoFileProducto;
        private static string nomArchivoProcesar;
        ProcesadorCargaArchivos procesadorArchivos;
        string pathInicial;

        public LNCargaArchivo(string directorioEntrada, string tipoColectiva, string directorioSalida)//, string log, string ip)
        {
            this.directorioEntrada = directorioEntrada;
            this.directorioSalida = directorioSalida;
            this.tipoColectiva = tipoColectiva;
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            procesadorArchivos = new ProcesadorCargaArchivos();
        }

        public LNCargaArchivo() {
            procesadorArchivos = new ProcesadorCargaArchivos();
        }

        internal void NuevoArchivo(object sender, FileSystemEventArgs e)
        {

            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Hubo un Cambio " + elTipoCambio.ToString() + " en el directorio: " + PNConfig.Get("PROCESAEDOCUENTA", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
            leerArchivos(true, log);
        }

        [Transaction]
        internal bool leerArchivos(bool validaCopiadoFiles, string log)
        {
            Logueo.Evento("[GeneraEstadoCuentaDebito] leyendo archivo watcher");

            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            ApmNoticeWrapper.SetAplicationName("ProcesaEstadoDeCuenta");
            ApmNoticeWrapper.SetTransactionName("leerArchivos");
            Logueo.EventoDebug("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Validando archivo]");

            try
            {
                if (validaCopiadoFiles)
                {
                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Iniciar espera de copiado de Archivo]");
                    Thread.Sleep(1000 * 60 * 1);
                    Logueo.Evento(" [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Termina espera de copiado de Archivo ]");
                }

                int resp;
                FileInfo archivo = null;
                resp = 0;
                try
                {
                    List<string> existeArchivo = Directory.GetFiles(directorioEntrada, "EDOCTA*.*").ToList();
                    //ene stew caso se pegan tres archivos, sin embargo a pesar de que se ejcutara el watcher
                    //3 veces solo entrara aqui la primera pues aqui se procsaran los tres
                    //las otras dos veces que entre el watcher entonces ya noi havra archivos que procesar
                    if (existeArchivo.Count > 0)
                    {
                        try
                        {
                            foreach (var dato in existeArchivo)
                            {
                                archivo = new FileInfo(dato);
                                nomArchivoProcesar = archivo.FullName;
                                if (validarExtension(archivo.FullName, ".csv"))
                                {
                                    pathInicial = directorioEntrada + "EN_PROCESO_" + archivo.Name;
                                    FuncionesArchivos.moverArchivo(archivo.FullName, pathInicial);
                                    archivo = new FileInfo(pathInicial);
                                    procesadorArchivos.decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName), directorioSalida);
                                }
                                else
                                {
                                    Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [validarArchivos] [error en la extension]");
                                    string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                                    FuncionesArchivos.moverArchivo(archivo.FullName, pathArchivosInvalidos);
                                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                                    //  return false;
                                }
                            }
                            //return true;
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                            string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                            FuncionesArchivos.moverArchivo(archivo.FullName, pathArchivosInvalidos);
                            Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                            ApmNoticeWrapper.NoticeException(ex);
                            return false;
                        }

                        //procesando archivos cargados
                        Logueo.Evento("[GeneraEstadoCuentaDebito] procesando archivos watcher");

                        try
                        {
                            LNCorteTrafalgarArchExternos _lnIniciocorte = new LNCorteTrafalgarArchExternos(null);
                            return _lnIniciocorte.inicioSinConexionSFTP();
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivosPA] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                        }
                    }

                    return false;

                }

                catch (Exception ex)
                {
                    resp = 1;
                    Console.WriteLine(ex.ToString());
                    Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Erorr " + ex.Message + " , " + ex.StackTrace + "]");
                    ApmNoticeWrapper.NoticeException(ex);
                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }
        }

        bool validarExtension(String dirArchivo, string extensionArchivo)
        {
            string extension = Path.GetExtension(dirArchivo);
            return extensionArchivo.Equals(extension);
        }

        public void crearDirectorio()
        {//

            if (!Directory.Exists(directorioEntrada))
                Directory.CreateDirectory(directorioEntrada);
            if (!Directory.Exists(directorioEntrada + "\\Procesados"))
                Directory.CreateDirectory(directorioEntrada + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorioSalida + "\\Erroneos"))
                Directory.CreateDirectory(directorioSalida + "\\Erroneos");
            if (!Directory.Exists(directorioSalida + "\\Correctos"))
                Directory.CreateDirectory(directorioSalida + "\\Correctos");
        }

        public static void crearDirectorio(string directorioEntrada,string directorioSalida)
        {//

            if (!Directory.Exists(directorioEntrada))
                Directory.CreateDirectory(directorioEntrada);
            if (!Directory.Exists(directorioEntrada + "\\Procesados"))
                Directory.CreateDirectory(directorioEntrada + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorioSalida + "\\Erroneos"))
                Directory.CreateDirectory(directorioSalida + "\\Erroneos");
            if (!Directory.Exists(directorioSalida + "\\Correctos"))
                Directory.CreateDirectory(directorioSalida + "\\Correctos");
        }


        [Transaction]
        internal void OcurrioError(object sender, ErrorEventArgs e)
        {
            ApmNoticeWrapper.SetAplicationName("ProcesaEstadoDeCuenta");
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
            throw new NotImplementedException();
        }

        internal bool leerArchivosManual(bool validaCopiadoFiles, string log, List<String> listaDirectoriosDia, string directorioEntradaSalida,Archivo archivoLog,string directorioEntradaSalidaFecha)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            ApmNoticeWrapper.SetAplicationName("ProcesaEstadoDeCuenta");
            ApmNoticeWrapper.SetTransactionName("leerArchivos");
            Logueo.EventoDebug("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Validando archivo]");
            //List<String> listaArchivosProcesados = new List<string>();
            try
            {
                if (validaCopiadoFiles)
                {
                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Iniciar espera de copiado de Archivo]");
                    Thread.Sleep(1000 * 60 * 1);
                    Logueo.Evento(" [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Termina espera de copiado de Archivo ]");
                }

                int resp;
                FileInfo archivo = null;
                resp = 0;
                foreach (string ruta in listaDirectoriosDia)
                {
                    try
                    {
                        Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Buscando archivos:"+ ruta + "]");

                        List<string> listaArchivos = Directory.GetFiles(directorioEntradaSalidaFecha + ruta, "EDOCTA*.*").ToList();
                        archivoLog.Ruta = archivoLog.Ruta + ruta;
                        if (listaArchivos.Count > 0)
                        {
                            try
                            {

                                foreach (var dato in listaArchivos)
                                {
                                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [validando archivo:" + dato + "]");

                                    archivo = new FileInfo(dato);
                                    archivoLog.Nombre = archivo.Name;//este archivo log solo es para generar el archivo de reporte
                                    archivoLog.Extension = archivo.Extension;
                                    nomArchivoProcesar = archivo.FullName;
                                    if (validarExtension(archivo.FullName, ".csv"))
                                    {
                                        string nombreEnSFTP = ruta + "\\" + archivo.Name;
                                        pathInicial = directorioEntradaSalidaFecha + ruta + "\\" + "EN_PROCESO_" + archivo.Name;
                                        FuncionesArchivos.moverArchivo(archivo.FullName, pathInicial);
                                        archivo = new FileInfo(pathInicial);
                                        Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [procesando archivo:" + archivo.Name + "]");

                                        procesadorArchivos.decodificarArchivoManual(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName), directorioEntradaSalida, ruta, archivoLog);
                                    }
                                    else
                                    {
                                        archivoLog.Procesado = false;
                                        Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [validarArchivos] [error en la extension]");
                                        string pathArchivosInvalidos = directorioEntradaSalida+ "\\Erroneos\\" + ruta + "\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                                        FuncionesArchivos.moverArchivo(archivo.FullName, pathArchivosInvalidos);
                                        Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar con error");
                                        // return false;
                                        resp = 1;
                                    }
                                }
                                // return true;
                            }
                            catch (Exception ex)
                            {
                                archivoLog.Procesado = false;
                                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                                string pathArchivosInvalidos = directorioEntradaSalida+ "\\Erroneos\\" + ruta + "\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                                FuncionesArchivos.moverArchivo(archivo.FullName, pathArchivosInvalidos);
                                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta con errores");
                                ApmNoticeWrapper.NoticeException(ex);
                                resp = 1;
                                //  return false;
                            }
                        }

                      
                    }

                    catch (Exception ex)
                    {
                        archivoLog.Procesado = false;
                        resp = 1;
                        Console.WriteLine(ex.ToString());
                        Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Erorr " + ex.Message + " , " + ex.StackTrace + "]");
                        ApmNoticeWrapper.NoticeException(ex);
                    }
                }

                if (resp == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }
        }




    }
}
