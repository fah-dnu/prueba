using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.BaseDatos;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_ConciliadorCacao.LogicaNegocio
{
    class LNArchivo
    {
        private string directorio;
        private string tipoColectiva;
        private static string directorioSalida;
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PROCONCILIACACAO";
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


        public LNArchivo(string directorio, string tipoColectiva)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
        }


        public static Boolean ProcesaArch(Archivo unArchivo, String path, String apiKey, String user)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                Int64 ID_Fichero = 0;

                ObtieneRegistrosDeArchivo(path, ref unArchivo);



                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(unArchivo, conn, transaccionSQL);

                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            //Guarda los detalles
                            DataTable dtDetalle = DAOArchivo.GeneraDataTableDetalle(unArchivo, ID_Fichero);

                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(dtDetalle);

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                 LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            }

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message + "]");
                            transaccionSQL.Rollback();
                        }
                    }
                }

                //si es de Comparacion, obtiene los datos de la base de datos.

                //Realiza las Comparaciones.

                //Ejecuta los Eventos de cada fila del Archivo.
                DatosBaseDatos laConfigForConmparacion = DAODatosBase.ObtenerConsulta(unArchivo.ID_Archivo);


                if (ID_Fichero == 0)
                {
                    throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
                }

                //Insertar los datos obtenidos en la DB de Archivos
                string minProcessFileDate = DAODatosBase.GetMinProcessDateFromFile(unArchivo);
                string maxProcessFileDate = DAODatosBase.GetMaxProcessDateFromFile(unArchivo);
                int OperacionesBD = DAODatosBase.InsertaDatosEnTabla(laConfigForConmparacion, ID_Fichero,
                    unArchivo, minProcessFileDate, maxProcessFileDate, apiKey, user);

                //OBTINE LAS CONFIGURACIONES DE CAMPOS-FILAS
                List<ComparadorConfig> lasConfiguraciones = DAOComparador.ObtenerConfiguracionFila(ID_Fichero);

                if (lasConfiguraciones.Count == 0)
                    return true;

                StringBuilder lasRespuesta = new StringBuilder();

                String laConsultaWhere = LNComparacion.GeneraConsultaWhereSQL(lasConfiguraciones,
                    minProcessFileDate);

                SyscardConciliation postConciliation = new SyscardConciliation(DAODatosBase.GetEmisor(unArchivo),
                    maxProcessFileDate);

                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(ID_Fichero,
                    laConsultaWhere);
                AddDataToList(losNOArchivoSIBD, postConciliation, 1, maxProcessFileDate);

                DataSet losSIArchivoSIBD = DAOComparador.ObtieneDatosSIArchivoSIBaseDatos(ID_Fichero,
                    laConsultaWhere);
                AddDataToList(losSIArchivoSIBD, postConciliation, 2, maxProcessFileDate);


                DataSet losSIArchivoNOBD = DAOComparador.ObtieneDatosSIArchivoNOBaseDatos(ID_Fichero,
                    laConsultaWhere);
                AddDataToList(losSIArchivoNOBD, postConciliation, 3, maxProcessFileDate);

                var result = DAODatosBase.InsertaConciliacionCacao(laConfigForConmparacion,
                    postConciliation, apiKey, user);

                return true;

            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [getUsuarios(): " + err.Message + "]");
                return false;
            }
        }

        public void crearDirectorio()
        {//
            directorioSalida = PNConfig.Get("PROCONCILIACACAO", "DirectorioSalida");
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
            if (!Directory.Exists(directorio + "\\Procesados"))
                Directory.CreateDirectory(directorio + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorioSalida + "\\Erroneos"))
                Directory.CreateDirectory(directorioSalida + "\\Erroneos");
            if (!Directory.Exists(directorioSalida + "\\Correctos"))
                Directory.CreateDirectory(directorioSalida + "\\Correctos");
        }

        private static void AddDataToList(DataSet theData, SyscardConciliation rowsToPost,
            int caseFile, String maxProcessFileDate)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            foreach (DataRow row in theData.Tables[0].Rows)
            {
                try
                {
                    switch (caseFile)
                    {
                        case 1:
                            if (row[2].ToString() == maxProcessFileDate)
                            {
                                rowsToPost.transacciones.Add(new Transaccion
                                {
                                    //ClaveEmisor = Convert.ToInt64(row[0]),
                                    codigo_movimiento = row[1].ToString().Trim(),
                                    //FechaMovimiento = row[2].ToString(),
                                    hora = row[3].ToString().Trim(),
                                    monto = row[4].ToString(),
                                    numero_tarjeta = row[5].ToString(),
                                    documento = row[6].ToString(),
                                    en_siscard = 0,
                                    en_cacao = 1
                                });
                            }
                            break;
                        case 2:
                            rowsToPost.transacciones.Add(new Transaccion
                            {
                                //ClaveEmisor = Convert.ToInt64(row[0]),
                                codigo_movimiento = row[1].ToString().Trim(),
                                //FechaMovimiento = row[2].ToString().Trim(),
                                hora = row[3].ToString().Trim(),
                                numero_tarjeta = row[4].ToString().Trim(),
                                monto = row[5].ToString().Trim(),
                                documento = row[6].ToString().Trim(),
                                en_cacao = 1,
                                en_siscard = 1
                            });
                            break;
                        case 3:
                            rowsToPost.transacciones.Add(new Transaccion
                            {
                                //ClaveEmisor = Convert.ToInt64(row[0]),
                                codigo_movimiento = row[1].ToString().Trim(),
                                //FechaMovimiento = row[2].ToString(),
                                hora = row[3].ToString().Trim(),
                                monto = row[4].ToString(),
                                numero_tarjeta = row[5].ToString(),
                                documento = row[6].ToString(),
                                en_siscard = 1,
                                en_cacao = 0
                            });
                            break;

                        default:
                            break;

                    }
                }
                catch (Exception ex)
                {
                    LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [AddDataToList(): " + ex.Message + "]");
                }
            }
        }

        internal static string LimpiarArchivo(String elArchivo, string path)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(elArchivo, Encoding.Default))
            {
                String claveMov = String.Empty;
                String descMov = String.Empty;
                String account = String.Empty;
                String total = String.Empty;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();

                    if (Regex.Match(line, @"Cuentas.:").Success)
                    {
                        account = line.Substring(line.IndexOf("Cuentas.:"), 19).Split(':')[1].Trim();
                        account = account.PadRight(10);
                    }

                    if (Regex.Match(line, @"^Movimiento:   ").Success)
                    {
                        String title = line.Split(':')[1].Trim();
                        claveMov = title.Substring(0, 5);
                        //descMov = title.Substring(5, (title.Length - claveMov.Length)).Trim();
                        claveMov = claveMov.Trim();
                        //descMov = descMov.PadRight(50);
                    }

                    if (Regex.Match(line, @"^[0-9]{2}/[0-9]{2}/[0-9]{2}[\s][0-9]{6}[\s]{3}[0-9]{13}[\s]{3}[0-9]{13}[\s]{3}(M|I)").Success)
                    {
                        line = line.Replace('.', '*').Replace(',', '.').Replace('*', ' ');

                        var limiteInicial = 9;

                        var fechaTempString = line.Substring(0, limiteInicial).Trim();

                        var fecha = String.Format("{0}{1}{2}",
                            CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(fechaTempString.Substring(6, 2))),
                            fechaTempString.Substring(3, 2),
                            fechaTempString.Substring(0, 2));

                        //var fecha = DateTime.Parse(line.Substring(0, limiteInicial).Trim()).ToString("yyyyMMdd");


                        var segundolimite = limiteInicial + 9;
                        var hora = line.Substring(limiteInicial, (segundolimite - limiteInicial)).Trim();

                        var tercerLimite = segundolimite + 16;
                        var documento1 = line.Substring(segundolimite, (tercerLimite - segundolimite)).Trim();

                        var cuartoLimite = tercerLimite + 16;
                        var documento2 = line.Substring(tercerLimite, (cuartoLimite - tercerLimite));

                        var quintoLimite = cuartoLimite + 4;
                        var f = line.Substring(cuartoLimite, (quintoLimite - cuartoLimite)).Trim();

                        var sextoLimite = quintoLimite + 4;
                        var e = line.Substring(quintoLimite, (sextoLimite - quintoLimite)).Trim();

                        var septimoLimite = sextoLimite + 10;
                        var consumo = line.Substring(sextoLimite, (septimoLimite - sextoLimite)).Trim();

                        var octavoLimite = septimoLimite + 22;
                        var tarjeta = line.Substring(septimoLimite, (octavoLimite - septimoLimite)).Trim().Replace(" ", string.Empty);

                        var novenoLimite = octavoLimite + 43;
                        var nombre = line.Substring(octavoLimite, (novenoLimite - octavoLimite)).Trim();

                        var decimoLimite = novenoLimite + 13;
                        var comercio = line.Substring(novenoLimite, (decimoLimite - novenoLimite)).Trim();

                        var decimoPrimerLimite = decimoLimite + 11;
                        var comision = line.Substring(decimoLimite,
                            (decimoPrimerLimite - decimoLimite)).Trim();

                        var decimoSegundoLimite = decimoPrimerLimite + 3;
                        var mo = line.Substring(decimoPrimerLimite,
                            (decimoSegundoLimite - decimoPrimerLimite)).Trim();

                        var decimoTercerLimite = decimoSegundoLimite + 18;
                        var monto = line.Substring(decimoSegundoLimite,
                            (decimoTercerLimite - decimoSegundoLimite)).Trim().Replace(" ", string.Empty); ;

                        var decimoCuartoLimite = decimoTercerLimite + 10;
                        var usuario = line.Substring(decimoTercerLimite).Trim();//line.Substring(decimoTercerLimite,(decimoCuartoLimite - decimoTercerLimite)).Trim();

                        var cia = string.Empty;//line.Substring(decimoCuartoLimite).Trim();

                        sb.AppendLine(String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|" +
                            "{10}|{11}|{12}|{13}|{14}|{15}|{16}", account, claveMov, fecha, hora,
                            documento1, documento2, f, e, consumo, tarjeta, nombre, comercio, comision,
                            mo, monto, usuario, cia));
                    }
                }
            }
            File.Delete(elArchivo);
            using (StreamWriter sw = File.CreateText(elArchivo))
            {
                sw.WriteLine(sb.ToString());
            }

            return elArchivo;
        }


        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
             LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Hubo un Cambio " + elTipoCambio.ToString() + " en el directorio: " + PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            // Thread.Sleep(1000 * 60 * 1);
            LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
            validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ConciliacionCacao OcurrioError, El evento de fileWatcher no inicio correctamente, Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }

        [Transaction]
        public Boolean validarArchivos(bool validaCopiadoFiles)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarArchivos");
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            String elpathFile = string.Empty;
            try
            {
                if (validaCopiadoFiles)
                {
                    Thread.Sleep(1000 * 60 * 1);
                }


                ConfiguracionContexto.InicializarContexto();
                var apiKey = PNConfig.Get("PROCONCILIACACAO", "ApiKey");
                var user = PNConfig.Get("PROCONCILIACACAO", "User");

                DirectoryInfo directory = new DirectoryInfo(directorio);



                List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                Archivo elArchivo = losArchivos.Where(m => m.ClaveArchivo.Equals("PROCONCILIACACAO")).FirstOrDefault();
                //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.
                if (elArchivo == null)
                {
                    ApmNoticeWrapper.NoticeException(new Exception("No se ha encontrado el archivo de configuración para el proceso"));
                    LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [No se ha encontrado el archivo de configuración para el proceso]");
                    throw new Exception("No se ha encontrado el archivo de configuración para el proceso");
                    
                    
                }

                FileInfo[] files = directory.GetFiles(elArchivo.Nombre + "*.*");

                if (files.Length == 0)
                {
                    LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [NO HAY ARCHIVO QUE PROCESAR: PROCONCILIACACAO" + elArchivo.Nombre + "*.*]");
                }

                for (int i = 0; i < files.Length; i++)
                {
                    bool resp = true;
                    elpathFile = string.Empty;

                    elpathFile = (((FileInfo)files[i]).FullName);
                    elArchivo.UrlArchivo = elpathFile;

                    //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                    LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
                    string filename1 = Path.GetFileName(elpathFile);
                    string root1 = Path.GetDirectoryName(elpathFile);

                    //Limpiar archivo
                    var tmpNameFile =
                        LNArchivo.LimpiarArchivo(((FileInfo)files[i]).FullName, directory.FullName);



                    elpathFile = renombraArchivoEnProceso(tmpNameFile, elpathFile, root1, filename1);

                    LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "]");

                    resp = LNArchivo.ProcesaArch(elArchivo, elpathFile, apiKey, user);

                    //RENOMBRAR el ARCHIVO
                    if (resp)
                    {
                        renombraRespuestaCorrecta(elpathFile);
                    }
                    else
                    {
                        renombraRespuestaIncorrecta(elpathFile);
                    }


                }

                return true;

            }
            catch (Exception err)
            {
                renombraRespuestaIncorrecta(elpathFile);
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("validarArchivos: {0} -Stack: {1}" + err.Message, err.StackTrace) + "]");
                ApmNoticeWrapper.NoticeException(err);
                return false;
            }

        }

        private string renombraArchivoEnProceso(string tmpNameFile, string elpathFile, string root1, string filename1)
        {

            String elpathFiletmp = root1 + "\\EN_PROCESO_" + filename1;

            File.Move(tmpNameFile, elpathFile);

            if (File.Exists(elpathFiletmp))
            {
#if DEBUG
                var directorio = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var directorio = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif
                DirectoryInfo directory = new DirectoryInfo(directorio);
                var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(filename1)).Count() + 1;
                elpathFiletmp = String.Format("{0}\\EN_PROCESO_{1}_{2}_{3}", root1, consecutivo, DateTime.Now.Hour, filename1);

            }

            File.Move(elpathFile, elpathFiletmp);
            return elpathFiletmp;
        }

        private void renombraRespuestaIncorrecta(string elpathFile)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
            string filename = Path.GetFileName(elpathFile);
            string finalFileName = directorioSalida + "\\Erroneos\\" + "\\PROCESADO_CON_ERROR_" + filename.Replace("EN_PROCESO_", "");

            if (File.Exists(finalFileName))
            {
                var tempName = Path.GetFileName(finalFileName);
#if DEBUG
                var directorio = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var directorio = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif
                DirectoryInfo directory = new DirectoryInfo(directorio);
                var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(tempName.Replace("PROCESADO_CON_ERROR_", ""))).Count() + 1;
                finalFileName = finalFileName.Replace("PROCESADO_CON_ERROR_", string.Format("PROCESADO_CON_ERROR{0}_{1}_", consecutivo, DateTime.Now.Hour));
            }

            File.Move(elpathFile, finalFileName);
        }

        private void renombraRespuestaCorrecta(string elpathFile)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
            string filename = Path.GetFileName(elpathFile);

            string finalFileName = directorioSalida + "\\Correctos\\" + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");

            if (File.Exists(finalFileName))
            {
                var tempName = Path.GetFileName(finalFileName);
#if DEBUG
                var directorio = PNConfig.Get("PROCONCILIACACAO", "Directorio");
#else
                var directorio = PNConfig.Get("PROCONCILIACACAO", "DirectorioEntrada");
#endif
                DirectoryInfo directory = new DirectoryInfo(directorio);
                var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(tempName.Replace("PROCESADO_", ""))).Count() + 1;
                finalFileName = finalFileName.Replace("PROCESADO_", string.Format("PROCESADO_{0}_{1}_", consecutivo, DateTime.Now.Hour));
            }


            File.Move(elpathFile, finalFileName);
        }

        public static void ObtieneRegistrosDeArchivo(String elPath, ref Archivo elArchivo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            StreamReader objReader = new StreamReader(elPath);
            string sLine = "";
            //List<Fila> losDetalles = new List<Fila>();
            int noFilaDeARchivo = 0;
            String UltimaFilaLeida = "";
            elArchivo.LosDatos.Clear();
            try
            {
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();

                    if (sLine == null)
                    {
                        break;
                    }

                    elArchivo.LosDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura));

                    noFilaDeARchivo++; //INCREMENTA LA FILA LEIDA

                    UltimaFilaLeida = sLine;
                }

                elArchivo.Nombre = elPath;

            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }
            finally
            {
                objReader.Close();
            }

            return;
        }

        public static Fila DecodificaFila(String Cadena, FilaConfig laConfiguracionDelaFila)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            Fila laFila = new Fila();
            try
            {
                laFila.laConfigDeFila = laConfiguracionDelaFila;
                laFila.DetalleCrudo = Cadena;

                //decodifica el Header.
                if (laConfiguracionDelaFila.PorSeparador)
                {
                    string[] losDato = Cadena.Split(laConfiguracionDelaFila.CaracterSeparacion.ToCharArray());

                    for (int k = 0; k < losDato.Length; k++)
                    {
                        laFila.losCampos.Add(k, losDato[k]);
                    }

                }
                else if (laConfiguracionDelaFila.PorLongitud)
                {

                    Int32 elNumeroCampo = 1;

                    while (Cadena.Length != 0)
                    {
                        try
                        {
                            CampoConfig unaConfig = laConfiguracionDelaFila.LosCampos[elNumeroCampo];

                            laFila.losCampos.Add(elNumeroCampo, Cadena.Substring(0, unaConfig.Longitud));

                            Cadena = Cadena.Substring(unaConfig.Longitud);

                            elNumeroCampo++;
                        }
                        catch (Exception err)
                        {
                            LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
                            throw new Exception("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                        }
                    }

                }


            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [DecodificaFila()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
             }

            return laFila;
        }

    }
}
