using CommonProcesador;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNAltaTarjetaNominalV2
    {

        private string directorio;
        private string tipoColectiva;
        private const string expRegFecha = @"^\d{2}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegHora = "^([0-1][0-9]|2[0-3])[0-5][0-9]([0-5][0-9])?$";
        private  string directorioSalida;
        private static string connArchivosCacao = PNConfig.Get("ALTAEMPLEADODNU", "BDWriteProcesadorArchivosCacao");
        private static string nomArchivoProcesar;
        string log; string ip;
        LogueoAltaEmpleadoV2 logEmpelado;
        LNDecodificacionArchivos lnDecodificacionArchivos;


        public LNAltaTarjetaNominalV2(string directorio, string tipoColectiva, LogueoAltaEmpleadoV2 logEmpelado,string directorioSalida)
        {
            this.directorio = directorio;
            this.directorioSalida = directorioSalida;
            this.tipoColectiva = tipoColectiva;
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            this.logEmpelado = logEmpelado;
            lnDecodificacionArchivos = new LNDecodificacionArchivos(this.logEmpelado, this.directorio, this.directorioSalida, tipoColectiva);


        }

        public bool validarArchivos(bool validaCopiadoFiles)
        {
            logEmpelado.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO][" + log + "][AltaEmpleado]  [Iniciando proceso de alta]");

            if (validaCopiadoFiles)
            {
                Thread.Sleep(1000 * 60 * 1);
            }
            FileInfo archivo = null;
            List<string> existeArchivo = Directory.GetFiles(directorio, "PERSOCARDS_*.*").ToList();
            if (existeArchivo.Count > 0)
            {
                try
                {
                    foreach (var dato in existeArchivo)
                    {
                        archivo = new FileInfo(dato);
                        nomArchivoProcesar = archivo.FullName;
                        string pathInicial = directorio + "\\EN_PROCESO_" + archivo.Name;
                        LNOperacionesArchivos.moverArchivo(archivo.FullName, pathInicial);
                        lnDecodificacionArchivos.decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName), nomArchivoProcesar);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogueoAltaEmpleado.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarArchivos, error al obtener archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    LNOperacionesArchivos.moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    logEmpelado.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                    return false;
                }
            }
            return false;
        }
        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {//
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            logEmpelado.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("ALTAEMPLEADODNU", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            logEmpelado.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            logEmpelado.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
            validarArchivos(true);
        }

        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            logEmpelado.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, OcurrioError, El evento de fileWatcher no inicio correctamente, Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
        }
    }
}
