#define ESPRODUCCION
using CommonProcesador;
using CommonProcesador.Utilidades;
using Dnu_AutorizadorCacao_NCliente.LogicaNegocio;
using DNU_ParabiliaAltaTarjetasNominales.BaseDatos;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    public class LNAltaTarjetaNominal
    {
        private string directorio;
        private string tipoColectiva;
        private const string expRegFecha = @"^\d{2}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegHora = "^([0-1][0-9]|2[0-3])[0-5][0-9]([0-5][0-9])?$";
        private static DataTable dtContenidoFile;
        private static DataTable dtContenidoFileProducto;
        private static string directorioSalida;
        private static string connArchivosCacao = PNConfig.Get("ALTAEMPLEADOCACAO", "BDWriteProcesadorArchivosCacao");
        private static string nomArchivoProcesar;
        string log; string ip;//
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "ALTAEMPLEADOCACAO";
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

        public LNAltaTarjetaNominal(string directorio, string tipoColectiva)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();
            ThreadContext.Properties["log"] = idLog;
            ThreadContext.Properties["ip"] = direccionIP;
            log = ThreadContext.Properties["log"].ToString();
            ip = ThreadContext.Properties["ip"].ToString();
        }

        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {//
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath + "]");
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Espera a que se copie el archivo completo y lo libere el proceso de copiado]");
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath + "]");
            validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, OcurrioError, El evento de fileWatcher no inicio correctamente, Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }


        private void decodificarArchivo(string pPath, string pNomArchivo)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            FileInfo InfoArchivo = new FileInfo(pPath);
            Guid idLog = Guid.NewGuid();

            try
            {
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Inicia Validacion del Archivo: " + pPath + "" + idLog + "]");
                if (validarContenido(InfoArchivo))
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Insertando informacion]");

                    if (insertaDatos(pNomArchivo))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados]");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                    }
                }
                else if (validarContenidoCredito(InfoArchivo))
                {
                    if (insertaDatos(pNomArchivo, true, idLog))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados " + "[" + idLog + "]");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
                    }

                }
                else
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, decodificarArchivo, El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]" + "[" + idLog + "]");
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
            }
        }

        #region Validaciones
        [Transaction]
        public bool validarArchivos(bool validaCopiadoFiles)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarArchivos");
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO][" + log + "][AltaEmpleado]  [Iniciando proceso de alta]");

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
                        moverArchivo(archivo.FullName, pathInicial);
                        decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarArchivos, error al obtener archivo, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
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
            Regex rgxHora = new Regex(expRegHora);
            if (ExtensionArchivo.Equals(extension) && rgx.IsMatch(nomArchivo.Substring(2, 6))
                    && rgxHora.IsMatch(nomArchivo.Substring(8)))
                result = true;
            return result;
        }

        private bool validarContenido(FileInfo pInfoArchivo)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            int counter = 0, contadorAltasTotales = 0, contadorEmpleadoras = 0, contadorAltas = 0;
            string line, fechaEnvio = null, claveCliente = null, claveBIN = null;

            bool resultHeaderA = false;
            //bool resultHeaderB = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;
            //bool resultTrailerB = false;

            dtContenidoFile = crearDataTable();
            dtContenidoFileProducto = crearDataTableProducto();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 361 && line.Count() != 372)
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ValidarContenido, La línea numero: " + (counter + 1) + " no tinen los 361 caracteres, \\LNAltaEmpleado.cs: línea 156]");
                    file.Close();
                    return false;
                }
                if (line.StartsWith("11"))
                {
                    resultHeaderA = validaHeaderA(line);
                    fechaEnvio = line.Substring(9, 8);
                    claveCliente = line.Substring(17, 10);
                    claveBIN = line.Substring(27, 8);
                    if (!resultHeaderA) {
                        file.Close();
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Error en validacion encabezado]");

                        return false;

                    }
                }
                if (line.StartsWith("21"))
                {
                    resultDetalle = validarDetalle(line, counter, claveCliente, fechaEnvio, claveBIN);
                    contadorAltasTotales += 1;
                    contadorAltas += 1;
                    if (!resultDetalle) {
                        file.Close();
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Error en validacion datos]");

                        return false; }
                }
                if (line.StartsWith("92"))
                {
                    resultTrailerA = validarTrailerA(line, contadorAltasTotales, contadorEmpleadoras);
                    contadorAltasTotales = 0;
                    contadorEmpleadoras = 0;
                    if (!resultTrailerA) {
                        file.Close();
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Error en validacion pie archivo]");

                        return false; }
                }
                counter++;
            }
            file.Close();

            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado Obteniendo contratos]");

            //Se obtienen los datos del contrato.
            Contrato.contrato =
                BDsps.ObtenerDatosContrato(dtContenidoFile.Rows[0]["ClaveCliente"].ToString());

            return true;
        }

        private bool validarContenidoCredito(FileInfo pInfoArchivo)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            int counter = 0, contadorAltasTotales = 0, contadorEmpleadoras = 0, contadorAltas = 0;
            string line, fechaEnvio = null, claveCliente = null, claveBIN = null;

            bool resultHeaderA = false;
            //bool resultHeaderB = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;
            //bool resultTrailerB = false;

            dtContenidoFile = crearDataTableCredito();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 383)
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ValidarContenido, La línea numero: " + (counter + 1) + " no tinen los 361 caracteres, \\LNAltaEmpleado.cs: línea 156]");
                    file.Close();
                    return false;
                }
                if (line.StartsWith("11"))
                {
                    resultHeaderA = validaHeaderA(line);
                    fechaEnvio = line.Substring(9, 8);
                    claveCliente = line.Substring(17, 10);
                    claveBIN = line.Substring(27, 8);
                    if (!resultHeaderA) { file.Close(); return false; }
                }
                if (line.StartsWith("21"))
                {
                    resultDetalle = validarDetalleCredito(line, counter, claveCliente, fechaEnvio, claveBIN);
                    contadorAltasTotales += 1;
                    contadorAltas += 1;
                    if (!resultDetalle) { file.Close(); return false; }
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

            //Se obtienen los datos del contrato.
            Contrato.contrato =
                BDsps.ObtenerDatosContrato(dtContenidoFile.Rows[0]["ClaveCliente"].ToString());

            return true;
        }

        private bool validaHeaderA(string pLine)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            string identificacionReg = pLine.Substring(2, 7);
            string fechaEnvio = pLine.Substring(9, 8);
            string claveCliente = pLine.Substring(17, 10);
            string claveBIN = pLine.Substring(27, 8);
            string filler = pLine.Substring(35, 255);

            Regex rgxAlfaNumerico7 = new Regex(@"\w{7}");
            Regex rgxFecha = new Regex(expRegFecha2);
            Regex rgxFiller = new Regex("0{255}");

            if (!rgxAlfaNumerico7.IsMatch(identificacionReg))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + identificacionReg + ", \\LNAltaEmpleado.cs:línea 213]");
                return false;
            }
            if (!rgxFecha.IsMatch(fechaEnvio))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + fechaEnvio + ", \\LNAltaEmpleado.cs:línea 218]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + filler + ", \\LNAltaEmpleado.cs:línea 223]");
                return false;
            }

            return true;
        }

        private bool validaHeaderB(string pLine)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            bool result = false;
            string claveEmpleadora = pLine.Substring(2, 11);
            string filler = pLine.Substring(13, 207);

            Hashtable ht = new Hashtable();

            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{11}");
            Regex rgxFiller = new Regex("0{207}");

            if (rgxAlfaNumerico10.IsMatch(claveEmpleadora)
                && rgxFiller.IsMatch(filler))
            {
                ht.Add("@claveEmpl", claveEmpleadora.TrimEnd(' '));
                ht.Add("@tipoColectiva", this.tipoColectiva);
                DataTable dtEmpleadora = BDsps.EjecutarSP("procnoc_travel_ObtieneEmpleadora", ht, PNConfig.Get("ALTAEMPLEADOCACAO", "BDReadAutorizador")).Tables["Table"];
                if (dtEmpleadora.Rows.Count > 0)
                {
                    result = true;
                }
                else
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validaHeaderB, La empleadora con clave " + claveEmpleadora + " no se encuentra dada de Alta, \\LNAltaEmpleado.cs:línea 249]");
                    result = false;
                }
            }
            else
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validaHeaderB, hay un error en el Header B con empleadora:" + claveEmpleadora + ", \\LNAltaEmpleado.cs:línea 243]");
            }

            return result;
        }

        private bool validarDetalle(string pLine, int pNumLinea, string pClaveCliente, string pFechaEnvio, string pClaveBIN)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            string claveEmpresa = pLine.Substring(2, 11);
            string regCliente = pLine.Substring(13, 10);
            string nombre = pLine.Substring(23, 50).Trim();
            string primerApellido = pLine.Substring(73, 15).Trim();
            string segundoApellido = pLine.Substring(88, 15).Trim();
            string nombreEmbozado = pLine.Substring(103, 21).Trim();
            string telefono = pLine.Substring(124, 10).Trim();
            string correo = pLine.Substring(134, 100).Trim();
            string TipoMedioAcceso = pLine.Substring(234, 5).Trim();
            string medioAcceso = pLine.Substring(239, 50).Trim();
            string tipoTarjeta = pLine.Substring(289, 1);
            string tarjetaTitular = pLine.Substring(290, 16);
            string tipoMedioAccesoTitular = pLine.Substring(306, 5);
            string medioAccesoTitular = pLine.Substring(311, 50);

            string subproducto = "";
            if (pLine.Count() == 372)
            {
                subproducto = pLine.Substring(361, 11).Trim();
               // correo = correo + "|" + subproducto;
            }
            Regex rgxClaveEmpresa = new Regex("[A-Za-z0-9 _]{11}");
            Regex rgxClaveSub = new Regex("[A-Za-z0-9 _]");
            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{10}");
            Regex rgxNombre = new Regex(@"([a-zA-Z])*([\s])*([a-zA-Z])+");
            Regex rgxApellido = new Regex(@"([a-zA-Z])*([\s])*([a-zA-Z])+");
            Regex rgxTelefono = new Regex(@"\d{10}");
            Regex rgxTipoTarjeta = new Regex("[A|T]");
            Regex rgxTarjetaTitular = new Regex(@"\d{16}");


            if (!rgxClaveEmpresa.IsMatch(claveEmpresa))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave empresa no cumple el formato, \\LNAltaEmpleado.cs:línea 308]");
                return false;
            }

            if (!rgxAlfaNumerico10.IsMatch(regCliente))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el numero de cliente no cumple el formato, \\LNAltaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el nombre no cumple el formato: " + nombre + ", \\LNAltaEmpleado.cs:línea 318]");
                return false;
            }
            if (!rgxTelefono.IsMatch(telefono))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado] [validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Telefono no cumple el formato: " + telefono + ", \\LNAltaEmpleado.cs:línea 328]");
                return false;
            }
            if (!rgxTipoTarjeta.IsMatch(tipoTarjeta))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Tipo Tarjeta no cumple el formato: " + tipoTarjeta + "]");
                return false;
            }
            if (tipoTarjeta.Equals("A"))
            {
                if (!rgxTarjetaTitular.IsMatch(tarjetaTitular))
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la TarjetaTitular no cumple el formato: " + tarjetaTitular + "]");
                    return false;
                }
            }

            if (pLine.Count() == 372)
            {
                if (!rgxClaveSub.IsMatch(subproducto))
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave subproducto no cumple el formato, \\LNAltaEmpleado.cs:línea 414]");
                    return false;
                }

            }

            try
            {

                var validaCorreo = new MailAddress(correo);
                dtContenidoFile.Rows.Add(new Object[] {pFechaEnvio,
                                            string.IsNullOrEmpty(pClaveCliente.Trim())
                                                ? null
                                                : pClaveCliente.Trim(), pClaveBIN,
                                            string.IsNullOrEmpty(claveEmpresa.Trim())
                                                ? null
                                                : claveEmpresa.Trim(),regCliente,
                    nombre, primerApellido, segundoApellido,  nombreEmbozado,  telefono, correo, TipoMedioAcceso, medioAcceso,
                    tipoTarjeta, tarjetaTitular, tipoMedioAccesoTitular, medioAccesoTitular,"1", "0","",subproducto});

                //dtContenidoFileProducto.Rows.Add(new Object[] {pFechaEnvio,
                //                            string.IsNullOrEmpty(pClaveCliente.Trim())
                //                                ? null
                //                                : pClaveCliente.Trim(), pClaveBIN,
                //                            string.IsNullOrEmpty(claveEmpresa.Trim())
                //                                ? null
                //                                : claveEmpresa.Trim(),regCliente,
                //    nombre, primerApellido, segundoApellido,  nombreEmbozado,  telefono, correo, TipoMedioAcceso, medioAcceso,
                //    tipoTarjeta, tarjetaTitular, tipoMedioAccesoTitular, medioAccesoTitular,"1", "0",subproducto});
                return true;
            }
            catch (Exception exp)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, Correos inválidos en la línea " + (pNumLinea + 1) + ", Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return false;
            }
        }

        private bool validarDetalleCredito(string pLine, int pNumLinea, string pClaveCliente, string pFechaEnvio, string pClaveBIN)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            string claveEmpresa = pLine.Substring(2, 11);
            string regCliente = pLine.Substring(13, 10);
            string nombre = pLine.Substring(23, 50).Trim();
            string primerApellido = pLine.Substring(73, 15).Trim();
            string segundoApellido = pLine.Substring(88, 15).Trim();
            string nombreEmbozado = pLine.Substring(103, 21).Trim();
            string telefono = pLine.Substring(124, 10).Trim();
            string correo = pLine.Substring(134, 100).Trim();
            string TipoMedioAcceso = pLine.Substring(234, 5).Trim();
            string medioAcceso = pLine.Substring(239, 50).Trim();
            string tipoTarjeta = pLine.Substring(289, 1);
            string tarjetaTitular = pLine.Substring(290, 16);
            string tipoMedioAccesoTitular = pLine.Substring(306, 5);
            string medioAccesoTitular = pLine.Substring(311, 50);
            string claveSubproducto = pLine.Substring(361, 10);
            string limiteCredito = pLine.Substring(371, 10);
            string diaCorte = pLine.Substring(381, 2);


            Regex rgxClaveEmpresa = new Regex("[A-Za-z0-9 _]{11}");
            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{10}");
            Regex rgxNombre = new Regex(@"([a-zA-Z])*([\s])*([a-zA-Z])+");
            Regex rgxApellido = new Regex(@"([a-zA-Z])*([\s])*([a-zA-Z])+");
            Regex rgxTelefono = new Regex(@"\d{10}");
            Regex rgxTipoTarjeta = new Regex("[A|T]");
            Regex rgxTarjetaTitular = new Regex(@"\d{16}");
            Regex rgxClaveSubproducto = new Regex("[A-Za-z0-9 _]{5}");
            Regex rgxLimiteCredito = new Regex(@"\d{10}");
            Regex rgxDiaCorte = new Regex(@"\d{2}");


            if (!rgxClaveEmpresa.IsMatch(claveEmpresa))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave empresa no cumple el formato, \\LNAltaEmpleado.cs:línea 308]");
                return false;
            }

            if (!rgxAlfaNumerico10.IsMatch(regCliente))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el numero de cliente no cumple el formato, \\LNAltaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el nombre no cumple el formato: " + nombre + ", \\LNAltaEmpleado.cs:línea 318]");
                return false;
            }
            if (!rgxTelefono.IsMatch(telefono))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Telefono no cumple el formato: " + telefono + ", \\LNAltaEmpleado.cs:línea 328]");
                return false;
            }
            if (!rgxTipoTarjeta.IsMatch(tipoTarjeta))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Tipo Tarjeta no cumple el formato: " + tipoTarjeta + "]");
                return false;
            }
            if (!rgxClaveSubproducto.IsMatch(claveSubproducto))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave Subproducto no cumple el formato: " + claveSubproducto + "]");
                return false;
            }
            if (!rgxLimiteCredito.IsMatch(limiteCredito))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Limite de Credito no cumple el formato: " + limiteCredito + "]");
                return false;
            }
            if (!rgxDiaCorte.IsMatch(diaCorte))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Dia de corte no cumple el formato: " + diaCorte + "]");
                return false;
            }
            if (tipoTarjeta.Equals("A"))
            {
                if (!rgxTarjetaTitular.IsMatch(tarjetaTitular))
                {
                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la TarjetaTitular no cumple el formato: " + tarjetaTitular + "]");
                    return false;
                }
            }

            try
            {

                var validaCorreo = new MailAddress(correo);
                dtContenidoFile.Rows.Add(new Object[] {pFechaEnvio,
                                            string.IsNullOrEmpty(pClaveCliente.Trim())
                                                ? null
                                                : pClaveCliente.Trim(), pClaveBIN,
                                            string.IsNullOrEmpty(claveEmpresa.Trim())
                                                ? null
                                                : claveEmpresa.Trim(),regCliente,
                    nombre, primerApellido, segundoApellido,  nombreEmbozado,  telefono, correo, TipoMedioAcceso, medioAcceso,
                    tipoTarjeta, tarjetaTitular, tipoMedioAccesoTitular, medioAccesoTitular,claveSubproducto,limiteCredito,diaCorte,"1", "0"});
                return true;
            }
            catch (Exception exp)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarDetalle, Correos inválidos en la línea " + (pNumLinea + 1) + ", Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return false;
            }
        }

        private bool validarTrailerB(string pLine, int pContadorAltas)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            int numAltas = Convert.ToInt32(pLine.Substring(2, 5));
            string filler = pLine.Substring(7, 213);

            Regex rgxFiller = new Regex("0{213}");

            if (!(numAltas == pContadorAltas))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarTrailerB, hay un error en el Trailer B no corresponde el numero de Altas por empleadora:" + numAltas + ", \\LNAltaEmpleado.cs:línea 341]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarTrailerB, hay un error en el Trailer B en el filler:" + filler + ", \\LNAltaEmpleado.cs:línea 346]");
                return false;
            }
            return true;
        }

        private bool validarTrailerA(string pLine, int pContadorAltas, int pContadorEmpleadoras)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            int numEmpleadoras = Convert.ToInt32(pLine.Substring(2, 4));
            int numMovimientos = Convert.ToInt32(pLine.Substring(6, 5));
            string filler = pLine.Substring(11, 279);

            Regex rgxFiller = new Regex("0{279}");

            //if (!(numEmpleadoras == pContadorEmpleadoras))
            //{
            //    Logueo.Error("[AltaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el número de Empleadoras:" + numEmpleadoras + "] [\\LNAltaEmpleado.cs:línea 362]");
            //    return false;
            //}
            if (!(numMovimientos == pContadorAltas))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarTrailerA, hay un error en el Trailer A no corresponde el número de Altas:" + numMovimientos + ", \\LNAltaEmpleado.cs:línea 367]");
                return false;
            }
            if (!(rgxFiller.IsMatch(filler)))
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, validarTrailerA, hay un error en el Trailer A no corresponde el filler:" + filler + "]");
                return false;
            }

            return true;
        }

        #endregion

        private bool insertaDatos(string pNomArchivo, bool esCredito = false, Guid? idLog = null)
        {
            bool result = false;
            Hashtable htFile = new Hashtable();
            htFile.Add("@descripcion", "Archivo de Alta Empleados");
            htFile.Add("@claveProceso", "ALTAEMPLEADOCACAO");
            htFile.Add("@nombre", pNomArchivo);
            htFile.Add("@tipoArchivo", ".txt");
            string idFile = BDsps.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, connArchivosCacao).Tables["Table"].Rows[0]["ID_Archivo"].ToString();
            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);
            //ht.Add("@tblContent", dtContenidoFile);
            DataTable tmpDatosInsert = null;
            if (esCredito)
            {
                tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoAltasCreditoCACAO", ht, connArchivosCacao).Tables["Table"];
            }
            else
            {
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Bulk info]");
                bool insertarDatosBulk = BDsps.bulkInsertarDatosArchivoDetalle(dtContenidoFile, connArchivosCacao, "ArchivoDetalleAltas", idFile);
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Actualizando info bulk]");
                tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoAltasCACAO", ht, connArchivosCacao).Tables["Table"];
            }
            string tipoManuFactura = pNomArchivo.Substring(10, 3);
            tipoManuFactura = pNomArchivo.Substring(10, 3);
            if (tmpDatosInsert != null)
            {
                if (tmpDatosInsert.Rows.Count == 0)
                {
                    if (esCredito)
                    {
                        return enviarDatosParabiliaCredito(idFile, pNomArchivo, idLog);//en este caso el corte de debito se hara dentro del sps

                    }
                    else
                    {
                        return enviarDatosParabilia(idFile, pNomArchivo);//en este caso el corte de debito se hara dentro del sps

                    }
                }
                if (esCredito)
                {

                    if (tipoManuFactura.ToUpper().Equals("_V_"))
                    {
                        result = LNVirtual.enviaDatosCacaoCreditoVirtuales(tmpDatosInsert, idFile, pNomArchivo, this, connArchivosCacao, idLog);

                    }
                    else
                    {
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Actualizando info bulk]");
                        result = enviaDatosCacaoCredito(tmpDatosInsert, idFile, pNomArchivo, idLog);
                    }
                }
                else
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Creando tarjeta]");
                    if (tipoManuFactura.ToUpper().Equals("_V_"))
                    {
                        result = enviaDatosCacao(tmpDatosInsert, idFile, pNomArchivo, dtContenidoFileProducto);
                        //enviaDatosCacaoCreditoVirtuales(tmpDatosInsert, idFile, pNomArchivo);

                    }
                    else
                    {
                        result = enviaDatosCacao(tmpDatosInsert, idFile, pNomArchivo, dtContenidoFileProducto);
                    }
                }
            }

            return result;
        }

        private bool enviaDatosCacao(DataTable pDtContenidoFile, string pIdArchivo, string nomFile, DataTable tContenidoFileProducto)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            XmlDocument xm = new XmlDocument();
            SqlConnection connection = new SqlConnection(connArchivosCacao);
            SqlConnection connectionParabilia = new SqlConnection(PNConfig.Get("ALTAEMPLEADOCACAO", "BDReadAutorizador"));
            DataTable respValidarTarjetaTitular = null;
            bool respValidarTitular = true;
            string tarjetaTitular = null;
            try
            {


                connection.Open();
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {
                    string response = null;
                    try
                    {
                        var row = pDtContenidoFile.Rows[contador];
                        if (row["ID_EstatusCACAO"].ToString() != "2")
                        {
                            if (row["TipoTarjeta"].Equals("A"))
                            {
                                connectionParabilia.Open();
                                respValidarTarjetaTitular = BDsps.validarTarjetaTitular(row["TarjetaTitular"].ToString(),
                                                            row["TipoMedioAccesoTitular"].ToString(), row["MedioAccesoTitular"].ToString(),
                                                            string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), connectionParabilia)
                                                            .Tables["Table"];
                                if (respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() == "0000")
                                    tarjetaTitular = respValidarTarjetaTitular.Rows[0]["TarjetaTitular"].ToString();
                                else
                                    respValidarTitular = false;
                                connection.Close();
                            }

                            if (respValidarTitular)
                            {
                                string sTipo_Registro = string.Empty;
                                string sNumero_cuenta_Siscard = string.Empty;

                                //if (row["TipoTarjeta"].ToString() == "T")
                                //    sTipo_Registro = "PRI";
                                //else
                                //    sTipo_Registro = "PRI"; //"ADI",
                                sTipo_Registro = "PRI";

                                //if (row["TipoTarjeta"].ToString() == "T")
                                //    sNumero_cuenta_Siscard = "".PadRight(13);

                                sNumero_cuenta_Siscard = "".PadRight(13);

                                EntradaWSCACAOAlta datosEntrada = new EntradaWSCACAOAlta()
                                {
                                    Numero_Solicitud = row["Id_ArchivoDetalle"].ToString().PadRight(8),
                                    Tipo_Registro = sTipo_Registro,
                                    Numero_Cedula = ((string.IsNullOrEmpty(row["ClaveEmisor"].ToString().TrimEnd(' '))
                                                            ? row["ClaveEmpleadora"].ToString().TrimEnd(' ')
                                                            : row["ClaveEmisor"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
                                    Personeria = "F",
                                    Nombre_Cliente = row["Nombre"].ToString().PadRight(20),
                                    Segundo_Nombre = "".PadRight(20),
                                    Primer_Apellido = row["PrimerApellido"].ToString().PadRight(15),
                                    Segundo_Apellido = row["SegundoApellido"].ToString().PadRight(15),
                                    Apellido_Casada = "".PadRight(15),
                                    Estado_Civil = "N",
                                    Fecha_Nacimiento = DateTime.Today.AddYears(-21).ToString("yyyyMMdd"),
                                    Monto_ingreso = "".PadRight(12),
                                    Moneda_ingreso = "".PadRight(2),
                                    Codigo_Profesion = "".PadRight(2),
                                    Nacionalidad = "004",
                                    Sexo = "M",
                                    Provincia_Tarjetahabiente = "".PadRight(2),
                                    Canton_Tarjetahabiente = "".PadRight(2),
                                    Distrito_Tarjetahabiente = "".PadRight(2),
                                    Direccion_Tarjetahabiente = "".PadRight(80),
                                    Telefono_habitacion = "".PadRight(15),
                                    Numero_fax = "".PadRight(15),
                                    Telefono_celular = "".PadRight(15),
                                    Radio_localizador = "".PadRight(15),
                                    Correo_electronico = "".PadRight(45),
                                    Codigo_Postal = "".PadRight(12),
                                    Apartado_Postal = "".PadRight(20),
                                    Nombre_Patrono = "".PadRight(30),
                                    Telefono_Oficina = "".PadRight(15),
                                    Fax_Trabajo = "".PadRight(15),
                                    Provincia_trabajo = "".PadRight(2),
                                    Canton_trabajo = "".PadRight(2),
                                    Distrito_trabajo = "".PadRight(2),
                                    Direccion_Patrono = "".PadRight(60),
                                    Zona_Postal_Patrono = "".PadRight(12),
                                    Apartado_Postal_Patrono = "".PadRight(20),
                                    Codigo_parentesco_pariente = "".PadRight(1),
                                    Limite_Credito = PNConfig.Get("ALTAEMPLEADOCACAO", "LimiteCredito"),
                                    Moneda_Emisor = "MX",//por confirmar Evertec
                                    Sucursal_Retiro_Tarjeta = "01",
                                    Ciclo_Corte = PNConfig.Get("ALTAEMPLEADOCACAO", "CicloCorte"),//por confirmar Evertec
                                                                                                  //Emisor = (PNConfig.Get("ALTAEMPLEADOCACAO", "Emisor") + row["ClaveEmisor"].ToString()).PadRight(8), // Clave BIN
                                    Emisor = row["ClaveBIN"].ToString().PadRight(8), // Clave BIN
                                    Nombre_Embozar = row["NombreEmbozado"].ToString().Length > 21 ?
                                    row["NombreEmbozado"].ToString().Substring(0, 21) :
                                    row["NombreEmbozado"].ToString().PadRight(21),
                                    Cuenta_CMB_CDB = "".PadRight(8),
                                    Tipo_Garantia = "".PadRight(2),
                                    Descuento_membresia = "".PadRight(6),
                                    Tipo_Cliente = "1",
                                    ICE = "".PadRight(16),
                                    Tarjeta_Telefonica = "".PadRight(14),
                                    Indicador_Plan = "".PadRight(1),
                                    Numero_Empresa = "".PadRight(8),
                                    Cedula_Conyuge = "".PadRight(13),
                                    Nombre_Conyuge = "".PadRight(50),
                                    Cuenta_Valor = "".PadRight(19),
                                    Tipo_Poliza = "".PadRight(2),
                                    //Codigo_Vendedor = PNConfig.Get("ALTAEMPLEADOCACAO", "CodigoVendedor").PadRight(3), //obtener del contrato
                                    Codigo_Vendedor = Contrato.CodigoVendedor.PadRight(3), //obtener del contrato
                                    Direccion_EstadoCuenta = "".PadRight(80),
                                    Ciudad_Envio = "".PadRight(15),
                                    Codigo_Postal_envio = "".PadRight(12),
                                    Apartado_Postal_envio = "".PadRight(20),
                                    Nombre_Pariente = "".PadRight(20),
                                    Primer_Apellido_Pariente = "".PadRight(15),
                                    Segundo_Apellido_Pariente = "".PadRight(15),
                                    Telefono_Pariente = "".PadRight(15),
                                    Direccion_Pariente = "".PadRight(40),
                                    Ciudad_Pariente = "".PadRight(15),
                                    Codigo_Postal_Pariente = "".PadRight(12),
                                    Apartado_Postal_Pariente = "".PadRight(20),
                                    Programa_Premiacion = "".PadRight(2),
                                    //Codigo_superfranquicia = "EE", //Obtener del contrato de la colevtiva.
                                    Codigo_superfranquicia = Contrato.CodigoSuperFranquicia.PadRight(2),
                                    Tipo_Tarjeta = "".PadRight(1),
                                    Codigo_Diseno = "".PadRight(2),
                                    Correo_electronico_EstadoCTA = "".PadRight(50),
                                    Filler_01 = "".PadRight(1),
                                    Filler_02 = "".PadRight(1),
                                    Filler_03 = "".PadRight(1),
                                    Couries = "".PadRight(2),
                                    Filler_05 = "".PadRight(8),
                                    Filler_06 = "".PadRight(5),
                                    Tipo_identificacion = "001",
                                    Codigo_emision_Identidad = "MEXICO", //por confirmar Evertec
                                    Numero_NIT = "".PadRight(25),
                                    Lugar_emision_NIT = "".PadRight(3),
                                    Categoria_Cliente = "".PadRight(1),
                                    Campania = "".PadRight(30),
                                    Localidad_cuenta = "MX", //por confirmar Evertec
                                    Numero_cliente = ((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                            ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                            : row["ClaveEmpleadora"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
                                    Indicador_cuota_fija = "".PadRight(1),
                                    Cuota_Fija = "".PadRight(7),
                                    Codigo_Postal_Est_Cuenta = "".PadRight(5),
                                    Codigo_Postal_Apartado = "".PadRight(5),
                                    Region = "".PadRight(4),
                                    Zona = "".PadRight(4),
                                    Sector = "".PadRight(4),
                                    Plan_Poliza = "".PadRight(3),
                                    Numero_Poliza = "".PadRight(16),
                                    Direccion_3_TH = "".PadRight(40),
                                    Direccion_3_EC = "".PadRight(40),
                                    Shipping_address = "".PadRight(90),
                                    Plan_base_compras = "".PadRight(5),//por confirmar Evertec
                                    Plan_base_adelantos = "".PadRight(5), //por confirmar Evertec
                                    Razon_creacion_cta = "".PadRight(1),
                                    Aplica_Recargo_cuota_vencida = "".PadRight(1),
                                    Cobra_comision_adelanto = "".PadRight(1),
                                    Cobra_comision_cuasi_cash = "".PadRight(1),
                                    Aplica_cargo_sobregiro = "".PadRight(1),
                                    Cobra_interes = "".PadRight(1),
                                    Aplica_cargo_cheque_devuelto = "".PadRight(1),
                                    Permite_rebajo_pago_automatico = "".PadRight(1),
                                    Rebajo_pago_fijo = "".PadRight(1),
                                    Monto_pago_fijo = "".PadRight(12),
                                    Rebajo_pago_contado = "".PadRight(1),
                                    Numero_cuenta = "".PadRight(19),
                                    Seguro_social = "".PadRight(9),
                                    Calificativo = "SR",
                                    Telefono_conyuge = "".PadRight(11),
                                    Telefono_trabajo_conyuge = "".PadRight(11),
                                    Lugar_trabajo_conyuge = "".PadRight(30),
                                    Cobra_impuestos_FECI = "".PadRight(1),
                                    Fecha_Cobro_Membresia = "".PadRight(8),
                                    Filler_07 = "".PadRight(8),
                                    Filler_08 = "".PadRight(1),
                                    Filler_09 = "".PadRight(1),
                                    Filler_10 = "".PadRight(12),
                                    Filler_11 = "".PadRight(6),
                                    Codigo_Autonomia = "".PadRight(3),
                                    Codigo_usuario_autonomia = "".PadRight(3),
                                    Empresa_Cobro = "".PadRight(6),
                                    Registro_Tributario_Nacional = "".PadRight(20),
                                    Numero_cuenta_Siscard = sNumero_cuenta_Siscard, //tarjetaTitular.Substring(0, 13),
                                    Poliza_2 = "".PadRight(3),
                                    Numero_Poliza_2 = "".PadRight(16),
                                    Tipo_Vivienda = "".PadRight(1),
                                    Area_Actividad_economica = "".PadRight(3),
                                    Actividad_Economica = "".PadRight(4),
                                    //Usuario = PNConfig.Get("ALTAEMPLEADOCACAO", "UsuarioPros").PadRight(10), // obtener de la configuración del contrato usuario
                                    Usuario = Contrato.Usuario.PadRight(10), // obtener de la configuración del contrato usuario
                                    Forma_Envio_Estados_Cuenta = "N",
                                    Numero_Documento_Garantia = "".PadRight(13),
                                    Valor_En_Garantia = "".PadRight(12)
                                };

#if ESPRODUCCION
                                response = WebService.WSAltaEmpleadoCACAO(datosEntrada);
#else

                                //Registros Dummy////////
                                string tmpNumTarjeta = row["ClaveBIN"].ToString().Trim() + "002" + (new Random().Next(10000, 99999));
                                 //  string tmpNumTarjeta = "41526385967" + (new Random().Next(10000, 99999));
                                string tmpNumCuenta = (new Random().Next(10000, 99999)).ToString();
                                String tmpNombreEmbozo = row["NombreEmbozado"].ToString().Length > 21 ?
                                        row["NombreEmbozado"].ToString().Substring(0, 21) :
                                        row["NombreEmbozado"].ToString().PadRight(21);

                                response = "<Resultado><Fecha>" + DateTime.Today.ToString("yyyyMMdd") + "</Fecha><Hora>" + DateTime.Now.ToString("HHmmss") +
                                                    "</Hora><Cuenta>" + tmpNumCuenta + "</Cuenta><Tarjeta>" + tmpNumTarjeta + "</Tarjeta>" +
                                                    "<Nombre>" + tmpNombreEmbozo + "</Nombre><Identificacion>25863</Identificacion><Respuestas>" +
                                                       "<Respuesta><Codigo>00</Codigo><Descripcion>Datos Correctos</Descripcion>" +
                                                    "</Respuesta></Respuestas></Resultado>";
#endif
                                response = response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#xD;", "");
                                xm = new XmlDocument();
                                xm.LoadXml(response);
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación] [RECIBIDO] [enviaDatosEvertec] [NOMBRE:" + xm.GetElementsByTagName("Nombre")[0].InnerText + ";FECHA:"
                                    + xm.GetElementsByTagName("Fecha")[0].InnerText + ";HORA:"
                                    + xm.GetElementsByTagName("Hora")[0].InnerText + ";Código:"
                                    + xm.GetElementsByTagName("Codigo")[0].InnerText + ";Descripción"
                                    + xm.GetElementsByTagName("Descripcion")[0].InnerText + "]", "PROCNOC", true + "]");

                                BDsps.insertarRespuestaCacaoAltas(xm, connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Tarjeta generada]");

                            }
                            else
                            {
                                BDsps.insertarRespuestaValidarTitular(respValidarTarjetaTitular.Rows[0]["Codigo"].ToString(),
                                                        respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString(),
                                                        connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación] [validarTarjetaTitular] [Codigo: " + respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() +
                                                                "] [Descripcion: " + respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString() + "]");
                            }

                        }
                        else
                        {
                            BDsps.insertarRespuestaCacaoAltasReproceso(connection, row["Id_ArchivoDetalle"].ToString()
                                        , string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), row["RegistroEmpleado"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Resulto un error al convertir la respuesta " + response + " en XML:" + ex.Message + ", " + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Error al guardar respuesta CACAO, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
                connectionParabilia.Close();
            }
            return enviarDatosParabilia(pIdArchivo, nomFile);
        }

        private bool enviaDatosCacaoCredito(DataTable pDtContenidoFile, string pIdArchivo, string nomFile, Guid? idLog)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            XmlDocument xm = new XmlDocument();
            SqlConnection connection = new SqlConnection(connArchivosCacao);
            //  SqlConnection connectionParabilia = new SqlConnection(PNConfig.Get("ALTATARJETACREDCACAO", "BDReadAutorizador"));
            DataTable respValidarTarjetaTitular = null;
            bool respValidarTitular = true;
            string tarjetaTitular = null;
            try
            {


                connection.Open();
                string tarjetaRepetida = "";
                string cuentaRepetida = "";
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {

                    string response = null;
                    try
                    {
                        var row = pDtContenidoFile.Rows[contador];
                        if (row["ID_EstatusCACAO"].ToString() != "2")
                        {
                            if (row["TipoTarjeta"].Equals("A"))
                            {
                                connection.Close();
                                connection.Open();
                                respValidarTarjetaTitular = BDsps.validarTarjetaTitular(row["TarjetaTitular"].ToString(),
                                                            row["TipoMedioAccesoTitular"].ToString(), row["MedioAccesoTitular"].ToString(),
                                                            string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), connection)
                                                            .Tables["Table"];
                                if (respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() == "0000")
                                    tarjetaTitular = respValidarTarjetaTitular.Rows[0]["TarjetaTitular"].ToString();
                                else
                                    respValidarTitular = false;
                                connection.Close();
                                connection.Open();
                            }

                            if (respValidarTitular)
                            {
                                EntradaWSCACAOAlta datosEntrada = new EntradaWSCACAOAlta()
                                {
                                    Numero_Solicitud = row["Id_ArchivoDetalle"].ToString().PadRight(8),
                                    Tipo_Registro = row["TipoTarjeta"].ToString() == "T"
                                                ? "PRI"
                                                : "ADI",
                                    Numero_Cedula = ((string.IsNullOrEmpty(row["ClaveEmisor"].ToString().TrimEnd(' '))
                                                            ? row["ClaveEmpleadora"].ToString().TrimEnd(' ')
                                                            : row["ClaveEmisor"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
                                    Personeria = "F",
                                    Nombre_Cliente = row["Nombre"].ToString().PadRight(20),
                                    Segundo_Nombre = "".PadRight(20),
                                    Primer_Apellido = row["PrimerApellido"].ToString().PadRight(15),
                                    Segundo_Apellido = row["SegundoApellido"].ToString().PadRight(15),
                                    Apellido_Casada = "".PadRight(15),
                                    Estado_Civil = "N",
                                    Fecha_Nacimiento = DateTime.Today.AddYears(-21).ToString("yyyyMMdd"),
                                    Monto_ingreso = "".PadRight(12),
                                    Moneda_ingreso = "".PadRight(2),
                                    Codigo_Profesion = "".PadRight(2),
                                    Nacionalidad = "004",
                                    Sexo = "M",
                                    Provincia_Tarjetahabiente = "".PadRight(2),
                                    Canton_Tarjetahabiente = "".PadRight(2),
                                    Distrito_Tarjetahabiente = "".PadRight(2),
                                    Direccion_Tarjetahabiente = "".PadRight(80),
                                    Telefono_habitacion = "".PadRight(15),
                                    Numero_fax = "".PadRight(15),
                                    Telefono_celular = "".PadRight(15),
                                    Radio_localizador = "".PadRight(15),
                                    Correo_electronico = "".PadRight(45),
                                    Codigo_Postal = "".PadRight(12),
                                    Apartado_Postal = "".PadRight(20),
                                    Nombre_Patrono = "".PadRight(30),
                                    Telefono_Oficina = "".PadRight(15),
                                    Fax_Trabajo = "".PadRight(15),
                                    Provincia_trabajo = "".PadRight(2),
                                    Canton_trabajo = "".PadRight(2),
                                    Distrito_trabajo = "".PadRight(2),
                                    Direccion_Patrono = "".PadRight(60),
                                    Zona_Postal_Patrono = "".PadRight(12),
                                    Apartado_Postal_Patrono = "".PadRight(20),
                                    Codigo_parentesco_pariente = "".PadRight(1),
                                    Limite_Credito = row["LimiteCredito"].ToString().PadLeft(12, '0'),//PNConfig.Get("ALTAEMPLEADOCACAO", "LimiteCredito"),
                                    Moneda_Emisor = "MX",//por confirmar Evertec
                                    Sucursal_Retiro_Tarjeta = "01",
                                    Ciclo_Corte = row["DiaCorte"].ToString().PadRight(2),//PNConfig.Get("ALTAEMPLEADOCACAO", "CicloCorte"),//por confirmar Evertec
                                                                                         //Emisor = (PNConfig.Get("ALTAEMPLEADOCACAO", "Emisor") + row["ClaveEmisor"].ToString()).PadRight(8), // Clave BIN
                                    Emisor = row["ClaveBIN"].ToString().PadRight(8), // Clave BIN
                                    Nombre_Embozar = row["NombreEmbozado"].ToString().Length > 21 ?
                                    row["NombreEmbozado"].ToString().Substring(0, 21) :
                                    row["NombreEmbozado"].ToString().PadRight(21),
                                    Cuenta_CMB_CDB = "".PadRight(8),
                                    Tipo_Garantia = "".PadRight(2),
                                    Descuento_membresia = "".PadRight(6),
                                    Tipo_Cliente = "1",
                                    ICE = "".PadRight(16),
                                    Tarjeta_Telefonica = "".PadRight(14),
                                    Indicador_Plan = "".PadRight(1),
                                    Numero_Empresa = "".PadRight(8),
                                    Cedula_Conyuge = "".PadRight(13),
                                    Nombre_Conyuge = "".PadRight(50),
                                    Cuenta_Valor = "".PadRight(19),
                                    Tipo_Poliza = "".PadRight(2),
                                    Codigo_Vendedor = string.IsNullOrEmpty(Contrato.CodigoVendedor) ? PNConfig.Get("ALTAEMPLEADOCACAO", "CodigoVendedor").PadRight(3) : Contrato.CodigoVendedor.PadRight(3), //obtener del contrato
                                    //Codigo_Vendedor = string.IsNullOrEmpty(Contrato.CodigoVendedor.PadRight(3)) ? "" : Contrato.CodigoVendedor.PadRight(3), //obtener del contrato
                                    Direccion_EstadoCuenta = "".PadRight(80),
                                    Ciudad_Envio = "".PadRight(15),
                                    Codigo_Postal_envio = "".PadRight(12),
                                    Apartado_Postal_envio = "".PadRight(20),
                                    Nombre_Pariente = "".PadRight(20),
                                    Primer_Apellido_Pariente = "".PadRight(15),
                                    Segundo_Apellido_Pariente = "".PadRight(15),
                                    Telefono_Pariente = "".PadRight(15),
                                    Direccion_Pariente = "".PadRight(40),
                                    Ciudad_Pariente = "".PadRight(15),
                                    Codigo_Postal_Pariente = "".PadRight(12),
                                    Apartado_Postal_Pariente = "".PadRight(20),
                                    Programa_Premiacion = "".PadRight(2),
                                    //Codigo_superfranquicia = "EE", //Obtener del contrato de la colevtiva.
                                    Codigo_superfranquicia = string.IsNullOrEmpty(Contrato.CodigoSuperFranquicia) ? "XA" : Contrato.CodigoSuperFranquicia.PadRight(2),
                                    Tipo_Tarjeta = "".PadRight(1),
                                    Codigo_Diseno = "".PadRight(2),
                                    Correo_electronico_EstadoCTA = "".PadRight(50),
                                    Filler_01 = "".PadRight(1),
                                    Filler_02 = "".PadRight(1),
                                    Filler_03 = "".PadRight(1),
                                    Couries = "".PadRight(2),
                                    Filler_05 = "".PadRight(8),
                                    Filler_06 = "".PadRight(5),
                                    Tipo_identificacion = "001",
                                    Codigo_emision_Identidad = "MEXICO", //por confirmar Evertec
                                    Numero_NIT = "".PadRight(25),
                                    Lugar_emision_NIT = "".PadRight(3),
                                    Categoria_Cliente = "".PadRight(1),
                                    Campania = "".PadRight(30),
                                    Localidad_cuenta = "MX", //por confirmar Evertec
                                    Numero_cliente = ((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                            ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                            : row["ClaveEmpleadora"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
                                    Indicador_cuota_fija = "".PadRight(1),
                                    Cuota_Fija = "".PadRight(7),
                                    Codigo_Postal_Est_Cuenta = "".PadRight(5),
                                    Codigo_Postal_Apartado = "".PadRight(5),
                                    Region = "".PadRight(4),
                                    Zona = "".PadRight(4),
                                    Sector = "".PadRight(4),
                                    Plan_Poliza = "".PadRight(3),
                                    Numero_Poliza = "".PadRight(16),
                                    Direccion_3_TH = "".PadRight(40),
                                    Direccion_3_EC = "".PadRight(40),
                                    Shipping_address = "".PadRight(90),
                                    Plan_base_compras = "".PadRight(5),//por confirmar Evertec
                                    Plan_base_adelantos = "".PadRight(5), //por confirmar Evertec
                                    Razon_creacion_cta = "".PadRight(1),
                                    Aplica_Recargo_cuota_vencida = "".PadRight(1),
                                    Cobra_comision_adelanto = "".PadRight(1),
                                    Cobra_comision_cuasi_cash = "".PadRight(1),
                                    Aplica_cargo_sobregiro = "".PadRight(1),
                                    Cobra_interes = "".PadRight(1),
                                    Aplica_cargo_cheque_devuelto = "".PadRight(1),
                                    Permite_rebajo_pago_automatico = "".PadRight(1),
                                    Rebajo_pago_fijo = "".PadRight(1),
                                    Monto_pago_fijo = "".PadRight(12),
                                    Rebajo_pago_contado = "".PadRight(1),
                                    Numero_cuenta = "".PadRight(19),
                                    Seguro_social = "".PadRight(9),
                                    Calificativo = "SR",
                                    Telefono_conyuge = "".PadRight(11),
                                    Telefono_trabajo_conyuge = "".PadRight(11),
                                    Lugar_trabajo_conyuge = "".PadRight(30),
                                    Cobra_impuestos_FECI = "".PadRight(1),
                                    Fecha_Cobro_Membresia = "".PadRight(8),
                                    Filler_07 = "".PadRight(8),
                                    Filler_08 = "".PadRight(1),
                                    Filler_09 = "".PadRight(1),
                                    Filler_10 = "".PadRight(12),
                                    Filler_11 = "".PadRight(6),
                                    Codigo_Autonomia = "".PadRight(3),
                                    Codigo_usuario_autonomia = "".PadRight(3),
                                    Empresa_Cobro = "".PadRight(6),
                                    Registro_Tributario_Nacional = "".PadRight(20),
                                    Numero_cuenta_Siscard = row["TipoTarjeta"].ToString() == "T"
                                                        ? "".PadRight(13)
                                                        : tarjetaTitular.Substring(0, 13),
                                    Poliza_2 = "".PadRight(3),
                                    Numero_Poliza_2 = "".PadRight(16),
                                    Tipo_Vivienda = "".PadRight(1),
                                    Area_Actividad_economica = "".PadRight(3),
                                    Actividad_Economica = "".PadRight(4),
                                    //Usuario = PNConfig.Get("ALTATARJETACREDCACAO", "UsuarioPros").PadRight(10), // obtener de la configuración del contrato usuario
                                    Usuario = string.IsNullOrEmpty(Contrato.Usuario) ? "CAC00703".PadRight(10) : Contrato.Usuario.PadRight(10),//string.IsNullOrEmpty(Contrato.Usuario.PadRight(10))? "CAC00703": Contrato.Usuario.PadRight(10), // obtener de la configuración del contrato usuario
                                    Forma_Envio_Estados_Cuenta = "N",
                                    Numero_Documento_Garantia = "".PadRight(13),
                                    Valor_En_Garantia = "".PadRight(12)
                                };

#if ESPRODUCCION
                                response = WebService.WSAltaEmpleadoCACAO(datosEntrada);
#else
                                //
                                //Registros Dummy////////"41526385967"
                                string tmpNumTarjeta = row["ClaveBIN"].ToString().PadRight(8)+"122" + (new Random().Next(10000, 99999));
                                if (tarjetaRepetida == tmpNumTarjeta)
                                {
                                    tmpNumTarjeta = "99000002011" + (new Random().Next(10000, 99999));
                                    tarjetaRepetida = tmpNumTarjeta;
                                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [  LogueoAltaEmpleadoCacao.Error([" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [se genero tarjeta duplicada] " + tmpNumTarjeta + "]"); 
                    
                                }
                                else
                                {
                                    tarjetaRepetida = tmpNumTarjeta;
                                }


                                string tmpNumCuenta = (new Random().Next(10000, 99999)).ToString();

                                if (cuentaRepetida == tmpNumCuenta)
                                {
                                    tmpNumCuenta = (new Random().Next(10000, 99999)).ToString();
                                    cuentaRepetida = tmpNumCuenta;
                                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [  LogueoAltaEmpleadoCacao.Error(["+ ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [se genero cuenta no reptida " + tmpNumCuenta + "]");
                    
                                }
                                else
                                {
                                    cuentaRepetida = tmpNumCuenta;
                                   LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [se genero cuenta no reptida " + tmpNumCuenta+"]");
                    
                                }
                                String tmpNombreEmbozo = row["NombreEmbozado"].ToString().Length > 21 ?
                                        row["NombreEmbozado"].ToString().Substring(0, 21) :
                                        row["NombreEmbozado"].ToString().PadRight(21);

                                response = "<Resultado><Fecha>" + DateTime.Today.ToString("yyyyMMdd") + "</Fecha><Hora>" + DateTime.Now.ToString("HHmmss") +
                                                    "</Hora><Cuenta>" + tmpNumCuenta + "</Cuenta><Tarjeta>" + tmpNumTarjeta + "</Tarjeta>" +
                                                    "<Nombre>" + tmpNombreEmbozo + "</Nombre><Identificacion>25863</Identificacion><Respuestas>" +
                                                       "<Respuesta><Codigo>00</Codigo><Descripcion>Datos Correctos</Descripcion>" +
                                                    "</Respuesta></Respuestas></Resultado>";
#endif
                                response = response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#xD;", "");
                                xm = new XmlDocument();
                                xm.LoadXml(response);

                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación] [RECIBIDO] [enviaDatosEvertec] [NOMBRE:" + xm.GetElementsByTagName("Nombre")[0].InnerText + ";FECHA:"
                                    + xm.GetElementsByTagName("Fecha")[0].InnerText + ";HORA:"
                                    + xm.GetElementsByTagName("Hora")[0].InnerText + ";Código:"
                                    + xm.GetElementsByTagName("Codigo")[0].InnerText + ";Descripción"
                                    + xm.GetElementsByTagName("Descripcion")[0].InnerText + "]", "PROCNOC", true + "]");

                                BDsps.insertarRespuestaCacaoAltasCredito(xm, connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                            }
                            else
                            {
                                BDsps.insertarRespuestaValidarTitular(respValidarTarjetaTitular.Rows[0]["Codigo"].ToString(),
                                                        respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString(),
                                                        connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación] [validarTarjetaTitular] [Codigo: " + respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() +
                                                                "] [Descripcion: " + respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString() + "]");
                            }

                        }
                        else
                        {
                            BDsps.insertarRespuestaCacaoAltasReproceso(connection, row["Id_ArchivoDetalle"].ToString()
                                        , string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), row["RegistroEmpleado"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO Resulto un error al convertir la respuesta " + response + " en XML:" + ex.Message + ", " + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Error al guardar respuesta CACAO, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
                //  connectionParabilia.Close();
            }

            return enviarDatosParabiliaCredito(pIdArchivo, nomFile, idLog);
        }



        private bool enviarDatosParabilia(string pIdArchivoEntrada, string nomArchivo)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Guardabdo info tarjeta]");

            string fechaVencimiento = null;
            bool procesoFechaVencimiento = true;
            string numTarjeta = null, tipoTarjeta = null, tarjetaTitular = null;
            string datosSalida = null;
            DataTable respuestaParabilia;

            SqlConnection connection = new SqlConnection(PNConfig.Get("ALTAEMPLEADOCACAO", "BDReadAutorizador"));
            try
            {
                Hashtable htProcesar = new Hashtable();
                htProcesar.Add("@idArchivo", pIdArchivoEntrada);
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [enviarDatosParabilia, con Id de Archivo: " + pIdArchivoEntrada + "]");
                DataTable dtDatosProcesar = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosCACAOAltas", htProcesar, connArchivosCacao).Tables["Table"];
                connection.Open();
                if (dtDatosProcesar != null)
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [El SP procnoc_travel_ObtieneRegistrosCACAOAltas trae " + dtDatosProcesar.Rows.Count + " registros]");
                    for (int contador = 0; contador < dtDatosProcesar.Rows.Count; contador++)
                    {
                        var row = dtDatosProcesar.Rows[contador];
                        numTarjeta = row["Tarjeta"].ToString();
                        tipoTarjeta = row["TipoTarjeta"].ToString();
                        tarjetaTitular = row["TarjetaTitular"].ToString();

                        if (procesoFechaVencimiento)
                        {
                            try
                            {
                                fechaVencimiento = obtenerFechaVencimiento(numTarjeta);
                                procesoFechaVencimiento = false;
                            }
                            catch (Exception exp)
                            {
                                DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(5).ToString("yyyyMMdd");
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Error en la fecha de vencimiento: " + exp.Message + ", TRACE: " + exp.StackTrace + "]");
                                //Logueo.Evento("[Eror en la fecha de vencimiento: " + exp.Message + "] [TRACE: " + exp.StackTrace + "]");
                            }
                        }
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Inicio de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                        ParamParabiliaAltas paramParabilia = new ParamParabiliaAltas()
                        {
                            ClaveColectiva = ((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')),
                            Nombre = row["Nombre"].ToString(),
                            Tarjeta = row["Tarjeta"].ToString(),
                            IdArchivoDetalle = row["Id_ArchivoDetalle"].ToString(),
                            NumeroCuenta = row["Cuenta"].ToString(),
                            IdEmpleadoraPadre = string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                            FechaVencimientoTarjeta = fechaVencimiento,
                            correo = row["Correo"].ToString().Trim(),
                            telefono = validaString(row["Telefono"]),
                            medioAcceso = row["MedioAcceso"].ToString(),
                            tipoMedioAcceso = row["TipoMedioAcceso"].ToString(),
                            tarjetaTitular = row["TarjetaTitular"].ToString(),
                            tipoMedioAccesoTitular = row["TipoMedioAccesoTitular"].ToString(),
                            medioAccesoTitular = row["MedioAccesoTitular"].ToString(),
                            nombreEmbozar = row["NombreEmbozado"].ToString(),
                            SubProducto = row["Subproducto"].ToString()
                        };
                        if (paramParabilia.correo.Contains("|") && string.IsNullOrEmpty(paramParabilia.SubProducto))
                        {
                            paramParabilia.SubProducto = paramParabilia.correo.Split('|')[1];
                            //  paramParabilia.correo = paramParabilia.correo.Split('|')[0];
                        }
                        string tempTipoMF = nomArchivo.Substring(10, 2);
                        if (tempTipoMF.ToUpper().Equals("_V"))
                            paramParabilia.tipoManufactura = "V";
                        datosSalida = new JavaScriptSerializer().Serialize(paramParabilia);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, ENVIADO, enviarDatosParabilia, " + datosSalida + ", ", "PROCNOC", false + "]");
                        //if (tipoTarjeta.Equals("T"))
                        respuestaParabilia = BDsps.insertarDatosParabiliaAltas(paramParabilia, connection).Tables["Table"];
                        //else
                        //    respuestaParabilia = BDsps.insertarDatosParabiliaAltasAdicional(paramParabilia, connection).Tables["Table"];
                        if (paramParabilia.correo.Contains("|"))
                        {
                            // paramParabilia.SubProducto = paramParabilia.correo.Split('|')[1];
                            paramParabilia.correo = paramParabilia.correo.Split('|')[0];
                        }
                        var codigo = respuestaParabilia.Rows[0]["Codigo"].ToString();
                        var descripcion = respuestaParabilia.Rows[0]["Descripcion"].ToString();
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia,RECIBIDO, enviarDatosParabilia, CODIGO:" + codigo + ";RESPUESTA:" + descripcion + ", ", "PROCNOC", true + "]");
                        actualizaRegistrosBDPRocesadorCacaoAltas(codigo, paramParabilia.IdArchivoDetalle, paramParabilia.ClaveColectiva);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                    }
                }
                if (dtDatosProcesar != null && dtDatosProcesar.Rows.Count == 0)
                {
                    DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                }
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Archivo: " + nomArchivoProcesar + "]");
                return crearArchivoSalida(fechaVencimiento, pIdArchivoEntrada);
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Error al guardar datos parabilia:" + datosSalida + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        internal bool enviarDatosParabiliaCredito(string pIdArchivoEntrada, string nomArchivo, Guid? idLog)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            string fechaVencimiento = null;
            bool procesoFechaVencimiento = true;
            string numTarjeta = null, tipoTarjeta = null, tarjetaTitular = null;
            string datosSalida = null;
            DataTable respuestaParabilia;
            bool retornoMetodo = false;
            SqlConnection connection = new SqlConnection(PNConfig.Get("ALTAEMPLEADOCACAO", "BDReadAutorizador"));
            try
            {
                Hashtable htProcesar = new Hashtable();
                htProcesar.Add("@idArchivo", pIdArchivoEntrada);
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [enviarDatosParabilia, con Id de Archivo: " + pIdArchivoEntrada + ", " + idLog + "]");
                DataTable dtDatosProcesar = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosCACAOAltasCredito", htProcesar, connArchivosCacao).Tables["Table"];
                connection.Open();
                if (dtDatosProcesar != null)
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [El SP procnoc_travel_ObtieneRegistrosCACAOAltasCredito trae " + dtDatosProcesar.Rows.Count + " registros," + "," + idLog + "]");
                    for (int contador = 0; contador < dtDatosProcesar.Rows.Count; contador++)
                    {
                        var row = dtDatosProcesar.Rows[contador];
                        numTarjeta = row["Tarjeta"].ToString();
                        tipoTarjeta = row["TipoTarjeta"].ToString();
                        tarjetaTitular = row["TarjetaTitular"].ToString();

                        if (procesoFechaVencimiento)
                        {
                            try
                            {
                                fechaVencimiento = obtenerFechaVencimiento(numTarjeta);
                                procesoFechaVencimiento = false;
                            }
                            catch (Exception exp)
                            {
                                DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(5).ToString("yyyyMMdd");
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Eror en la fecha de vencimiento: " + exp.Message + ", TRACE: " + exp.StackTrace + "," + "," + idLog + "]");
                            }
                        }
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + ", Inicio de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + ", " + idLog + "]");
                        ParamParabiliaAltas paramParabilia = new ParamParabiliaAltas()
                        {
                            ClaveColectiva = ((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                               ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                               : row["ClaveEmpleadora"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')),
                            Nombre = row["Nombre"].ToString(),
                            Tarjeta = row["Tarjeta"].ToString(),
                            IdArchivoDetalle = row["Id_ArchivoDetalle"].ToString(),
                            NumeroCuenta = row["Cuenta"].ToString(),
                            IdEmpleadoraPadre = string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                               ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                               : row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                            FechaVencimientoTarjeta = fechaVencimiento,
                            correo = row["Correo"].ToString().Trim(),
                            telefono = validaString(row["Telefono"]),
                            medioAcceso = row["MedioAcceso"].ToString(),
                            tipoMedioAcceso = row["TipoMedioAcceso"].ToString(),
                            tarjetaTitular = row["TarjetaTitular"].ToString(),
                            tipoMedioAccesoTitular = row["TipoMedioAccesoTitular"].ToString(),
                            medioAccesoTitular = row["MedioAccesoTitular"].ToString(),
                            nombreEmbozar = row["NombreEmbozado"].ToString(),
                            CicloCorte = row["DiaCorte"].ToString(),
                            LimiteCredito = row["LimiteCredito"].ToString(),
                            SubProducto = row["ClaveSubproducto"].ToString()
                        };
                        string tempTipoMF = nomArchivo.Substring(10, 3);
                        string tipoManufactura = row["TipoManufactura"].ToString();
                        if (tipoManufactura.ToUpper() == "F" || tipoManufactura == "")
                        {
                            tipoManufactura = "";
                        }
                        bool manufacturaCorrecta = false;
                        if (tempTipoMF.ToUpper().Equals("_V_") && tipoManufactura.ToUpper() == "V")
                        {
                            paramParabilia.tipoManufactura = "V";
                            manufacturaCorrecta = true;
                        }
                        else if ((!tempTipoMF.ToUpper().Equals("_V_")) && tipoManufactura == "")
                        {
                            manufacturaCorrecta = true;
                        }
                        datosSalida = new JavaScriptSerializer().Serialize(paramParabilia);
                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, ENVIADO, enviarDatosParabilia, " + datosSalida + "," + "," + idLog + ",", "PROCNOC", false + "]");

                        //antes de entrar a la transaccion verificar si existe el subproducto
                        DataTable respuestaVerificarSubproducto = BDsps.obtenerDatosConsultaSubproducto(row["ClaveSubproducto"].ToString(), connection).Tables["Table"];
                        var respuestaSubproducto = respuestaVerificarSubproducto.Rows[0]["tipo"].ToString();
                        var credito = respuestaVerificarSubproducto.Rows[0]["esPrepago"].ToString();
                        if ((respuestaSubproducto == "correcto") && (credito == "0") && manufacturaCorrecta)
                        {
                            //
                            bool rollback = true;
                            SqlTransaction transaccionSQL = connection.BeginTransaction();
                            using (transaccionSQL)
                            {
                                paramParabilia.tipoManufactura = tipoManufactura;
                                if (tipoTarjeta.Equals("T"))
                                {
                                    respuestaParabilia = BDsps.insertarDatosParabiliaAltasCredito(paramParabilia, connection, transaccionSQL).Tables["Table"];
                                }
                                else
                                {
                                    respuestaParabilia = BDsps.insertarDatosParabiliaAltasAdicionalCredito(paramParabilia, connection, transaccionSQL).Tables["Table"];

                                }
                                var codigo = respuestaParabilia.Rows[0]["Codigo"].ToString();
                                var descripcion = respuestaParabilia.Rows[0]["Descripcion"].ToString();
                                if (codigo == "00")
                                {
                                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + ",  Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + " insertado en parabilia" + "," + idLog + "]");
                                    //paramParabilia.LimiteCredito = paramParabilia.LimiteCredito.Insert(paramParabilia.LimiteCredito.Length - 2, ".");
                                    DataTable respuestaCreacionCorte = new DataTable();
                                    string codigoCreacionCorte = "correcto";
                                    if (tipoTarjeta.Equals("T"))
                                    {
                                        //    respuestaCreacionCorte = BDsps.insertarDatosParabiliaAltasCorte(paramParabilia, paramParabilia.LimiteCredito, connection, transaccionSQL).Tables["Table"];
                                        //   codigoCreacionCorte = respuestaCreacionCorte.Rows[0]["tipo"].ToString();
                                    }
                                    if (tipoTarjeta.Equals("A") || codigoCreacionCorte == "correcto")
                                    {
                                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + ",  Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + " creacion correcta del corte" + "" + idLog + "]");

                                        //asignando el limite de credito
                                        if (Convert.ToDecimal(paramParabilia.LimiteCredito) > 0)
                                        {
                                            DataTable respuestaAsignaLimiteCredito = BDsps.obtenerDatosTarjetaEventoAsigLimCreditoParaExecute(paramParabilia, connection, transaccionSQL).Tables["Table"];
                                            var codigoAsignaLimiteCredito = respuestaAsignaLimiteCredito.Rows[0][0].ToString();
                                            if (codigoAsignaLimiteCredito != "error")
                                            {
                                                Response respuesta = new Response();
                                                try
                                                {
                                                    Bonificacion bonificacion = LNFondeosYRetiros.obtenerDatosParaDiccionario(respuestaAsignaLimiteCredito);
                                                    // bonificacion.ClaveColectiva = solicitudFondearTarjeta.Cuenta;
                                                    bonificacion.Importe = paramParabilia.LimiteCredito.ToString();
                                                    Dictionary<String, Parametro> parametrosAgregarTarjeta = BDsps.ObtenerDatosParametros(bonificacion, respuestaAsignaLimiteCredito, respuesta, connection, transaccionSQL);
                                                    string fecha = "";
                                                    RespuestaPoliza respuestaPoliza = new RespuestaPoliza();
                                                    if (BDsps.RealizarFondeoORetiroTraspaso(bonificacion, parametrosAgregarTarjeta, fecha, respuesta, transaccionSQL, connection, respuestaPoliza))
                                                    {
                                                        LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + ",  Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + " asignacion correcta de limite credito]");
                                                        //todo correcto
                                                        rollback = false;

                                                    }
                                                    else
                                                    {
                                                        codigo = "9090";
                                                        descripcion = "Error al asignar limite de credito";
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (respuesta.CodRespuesta is null)
                                                    {
                                                        codigo = "9090";
                                                        descripcion = "Error al asignar limite de credito ex" + ex.Message + " " + ex.StackTrace;
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                codigo = respuestaCreacionCorte.Rows[0]["Codigo"].ToString();
                                                descripcion = respuestaCreacionCorte.Rows[0]["error"].ToString();

                                            }
                                        }
                                        else
                                        {
                                            rollback = false;
                                        }
                                    }
                                    else
                                    {
                                        codigo = respuestaCreacionCorte.Rows[0]["Codigo"].ToString();
                                        descripcion = respuestaCreacionCorte.Rows[0]["error"].ToString();
                                    }



                                }
                                //ejecutando el commit o rollback
                                if (rollback)
                                {
                                    transaccionSQL.Rollback();
                                }
                                else
                                {
                                    try
                                    {
                                        transaccionSQL.Commit();

                                    }
                                    catch (Exception excommit)
                                    {
                                        transaccionSQL.Rollback();
                                        rollback = true;
                                    }

                                }
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, RECIBIDO, enviarDatosParabilia, CODIGO:" + codigo + ";RESPUESTA:" + descripcion + ",", "PROCNOC", true + "]");
                                actualizaRegistrosBDPRocesadorCacaoAltasCredito(codigo, paramParabilia.IdArchivoDetalle, paramParabilia.ClaveColectiva);
                                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                            }
                        }
                        else
                        {
                            if (!manufacturaCorrecta)
                            {
                                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Manufactura incorrecta:" + "Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                            }
                            var codigoBusquedaSubProducto = respuestaVerificarSubproducto.Rows[0]["Codigo"].ToString();
                            var descripcionBusquedaSubProducto = respuestaVerificarSubproducto.Rows[0]["error"].ToString();
                            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, RECIBIDO, enviarDatosParabilia, CODIGO:" + codigoBusquedaSubProducto + ";RESPUESTA:" + descripcionBusquedaSubProducto + ", ", "PROCNOC", true + "]");
                            actualizaRegistrosBDPRocesadorCacaoAltas(codigoBusquedaSubProducto, paramParabilia.IdArchivoDetalle, paramParabilia.ClaveColectiva);
                            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");

                        }
                    }
                    if (dtDatosProcesar.Rows.Count == 0)
                    {
                        DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                    }
                }
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Archivo: " + nomArchivoProcesar + "]");
                retornoMetodo = crearArchivoSalidaCredito(fechaVencimiento, pIdArchivoEntrada);

            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Error al guardar datos parabilia:" + datosSalida + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                retornoMetodo = false;
            }
            finally
            {
                connection.Close();
            }
            return retornoMetodo;
        }



        private string validaString(Object paramString)
        {
            if (paramString == null || paramString == "")
                return "";
            return paramString.ToString();
        }

        private void actualizaRegistrosBDPRocesadorCacaoAltas(string pCodigo, string pIdArchivoDetalle, string pClaveColectiva)
        {

            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                if (pCodigo.Equals("00"))
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "0");
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Se creo la colectiva: " + pClaveColectiva + "]");
                }
                else
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "1");
                }
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, actualizaRegistrosBDPRocesadorCacaoAltas, Error al actualizar el registro detalle con id: " + pIdArchivoDetalle + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        private void actualizaRegistrosBDPRocesadorCacaoAltasCredito(string pCodigo, string pIdArchivoDetalle, string pClaveColectiva)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                if (pCodigo.Equals("00"))
                {
                    actualizaRegsAltasCredito(pIdArchivoDetalle, "0");
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Se creo la colectiva: " + pClaveColectiva + "]");
                }
                else
                {
                    actualizaRegsAltasCredito(pIdArchivoDetalle, "1");
                }
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, actualizaRegistrosBDPRocesadorCacaoAltas, Error al actualizar el registro detalle con id: " + pIdArchivoDetalle + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        public void actualizaRegsAltas(string pIdArchivoDetalle, string pIdentificador)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleAltas", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesAltas", htProcesar, connArchivosCacao);
        }
        public void actualizaRegsAltasCredito(string pIdArchivoDetalle, string pIdentificador)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleAltas", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesAltasCredito", htProcesar, connArchivosCacao);
        }

        private bool crearArchivoSalida(string fechaVencimiento, string pIdArchivoEntrada)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            Hashtable ht = new Hashtable();
            bool validaError = false;
            ht.Add("@IdArchivo", pIdArchivoEntrada);
            DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaCACAOAltas", ht, connArchivosCacao).Tables["Table"];
            //string nombreArchivoSalida = directorioSalida + "\\" + datosSalida.Rows[0]["NombreArchivo"].ToString().Replace("AT", "RE") + ".txt";
            string nombreArchivoSalida = directorioSalida + "\\PROCESADO_" + datosSalida.Rows[0]["NombreArchivo"].ToString() + ".txt";
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] [Inicia Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
            StreamWriter streamSalida = File.CreateText(nombreArchivoSalida);
            try
            {

                streamSalida.WriteLine(
                    "13" +
                    "EMP_RES " +
                    datosSalida.Rows[0]["FechaEnvio"].ToString() +
                    datosSalida.Rows[0]["ClaveEmisor"].ToString().PadLeft(10, '0') +
                    datosSalida.Rows[0]["ClaveBIN"].ToString() +
                    "".PadRight(38, '0'));

                int numAltasEmpleadora = 0, numRechazosEmpleadora = 0, numTotAltasEmpleadora = 0, numTotRechazosEmpleadora = 0, numTotEmpleadoras = 1;
                for (int contador = 0; contador < datosSalida.Rows.Count; contador++)
                {
                    var row = datosSalida.Rows[contador];

                    string cuentaArchivoSalida = "";
                    string cuentaClabe = "";
                    try
                    {
                        cuentaArchivoSalida = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CACAO");
                    }
                    catch (Exception ex)
                    {
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + ", AltaEmpleado, crearArchivoSalida, No se pudo obtener la clave del tipo CACAO: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    try
                    {
                        cuentaClabe = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CLABE");
                    }
                    catch (Exception ex)
                    {
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, crearArchivoSalida, No se pudo obtener la CLABE: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }


                    if (row["ID_EstatusCACAO"].ToString() == "2" && row["ID_EstatusParabilia"].ToString() == "2"
                        && row["Repetido"].ToString() == "0")
                    {
                        streamSalida.WriteLine("22" +
                        row["RegistroEmpleado"].ToString().PadRight(10) +
                        cuentaArchivoSalida.PadRight(16) +
                        SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                        "0000" +
                        fechaVencimiento +
                        cuentaClabe.PadRight(18));

                        numAltasEmpleadora += 1;
                        numTotAltasEmpleadora += 1;

                    }
                    else
                    {
                        validaError = true;
                        if (row["Repetido"].ToString() == "0")
                        {

                            streamSalida.WriteLine(
                                "22" +
                                row["RegistroEmpleado"].ToString().PadRight(10) +
                                cuentaArchivoSalida.PadRight(16) +
                                SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                                (row["CodigoRP"].ToString() == "0000" || row["CodigoRP"].ToString() == "00"
                                ? "9999"
                                : row["CodigoRP"].ToString().PadRight(4)) +
                                fechaVencimiento +
                                cuentaClabe.PadRight(18));
                        }
                        else
                        {
                            streamSalida.WriteLine(
                            "22" +
                            row["RegistroEmpleado"].ToString().PadRight(10) +
                            "".PadRight(16) +
                            "".PadRight(16) +
                            "1126" +
                            "".PadRight(8) +
                            "".PadRight(18));
                        }

                        numRechazosEmpleadora += 1;
                        numTotRechazosEmpleadora += 1;
                    }

                }

                streamSalida.WriteLine("94" +
                    numTotEmpleadoras.ToString().PadLeft(4, '0') +
                    numTotAltasEmpleadora.ToString().PadLeft(5, '0') +
                    numTotRechazosEmpleadora.ToString().PadLeft(5, '0') +
                    "".PadRight(58, '0'));
                streamSalida.Close();


                if (validaError)
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + ("PROCESADO_" + datosSalida.Rows[0]["NombreArchivo"].ToString() + ".txt").Replace("PROCESADO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(nombreArchivoSalida, pathArchivosInvalidos);
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + pathArchivosInvalidos + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                }
                else
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + " Finaliza Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
                }

                return true;
            }
            catch (Exception ex)
            {
                streamSalida.Close();
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, crearArchivoSalida, Error al crear el archivo de salida con Identificador: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        private bool crearArchivoSalidaCredito(string fechaVencimiento, string pIdArchivoEntrada)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            Hashtable ht = new Hashtable();
            bool validaError = false;
            ht.Add("@IdArchivo", pIdArchivoEntrada);
            DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaCACAOAltasCredito", ht, connArchivosCacao).Tables["Table"];
            //string nombreArchivoSalida = directorioSalida + "\\" + datosSalida.Rows[0]["NombreArchivo"].ToString().Replace("AT", "RE") + ".txt";
            string nombreArchivoSalida = directorioSalida + "\\PROCESADO_" + datosSalida.Rows[0]["NombreArchivo"].ToString() + ".txt";
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + ", Inicia Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
            StreamWriter streamSalida = File.CreateText(nombreArchivoSalida);
            try
            {

                streamSalida.WriteLine(
                    "13" +
                    "EMP_RES " +
                    datosSalida.Rows[0]["FechaEnvio"].ToString() +
                    datosSalida.Rows[0]["ClaveEmisor"].ToString().PadLeft(10, '0') +
                    datosSalida.Rows[0]["ClaveBIN"].ToString() +
                    "".PadRight(38, '0'));

                int numAltasEmpleadora = 0, numRechazosEmpleadora = 0, numTotAltasEmpleadora = 0, numTotRechazosEmpleadora = 0, numTotEmpleadoras = 1;
                for (int contador = 0; contador < datosSalida.Rows.Count; contador++)
                {
                    var row = datosSalida.Rows[contador];

                    string cuentaArchivoSalida = "";
                    string cuentaClabe = "";
                    try
                    {
                        cuentaArchivoSalida = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CACAO");
                    }
                    catch (Exception ex)
                    {
                        cuentaArchivoSalida = "";
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, crearArchivoSalida, No se pudo obtener la clave del tipo CACAO: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    try
                    {
                        cuentaClabe = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CLABE");
                    }
                    catch (Exception ex)
                    {
                        cuentaClabe = "";
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, crearArchivoSalida, No se pudo obtener la CLABE: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }


                    if (row["ID_EstatusCACAO"].ToString() == "2" && row["ID_EstatusParabilia"].ToString() == "2"
                        && row["Repetido"].ToString() == "0")
                    {
                        //aqui se escribe el archivo de salida con los datos de la trajeta
                        streamSalida.WriteLine("22" +
                        row["RegistroEmpleado"].ToString().PadRight(10) +
                        cuentaArchivoSalida.PadRight(16) +
                        SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                        "0000" +
                        fechaVencimiento +
                        cuentaClabe.PadRight(18));

                        numAltasEmpleadora += 1;
                        numTotAltasEmpleadora += 1;

                    }
                    else
                    {
                        validaError = true;
                        if (row["Repetido"].ToString() == "0")
                        {

                            streamSalida.WriteLine(
                                "22" +
                                row["RegistroEmpleado"].ToString().PadRight(10) +
                                cuentaArchivoSalida.PadRight(16) +
                                SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                                (row["CodigoRP"].ToString() == "0000" || row["CodigoRP"].ToString() == "00"
                                ? "9999"
                                : row["CodigoRP"].ToString().PadRight(4)) +
                                fechaVencimiento +
                                cuentaClabe.PadRight(18));
                        }
                        else
                        {
                            streamSalida.WriteLine(
                            "22" +
                            row["RegistroEmpleado"].ToString().PadRight(10) +
                            "".PadRight(16) +
                            "".PadRight(16) +
                            "1126" +
                            "".PadRight(8) +
                            "".PadRight(18));
                        }

                        numRechazosEmpleadora += 1;
                        numTotRechazosEmpleadora += 1;
                    }

                }

                streamSalida.WriteLine("94" +
                    numTotEmpleadoras.ToString().PadLeft(4, '0') +
                    numTotAltasEmpleadora.ToString().PadLeft(5, '0') +
                    numTotRechazosEmpleadora.ToString().PadLeft(5, '0') +
                    "".PadRight(58, '0'));
                streamSalida.Close();


                if (validaError)
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + ("PROCESADO_" + datosSalida.Rows[0]["NombreArchivo"].ToString() + ".txt").Replace("PROCESADO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(nombreArchivoSalida, pathArchivosInvalidos);
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, Archivo " + pathArchivosInvalidos + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                }
                else
                {
                    LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + " Finaliza Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
                }

                return true;
            }
            catch (Exception ex)
            {
                streamSalida.Close();
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, crearArchivoSalida, Error al crear el archivo de salida con Identificador: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }
        private string obtenerFechaVencimiento(string pNumTarjeta)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                WSObtenerFechaVencimiento param = new WSObtenerFechaVencimiento()
                {
                    numeroTarjeta = SeguridadCifrado.descifrar(pNumTarjeta)
                };

#if ESPRODUCCION
                string response = WebService.WSObtenerFechaVencimiento(param);
#else

                /////////DATOS DUMMY
                string response = "<Resultado><NumeroCedula>" + DateTime.Today.ToString("yyyyMMdd") + "</NumeroCedula><InfoCuenta>" +
                                              "<FechaVencimientoCuenta>20251225" +
                                           "</FechaVencimientoCuenta></InfoCuenta></Resultado>";
#endif
                XmlDocument xm = new XmlDocument();

                xm.LoadXml(response);
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.Evertec.ConsultaVigencia, RECIBIDO, obtenerFechaVencimiento, " + response.Replace("\n", " ").Replace("\r", " ") + ",", "PROCNOC", true + "]");
                return xm.GetElementsByTagName("fechaVencimientoCuenta")[0].InnerText;
            }
            catch (Exception exp)
            {
                DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                string fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [Eror en la fecha de vencimiento: " + exp.Message + ", TRACE: " + exp.StackTrace + "]");
                return fechaVencimiento;
            }
        }

        private void enviarCorreoAlta(string correo, string mailCFDI, string nombre, string ClaveColectiva)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();

            Hashtable ht = new Hashtable();
            String nombreEmpresa = null;
            ht.Add("@ClaveColectiva", ClaveColectiva);
            DataTable datosSalida = BDsps.EjecutarSP("Procnoc_Travel_ObtieneNombreColectivaPadre", ht, PNConfig.Get("ALTAEMPLEADOCACAO", "BDReadAutorizador")).Tables["Table"];
            if (datosSalida.Rows.Count > 0)
                nombreEmpresa = datosSalida.Rows[0]["NombreORazonSocial"].ToString();
            else
                LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado,  No se obtuvo el Nombre a Embozar de la Colectiva padre]");
            Correo datoCorreo = new Correo()
            {
                body = construirBody(correo, mailCFDI, nombre, nombreEmpresa),
                subject = PNConfig.Get("ALTAEMPLEADOCACAO", "SubjectMail"),
                email = correo
            };
            RWSMail rWSmail = WebService.WSEnviaCorreoBienvenida(datoCorreo);
        }

        public bool generarCuentasCorreo(string pCorreoCFDI, string pCorreoEmpresaCFDI)
        {
            EmployeeAccount employee = new EmployeeAccount()
            {
                email = pCorreoCFDI.Split('@')[0],
                redirectEmail = pCorreoEmpresaCFDI,
                step = "0"
            };
            RWSMail rWSmailEmployee = WebService.WSCrearCorreoEmployee(employee);

            if (rWSmailEmployee.responseCode == 0 || rWSmailEmployee.responseCode == 1)
                return true;
            return false;
        }

        private string construirBody(string correo, string mailCFDI, string nombre, string nomEmpresa)
        {
            //string log = ThreadContext.Properties["log"].ToString();
            //string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                string fileName = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioPlantilla");

                var template = File.ReadAllText(fileName);
                template = template.Replace("[NOMBREEMPRESA]", nomEmpresa);
                template = template.Replace("[EMPRESA]", nomEmpresa);
                template = template.Replace("[MAILCFDI]", mailCFDI);
                template = template.Replace("[NOMBRE]", nombre);
                return template;
            }
            catch (Exception exp)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [AltaEmpleado, construirBody, El archivo html para el cuerpo del correo:" + correo + " no se encuentra, Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return null;
            }

        }

        private DataTable crearDataTable()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));
            var dc19 = new DataColumn("ID_Archivo", Type.GetType("System.String"));
            var dc20 = new DataColumn("Subproducto", Type.GetType("System.String"));


            dtDatosnew.Columns.Add(dc);
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
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);


            return dtDatosnew;
        }

        private DataTable crearDataTableProducto()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));
            var dc19 = new DataColumn("ClaveSubproducto", Type.GetType("System.String"));
            var dc20 = new DataColumn("ID_Archivo", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
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
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);

            return dtDatosnew;
        }

        private DataTable crearDataTableCredito()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ClaveSubproducto", Type.GetType("System.String"));
            var dc18 = new DataColumn("LimiteCredito", Type.GetType("System.String"));
            var dc19 = new DataColumn("DiaCorte", Type.GetType("System.String"));
            var dc20 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc21 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
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
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);
            dtDatosnew.Columns.Add(dc21);

            return dtDatosnew;
        }

        #region OperacionesFile

        public void crearDirectorio()
        {//
#if DEBUG
            directorioSalida = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioSalida");
#else
            directorioSalida = PNConfig.Get("ALTAEMPLEADOCACAO", "DirectorioSalida");
#endif
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
