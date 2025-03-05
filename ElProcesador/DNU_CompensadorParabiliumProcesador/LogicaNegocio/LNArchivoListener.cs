
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Threading;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumProcesador.LogicaNegocio;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.LogicaNegocio;
using log4net;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using System.Text.RegularExpressions;
using DNU_CompensadorParabiliumConversorArchivo;
using System.Runtime.CompilerServices;

namespace DNU_CompensadorParabiliumProcesador
{
    public class LNArchivoListener
    {
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        public static Boolean enProceso = false;
        private static readonly object balanceLock = new object();
        private static string _NombreNewRelic;
        private static string etiquetaProcesoComp = Processes.PROCESA_COMPENSACION.ToString();
        static Func<string, string> GetConfig = NameConfig => PNConfig.Get(Processes.PROCESA_COMPENSACION.ToString(), NameConfig);
        private static string _PathProcesados = GetConfig("rutaCargaArchivosCompensacion") + "\\Procesados\\";
        private static string _PathErrores = GetConfig("rutaCargaArchivosCompensacion") + "\\Errores\\";
        private static string _PathArchivosOriginales = GetConfig("rutaCargaArchivosCompensacion") + "\\ArchivosOriginales\\";


        static Func<string, string, string, string> GetJsonError = (NameFile, descError, codigoError) => "{ \"responseCode\":\"" + codigoError + "\"" +
                                                                    ", \"responseDate\":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"" +
                                                                    ", \"responseMessage\":\"" + descError + "\"" +
                                                                    ", \"objectName\":\"" + typeof(LNArchivoListener).Name + "\"" +
                                                                    ", \"objectVersion\":\"6\"" +
                                                                    ", \"objectType\":\"Clase\"" +
                                                                    ", \"parameters\":  {" +
                                                                                        "\"@nombreFicheroOriginal\":\"" + NameFile + "\"" +
                                                                                        "} " +
                                                                    "}";



        private static string NombreNewRelicPROCESA_COMPENSACION
        {
            set
            {

            }
            get
            {
                string ClaveProceso = etiquetaProcesoComp;
                _NombreNewRelic = GetConfig("NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                }
                return _NombreNewRelic;
            }
        }

        private static FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get(etiquetaProcesoComp, "rutaCargaArchivosCompensacion"));

        public static void EscucharDirectorio(string etiquetaProcesoCompLog)
        {
            try
            {
                LogueoCompensador.Info(etiquetaProcesoCompLog + " [INICIA la escucha de la carpeta: " + PNConfig.Get(etiquetaProcesoComp, "rutaCargaArchivosCompensacion") + " en espera de archivos]");
                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
                elObservador.Created += alCambiar;
                elObservador.Error += alOcurrirUnError;
                elObservador.EnableRaisingEvents = true;
                LogueoCompensador.Info(etiquetaProcesoCompLog + " [TERMINA la escucha de la carpeta: " + PNConfig.Get(etiquetaProcesoComp, "rutaCargaArchivosCompensacion") + " en espera de archivos]");
            }
            catch (Exception err)
            {
                LogueoCompensador.Error(etiquetaProcesoCompLog + " [EscucharDirectorio(): " + err.Message + "]");
            }
        }

        public static void DetenerDirectorio(string etiquetaprocesoCompLog)
        {
            try
            {
                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
                elObservador.Created -= alCambiar;
                elObservador.Error -= alOcurrirUnError;
                elObservador.EnableRaisingEvents = false;
            }
            catch (Exception ex)
            {
                LogueoCompensador.Error(etiquetaprocesoCompLog + " [DetenerDirectorio(): " + ex.Message + "]");
            }
        }


        [Transaction]
        public static void alCambiar(object source, FileSystemEventArgs el)
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [" + etiquetaProcesoComp + "] ";
            try
            {
                ApmNoticeWrapper.SetAplicationName(NombreNewRelicPROCESA_COMPENSACION);
                ApmNoticeWrapper.SetTransactionName("alCambiar");

                DetenerDirectorio(etiquetaProcesoCompLog);

                try
                {
                    LogueoCompensador.Info(etiquetaProcesoCompLog + " [Esperando " + (Int32.Parse(GetConfig("MinEsperaProceso"))).ToString() + " Minutos. a que se copie el archivo completo y lo libere el proceso de copiado]");
#if !DEBUG
                    Thread.Sleep(1000 * 60 * Int32.Parse(GetConfig( "MinEsperaProceso")));
#endif
                }
                catch (Exception err)
                {
                    LogueoCompensador.Info(etiquetaProcesoCompLog + " [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
                    Thread.Sleep(1000 * 60 * 10);
                    LogueoCompensador.Info(etiquetaProcesoCompLog + " [Esperando 10 Minutos.]");
                    ApmNoticeWrapper.NoticeException(err, idLog);
                }

                RespuestaProceso laRespuestProceso = new RespuestaProceso();

                try
                {
                    lock (balanceLock)
                    {
                        DirectoryInfo directory = new DirectoryInfo(GetConfig("rutaCargaArchivosCompensacion"));
                        FileInfo[] files;
                        files = directory.GetFiles("*.*");

                        LogueoCompensador.Info(etiquetaProcesoCompLog + " [" + String.Format("Archivos configurados {0}", files.Length) + "]");

                        for (int i = 0; i < files.Length; i++)
                        {
                            RespuestaInsertFicheroTemp respFicheroTemp = new RespuestaInsertFicheroTemp();
                            string prefijoError = "ERROR00_";
                            string respSP = string.Empty;
                            string nombreFechaHora = string.Empty;
                            string nombreSinExtencion = string.Empty;
                            string codigoError = "500";
                            FileInfo fileDecrypt = null;
                            try
                            {
                                nombreSinExtencion = files[i].Name.TrimEnd(files[i].Extension.ToCharArray());
                                nombreFechaHora = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{files[i].Name}";

                                if (!File.Exists(_PathArchivosOriginales + nombreFechaHora))
                                    File.Copy(files[i].FullName, _PathArchivosOriginales + nombreFechaHora);

                                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                                {
                                    LogueoCompensador.Info(etiquetaProcesoCompLog + $"[Procesando archivo: {files[i].Name}] [Nombre con Fecha y Hora: {nombreFechaHora}]");

                                    prefijoError = "ERROR01_";
                                    respFicheroTemp = DAOArchivo.GuardaFicheroTemp(files[i].FullName.TrimEnd(files[i].Name.ToCharArray()), nombreSinExtencion, conn, etiquetaProcesoCompLog);

                                    prefijoError = "ERROR02_";
                                    var definicionFicheroTemp = DAOArchivo.ObtieneDefinicionFicheroTemp(respFicheroTemp, conn, etiquetaProcesoCompLog);
                                    bool isJson = false;
                                    switch (respFicheroTemp.extensionFile.ToLower().TrimStart('.'))
                                    {
                                        case "txt":
                                            break;
                                        case "json":
                                            prefijoError = "ERROR04_";
                                            DAOArchivo.LlenarTablaFicheroProvicionalTags(definicionFicheroTemp, respFicheroTemp, files[i], etiquetaProcesoCompLog);
                                            isJson = true;
                                            break;
                                        case "ipm":
                                            prefijoError = "ERROR03_";
                                            procesarArchivosIPM(files[i], nombreFechaHora, respFicheroTemp.extensionFile.ToLower(), etiquetaProcesoCompLog, ref codigoError);
                                            break;
                                        case "pgp":
                                            #region Bloque para desenciptar archivos

                                            string extensionFile = files[i].Extension;
                                            try
                                            {
                                                List<string> lstExtCifrado = ConfigurationManager.AppSettings["extCifrado"].Split(',').ToList();

                                                if (lstExtCifrado.Contains(extensionFile))
                                                {
                                                    LogueoCompensador.Info("Entra proceso de Descifrado");
                                                    string fileTempEncript = files[i].FullName;
                                                    string fileTempDecript = files[i].FullName.Replace(extensionFile, "");
                                                    string nomTempOriginal = files[i].Name.Replace(extensionFile, "");

                                                    string keyFileName = GetConfig("PathFile");


                                                    EncryptionService.DecryptFile(fileTempEncript, keyFileName
                                                        , GetConfig("Phrase")
                                                        , fileTempDecript);

                                                    fileDecrypt = new FileInfo(fileTempDecript);
                                                }
                                            }
                                            catch (Exception exp)
                                            {
                                                throw new Exception("[Ocurrio un problema al desencriptar Archivo " + files[i].Name + "] [" + exp.Message + "] [" + exp.StackTrace + "]");
                                            }

                                            #endregion

                                            break;
                                        default:
                                            throw new Exception($"Extención del archivo ({respFicheroTemp.extensionFile}) no permitida");
                                            break;
                                    }

                                    if (!isJson)
                                    {
                                        prefijoError = "ERROR04_";
                                        DAOArchivo.LlenarTablaFicheroProvicional(definicionFicheroTemp, respFicheroTemp, files[i], etiquetaProcesoCompLog, fileDecrypt);
                                    }

                                    
                                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                                    {
                                        var laRespu = DAOArchivo.bulkInsertarDatosArchivoDetalle(respFicheroTemp, conn, transaccionSQL, BulkTables.FicheroProvisional.ToString(), GetConfig("TimeoutBulk"));

                                        transaccionSQL.Commit();
                                        respFicheroTemp = DAOArchivo.ActualizaEstatusFicheroTemp(respFicheroTemp, "OK", conn, etiquetaProcesoCompLog);
                                        LogueoCompensador.Info(etiquetaProcesoCompLog + " [" + String.Format("Se actualiza estado del fichero a OK con respuesta: {0}, y descripción: {1}",
                                        respFicheroTemp.CodRespuesta, respFicheroTemp.DescRespuesta) + "]");
                                    }

                                    if (conn.State == ConnectionState.Open)
                                        conn.Close();

                                    File.Move(files[i].FullName, _PathProcesados + nombreFechaHora);
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    LogueoCompensador.Error(etiquetaProcesoCompLog + " [alCambiar(): " + ex.Message + ", " + ex.StackTrace + "]");
                                    File.Move(files[i].FullName, _PathErrores + prefijoError + nombreFechaHora);

                                    if (!respFicheroTemp.DescRespuesta.Contains("LogueoSPS.Error"))
                                    {
                                        respFicheroTemp.DescRespuesta = GetJsonError(nombreSinExtencion, ex.Message, codigoError);
                                        respFicheroTemp = DAOArchivo.ActualizaEstatusFicheroTemp(respFicheroTemp, "ERROR", new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo), etiquetaProcesoCompLog);
                                    }
                                }
                                catch (Exception ex2) { }
                            }
                            finally
                            {
                                try
                                {
                                    if (fileDecrypt != null && File.Exists(fileDecrypt.FullName))
                                        File.Delete(fileDecrypt.FullName);

                                    respFicheroTemp = DAOArchivo.RestableceFicheroTemp(respFicheroTemp, new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo), etiquetaProcesoCompLog);
                                    LogueoCompensador.Info(etiquetaProcesoCompLog + $" [Se restablece el fichero con código: {respFicheroTemp.CodRespuesta}, y descripción: {respFicheroTemp.DescRespuesta}");

                                    

                                }
                                catch (Exception ex2) { }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogueoCompensador.Error(etiquetaProcesoCompLog + " [alCambiar(): " + ex.Message + ",  " + ex.ToString() + "]");
                    ApmNoticeWrapper.NoticeException(ex, idLog);
                }
            }
            catch (Exception err)
            {
                LogueoCompensador.Error(etiquetaProcesoCompLog + " [alCambiar(): " + err.Message + ",  " + err.ToString() + "]");
                ApmNoticeWrapper.NoticeException(err, idLog);
            }
            finally
            {
                //poner a escuchar otra vez el file watcher
                EscucharDirectorio(etiquetaProcesoCompLog);
            }
        }



        [Transaction]
        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueoCompensador.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + e.GetException().Message + "]");
                ApmNoticeWrapper.SetAplicationName(NombreNewRelicPROCESA_COMPENSACION);
                ApmNoticeWrapper.SetTransactionName("alOcurrirUnError");
                ApmNoticeWrapper.NoticeException(e.GetException());

            }
            catch (Exception err)
            {
                LogueoCompensador.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + err.Message + "]");
                ApmNoticeWrapper.SetAplicationName(NombreNewRelicPROCESA_COMPENSACION);
                ApmNoticeWrapper.SetTransactionName("alOcurrirUnError");
                ApmNoticeWrapper.NoticeException(err);

            }
        }


        private static void procesarArchivosIPM(FileInfo archivo, string nombreFechaHora, string extensionFile, string etiquetaCompensadorLog, ref string codigoError)
        {
            IPM ipm;
            string archivoEntrada = archivo.FullName, archivoSalida, archivoProcesando, logueoActivo;

            try
            {
                archivoSalida = GetConfig("rutaCargaArchivosCompensacion") + "\\" + archivo.Name;
                archivoProcesando = GetConfig("rutaCargaArchivosCompensacion") + "\\EN_PROCESO_" + nombreFechaHora;

                LogueoCompensador.Info(etiquetaCompensadorLog + $"[procesarArchivosIPM()] [Iniciar el renombramiento del Archivo IPM a TXT procesado: {archivoEntrada}]");

                if (!archivoEntrada.ToLower().EndsWith(extensionFile))
                {
                    throw new Exception("[El nombre del archivo es invalidó: " + archivoEntrada + $", extención: {extensionFile}]");
                }

                if (archivoEntrada.Contains("EN_PROCESO_"))
                {
                    throw new Exception("[El archivo ya fue procesado anteriormente: " + archivoEntrada + "]");
                }
                else if (File.Exists(archivoProcesando))
                {
                    throw new Exception("[El archivo a convertir ya existe: " + archivoProcesando + "]");
                }

                File.Move(archivoEntrada, archivoProcesando);

                LogueoCompensador.Info(etiquetaCompensadorLog + $"[procesarArchivosIPM()] [Inicia proceso de archivo IPM: {archivoProcesando}]");

                logueoActivo = GetConfig("LogueoActivoConversor");
                ipm = new IPM(logueoActivo);
                try
                {
                    bool isEBCDIC1014 = GetConfig("isEBCDIC1014") == "1";
                    int NumOfCharacters = Convert.ToInt32(GetConfig("NumOfCharacters"));
                    int NumPositions = Convert.ToInt32(GetConfig("NumPositions"));
                    string patternIpmCodes = GetConfig("patternIpmCodes");
                    int result = -100;

                    result = ipm.LeerIPM(archivoProcesando, archivoSalida, logueoActivo, isEBCDIC1014, NumOfCharacters, NumPositions, patternIpmCodes);

                    LogueoCompensador.Info(etiquetaCompensadorLog + $"[procesarArchivosIPM()] [Finaliza lectura IPM a TXT con código: {result}]");

                    if (result == -99 || result == -100)
                    {
                        File.Delete(archivoProcesando);
                        codigoError = "404";
                        throw new Exception($"[Procesar IPM a TXT, el archivo {archivo.Name} no contiene registros para procesar]");
                    }
                    else
                    {
                        if (new FileInfo(archivoSalida).Exists && new FileInfo(archivoSalida).Length > 0)
                        {
                            File.Delete(archivoProcesando);
                            LogueoCompensador.Info(etiquetaCompensadorLog + "[procesarArchivosIPM()] [Finaliza proceso IPM a TXT con éxito");
                            return;
                        }
                        else
                        {
                            codigoError = "404";
                            File.Delete(archivoProcesando);
                            throw new Exception($"[Procesar IPM a TXT, el archivo {archivo.Name} se proceso con errores]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (File.Exists(archivoProcesando))
                        File.Delete(archivoProcesando);
                    throw new Exception(ex.Message);
                }
            }
            catch (Exception err)
            {
                throw new Exception("[procesarArchivosIPM()] " + err.Message);
            }
        }

        private static DateTime GetDateFromJulian(double julianDate)
        {
            DateTime dinicio = new DateTime(1840, 12, 31);
            dinicio = dinicio.AddDays(julianDate);
            return dinicio;
        }

        /// <summary>
        /// Devuelve el nombre del metodo que llamo a este metodo.
        /// Ejemplo el metodo consultaDato llama a este metodo el resultado es: consultaDato
        /// </summary>
        /// <returns>Nombre del metodo que lo invocó</returns>
        private static string NombreMetodo([CallerMemberName] string metodo = null)
        {
            return metodo;
        }
    }
}
