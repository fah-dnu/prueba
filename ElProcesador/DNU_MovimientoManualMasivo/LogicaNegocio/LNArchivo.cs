using CommonProcesador;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DNU_MovimientoManualMasivo.Utilidades;
using DNU_MovimientoManualMasivo.Entidades;
using ExcelDataReader;
using DNU_MovimientoManualMasivo.BaseDatos;
using Interfases.Entidades;
using ClosedXML.Excel;

namespace DNU_MovimientoManualMasivo.LogicaNegocio
{
    public class LNArchivo
    {
        private string directorio;
        private static readonly object balanceLock = new object();
        private static string directorioSalida;

        private static string stringRegexImporte = @"^[0-9]*[\\.]*[0-9]*$";
        private static string stringRegexTarjeta = @"^[0-9]{16}$";
        private static string stringRegexFechaHora = @"^(19|[2-9][0-9])\d\d(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])(((([0-1][0-9])|(2[0-3]))[0-5][0-9][0-5][0-9]+)).xlsx$";
        private static string stringRegexNumerico = @"^[0-9]{0,15}$";

        private static FileSystemWatcher elObservadorALC = new FileSystemWatcher(PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada"));

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
                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Inicia Escucha en Directorio MOVIMIENTO_MASIVO WATCHER: " + directorio);
                elObservadorALC.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
                elObservadorALC.Created += NuevoArchivo;
                elObservadorALC.Error += alOcurrirUnError;
                elObservadorALC.EnableRaisingEvents = true;

                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Termina Escucha en Directorio MOVIMIENTO_MASIVO WATCHER: ");

            }
            catch (Exception err)
            {
                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Escuchar directorios: " + err.Message);
            }
        }

        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                LogueoMovimientoMasivo.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + e.GetException().Message + "]");
            }
            catch (Exception err)
            {
                LogueoMovimientoMasivo.Error("[" + ip + "] [ParabiliumProcesador] [PROCESADORNOCTURNO] [" + log + "] [alOcurrirUnError(): e: " + err.Message + "]");
            }
        }

        public static Boolean ProcesaArch(String path, string ip, string log, string pathOriginal)
        {
            List<DMovimientoMasivo> lstMovimientoMasivo = new List<DMovimientoMasivo>();
            try
            {
                ObtieneRegistrosDeArchivo(path, ref lstMovimientoMasivo, ip, log);

                var ejecucionCorrecta = ejecutaAsignacionLimite(ref lstMovimientoMasivo, log, ip);

                renombraArchivo(path, ejecucionCorrecta, lstMovimientoMasivo, ip, log, pathOriginal);

                return true;


            }
            catch (Exception err)
            {
                LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "] [" + err.StackTrace + " ]");
                renombraArchivoPorError(path, lstMovimientoMasivo, err.Message, ip, log, pathOriginal);
                return false;
            }
        }

        private static void renombraArchivoPorError(string path, List<DMovimientoMasivo> lstArchivo,
            string message, string ip, string log, string pathOriginal)
        {
            foreach (var item in lstArchivo)
            {
                item.Resultado = message;
            }

            renombraArchivo(path, false, lstArchivo, ip, log, pathOriginal);
        }


        public void crearDirectorio()
        {//
            directorioSalida = PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioSalida");
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
                    LogueoMovimientoMasivo.Info("[MovimientoMasivo] [PROCESADORNOCTURNO] [Hubo un Cambio en el directorio: " + PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada") + " se recibio el archivo : " + e.FullPath + "]");
                    LogueoMovimientoMasivo.Info("[MovimientoMasivo] [PROCESADORNOCTURNO] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");

                    validarArchivos(true, direccionIP, idLog);
                }
            }
            catch (Exception ex)
            {
                LogueoMovimientoMasivo.Error("[MovimientoMasivo] [PROCESADORNOCTURNO] [NuevoArchivo(): " + ex.Message + ",  " + ex.ToString() + "]");
            }
            finally
            {
                //poner a escuchar otra vez el file watcher
                EscucharDirectorio(direccionIP, idLog);
            }
        }

        internal void DetenerDirectorio()
        {
            LogueoMovimientoMasivo.Info("[ParabiliumProcesador] [PROCESADORNOCTURNO] [Se Detiene a escucha del Directorio: " + PNConfig.Get("MOVIMIENTO_MASIVO", "DirectorioEntrada"));

            elObservadorALC.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
            elObservadorALC.Created -= NuevoArchivo;
            elObservadorALC.Error -= alOcurrirUnError;
            elObservadorALC.EnableRaisingEvents = false;


        }

        private static void renombraArchivo(string elpathFile, bool ejecucionCorrecta, List<DMovimientoMasivo> lstRegistros
                    , string ip, string log, string pathOriginal)
        {
            LogueoMovimientoMasivo.Info("[" + ip + "][MovimientoMasivo] [PROCESADORNOCTURNO][" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
            string filename = System.IO.Path.GetFileName(elpathFile);

            string finalFileName = directorioSalida + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");
            if (File.Exists(finalFileName))
            {
                finalFileName = finalFileName.Replace("\\PROCESADO_", "\\PROCESADO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_");
            }

            File.Move(elpathFile, finalFileName);


            actualizaRegistrosArchivo(lstRegistros, finalFileName, log, ip, pathOriginal);
        }

        internal bool validarArchivos(bool validaCopiadoFiles, string ip, string log)
        {
            try
            {
                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Inicia proceso limite de credito]");

                if (validaCopiadoFiles)
                {
                    LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Iniciar espera de copiado de Archivo MOVIMIENTO_MASIVO]");
                    Thread.Sleep(Convert.ToInt32(PNConfig.Get("MOVIMIENTO_MASIVO", "MiliEsperaProceso")));
                }

                int resp;

                resp = 0;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    FileInfo[] files = directory.GetFiles(PNConfig.Get("MOVIMIENTO_MASIVO", "TokenArchivos") + "*.xlsx");

                    if (files.Length == 0)
                    {
                        LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [NO HAY ARCHIVO QUE PROCESAR: MOVIMIENTO_MASIVO]");
                    }

                    for (int i = 0; i < files.Length; i++)
                    {
                        try
                        {
                            String elpathFile = (((FileInfo)files[i]).FullName);

                            //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                            LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Inicia el renombramiento del Archivo procesado: " + elpathFile + "]");
                            string filename1 = System.IO.Path.GetFileName(elpathFile);
                            string root1 = System.IO.Path.GetDirectoryName(elpathFile);

                            string[] componentesTitulo = filename1.Split('_');

                            if (Regex.Match(componentesTitulo[1], stringRegexFechaHora).Success)
                            {
                                File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                                elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Procesar archivo desde hora programada " + ((FileInfo)files[i]).FullName + "]");

                                LNArchivo.ProcesaArch(elpathFile, ip, log, filename1);
                            }
                            else
                            {
                                LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Formato de fecha Inválido en el nombre del archivo]");
                            }
                        }
                        catch (Exception ex)
                        {
                            resp = 1;
                            LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Erorr, " + ex.Message + " , " + ex.StackTrace + "]");

                        }
                    }

                }

                catch (Exception ex)
                {
                    resp = 1;
                    LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [Erorr, " + ex.Message + " , " + ex.StackTrace + "]");
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
                LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + err.Message + "]");
                return false;
            }
        }

        private static bool ejecutaAsignacionLimite(ref List<DMovimientoMasivo> lstRegistros
                        , string log, string ip)
        {
            var ejecucionCorrecta = true;

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
                            if (Convert.ToDecimal(item.Importe) > 0)
                            {
                                item.Importe = item.Importe.Replace(",", "");
                                DataTable respuestaMovManual = DAODatosBase.obtenerDatosTarjetaEventoManualMasivoParaExecute(item.Tarjeta
                                                        , item.Importe, conn, ip, log).Tables[0];
                                var codigoMovManual = respuestaMovManual.Rows[0][0].ToString();

                                if (codigoMovManual != "error")
                                {
                                    respuestaMovManual = getEncValuesTableAE(respuestaMovManual);
                                    Bonificacion bonificacion = LNFondeosYRetiros.obtenerDatosParaDiccionario(respuestaMovManual);

                                    bonificacion.Importe = item.Importe;
                                    bonificacion.Concepto = PNConfig.Get("MOVIMIENTO_MASIVO", "Concepto");
                                    bonificacion.RefNumerica = string.IsNullOrEmpty(item.Referencia)
                                                                ? 0
                                                                : Convert.ToInt32(item.Referencia);
                                    bonificacion.Observaciones = item.Observaciones;

                                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                                    {
                                        Dictionary<String, Parametro> parametrosAgregarTarjeta = DAODatosBase.ObtenerDatosParametros(bonificacion, respuestaMovManual, respuesta, ip, log, conn, transaccionSQL);
                                        try
                                        {
                                            if (DAODatosBase.RealizarFondeoORetiro(bonificacion, parametrosAgregarTarjeta, respuesta, transaccionSQL, conn, ip, log))
                                            {
                                                LogueoMovimientoMasivo.Info("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [MOVIMIENTO_MASIVO, Asignación Correcta Tarjeta:" + tarjeta + "]");

                                                item.CodRespuesta = "0000";
                                                item.Resultado = "Aprobada";
                                                item.Concepto = bonificacion.Concepto;
                                                transaccionSQL.Commit();

                                            }
                                            else
                                            {
                                                item.CodRespuesta = "0011";
                                                item.Concepto = "";
                                                item.Resultado = "Error al aplicar el Movimiento";
                                                transaccionSQL.Rollback();
                                            }

                                        }
                                        catch (Exception tran)
                                        {
                                            item.CodRespuesta = "9999";
                                            item.Resultado = tran.Message;
                                            item.Concepto = "";
                                            LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + tran.StackTrace + "]");
                                            transaccionSQL.Rollback();
                                        }
                                    }
                                }
                                else
                                {
                                    item.CodRespuesta = "9999";
                                    item.Resultado = respuestaMovManual.Rows[0]["error"].ToString();
                                    item.Concepto = "";
                                    LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + item.Resultado + "]");
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
                            LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN MOVIMIENTO MASIVO {0} - ERROR {1}", tarjeta, ex.Message) + "1] " + ex.StackTrace);

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
                        LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("ERROR EN ASIGNAR LIMITE CREDITO {0} - ERROR {1}", tarjeta, ex.Message) + "2]" + ex.StackTrace);
                        ejecucionCorrecta = false;
                    }
                }
            }


            return ejecucionCorrecta;
        }

        private static void ObtieneRegistrosDeArchivo(String elPath, ref List<DMovimientoMasivo> elArchivo, string ip, string log)
        {
            int contador = 0;
            elArchivo.Clear();
            DMovimientoMasivo dMovMasivo = null;
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
                                if (!string.IsNullOrEmpty(rows.ItemArray[0].ToString()))
                                {
                                    if (!Regex.Match(rows.ItemArray[0].ToString(), stringRegexTarjeta).Success)
                                    {
                                        dMovMasivo = new DMovimientoMasivo()
                                        {
                                            Tarjeta = rows.ItemArray[0].ToString(),
                                            Importe = rows.ItemArray[1].ToString(),
                                            Referencia = rows.ItemArray[2].ToString(),
                                            Observaciones = rows.ItemArray[3].ToString(),
                                            Concepto = rows.ItemArray[4].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "La Tarjeta no cumple el formato correcto (16 dígitos)"
                                        };

                                        elArchivo.Add(dMovMasivo);
                                    }
                                    else if (!Regex.Match(rows.ItemArray[1].ToString(), stringRegexImporte).Success)
                                    {
                                        dMovMasivo = new DMovimientoMasivo()
                                        {
                                            Tarjeta = rows.ItemArray[0].ToString(),
                                            Importe = rows.ItemArray[1].ToString(),
                                            Referencia = rows.ItemArray[2].ToString(),
                                            Observaciones = rows.ItemArray[3].ToString(),
                                            Concepto = rows.ItemArray[4].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "El importe no cumple el formato (0.00)"
                                        };

                                        elArchivo.Add(dMovMasivo);
                                    }
                                    else if (!Regex.Match(rows.ItemArray[2].ToString(), stringRegexNumerico).Success)
                                    {
                                        dMovMasivo = new DMovimientoMasivo()
                                        {
                                            Tarjeta = rows.ItemArray[0].ToString(),
                                            Importe = rows.ItemArray[1].ToString(),
                                            Referencia = rows.ItemArray[2].ToString(),
                                            Observaciones = rows.ItemArray[3].ToString(),
                                            Concepto = rows.ItemArray[4].ToString(),
                                            CodRespuesta = "9999",
                                            Resultado = "La Referencia no cumple el formato"
                                        };

                                        elArchivo.Add(dMovMasivo);
                                    }
                                    else if (!string.IsNullOrEmpty(rows.ItemArray[3].ToString()))
                                    {
                                        if (rows.ItemArray[3].ToString().Length > 200)
                                        {
                                            dMovMasivo = new DMovimientoMasivo()
                                            {
                                                Tarjeta = rows.ItemArray[0].ToString(),
                                                Importe = rows.ItemArray[1].ToString(),
                                                Referencia = rows.ItemArray[2].ToString(),
                                                Observaciones = rows.ItemArray[3].ToString(),
                                                Concepto = rows.ItemArray[4].ToString(),
                                                CodRespuesta = "9999",
                                                Resultado = "Las observaciones exceden la longitud de 200"
                                            };

                                            elArchivo.Add(dMovMasivo);
                                        }
                                        else
                                        {
                                            dMovMasivo = new DMovimientoMasivo()
                                            {
                                                Tarjeta = rows.ItemArray[0].ToString(),
                                                Importe = rows.ItemArray[1].ToString(),
                                                Referencia = rows.ItemArray[2].ToString(),
                                                Observaciones = rows.ItemArray[3].ToString(),
                                                Concepto = rows.ItemArray[4].ToString(),
                                                CodRespuesta = "0000"
                                            };

                                            elArchivo.Add(dMovMasivo);
                                        }
                                    }
                                    else
                                    {
                                        dMovMasivo = new DMovimientoMasivo()
                                        {
                                            Tarjeta = rows.ItemArray[0].ToString(),
                                            Importe = rows.ItemArray[1].ToString(),
                                            Referencia = rows.ItemArray[2].ToString(),
                                            Observaciones = rows.ItemArray[3].ToString(),
                                            Concepto = rows.ItemArray[4].ToString(),
                                            CodRespuesta = "0000"
                                        };

                                        elArchivo.Add(dMovMasivo);
                                    }
                                }
                            }
                            contador += 1;

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }
        }

        private static void actualizaRegistrosArchivo(List<DMovimientoMasivo> lstRegistros, string path, string log
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

                        ws1.Cell("E" + contadorRows).Value = lstRegistros[contadorLst].Concepto;
                        ws1.Cell("F" + contadorRows).Value = lstRegistros[contadorLst].Resultado;
                        contadorRows += 1;
                        contadorLst += 1;
                    }

                    workbook.Save();
                }
                DAODatosBase.GuardarArchivo(pathOriginal, path, lstRegistros);
            }
            catch (Exception err)
            {
                LogueoMovimientoMasivo.Error("[" + ip + "] [MovimientoMasivo] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
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
