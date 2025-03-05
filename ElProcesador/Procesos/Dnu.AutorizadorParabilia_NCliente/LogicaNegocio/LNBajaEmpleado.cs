using CommonProcesador;
using CommonProcesador.Utilidades;
using Dnu.AutorizadorParabilia_NCliente.BaseDatos;
using Dnu.AutorizadorParabilia_NCliente.Entidades;
using Dnu.AutorizadorParabilia_NCliente.Utilities;
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
    public class LNBajaEmpleado
    {
        private string directorio;
        private string tipoColectiva;
        private const string expRegFecha = @"^\d{2}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private static DataTable dtContenidoFile;
        private static string directorioSalida;
        private static string connArchivosEfectivale = PNConfig.Get("BAJAEMPLEADO", "BDWriteProcesadorArchivosEfec");
        private static string idEmpleadora;
        private static string nomArchivoProcesarBajas;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "BAJAEMPLEADO";
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

        public LNBajaEmpleado(string directorio, string tipoColectiva)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
        }

        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {


            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("BAJAEMPLEADO", "DirAltaEmp") + " el se recibio el archivo : " + e.FullPath);
            Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
            Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath);

            validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            Logueo.Error("[BajaEmpleado] [OcurrioError] [El evento de fileWatcher no inicio correctamente] [Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }


        private void decodificarArchivo(string pPath, string pNomArchivo)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            try
            {
                Logueo.Evento("[BajaEmpleado] Inicia Validacion del Archivo: " + pPath);
                if (validarContenido(InfoArchivo))
                {
                    if (insertaDatosBajas(pNomArchivo))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        Logueo.Evento("[BajaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        Logueo.Evento("[BajaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                    }
                }
                else
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[BajaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                Logueo.Error("[BajaEmpleado] [decodificarArchivo] [Ocurrio un error en la validación del contenido del archivo Baja Empleado] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                Logueo.Evento("[BajaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
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
            List<string> existeArchivo = Directory.GetFiles(directorio, "CP*").ToList();
            if (existeArchivo.Count > 0)
            {
                try
                {
                    foreach (var dato in existeArchivo)
                    {
                        archivo = new FileInfo(dato);
                        nomArchivoProcesarBajas = archivo.FullName;
                        if (validarNombreExtencion(archivo.FullName, ".txt"))
                        {
                            string pathInicial = directorio + "\\EN_PROCESO_" + archivo.Name;
                            moverArchivo(archivo.FullName, pathInicial);
                            decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName));
                        }
                        else
                        {
                            Logueo.Error("[BajaEmpleado] [validarArchivos] [Archivo no corresponde a la extencion .txt en Baja Empleado] [LNBajaEmpleado.cs:línea 108]");
                            string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                            moverArchivo(archivo.FullName, pathArchivosInvalidos);
                            Logueo.Evento("[BajaEmpleado] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logueo.Error("[BajaEmpleado] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[BajaEmpleado] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                    ApmNoticeWrapper.NoticeException(ex);
                    return false;
                }
            }
            return false;
        }
        private bool validarNombreExtencion(string DirArchivo, string ExtensionArchivo)
        {
            bool result = false;
            string extension = Path.GetExtension(DirArchivo);
            string nomArchivo = Path.GetFileNameWithoutExtension(DirArchivo);
            Regex rgx = new Regex(expRegFecha);
            if (ExtensionArchivo.Equals(extension) && rgx.IsMatch(nomArchivo.Substring(2)))
                result = true;
            return result;
        }

        private bool validarContenido(FileInfo pInfoArchivo)
        {
            int counter = 0, contadorAltasTotales = 0, contadorEmpleadoras = 0, contadorAltas = 0;
            string line, fechaEnvio = null, claveEmp = null;

            bool resultHeaderA = false;
            bool resultHeaderB = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;
            bool resultTrailerB = false;

            dtContenidoFile = crearDataTable();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 56)
                {
                    Logueo.Error("[BajaEmpleado] [ValidarContenido] [La línea numero: " + (counter + 1) + " no tinene los 56] [\\LNBajaEmpleado.cs: línea 156]");
                    file.Close();
                    return false;
                }
                if (line.StartsWith("11"))
                {
                    resultHeaderA = validaHeaderA(line);
                    fechaEnvio = line.Substring(9, 8);
                    if (!resultHeaderA) { file.Close(); return false; }
                }
                if (line.StartsWith("12"))
                {
                    resultHeaderB = validaHeaderB(line);
                    claveEmp = line.Substring(2, 11);
                    contadorEmpleadoras += 1;
                    if (!resultHeaderB) { file.Close(); return false; }
                }
                if (line.StartsWith("21"))
                {
                    resultDetalle = validarDetalle(line, counter, claveEmp, fechaEnvio);
                    contadorAltasTotales += 1;
                    contadorAltas += 1;
                    if (!resultDetalle) { file.Close(); return false; }
                }
                if (line.StartsWith("91"))
                {
                    resultTrailerB = validarTrailerB(line, contadorAltas);
                    contadorAltas = 0;
                    if (!resultTrailerB) { file.Close(); return false; }
                }
                if (line.StartsWith("92"))
                {
                    resultTrailerA = validarTrailerA(line, contadorAltasTotales, contadorEmpleadoras);
                    contadorAltasTotales = 0;
                    contadorEmpleadoras = 0;
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
            string filler = pLine.Substring(17, 39);

            Regex rgxAlfaNumerico7 = new Regex(@"\w{7}");
            Regex rgxFecha = new Regex(expRegFecha2);
            Regex rgxFiller = new Regex("0{39}");

            if (!rgxAlfaNumerico7.IsMatch(identificacionReg))
            {
                Logueo.Error("[BajaEmpleado] [validaHeaderA] [hay un error en el Header A en el campo: " + identificacionReg + "] [\\LNBajaEmpleado.cs:línea 212]");
                return false;
            }
            if (!rgxFecha.IsMatch(fechaEnvio))
            {
                Logueo.Error("[BajaEmpleado] [validaHeaderA] [hay un error en el Header A en el campo: " + fechaEnvio + "] [\\LNBajaEmpleado.cs:línea 217]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[BajaEmpleado] [validaHeaderA] [hay un error en el Header A en el campo: " + filler + "] [\\LNBajaEmpleado.cs:línea 222]");
                return false;
            }
            return true;
        }

        private bool validaHeaderB(string pLine)
        {
            bool result = false;
            string claveEmpleadora = pLine.Substring(2, 11);
            string filler = pLine.Substring(13, 43);

            Hashtable ht = new Hashtable();

            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{11}");
            Regex rgxFiller = new Regex("0{43}");

            if (rgxAlfaNumerico10.IsMatch(claveEmpleadora)
                && rgxFiller.IsMatch(filler))
            {
                result = true;
                ht.Add("@claveEmpl", claveEmpleadora.TrimEnd(' '));
                ht.Add("@tipoColectiva", this.tipoColectiva);
                DataTable dtEmpleadora = BDsps.EjecutarSP("procnoc_travel_ObtieneEmpleadora", ht, PNConfig.Get("BAJAEMPLEADO", "BDReadAutorizador")).Tables["Table"];
                if (dtEmpleadora.Rows.Count > 0)
                {
                    idEmpleadora = dtEmpleadora.Rows[0]["ID_Colectiva"].ToString();
                    result = true;
                }
                else
                {
                    Logueo.Error("[BajaEmpleado] [validaHeaderB] [No se encuentra dada de alta la Emmpleadora: " + claveEmpleadora + "] [\\LNBajaaEmpleado.cs:línea 247]");
                    result = false;
                }
            }
            else
            {
                Logueo.Error("[BajaEmpleado] [validaHeaderB] [hay un error en el Header B con empleadora: " + claveEmpleadora + "] [\\LNBajaEmpleado.cs:línea 238]");
            }

            return result;
        }

        private bool validarDetalle(string pLine, int pNumLinea, string pClaveEmp, string pFechaEnvio)
        {
            string claveTransaccion = pLine.Substring(2, 2);
            string regEmpleado = pLine.Substring(4, 10);
            string nombre = pLine.Substring(14, 26);
            string numTarjeta = pLine.Substring(40, 16);
            Hashtable ht = new Hashtable();

            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{10}");
            Regex rgxNombre = new Regex(@"[a-zA-Z]+(\s+[a-zA-Z]+)+");
            Regex rgxNumTarjeta = new Regex(@"\d{16}");

            if (!claveTransaccion.Equals("02"))
            {
                Logueo.Error("[BajaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " la clave transacción no corresponde] [\\LNBajaEmpleado.cs:línea 279]");
                return false;
            }
            if (!rgxAlfaNumerico10.IsMatch(regEmpleado))
            {
                Logueo.Error("[BajaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " en el número de empleado: " + regEmpleado + "] [\\LNBajaEmpleado.cs:línea 279]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                Logueo.Error("[BajaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " en el nombre: " + nombre + "] [\\LNBajaEmpleado.cs:línea 279]");
                return false;
            }
            if (!rgxNumTarjeta.IsMatch(numTarjeta))
            {
                Logueo.Error("[BajaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " en el Número de Tarjeta] [\\LNBajaEmpleado.cs:línea 279]");
                return false;
            }
            dtContenidoFile.Rows.Add(new Object[] { pFechaEnvio, pClaveEmp, claveTransaccion
                                    , regEmpleado, nombre, SeguridadCifrado.cifrar(numTarjeta)
                                    , "1", "1", "0", "0"});
            return true;
        }

        private bool validarTrailerB(string pLine, int pContadorBajas)
        {
            int numBajas = Convert.ToInt32(pLine.Substring(2, 5));
            string filler = pLine.Substring(7, 49);

            Regex rgxFiller = new Regex("0{49}");

            if (!(numBajas == pContadorBajas))
            {
                Logueo.Error("[BajaEmpleado] [validarTrailerB] [hay un error en el Trailer B no corresponde el numero de Bajas por empleadora:" + numBajas + "] [\\LNBajaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[BajaEmpleado] [validarTrailerB] [hay un error en el Trailer B en el filler:" + filler + "] [\\LNBajaEmpleado.cs:línea 317]");
                return false;
            }

            return true;
        }

        private bool validarTrailerA(string pLine, int pContadorBajas, int pContadorEmpleadoras)
        {
            int numEmpleadoras = Convert.ToInt32(pLine.Substring(2, 4));
            int numMovimientos = Convert.ToInt32(pLine.Substring(6, 5));
            string filler = pLine.Substring(11, 45);

            Regex rgxFiller = new Regex("0{45}");

            if (!(numEmpleadoras == pContadorEmpleadoras))
            {
                Logueo.Error("[BajaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el número de Empleadoras:" + numEmpleadoras + "] [\\LNBajaEmpleado.cs:línea 334]");
                return false;
            }
            if (!(numMovimientos == pContadorBajas))
            {
                Logueo.Error("[BajaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el número de Bajas:" + numMovimientos + "] [\\LNBajaEmpleado.cs:línea 339]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[BajaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el filler:" + filler + "] [\\LNBajaEmpleado.cs:línea 344]");
                return false;
            }

            return true;
        }

        #endregion

        private bool insertaDatosBajas(string pNomArchivo)
        {
            bool result = false;
            Hashtable htFile = new Hashtable();
            htFile.Add("@descripcion", "Archivo de Baja Empleados");
            htFile.Add("@claveProceso", "BAJAEMPLEADO");
            htFile.Add("@nombre", pNomArchivo);
            htFile.Add("@tipoArchivo", ".txt");
            string idFile = BDsps.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, connArchivosEfectivale).Tables["Table"].Rows[0]["ID_Archivo"].ToString();
            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);
            ht.Add("@tblContent", dtContenidoFile);
            DataTable tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoBajas", ht, connArchivosEfectivale).Tables["Table"];
            if (tmpDatosInsert != null)
            {
                if (tmpDatosInsert.Rows.Count == 0)
                {
                    return enviarDatosParabiliaBajas(idFile);
                }
                result = enviaDatosEvertecBajas(tmpDatosInsert, idFile);
            }

            return result;
        }

        private bool enviaDatosEvertecBajas(DataTable pDtContenidoFile, string pIdArchivo)
        {
            SqlConnection connection = new SqlConnection(connArchivosEfectivale);
            try
            {
                connection.Open();
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {

                    var row = pDtContenidoFile.Rows[contador];

                    if (row["ID_EstatusEvertec"].ToString() != "2")
                    {
                        EntradaWSEvertecBaja datosEntrada = new EntradaWSEvertecBaja()
                        {
                            numeroTarjeta = SeguridadCifrado.descifrar(row["NumeroTarjeta"].ToString()),
                            estadoDeseado = PNConfig.Get("BAJAEMPLEADO", "EdoDeseado"),
                            motivoCancelacion = PNConfig.Get("BAJAEMPLEADO", "MotCancel"),
                            usuarioSiscard = PNConfig.Get("BAJAEMPLEADO", "UsuSiscard")
                        };

                        string response = WebService.WSBajaEmpleadoEvertec(datosEntrada);
                        ///////DATOS DUMMY
                        //string response = "<Resultado><numeroTarjeta>" + datosEntrada.numeroTarjeta + "</numeroTarjeta><estadoDeseado>" + datosEntrada.estadoDeseado +
                        //                    "</estadoDeseado><motivoCancelacion>" + datosEntrada.motivoCancelacion + "</motivoCancelacion><codigoRespuesta>00</codigoRespuesta>" +
                        //                    "<descripcionRespuesta>Datos Correctos</descripcionRespuesta></Resultado>";
                        XmlDocument xm = new XmlDocument();
                        xm.LoadXml(response);
                        Logueo.EntradaSalida("[BajaEmpleado] [RECIBIDO] [enviaDatosEvertecBajas] [" + xm.GetElementsByTagName("estadoDeseado")[0].InnerText + ";"
                            + xm.GetElementsByTagName("motivoCancelacion")[0].InnerText + ";"
                            + xm.GetElementsByTagName("descripcionRespuesta")[0].InnerText + "]", "PROCNOC", true);
                        BDsps.insertarRespuestaEvertecBajas(xm, connection, row["ID_ArchivoDetalleBajas"].ToString());
                    }
                    else
                    {
                        BDsps.insertarRespuestaEvertecBajasReproceso(connection, row["ID_ArchivoDetalleBajas"].ToString()
                                    , row["ClaveEmpleadora"].ToString(), row["RegistroEmpleado"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[BajaEmpleado] [enviaDatosEvertec] [Error al guardar respuesta Evertec para el usuario] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
            }
            return enviarDatosParabiliaBajas(pIdArchivo);
        }

        private bool enviarDatosParabiliaBajas(string pIdArchivoEntrada)
        {
            string datosSalida = null;
            SqlConnection connection = new SqlConnection(PNConfig.Get("BAJAEMPLEADO", "BDReadAutorizador"));
            try
            {
                Hashtable htProcesar = new Hashtable();
                htProcesar.Add("@idArchivo", pIdArchivoEntrada);
                DataTable dtDatosProcesar = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosParabiliaBajas", htProcesar, connArchivosEfectivale).Tables["Table"];
                connection.Open();
                if (dtDatosProcesar != null)
                {
                    Logueo.Evento("[Si trae registros este sp: procnoc_travel_ObtieneRegistrosParabiliaBajas] cadena de conexion: " + connArchivosEfectivale);
                    for (int contador = 0; contador < dtDatosProcesar.Rows.Count; contador++)
                    {
                        var row = dtDatosProcesar.Rows[contador];
                        Logueo.Evento("[BajaEmpleado] [enviarDatosParabiliaBajas] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Inicio de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesarBajas);
                        ParamParabiliaBajas paramParabiliaBajas = new ParamParabiliaBajas()
                        {
                            ClaveColectiva = (row["ClaveEmpleadora"].ToString().TrimEnd(' ') + row["RegistroEmpleado"].ToString().TrimEnd(' ')),
                            NumeroTarjeta = row["NumeroTarjeta"].ToString(),
                            ArchivoDetalleBajas = row["Id_ArchivoDetalleBajas"].ToString(),
                            ClaveEmpleadora = row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                            ClaveTipoEmpleadora = this.tipoColectiva
                        };
                        datosSalida = new JavaScriptSerializer().Serialize(paramParabiliaBajas);
                        Logueo.EntradaSalida("[BajaEmpleado] [ENVIADO] [enviarDatosParabiliaBajas] [" + datosSalida + "]", "PROCNOC", false);
                        DataTable respuestaParabilia = BDsps.insertarDatosParabiliaBajas(paramParabiliaBajas, connection).Tables["Table"];
                        RespuestaSP respuesta = new RespuestaSP()
                        {
                            Codigo = respuestaParabilia.Rows[0]["Codigo"].ToString(),
                            Descripcion = respuestaParabilia.Rows[0]["Descripcion"].ToString(),
                            Saldo = respuestaParabilia.Rows[0]["Saldo"].ToString()
                        };
                        var datosEntrada = new JavaScriptSerializer().Serialize(respuesta);
                        Logueo.EntradaSalida("[BajaEmpleado] [RECIBIDO] [enviarDatosParabiliaBajas] [" + datosEntrada + "]", "PROCNOC", false);
                        actualizaRegistrosBDPRocesadorEfectivaleBajas(respuesta, paramParabiliaBajas.ArchivoDetalleBajas, paramParabiliaBajas.ClaveColectiva);
                        Logueo.Evento("[BajaEmpleado] [enviarDatosParabiliaBajas] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesarBajas);
                    }
                }
                Logueo.Evento("[BajaEmpleado] [enviarDatosParabiliaBajas] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Fin de Proceso de Archivo: " + nomArchivoProcesarBajas);
                return crearArchivoSalida(pIdArchivoEntrada);
            }
            catch (Exception ex)
            {
                Logueo.Error("[BajaEmpleado] [enviarDatosParabiliaBajas] [Error al guardar datos parabilia: " + datosSalida + ":" + ex.Message + "] [\\LNBajaEmpleado.cs:línea 430]");
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        private void actualizaRegistrosBDPRocesadorEfectivaleBajas(RespuestaSP pRespuesta, string pIdArchivoDetalle, string pClaveColectiva)
        {
            try
            {
                if (pRespuesta.Codigo.Equals("00"))
                {
                    actualizaRegsBajas(pIdArchivoDetalle, "0", pRespuesta);
                    Logueo.Evento("[BajaEmpleado] [Se creo la colectiva: " + pClaveColectiva + "]");
                }
                else
                {
                    actualizaRegsBajas(pIdArchivoDetalle, "1", pRespuesta);
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[BajaEmpleado] [actualizaRegistrosBDPRocesadorEfectivaleBajas] [Error al actualizar los registro de detalle con Id: " + pIdArchivoDetalle + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        public void actualizaRegsBajas(string pIdArchivoDetalle, string pIdentificador, RespuestaSP pRespuesta)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleBajas", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            htProcesar.Add("@codigoRespuesta", pRespuesta.Codigo);
            htProcesar.Add("@saldo", pRespuesta.Saldo);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesBajas", htProcesar, connArchivosEfectivale);
        }


        private bool crearArchivoSalida(string pIdArchivoEntrada)
        {
            try
            {
                Hashtable ht = new Hashtable();
                ht.Add("@IdArchivo", pIdArchivoEntrada);
                DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaBajas", ht, connArchivosEfectivale).Tables["Table"];

                string nombreArchivoSalida = directorioSalida + "\\" + datosSalida.Rows[0]["NombreArchivo"].ToString().Replace("CP", "RB") + ".txt";
                Logueo.Evento("[BajaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Inicia Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesarBajas + " con nombre: " + nombreArchivoSalida);
                StreamWriter streamSalida = File.CreateText(nombreArchivoSalida);
                streamSalida.WriteLine("13" + "CAN_RES" + datosSalida.Rows[0]["ClaveEmisor"].ToString() + datosSalida.Rows[0]["FechaEnvio"].ToString() + "".PadRight(62, '0'));

                string tmpClaveEmpleadora = datosSalida.Rows[0]["ClaveEmpleadora"].ToString();
                int numBajasEmpleadora = 0, numRechazosEmpleadora = 0, numTotBajasEmpleadora = 0, numTotRechazosEmpleadora = 0, numTotEmpleadoras = 1;
                streamSalida.WriteLine("14" + tmpClaveEmpleadora.PadRight(11, ' ') + "".PadRight(68, '0'));
                for (int contador = 0; contador < datosSalida.Rows.Count; contador++)
                {
                    var row = datosSalida.Rows[contador];
                    if (tmpClaveEmpleadora != row["ClaveEmpleadora"].ToString())
                    {
                        tmpClaveEmpleadora = row["ClaveEmpleadora"].ToString();
                        streamSalida.WriteLine("93" + numBajasEmpleadora.ToString().PadLeft(5, '0') + numRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(69, '0'));
                        streamSalida.WriteLine("14" + tmpClaveEmpleadora.PadRight(11) + "".PadRight(68, '0'));
                        numTotEmpleadoras += 1;
                        numBajasEmpleadora = 0;
                        numRechazosEmpleadora = 0;
                    }
                    string cuentaTmp = SeguridadCifrado.descifrar(row["NumeroCuenta"].ToString());
                    string cuentaArchivoSalida = "";
                    if (!string.IsNullOrEmpty(cuentaTmp))
                        cuentaArchivoSalida = cuentaTmp.Substring((cuentaTmp.Length - 5), 5);
                    
                    if (row["ID_EstatusEvertec"].ToString() == "2" && row["ID_EstatusParabilia"].ToString() == "2")
                    {
                        streamSalida.WriteLine("2202" + row["RegistroEmpleado"].ToString().PadRight(10) + row["Nombre"].ToString().PadRight(30)
                            + cuentaArchivoSalida.PadRight(8) + SeguridadCifrado.descifrar(row["NumeroTarjeta"].ToString()).PadRight(16) +
                                     "0" + row["Saldo"].ToString().PadLeft(10, '0') + "00");
                        numBajasEmpleadora += 1;
                        numTotBajasEmpleadora += 1;
                    }
                    else
                    {
                        string codResp = row["CodigoRP"].ToString().Count() > 2
                                        ? "99"
                                        : row["CodigoRP"].ToString();
                        streamSalida.WriteLine("2202" + row["RegistroEmpleado"].ToString().PadRight(10) + row["Nombre"].ToString().PadRight(30)
                            + cuentaArchivoSalida.PadRight(8) + SeguridadCifrado.descifrar(row["NumeroTarjeta"].ToString()).PadRight(16) +
                                     "0" + row["Saldo"].ToString().PadLeft(10, '0') + (codResp.Equals("00") ? "99" : codResp));
                        numRechazosEmpleadora += 1;
                        numTotRechazosEmpleadora += 1;
                    }

                }
                streamSalida.WriteLine("93" + numBajasEmpleadora.ToString().PadLeft(5, '0') + numRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(69, '0'));
                streamSalida.WriteLine("94" + numTotEmpleadoras.ToString().PadLeft(4, '0') + numTotBajasEmpleadora.ToString().PadLeft(5, '0') + numTotRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(65, '0'));
                streamSalida.Close();
                Logueo.Evento("[BajaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Finaliza Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesarBajas + " con nombre: " + nombreArchivoSalida);
                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[BajaEmpleado] [crearArchivoSalida] [Error al crear el archivo de salida con identificador: " + pIdArchivoEntrada + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }


        private DataTable crearDataTable()
        {
            DataTable dtDatosnew = new DataTable("DetalleBajaEmpleados");
            var dc1 = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveTransaccion", Type.GetType("System.String"));
            var dc4 = new DataColumn("RegistroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("NumeroTarjeta", Type.GetType("System.String"));
            var dc7 = new DataColumn("ID_EstatusEvertec", Type.GetType("System.String"));
            var dc8 = new DataColumn("ID_EstatusParabilia", Type.GetType("System.String"));
            var dc9 = new DataColumn("ReintentosEvertec", Type.GetType("System.String"));
            var dc10 = new DataColumn("ReintentosParabilia", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);
            dtDatosnew.Columns.Add(dc7);
            dtDatosnew.Columns.Add(dc8);
            dtDatosnew.Columns.Add(dc9);
            dtDatosnew.Columns.Add(dc10);

            return dtDatosnew;
        }

        #region OperacionesFile

        public void crearDirectorio()
        {
            directorioSalida = PNConfig.Get("BAJAEMPLEADO", "PathSalida");
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
