using CommonProcesador;
using CommonProcesador.Utilidades;
using Dnu.AutorizadorParabilia_NCliente.BaseDatos;
using Dnu.AutorizadorParabilia_NCliente.Entidades;
using Dnu.AutorizadorParabilia_NCliente.Utilities;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace Dnu.AutorizadorParabilia_NCliente.LogicaNegocio
{
    public class LNAsignacionSaldos
    {
        private string directorio;
        private string tipoColectiva;
        private const string expRegFecha = @"^\d{2}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private static DataTable dtContenidoFile;
        private static string directorioSalida;
        private static string connArchivosEfectivale = PNConfig.Get("ASIGNACIONSALDOS", "BDWriteProcesadorArchivosEfec");
        private static string connParabilia = PNConfig.Get("ASIGNACIONSALDOS", "BDReadAutorizador");
        private static string idEmpleadora;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "ASIGNACIONSALDOS";
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

        public LNAsignacionSaldos(string directorio, string tipoColectiva)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
        }

        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {
            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("ASIGNACIONSALDOS", "DirAltaEmp") + " el se recibio el archivo : " + e.FullPath);
            Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
            Thread.Sleep(1000 * 60 * 1);

            validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            Logueo.Error("[AsignacionSaldos] [OcurrioError] [El evento de fileWatcher no inicio correctamente] [Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }


        private void decodificarArchivo(string pPath, string pNomArchivo)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            try
            {

                Logueo.Evento("[AsignacionSaldos] Inicia Validacion del Archivo");
                if (validarContenido(InfoArchivo))
                {
                    if (insertaDatos(pNomArchivo))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        Logueo.Evento("[AsignacionSaldos] Archivo Decodificado Asignación de Saldos");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_").Replace("SD", "PROCESADO_CON_ERRORES_SD");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    }
                }
                else
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_").Replace("SD", "PROCESADO_CON_ERRORES_SD");
                    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_").Replace("SD", "PROCESADO_CON_ERRORES_SD");
                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                Logueo.Error("[AsignacionSaldos] [decodificarArchivo] [El proceso de validación del archivo:" + pNomArchivo + ex.Message + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        #region Validaciones

        [Transaction]
        public bool validarArchivos(bool validaCopiadoFiles)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("ValidarArchivos");
            if (validaCopiadoFiles)
            {
                Thread.Sleep(1000 * 60 * 1);
            }
            FileInfo archivo = null;
            List<string> existeArchivo = Directory.GetFiles(directorio, "SD*").ToList();
            if (existeArchivo.Count > 0)
            {
                try
                {
                    foreach (var dato in existeArchivo)
                    {
                        archivo = new FileInfo(dato);
                        if (validaConsecutivo(Path.GetFileNameWithoutExtension(archivo.FullName)))
                        {

                            if (validarNombreExtencion(archivo.FullName, ".txt"))
                            {
                                string pathInicial = directorio + "\\EN_PROCESO_" + archivo.Name;
                                moverArchivo(archivo.FullName, pathInicial);
                                decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName));
                            }
                            else
                            {
                                Logueo.Error("[AsignacionSaldos] [validarArchivos] [Archivo no corresponde a la extencion .txt o al formato de nombre] [LNAsignacionSaldos.cs: línea 109)]");
                                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_").Replace("SD", "PROCESADO_CON_ERRORES_SD");
                                moverArchivo(archivo.FullName, pathArchivosInvalidos);
                                return false;
                            }
                        }
                        else
                        {
                            Logueo.Error("[AsignacionSaldos] [ValidaConsecutivo] [El archivo " + Path.GetFileNameWithoutExtension(archivo.FullName) +
                                " no cumple con el consecutivo] [\\LNAsignacionSaldos.cs: línea 106]");
                            string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("SD", "PROCESADO_CON_ERRORES_SD");
                            moverArchivo(archivo.FullName, pathArchivosInvalidos);
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logueo.Error("[AsignacionSaldos] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_").Replace("SD", "PROCESADO_CON_ERRORES_SD");
                    moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    ApmNoticeWrapper.NoticeException(ex);
                    return false;
                }
            }
            return false;
        }

        private bool validaConsecutivo(string nomArchivo)
        {
            int consecutivoBD;
            DataRow drConsecutivo;
            Hashtable ht = new Hashtable();
            ht.Add("@select", 1);
            drConsecutivo = BDsps.EjecutarSP("procnoc_travel_SUConsecutivo", ht, connArchivosEfectivale).Tables["Table"].Rows[0];
            consecutivoBD = Convert.ToInt32(drConsecutivo["Consecutivo"].ToString());
            if (Convert.ToDateTime(drConsecutivo["FechaConsecutivo"]) == DateTime.Now.Date)
            {
                return actualizaConsecutivo(nomArchivo, consecutivoBD);
            }
            else
            {
                Hashtable htConsecutivo = new Hashtable();
                htConsecutivo.Add("@consecutivo", 1);
                htConsecutivo.Add("@tipo", 0);
                htConsecutivo.Add("@fecha", DateTime.Now.Date.ToString("yyyy/MM/dd"));
                htConsecutivo.Add("@select", 1);
                drConsecutivo = BDsps.EjecutarSP("procnoc_travel_SUConsecutivo", htConsecutivo, connArchivosEfectivale).Tables["Table"].Rows[0];
                consecutivoBD = Convert.ToInt32(drConsecutivo["Consecutivo"].ToString());
                return actualizaConsecutivo(nomArchivo, consecutivoBD);
            }
        }

        private bool actualizaConsecutivo(string nomArchivo, int consecutivoBD)
        {
            if (Convert.ToInt32(nomArchivo.Substring(8)) == consecutivoBD)
            {
                Hashtable htCon = new Hashtable();
                if (consecutivoBD == 6)
                    consecutivoBD = 1;
                else
                    consecutivoBD += 1;
                htCon.Add("@consecutivo", consecutivoBD);
                htCon.Add("@tipo", 1);
                BDsps.EjecutarSP("procnoc_travel_SUConsecutivo", htCon, connArchivosEfectivale);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool validarNombreExtencion(string DirArchivo, string ExtensionArchivo)
        {
            bool result = false;
            string extension = Path.GetExtension(DirArchivo);
            string nomArchivo = Path.GetFileNameWithoutExtension(DirArchivo);
            Regex rgx = new Regex(expRegFecha);
            if (ExtensionArchivo.Equals(extension) && rgx.IsMatch(nomArchivo.Substring(2, 6)))
                result = true;
            return result;
        }

        private bool validarContenido(FileInfo pInfoArchivo)
        {
            int counter = 0, contadorAbonosTotales = 0, contadorAbonos = 0;
            string line, fechaEnvio = null, claveEmp = null;
            double importeAbonos = 0, importeTotalAbonos = 0;

            bool resultHeaderA = false;
            bool resultHeaderB = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;
            bool resultTrailerB = false;

            dtContenidoFile = crearDataTable();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 24)
                {
                    Logueo.Error("[AsignacionSaldos] [ValidarContenido] [La línea numero: " + (counter + 1) + " no tinen los 24] [\\LNAsignacionSaldos.cs: línea 211]");
                    file.Close();
                    return false;
                }
                if (line.StartsWith("15"))
                {
                    resultHeaderA = validaHeaderA(line);
                    fechaEnvio = line.Substring(9, 8);
                    if (!resultHeaderA) { file.Close(); return false; }
                }
                if (line.StartsWith("16"))
                {
                    resultHeaderB = validaHeaderB(line);
                    claveEmp = line.Substring(2, 11);
                    if (!resultHeaderB) { file.Close(); return false; }
                }
                if (line.StartsWith("23"))
                {
                    resultDetalle = validarDetalle(line, counter, claveEmp, fechaEnvio);
                    contadorAbonos += 1;
                    contadorAbonosTotales += 1;
                    double importe = Convert.ToDouble(line.Substring(15, 7) + "." + line.Substring(22, 2));
                    importeAbonos = importe + importeAbonos;
                    importeTotalAbonos = importe + importeTotalAbonos;
                    if (!resultDetalle) { file.Close(); return false; }
                }
                if (line.StartsWith("95"))
                {
                    resultTrailerB = validarTrailerB(line, contadorAbonos, importeAbonos);
                    contadorAbonos = 0;
                    importeAbonos = 0;
                    if (!resultTrailerB) { file.Close(); return false; }
                }
                if (line.StartsWith("96"))
                {
                    resultTrailerA = validarTrailerA(line, contadorAbonosTotales, importeTotalAbonos);
                    contadorAbonosTotales = 0;
                    importeTotalAbonos = 0;
                    if (!resultTrailerA) { file.Close(); return false; }
                }
                counter++;
            }
            file.Close();

            return true;
        }

        private bool validaHeaderA(string pLine)
        {
            string identificacionReg = pLine.Substring(2, 7);
            string fechaEnvio = pLine.Substring(9, 8);
            string filler = pLine.Substring(17, 7);

            Regex rgxAlfaNumerico7 = new Regex(@"[\w+\s*]{7}");
            Regex rgxFecha = new Regex(expRegFecha2);
            Regex rgxFiller = new Regex("0{7}");

            if (!rgxAlfaNumerico7.IsMatch(identificacionReg))
            {
                Logueo.Error("[AsignacionSaldos] [validaHeaderA] [hay un error en el Header A en el campo: " + identificacionReg + "] [\\LNAsignacionSaldos.cs:línea 270]");
                return false;
            }
            if (!rgxFecha.IsMatch(fechaEnvio))
            {
                Logueo.Error("[AsignacionSaldos][validaHeaderA] [hay un error en el Header A en el campo: " + fechaEnvio + "] [\\LNAsignacionSaldos.cs:línea 275]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[AsignacionSaldos] [validaHeaderA] [hay un error en el Header A en el campo: " + filler + "] [\\LNAsignacionSaldos.cs:línea 280]");
                return false;
            }

            return true;
        }

        private bool validaHeaderB(string pLine)
        {
            bool result = false;
            string claveEmpleadora = pLine.Substring(2, 11);
            string filler = pLine.Substring(13, 11);

            Hashtable ht = new Hashtable();

            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{11}");
            Regex rgxFiller = new Regex("0{11}");

            if (rgxAlfaNumerico10.IsMatch(claveEmpleadora)
                && rgxFiller.IsMatch(filler))
            {
                ht.Add("@claveEmpl", claveEmpleadora.TrimEnd(' '));
                ht.Add("@tipoColectiva", this.tipoColectiva);
                DataTable dtEmpleadora = BDsps.EjecutarSP("procnoc_travel_ObtieneEmpleadora", ht, connParabilia).Tables["Table"];
                if (dtEmpleadora.Rows.Count > 0)
                {
                    idEmpleadora = dtEmpleadora.Rows[0]["ID_Colectiva"].ToString();
                    result = true;
                }
                else
                {
                    Logueo.Error("[AsignacionSaldos] [validaHeaderB] [La empleadora con clave " + claveEmpleadora + " no se encuentra dada de Alta en Asignacion de Saldos] [\\LNAsignacionSaldos.cs:línea 234]");
                    result = false;
                }
                result = true;
            }
            else
            {
                Logueo.Error("[AsignacionSaldos] [validaHeaderB] [hay un error en el Header B en Asignacion Saldos] [\\LNASignacionSaldos.cs:línea 242");
            }

            return result;
        }

        private bool validarDetalle(string pLine, int pNumLinea, string pClaveEmp, string pFechaEnvio)
        {
            string numCuenta = pLine.Substring(2, 13);
            string importeAbono = pLine.Substring(15, 9);

            Regex rgxNumCuenta = new Regex(@"[\w+\s*]{13}");
            Regex rgxAbono = new Regex(@"\d{9}");

            if (!rgxNumCuenta.IsMatch(numCuenta))
            {
                Logueo.Error("[AsignacionSaldos] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " en el campo número de cuenta] [\\LNASignacionSaldos.cs:línea 333]");
                return false;
            }
            if (!rgxAbono.IsMatch(importeAbono))
            {
                Logueo.Error("[AsignacionSaldos] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " en el campo Importe Abono] [\\LNASignacionSaldos.cs:línea 338]");
                return false;
            }
            double importe = Convert.ToDouble(pLine.Substring(15, 7) + "." + pLine.Substring(22, 2));
            dtContenidoFile.Rows.Add(new Object[] { pFechaEnvio, pClaveEmp
                            , SeguridadCifrado.cifrar(numCuenta.Trim()), importe
                                    ,"1", "0"});

            return true;
        }

        private bool validarTrailerB(string pLine, int pContadorAbonos, double pImporte)
        {
            int numAbonos = Convert.ToInt32(pLine.Substring(2, 6));
            double importeAbonos = Convert.ToDouble(pLine.Substring(8, 9) + "." + pLine.Substring(17, 2));
            string filler = pLine.Substring(19, 5);

            Regex rgxFiller = new Regex("0{5}");

            if (!(numAbonos == pContadorAbonos))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerB] [hay un error en el Trailer B no corresponde el numero de Abonos:" + pContadorAbonos + "] [\\LNASignacionSaldos.cs:línea 359]");
                return false;
            }
            if (!(importeAbonos == pImporte))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerB] [hay un error en el Trailer B no corresponde el importe de Abonos:" + pImporte + "] [\\LNASignacionSaldos.cs:línea 364]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerB] [hay un error en el Trailer B en el filler:" + filler + "] [\\LNASignacionSaldos.cs:línea 369]");
                return false;
            }

            return true;
        }

        private bool validarTrailerA(string pLine, int pNumTotAbonos, double pImporteTotAbonos)
        {
            int numTotAbonos = Convert.ToInt32(pLine.Substring(2, 6));
            double importeTotAbonos = Convert.ToDouble(pLine.Substring(8, 9) + "." + pLine.Substring(17, 2));
            string filler = pLine.Substring(19, 5);

            Regex rgxFiller = new Regex("0{5}");

            if (!(numTotAbonos == pNumTotAbonos))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerA] [hay un error en el Trailer A no corresponde el número total de Abonos] [\\LNASignacionSaldos.cs:línea 387]");
                return false;
            }
            if (!(importeTotAbonos == pImporteTotAbonos))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerA] [hay un error en el Trailer A no corresponde el Importe Total de Abonos] [\\LNASignacionSaldos.cs:línea 392]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[AsignacionSaldos] [validarTrailerA] [hay un error en el Trailer A no corresponde el filler:" + filler + "] [\\LNASignacionSaldos.cs:línea 397]");
                return false;
            }

            return true;
        }

        #endregion

        private bool insertaDatos(string pNomArchivo)
        {
            bool result = false;
            Hashtable htFile = new Hashtable();
            htFile.Add("@descripcion", "Archivo de Asignacion de Saldos");
            htFile.Add("@claveProceso", "ASIGNACIONSALDOS");
            htFile.Add("@nombre", pNomArchivo);
            htFile.Add("@tipoArchivo", ".txt");
            string idFile = BDsps.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, connArchivosEfectivale).Tables["Table"].Rows[0]["ID_Archivo"].ToString();
            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);
            ht.Add("@tblContent", dtContenidoFile);
            DataTable tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoAsigSaldos", ht, connArchivosEfectivale).Tables["Table"];
            if (tmpDatosInsert != null)
            {
                if (tmpDatosInsert.Rows.Count == 0)
                {
                    Logueo.Error("[AsignacionSaldos] [insertaDatos] [No se inserto Ningun Registro, debido a que ya se encnotraban procesados] [\\LNAsignacionSaldos.cs:línea 425]");
                    return false;
                }
                result = enviarDatosParabilia(tmpDatosInsert, idFile);
            }

            return result;
        }

        private bool enviarDatosParabilia(DataTable dtDatosProcesar, string pIdArchivoEntrada)
        {

            SqlConnection connection = new SqlConnection(connParabilia);
            try
            {
                connection.Open();
                for (int contador = 0; contador < dtDatosProcesar.Rows.Count; contador++)
                {
                    var row = dtDatosProcesar.Rows[contador];
                    try
                    {
                        DataTable datosCuenta = BDsps.ObtenerDatosCuenta(row["NumeroCuenta"].ToString(), connection).Tables["Table"];
                        var rowDatosCuenta = datosCuenta.Rows[0];
                        using (SqlTransaction transaccionSQL = connection.BeginTransaction())
                        {
                            try
                            {
                                Bonificacion abono = new Bonificacion()
                                {
                                    Cuenta = SeguridadCifrado.descifrar(row["NumeroCuenta"].ToString()),
                                    ClaveColectiva = rowDatosCuenta["ClaveColectiva"].ToString(),
                                    ClaveEvento = rowDatosCuenta["ClaveEvento"].ToString(),
                                    Concepto = "Dispersión de Fondos",
                                    IdColectiva = Convert.ToInt32(rowDatosCuenta["ID_Colectiva"]),
                                    IdEmisor = Convert.ToInt32(rowDatosCuenta["ID_Emisor"]),
                                    IdEvento = Convert.ToInt32(rowDatosCuenta["ID_Evento"]),
                                    IdTipoColectiva = Convert.ToInt32(rowDatosCuenta["ID_TipoColectiva"]),
                                    Importe = row["ImporteAbono"].ToString(),
                                    Observaciones = "",
                                    RefNumerica = 0,
                                    SaldoTotal = 0
                                };
                                Poliza laPoliza = null;

                                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();
                                losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                                    (abono.ClaveColectiva, abono.Cuenta, abono.ClaveEvento, "");

                                losParametros["@ID_CuentaHabiente"] = new Parametro()
                                {
                                    Nombre = "@ID_CuentaHabiente",
                                    Valor = abono.IdColectiva.ToString(),
                                    Descripcion = "ID CuentaHabiente",
                                    ID_TipoColectiva = abono.IdTipoColectiva
                                };
                                losParametros["@ID_Emisor"] = new Parametro()
                                {
                                    Nombre = "@ID_Emisor",
                                    Valor = PNConfig.Get("ASIGNACIONSALDOS", "ValorEmisor"),
                                    Descripcion = "ID Emisor",
                                    ID_TipoColectiva = Convert.ToInt32(PNConfig.Get("ASIGNACIONSALDOS", "EmisorTipoColectiva"))
                                };
                                losParametros["@Importe"] = new Parametro()
                                {
                                    Nombre = "@Importe",
                                    Valor = abono.Importe,
                                    Descripcion = "Importe"
                                };
                                losParametros["@MedioAcceso"] = new Parametro()
                                {
                                    Nombre = "@MedioAcceso",
                                    Valor = abono.Cuenta,
                                    ID_TipoColectiva = abono.IdTipoColectiva

                                };

                                losParametros["@TipoMedioAcceso"] = new Parametro()
                                {
                                    Nombre = "@TipoMedioAcceso",
                                    Valor = "NOCTA"

                                };

                                losParametros["@DescEvento"] = new Parametro()
                                {
                                    Nombre = "@DescEvento",
                                    Valor = abono.Concepto

                                };

                                //Genera y Aplica la Poliza
                                Executer.EventoManual aplicador = new Executer.EventoManual(abono.IdEvento,
                                    abono.Concepto, false, Convert.ToInt64(abono.RefNumerica), losParametros, abono.Observaciones, connection, transaccionSQL);
                                laPoliza = aplicador.AplicaContablilidad();

                                if (laPoliza.CodigoRespuesta != 0)
                                {
                                    actualizaEstatusParabilia(laPoliza.CodigoRespuesta, row["ID_ArchivoDetalleAsigSaldos"].ToString());
                                    transaccionSQL.Rollback();
                                    Logueo.Error("[AsignacionSaldos] [enviarDatosParabilia] [No se generó la Póliza; CODIGO:" + laPoliza.CodigoRespuesta + "; RESPUESTA:" + laPoliza.DescripcionRespuesta + "] [\\LNASignacionSaldos.cs:línea 514]");
                                }
                                else
                                {
                                    actualizaEstatusParabilia(laPoliza.CodigoRespuesta, row["ID_ArchivoDetalleAsigSaldos"].ToString());
                                    transaccionSQL.Commit();
                                    Logueo.Evento("[AsignacionSaldos] Se genero Poliza para la cuenta: " + abono.Cuenta);
                                }
                            }

                            catch (Exception err)
                            {
                                actualizaEstatusParabilia(99, row["ID_ArchivoDetalleAsigSaldos"].ToString());
                                transaccionSQL.Rollback();
                                Logueo.Error("[AsignacionSaldos] [enviarDatosParabilia] [Ocurrio un error al generar la poliza] [Mensaje: " + err.Message + " TRACE: " + err.StackTrace + "]");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        actualizaEstatusParabilia(98, row["ID_ArchivoDetalleAsigSaldos"].ToString());
                        Logueo.Error("[AsignacionSaldos] [enviarDatosParabilia] [El numero de Cuenta no se encuentra registrado para el archivo con Id: " + pIdArchivoEntrada + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                }
                return crearArchivoSalida(pIdArchivoEntrada);
            }
            catch (Exception ex)
            {
                Logueo.Error("[AsignacionSaldos] [enviarDatosParabilia] [Error al guardar datos parabilia para el archivo con ID: " + pIdArchivoEntrada + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        private void actualizaEstatusParabilia(int pCodigo, string pIdArchivoDetalle)
        {
            try
            {
                if (pCodigo == 0)
                {
                    actualizaRegsSaldos(pCodigo, pIdArchivoDetalle, "0");
                }
                else
                {
                    actualizaRegsSaldos(pCodigo, pIdArchivoDetalle, "1");
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[AsignacionSaldos] [actualizaEstatusParabilia] [Error al actualizar el estatus Parabilia para el detalle con ID: " + pIdArchivoDetalle + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        public void actualizaRegsSaldos(int codRespuesta, string pIdArchivoDetalle, string pIdentificador)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleAsigSaldos", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            htProcesar.Add("@codigoRespuesta", codRespuesta);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesAsignacionSaldos", htProcesar, connArchivosEfectivale);
        }

        private bool crearArchivoSalida(string pIdArchivoEntrada)
        {
            try

            {
                Hashtable ht = new Hashtable();
                ht.Add("@IdArchivo", pIdArchivoEntrada);
                DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaAsigSaldos", ht, connArchivosEfectivale).Tables["Table"];
                string nombreArchivoSalidaTmp = datosSalida.Rows[0]["Nombre"].ToString().Replace("SD", "RS") + ".txt";
                string pathArchivoSalida = directorioSalida + "\\" + nombreArchivoSalidaTmp.Substring(0, 8) + nombreArchivoSalidaTmp.Substring(8).Replace("0", "V");
                StreamWriter streamSalida = File.CreateText(pathArchivoSalida);
                streamSalida.WriteLine("17" + "SAL_RES" + datosSalida.Rows[0]["FechaEnvio"].ToString() + "".PadRight(19, '0'));

                string tmpClaveEmpleadora = datosSalida.Rows[0]["ClaveEmpleadora"].ToString();
                int numRechazosEmpleadora = 0, numAbonosEmpleadora = 0, numTotRechazos = 0, numTotAbonos = 0;
                Double importeRechazosEmpleadora = 0, importeAbonosEmpleadora = 0, importeTotRechazos = 0, importeTotAbonos = 0;
                streamSalida.WriteLine("18" + tmpClaveEmpleadora.PadRight(11) + "".PadRight(23, '0'));
                for (int contador = 0; contador < datosSalida.Rows.Count; contador++)
                {
                    var row = datosSalida.Rows[contador];
                    if (tmpClaveEmpleadora != row["ClaveEmpleadora"].ToString())
                    {
                        tmpClaveEmpleadora = row["ClaveEmpleadora"].ToString();
                        streamSalida.WriteLine("97" + numAbonosEmpleadora.ToString().PadLeft(6, '0') +
                                    importeAbonosEmpleadora.ToString().Replace(".", "").PadLeft(11, '0') +
                                    numRechazosEmpleadora.ToString().PadLeft(6, '0') +
                                    importeRechazosEmpleadora.ToString().Replace(".", "").PadLeft(11, '0'));
                        streamSalida.WriteLine("18" + tmpClaveEmpleadora.PadRight(11) + "".PadRight(23, '0'));
                        numAbonosEmpleadora = 0;
                        numRechazosEmpleadora = 0;
                        importeRechazosEmpleadora = 0;
                        importeAbonosEmpleadora = 0;
                    }
                    if (row["ID_EstatusParabilia"].ToString() == "2")
                    {
                        numAbonosEmpleadora += 1;
                        numTotAbonos += 1;
                        importeAbonosEmpleadora = importeAbonosEmpleadora + Convert.ToDouble(row["ImporteAbono"]);
                        importeTotAbonos = importeTotAbonos + Convert.ToDouble(row["ImporteAbono"]);
                    }
                    else
                    {
                        numRechazosEmpleadora += 1;
                        numTotRechazos += 1;
                        importeRechazosEmpleadora = importeRechazosEmpleadora + Convert.ToDouble(row["ImporteAbono"]);
                        importeTotRechazos = importeTotRechazos + Convert.ToDouble(row["ImporteAbono"]);
                    }
                    streamSalida.WriteLine("24" + SeguridadCifrado.descifrar(row["NumeroCuenta"].ToString()).PadLeft(13) + row["ImporteAbono"].ToString().Replace(".", "").PadLeft(9, '0') + row["CodigoRP"].ToString().PadLeft(4, '0') + "".PadRight(8, '0'));
                    procesoEnvioDeCorreo(SeguridadCifrado.descifrar(row["NumeroCuenta"].ToString()), Convert.ToDouble(row["ImporteAbono"]));
                }
                streamSalida.WriteLine("97" + numAbonosEmpleadora.ToString().PadLeft(6, '0') +
                                    importeAbonosEmpleadora.ToString().Replace(".", "").PadLeft(11, '0') +
                                    numRechazosEmpleadora.ToString().PadLeft(6, '0') +
                                    importeRechazosEmpleadora.ToString().Replace(".", "").PadLeft(11, '0'));
                streamSalida.WriteLine("98" + numTotAbonos.ToString().PadLeft(6, '0') +
                                    importeTotAbonos.ToString().Replace(".", "").PadLeft(11, '0') +
                                    numTotRechazos.ToString().PadLeft(6, '0') +
                                    importeTotRechazos.ToString().Replace(".", "").PadLeft(11, '0'));
                streamSalida.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[AsignacionSaldos] [crearArchivoSalida] [Error al crear el archivo de salida con Identificador: " + pIdArchivoEntrada + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        private void procesoEnvioDeCorreo(string pNumCuenta, double PImporte)
        {
            Hashtable ht = new Hashtable();
            ht.Add("@numCuenta", pNumCuenta);
            try
            {
                string correo = BDsps.EjecutarSP("procnoc_travel_ObtenerMail", ht, connParabilia).Tables["Table"].Rows[0]["Email"].ToString();

                enviarCorreoSaldo(correo.Trim(), PImporte);
            }
            catch (Exception exp)
            {
                Logueo.Error("[AsignacionSaldos] [procesoEnvioDeCorreo] [Error al obtener el correo para cuenta: ****" + pNumCuenta.Substring((pNumCuenta.Length - 3), 3) + "] [Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
            }
        }

        private void enviarCorreoSaldo(string correo, Double pImporte)
        {
            Correo datoCorreo = new Correo()
            {
                body = construirBody(correo, pImporte.ToString("C2")),
                subject = PNConfig.Get("ASIGNACIONSALDOS", "SubjectMail"),
                email = correo
            };
            RWSMail rWSmail = WebService.WSEnviaCorreoSaldo(datoCorreo);
        }

        private string construirBody(string correo, string pImporte)
        {
            try
            {
                string fileName = PNConfig.Get("ASIGNACIONSALDOS", "DirectorioPlantilla");

                var template = File.ReadAllText(fileName);
                template = template.Replace("[NOMBREEMPRESA]", PNConfig.Get("ASIGNACIONSALDOS", "NomEmpresa"));
                template = template.Replace("[IMPORTE]", pImporte);
                return template;
            }
            catch (Exception exp)
            {
                Logueo.Error("[AsignacionSaldos] [construirBody] [El archivo html para el cuerpo del correo:" + correo + " no se encuentra] [Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return null;
            }

        }

        private DataTable crearDataTable()
        {
            DataTable dtDatosnew = new DataTable("DetalleAsignacionSaldos");
            var dc1 = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc3 = new DataColumn("NumeroCuenta", Type.GetType("System.String"));
            var dc4 = new DataColumn("ImporteAbono", Type.GetType("System.String"));
            var dc5 = new DataColumn("ID_EstatusParabilia", Type.GetType("System.String"));
            var dc6 = new DataColumn("ReintentosParabilia", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);

            return dtDatosnew;
        }

        #region OperacionesFile

        public void crearDirectorio()
        {
            directorioSalida = PNConfig.Get("ASIGNACIONSALDOS", "PathSalida");
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);
            if (!Directory.Exists(directorio + "\\Procesados"))
                Directory.CreateDirectory(directorio + "\\Procesados");
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorio + "\\Erroneos"))
                Directory.CreateDirectory(directorio + "\\Erroneos");
        }

        private void moverArchivo(string pathOrigen, string pathDestino)
        {

            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
            }
            else
            {
                int numero = 0;
                bool validador = true;
                while (validador)
                {
                    if (numero == 0)
                    {
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                        numero = numero + 1;
                    }
                    else
                    {
                        numero = numero + 1;
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia (" + numero + ")." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                    }
                }
            }
        }


        private bool renombrarArchivo(string pathOrigen, string pathDestino)
        {
            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
