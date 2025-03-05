using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;

using System.Threading;
using DNU_ParabiliaAsignarLimiteCredito.BaseDatos;
using DNU_ParabiliaAsignarLimiteCredito.Entidades;
using Interfases.Entidades;
using DNU_ParabiliaAsignarLimiteCredito.Utilidades;
using log4net;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;
using ExcelDataReader;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing;

namespace DNU_ParabiliaAsignarLimiteCredito.LogicaNegocio
{
    public class LNArchivo
    {
        private static string directorioSalida;
        private string directorio;
        private static readonly object balanceLock = new object();

        private static string stringRegexImporte = @"^[0-9]{1,3}([\\,][0-9]{3})*[\\.][0-9]{2}$";
        private static string stringRegexTarjeta = @"^[0-9]{16}$";
        private static string stringRegexFechaHora = @"^(19|[2-9][0-9])\d\d(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])(((([0-1][0-9])|(2[0-3]))[0-5][0-9][0-5][0-9]+)).xlsx$";
        private static string stringRegexNumerico = @"^[0-9]+$";

        //ASIGNALIMITECREDITO
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "ASIGNALIMITECREDITO";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        private static FileSystemWatcher elObservadorALC = new FileSystemWatcher(PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada"));


        public LNArchivo(string directorio)
        {
            this.directorio = directorio;
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
        }

        public void EscucharDirectorio(string ip, string log)
        {
            try
            {
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio ASIGNALIMITECREDITO WATCHER: " + directorio);
                elObservadorALC.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
                elObservadorALC.Created += NuevoArchivo;
                elObservadorALC.Error += alOcurrirUnError;
                elObservadorALC.EnableRaisingEvents = true;

                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Termina Escucha en Directorio ASIGNALIMITECREDITO WATCHER: ");

            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Escuchar directorios: " + err.Message);
            }
        }

        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + e.GetException().Message + "]");
            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + err.Message + "]");
            }
        }


        public static Boolean ProcesaArch(String path, String claveColectiva, string ip, string log, string pathOriginal)
        {
            List<DLimiteCred> lstLimiteCredito = new List<DLimiteCred>();
            decimal totImporte = 0;
            try
            {
                ObtieneRegistrosDeArchivo(path, ref lstLimiteCredito, ip, log, ref totImporte);

                var ejecucionCorrecta = ejecutaAsignacionLimite(ref lstLimiteCredito, claveColectiva, log, ip, totImporte);

                renombraArchivo(path, ejecucionCorrecta, lstLimiteCredito, ip, log, pathOriginal);

                return true;


            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[AltaTarjetasAnonimas]ProcesaArch: {0} --- Stack : {1} ", err.Message, err.StackTrace) + " ]");
                renombraArchivoPorError(path, lstLimiteCredito, err.Message, ip, log, pathOriginal);
                return false;
            }
        }

        private static void renombraArchivoPorError(string path, List<DLimiteCred> lstArchivo,
            string message, string ip, string log, string pathOriginal)
        {
            foreach (var item in lstArchivo)
            {
                item.Resultado = message;
            }

            renombraArchivo(path, false, lstArchivo, ip, log, pathOriginal);
        }

        [Transaction]
        internal void OcurrioError(object sender, ErrorEventArgs e)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
            throw new NotImplementedException();
        }


        public void crearDirectorio()
        {//
            directorioSalida = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioSalida");
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
            if (!Directory.Exists(directorio + "\\Salida"))
                Directory.CreateDirectory(directorio + "\\Salida");
        }


        internal void NuevoArchivo(object sender, FileSystemEventArgs e)
        {
            string direccionIP = System.Net.Dns.GetHostName();
            string idLog = Guid.NewGuid().ToString();
            try
            {

                DetenerDirectorio();
                lock (balanceLock)

                {
                    //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                    LogueoAsignaLimiteCredito.Info("[LimiteCredito] [PROCESADORNOCTURNO] [Hubo un Cambio en el directorio: " + PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada") + " se recibio el archivo : " + e.FullPath + "]");
                    LogueoAsignaLimiteCredito.Info("[LimiteCredito] [PROCESADORNOCTURNO] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");

                    validarArchivos(true, direccionIP, idLog);
                }
            }
            catch (Exception ex)
            {
                LogueoAsignaLimiteCredito.Error("[LimiteCredito] [PROCESADORNOCTURNO] [NuevoArchivo(): " + ex.Message + ",  " + ex.ToString() + "]");
            }
            finally
            {
                //poner a escuchar otra vez el file watcher
                EscucharDirectorio(direccionIP, idLog);
            }
        }

        internal void DetenerDirectorio()
        {
            LogueoAsignaLimiteCredito.Info("[ParabiliumProcesador] [PROCESADORNOCTURNO] [Se Detiene a escucha del Directorio: " + PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada"));

            elObservadorALC.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
            elObservadorALC.Created -= NuevoArchivo;
            elObservadorALC.Error -= alOcurrirUnError;
            elObservadorALC.EnableRaisingEvents = false;


        }

        private static void renombraArchivo(string elpathFile, bool ejecucionCorrecta, List<DLimiteCred> lstRegistros
                    , string ip, string log, string pathOriginal)
        {
            LogueoAsignaLimiteCredito.Info("[" + ip + "][LimiteCredito] [PROCESADORNOCTURNO][" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
            string filename = System.IO.Path.GetFileName(elpathFile);

            string finalFileName = directorioSalida + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");
            if (File.Exists(finalFileName))
            {
                finalFileName = finalFileName.Replace("\\PROCESADO_", "\\PROCESADO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_");
            }

            File.Move(elpathFile, finalFileName);


            actualizaRegistrosArchivo(lstRegistros, finalFileName, log, ip, pathOriginal);
        }

        [Transaction]
        internal bool validarArchivos(bool validaCopiadoFiles, string ip, string log)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarArchivos");
            try
            {
                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Inicia proceso limite de credito]");

                if (validaCopiadoFiles)
                {
                    LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Iniciar espera de copiado de Archivo ASIGNALIMITECREDITO]");
                    Thread.Sleep(Convert.ToInt32(PNConfig.Get("ASIGNALIMITECREDITO", "MiliEsperaProceso")));
                }

                int resp;

                resp = 0;

                try
                {
                    ConfiguracionContexto.InicializarContexto();

                    var directorio = PNConfig.Get("ASIGNALIMITECREDITO", "DirectorioEntrada");
                    
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    FileInfo[] files = directory.GetFiles(PNConfig.Get("ASIGNALIMITECREDITO", "TokenArchivos") + "*.xlsx");

                    if (files.Length == 0)
                    {
                        LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [NO HAY ARCHIVO QUE PROCESAR: ASIGNALIMITECREDITO]");
                    }

                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            String elpathFile = (((FileInfo)files[i]).FullName);

                            //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                            LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Inicia el renombramiento del Archivo procesado: " + elpathFile + "]");
                            string filename1 = System.IO.Path.GetFileName(elpathFile);
                            string root1 = System.IO.Path.GetDirectoryName(elpathFile);

                            string[] componentesTitulo = filename1.Split('_');

                            String claveColectiva = filename1.Substring(filename1.IndexOf('_') + 1,
                                filename1.LastIndexOf('_') - (filename1.IndexOf('_') + 1)).Trim();

                            if (Regex.Match(componentesTitulo[2], stringRegexFechaHora).Success)
                            {
                                File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                                elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Procesar archivo desde hora programada " + ((FileInfo)files[i]).FullName + "]");

                                LNArchivo.ProcesaArch(elpathFile, claveColectiva, ip, log, filename1);
                            }
                            else
                            {
                                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Formato de fecha Inválido en el nombre del archivo]");
                            }
                        }
                        catch (Exception ex) 
                        {
                            resp = 1;
                            LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Erorr, " + ex.Message + " , " + ex.StackTrace + "]");

                        }
                    }

                }

                catch (Exception ex)
                {
                    resp = 1;
                    LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [Erorr, " + ex.Message + " , " + ex.StackTrace + "]");
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
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }
        }

        private static bool ejecutaAsignacionLimite(ref List<DLimiteCred> lstRegistros, String claveColectiva
                        , string log, string ip, decimal totImporte)
        {
            var ejecucionCorrecta = true;
            bool fondosSuficientes = true;
            decimal saldoCtaOrigen = 0;
            bool validaSaldo = true;

            foreach (var item in lstRegistros)
            {
                if (item.CodRespuesta.Equals("0000"))
                {
                    string tarjeta = item.Tarjeta.Substring(0, 6) + "******" + item.Tarjeta.Substring(12, 4);
                    try
                    {
                        SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraAutorizador);
                        try
                        {
                            Response respuesta = new Response();
                            conn.Open();
                            if (Convert.ToDecimal(item.LimiteCredito) > 0)
                            {
                                item.LimiteCredito = item.LimiteCredito.Replace(",", "");
                                DataTable respuestaAsignaLimiteCredito = DAODatosBase.obtenerDatosTarjetaEventoAsigLimCreditoParaExecute(item.Tarjeta, claveColectiva
                                                        , item.LimiteCredito, conn, ip, log, item.CuentaOrigen).Tables[0];
                                var codigoAsignaLimiteCredito = respuestaAsignaLimiteCredito.Rows[0][0].ToString();

                                if (string.IsNullOrEmpty(item.Usuario))
                                    item.Usuario = respuestaAsignaLimiteCredito.Rows[0]["NombreEmbozo"].ToString();

                                if (codigoAsignaLimiteCredito != "error")
                                {
                                    if (validaSaldo)
                                    {
                                        saldoCtaOrigen = Convert.ToDecimal(respuestaAsignaLimiteCredito.Rows[0][6]);

                                        if (totImporte > saldoCtaOrigen)
                                        {
                                            fondosSuficientes = false;
                                            break;
                                        }

                                        validaSaldo = false;
                                    }

                                    respuestaAsignaLimiteCredito = getEncValuesTableAE(respuestaAsignaLimiteCredito);
                                    Bonificacion bonificacion = LNFondeosYRetiros.obtenerDatosParaDiccionario(respuestaAsignaLimiteCredito);

                                    bonificacion.Importe = item.LimiteCredito;
                                    bonificacion.Concepto = item.Concepto;

                                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                                    {
                                        Dictionary<String, Parametro> parametrosAgregarTarjeta = DAODatosBase.ObtenerDatosParametros(bonificacion, respuestaAsignaLimiteCredito, respuesta, ip, log, conn, transaccionSQL);
                                        try
                                        {
                                            if (DAODatosBase.RealizarFondeoORetiro(bonificacion, parametrosAgregarTarjeta, respuesta, transaccionSQL, conn, ip, log))
                                            {
                                                LogueoAsignaLimiteCredito.Info("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [AsignaLimiteCredito, Asignación Correcta Tarjeta:" + tarjeta + "]");

                                                item.CodRespuesta = "0000";
                                                item.Resultado = "Aprobada";
                                                transaccionSQL.Commit();

                                            }
                                            else
                                            {
                                                item.CodRespuesta = "0011";
                                                item.Concepto = "";
                                                item.Resultado = "Error al asignar limite de credito";
                                                transaccionSQL.Rollback();
                                            }

                                        }
                                        catch (Exception tran)
                                        {
                                            item.CodRespuesta = "9999";
                                            item.Resultado = tran.Message;
                                            item.Concepto = "";
                                            LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + tran.StackTrace + "]");
                                            transaccionSQL.Rollback();
                                        }
                                    }
                                }
                                else
                                {
                                    item.CodRespuesta = "9999";
                                    item.Resultado = respuestaAsignaLimiteCredito.Rows[0]["error"].ToString();
                                    item.Concepto = "";
                                    LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + item.Resultado + "]");
                                }
                            }
                            else
                            {
                                item.CodRespuesta = "0010";
                                item.Resultado = "No se aplico movimiento por importe cero";
                                item.Concepto = "";
                            }
                        }
                        catch (Exception ex)
                        {

                            item.CodRespuesta = "9999";
                            item.Resultado = ex.Message;
                            item.Concepto = "";
                            LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN ASIGNAR LIMITE CREDITO {0} - ERROR {1}", tarjeta, ex.Message) + "1] " + ex.StackTrace);

                        }
                        finally
                        {
                            conn.Close();
                        }


                        if (item.CodRespuesta != "0000")
                        {
                            ejecucionCorrecta = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN ASIGNAR LIMITE CREDITO {0} - ERROR {1}", tarjeta, ex.Message) + "2]" + ex.StackTrace);
                        ejecucionCorrecta = false;
                    }
                }
            }

            if (!fondosSuficientes)
            {
                foreach (var item in lstRegistros)
                {
                    item.CodRespuesta = "1016";
                    item.Resultado = "El importe Total (" + totImporte +") es mayor al saldo de la cuenta Origen (" + saldoCtaOrigen  + ")";
                }
            }

            return ejecucionCorrecta;
        }

        private static void ObtieneRegistrosDeArchivo(String elPath, ref List<DLimiteCred> elArchivo, string ip, string log, ref decimal totImporte)
        {
            int contador = 0;
            elArchivo.Clear();
            bool importeCorrecto = true;
            DLimiteCred dlimiteCred = null;
            try
            {
                using (var stream = File.Open(elPath, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                    {
                        DataSet resultDataSet = excelDataReader.AsDataSet();
                        DataTable dtTable = resultDataSet.Tables[0];
                        foreach (DataRow rows in dtTable.Rows)
                        {
                            if (contador != 0)
                            {
                                if (!string.IsNullOrEmpty(rows.ItemArray[3].ToString()))
                                {
                                    importeCorrecto = true;

                                    

                                    if (!Regex.Match(rows.ItemArray[3].ToString(), stringRegexTarjeta).Success)
                                    {
                                        dlimiteCred = new DLimiteCred()
                                        {
                                            CuentaOrigen = rows.ItemArray[1].ToString(),
                                            Tarjeta = rows.ItemArray[3].ToString(),
                                            EstatusTarjeta = rows.ItemArray[4].ToString(),
                                            LimiteCredito = rows.ItemArray[5].ToString(),
                                            Concepto = rows.ItemArray[6].ToString(),
                                            Usuario = rows.ItemArray[2].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "La Tarjeta no cumple el formato correcto (16 dígitos)"
                                        };

                                        elArchivo.Add(dlimiteCred);
                                    }
                                    else if (!Regex.Match(rows.ItemArray[5].ToString(), stringRegexImporte).Success)
                                    {
                                        dlimiteCred = new DLimiteCred()
                                        {
                                            CuentaOrigen = rows.ItemArray[1].ToString(),
                                            Tarjeta = rows.ItemArray[3].ToString(),
                                            EstatusTarjeta = rows.ItemArray[4].ToString(),
                                            LimiteCredito = rows.ItemArray[5].ToString(),
                                            Concepto = rows.ItemArray[6].ToString(),
                                            Usuario = rows.ItemArray[2].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "El importe no cumple el formato (0,000.00)"
                                        };

                                        elArchivo.Add(dlimiteCred);
                                        importeCorrecto = false;
                                    }
                                    else if (!Regex.Match(rows.ItemArray[1].ToString(), stringRegexNumerico).Success)
                                    {
                                        dlimiteCred = new DLimiteCred()
                                        {
                                            CuentaOrigen = rows.ItemArray[1].ToString(),
                                            Tarjeta = rows.ItemArray[3].ToString(),
                                            EstatusTarjeta = rows.ItemArray[4].ToString(),
                                            LimiteCredito = rows.ItemArray[5].ToString(),
                                            Concepto = rows.ItemArray[6].ToString(),
                                            Usuario = rows.ItemArray[2].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "La cuenta Origen no cumple el formato númerico"
                                        };

                                        elArchivo.Add(dlimiteCred);
                                    }
                                    else if(!string.IsNullOrEmpty(rows.ItemArray[6].ToString()))
                                    {

                                        if (rows.ItemArray[6].ToString().Length > 200)
                                        {
                                            dlimiteCred = new DLimiteCred()
                                            {
                                                CuentaOrigen = rows.ItemArray[1].ToString(),
                                                Tarjeta = rows.ItemArray[3].ToString(),
                                                EstatusTarjeta = rows.ItemArray[4].ToString(),
                                                LimiteCredito = rows.ItemArray[5].ToString(),
                                                Concepto = rows.ItemArray[6].ToString(),
                                                Usuario = rows.ItemArray[2].ToString(),
                                                CodRespuesta = "9999",
                                                Resultado = "El concepto excede la longitud de 150"
                                            };

                                            elArchivo.Add(dlimiteCred);
                                        }
                                        else 
                                        {
                                            dlimiteCred = new DLimiteCred()
                                            {
                                                CuentaOrigen = rows.ItemArray[1].ToString(),
                                                Tarjeta = rows.ItemArray[3].ToString(),
                                                EstatusTarjeta = rows.ItemArray[4].ToString(),
                                                LimiteCredito = rows.ItemArray[5].ToString(),
                                                Concepto = string.IsNullOrEmpty(rows.ItemArray[6].ToString())
                                                        ? "Asignación_masiva_" + DateTime.Now.ToString("dd-MM-yy")
                                                        : rows.ItemArray[6].ToString(),
                                                Usuario = rows.ItemArray[2].ToString(),
                                                CodRespuesta = "0000"
                                            };

                                            elArchivo.Add(dlimiteCred);
                                        }
                                    }
                                    else
                                    {
                                        dlimiteCred = new DLimiteCred()
                                        {
                                            CuentaOrigen = rows.ItemArray[1].ToString(),
                                            Tarjeta = rows.ItemArray[3].ToString(),
                                            EstatusTarjeta = rows.ItemArray[4].ToString(),
                                            LimiteCredito = rows.ItemArray[5].ToString(),
                                            Concepto = string.IsNullOrEmpty(rows.ItemArray[6].ToString())
                                                        ? "Asignación_masiva_" + DateTime.Now.ToString("dd-MM-yy")
                                                        : rows.ItemArray[6].ToString(),
                                            Usuario = rows.ItemArray[2].ToString(),
                                            CodRespuesta = "0000"
                                        };

                                        elArchivo.Add(dlimiteCred);
                                    }

                                    if (importeCorrecto)
                                        totImporte = totImporte + Convert.ToDecimal(rows.ItemArray[5]);
                                }
                            }
                            contador += 1;

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }
        }

        private static void actualizaRegistrosArchivo(List<DLimiteCred> lstRegistros, string path, string log
                    , string ip, string pathOriginal)
        {
            int contadorRows = 2;
            int contadorLst = 0;
            try
            {
                using (var workbook = new XLWorkbook(path))
                {
                    var ws1 = workbook.Worksheet(1);
                    foreach (var registro in lstRegistros)
                    {
                        ws1.Cell("H" + contadorRows).Value = lstRegistros[contadorLst].Resultado;
                        ws1.Cell("G" + contadorRows).Value = lstRegistros[contadorLst].Concepto;
                        ws1.Cell("C" + contadorRows).Value = lstRegistros[contadorLst].Usuario;
                        contadorRows += 1;
                        contadorLst += 1;
                    }

                    workbook.Save();
                }
                DAODatosBase.GuardarArchivo(pathOriginal, path, lstRegistros);
            }
            catch (Exception err)
            {
                LogueoAsignaLimiteCredito.Error("[" + ip + "] [LimiteCredito] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }

        }

        private static DataTable getEncValuesTableAE(DataTable table)
        {
            DataColumnCollection columns = table.Columns;

            if (columns.Contains("encriptado"))
            {
                foreach (DataRow fila in table.Rows)
                {


                    if (!string.IsNullOrEmpty(fila["encriptado"].ToString()))
                    {
                        fila["valor"] = fila["encriptado"].ToString();
                    }
                }
                table.AcceptChanges();

            }
            return table;

        }
    }
}
