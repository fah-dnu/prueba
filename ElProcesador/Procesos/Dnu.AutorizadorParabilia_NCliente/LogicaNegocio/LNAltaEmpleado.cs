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

namespace Dnu.AutorizadorParabilia_NCliente.LogicaNegocio
{
    public class LNAltaEmpleado
    {
        private string directorio;
        private string tipoColectiva;
        private const string expRegFecha = @"^\d{2}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private const string expRegHora = "^([0-1][0-9]|2[0-3])[0-5][0-9]([0-5][0-9])?$";
        private static DataTable dtContenidoFile;
        private static string directorioSalida;
        private static string connArchivosEfectivale = PNConfig.Get("ALTAEMPLEADO", "BDWriteProcesadorArchivosEfec");
        private static string nomArchivoProcesar;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "ALTAEMPLEADO";
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

        public LNAltaEmpleado(string directorio, string tipoColectiva)
        {
            this.directorio = directorio;
            this.tipoColectiva = tipoColectiva;
        }

        public void NuevoArchivo(Object sender, FileSystemEventArgs e)
        {

            WatcherChangeTypes elTipoCambio = e.ChangeType;
            //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
            Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("ALTAEMPLEADO", "DirAltaEmp") + " el se recibio el archivo : " + e.FullPath);
            Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
            Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath);
            validarArchivos(true);
        }

        [Transaction]
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            Logueo.Error("[AltaEmpleado][OcurrioError] [El evento de fileWatcher no inicio correctamente] [Mensaje: " + e.GetException().Message + " TRACE: " + e.GetException().StackTrace + "]");
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
        }


        private void decodificarArchivo(string pPath, string pNomArchivo)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            try
            {

                Logueo.Evento("[AltaEmpleado] Inicia Validacion del Archivo: " + pPath);
                if (validarContenido(InfoArchivo))
                {
                    if (insertaDatos(pNomArchivo))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        moverArchivo(InfoArchivo.FullName, rutaFinal);
                        Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                    }
                }
                else
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                Logueo.Error("[AltaEmpleado] [decodificarArchivo] [El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
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
            List<string> existeArchivo = Directory.GetFiles(directorio, "AT*").ToList();
            if (existeArchivo.Count > 0)
            {
                try
                {
                    foreach (var dato in existeArchivo)
                    {
                        archivo = new FileInfo(dato);
                        nomArchivoProcesar = archivo.FullName;
                        if (validarNombreExtencion(archivo.FullName, ".txt"))
                        {
                            string pathInicial = directorio + "\\EN_PROCESO_" + archivo.Name;
                            moverArchivo(archivo.FullName, pathInicial);
                            decodificarArchivo(pathInicial, Path.GetFileNameWithoutExtension(archivo.FullName));
                        }
                        else
                        {
                            Logueo.Error("[AltaEmpleado] [validarArchivos] [Archivo no corresponde a la extencion .txt o al formato de nombre] [LNAltaEmpleado.cs:línea 108]");
                            string pathArchivosInvalidos = directorio + "\\Erroneos\\PROCESADO_CON_ERRORES_" + archivo.Name.Replace("EN_PROCESO_", "");
                            moverArchivo(archivo.FullName, pathArchivosInvalidos);
                            Logueo.Evento("[AltaEmpleado] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logueo.Error("[AltaEmpleado] [validarArchivos] [error al obtener archivo] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + archivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    moverArchivo(archivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[AltaEmpleado] Archivo " + archivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
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
                if (line.Count() != 220)
                {
                    Logueo.Error("[AltaEmpleado] [ValidarContenido] [La línea numero: " + (counter + 1) + " no tinen los 220 caracteres] [\\LNAltaEmpleado.cs: línea 156]");
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
            string filler = pLine.Substring(17, 203);

            Regex rgxAlfaNumerico7 = new Regex(@"\w{7}");
            Regex rgxFecha = new Regex(expRegFecha2);
            Regex rgxFiller = new Regex("0{203}");

            if (!rgxAlfaNumerico7.IsMatch(identificacionReg))
            {
                Logueo.Error("[AltaEmpleado] [validaHeaderA] [hay un error en el Header A en el campo: " + identificacionReg + "] [\\LNAltaEmpleado.cs:línea 213]");
                return false;
            }
            if (!rgxFecha.IsMatch(fechaEnvio))
            {
                Logueo.Error("[AltaEmpleado][validaHeaderA] [hay un error en el Header A en el campo: " + fechaEnvio + "] [\\LNAltaEmpleado.cs:línea 218]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[AltaEmpleado] [validaHeaderA] [hay un error en el Header A en el campo: " + filler + "] [\\LNAltaEmpleado.cs:línea 223]");
                return false;
            }

            return true;
        }

        private bool validaHeaderB(string pLine)
        {
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
                DataTable dtEmpleadora = BDsps.EjecutarSP("procnoc_travel_ObtieneEmpleadora", ht, PNConfig.Get("ALTAEMPLEADO", "BDReadAutorizador")).Tables["Table"];
                if (dtEmpleadora.Rows.Count > 0)
                {
                    result = true;
                }
                else
                {
                    Logueo.Error("[AltaEmpleado] [validaHeaderB] [La empleadora con clave " + claveEmpleadora + " no se encuentra dada de Alta] [\\LNAltaEmpleado.cs:línea 249]");
                    result = false;
                }
            }
            else
            {
                Logueo.Error("[AltaEmpleado] [validaHeaderB] [hay un error en el Header B con empleadora:" + claveEmpleadora + "] [\\LNAltaEmpleado.cs:línea 243]");
            }

            return result;
        }

        private bool validarDetalle(string pLine, int pNumLinea, string pClaveEmp, string pFechaEnvio)
        {
            string claveTransaccion = pLine.Substring(2, 2);
            string claveEmisor = pLine.Substring(4, 2);
            string regEmpleado = pLine.Substring(6, 10);
            string nombre = pLine.Substring(16, 26);
            string numCuenta = pLine.Substring(42, 8);
            string telefono = pLine.Substring(50, 10);
            string correo = pLine.Substring(60, 50);
            string correoCFDI = pLine.Substring(110, 50);
            string correoEmpresaCFDI = pLine.Substring(160, 50);
            string idEmpleado = pLine.Substring(210, 10);

            Regex rgxClaveEmisor = new Regex("\\w{2}");
            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{10}");
            Regex rgxNombre = new Regex(@"[a-zA-Z]+(\s+[a-zA-Z]+)+");
            Regex rgxNumCuenta = new Regex("0{8}");
            Regex rgxTelefono = new Regex(@"\d{10}");

            if (!claveTransaccion.Equals("01"))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " la clave transacción no corresponde] [\\LNAltaEmpleado.cs:línea 304]");
                return false;
            }
            if (!rgxClaveEmisor.IsMatch(claveEmisor))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " la clave emisor no cumple el formato] [\\LNAltaEmpleado.cs:línea 308]");
                return false;
            }
            if (!rgxAlfaNumerico10.IsMatch(regEmpleado))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " el numero de empleado no cumple el formato] [\\LNAltaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " el nombre no cumple el formato: " + nombre + "] [\\LNAltaEmpleado.cs:línea 318]");
                return false;
            }
            if (!rgxNumCuenta.IsMatch(numCuenta))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " el no Cuenta no cumple el formato] [\\LNAltaEmpleado.cs:línea 323]");
                return false;
            }
            if (!rgxTelefono.IsMatch(telefono))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " el Telefono no cumple el formato: " + telefono + "] [\\LNAltaEmpleado.cs:línea 328]");
                return false;
            }
            if (!rgxAlfaNumerico10.IsMatch(idEmpleado))
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [hay un error en la línea no. " + (pNumLinea + 1) + " el IdEmpleado no cumple el formato: " + idEmpleado + "] [\\LNAltaEmpleado.cs:línea 333]");
                return false;
            }

            try
            {
                var validaCorreo = new System.Net.Mail.MailAddress(correo);
                var validaCorreoCFDI = new System.Net.Mail.MailAddress(correoCFDI);
                dtContenidoFile.Rows.Add(new Object[] { claveEmisor, pFechaEnvio, pClaveEmp, claveTransaccion
                                    , regEmpleado, nombre, SeguridadCifrado.cifrar(numCuenta), telefono, correo, correoCFDI,correoEmpresaCFDI
                                    , idEmpleado, "1", "1", "0", "0"});
                return true;
            }
            catch (Exception exp)
            {
                Logueo.Error("[AltaEmpleado] [validarDetalle] [Correos inválidos en la línea " + (pNumLinea + 1) + "][Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return false;
            }
        }

        private bool validarTrailerB(string pLine, int pContadorAltas)
        {
            int numAltas = Convert.ToInt32(pLine.Substring(2, 5));
            string filler = pLine.Substring(7, 213);

            Regex rgxFiller = new Regex("0{213}");

            if (!(numAltas == pContadorAltas))
            {
                Logueo.Error("[AltaEmpleado] [validarTrailerB] [hay un error en el Trailer B no corresponde el numero de Altas por empleadora:" + numAltas + "] [\\LNAltaEmpleado.cs:línea 341]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                Logueo.Error("[AltaEmpleado] [validarTrailerB] [hay un error en el Trailer B en el filler:" + filler + "] [\\LNAltaEmpleado.cs:línea 346]");
                return false;
            }
            return true;
        }

        private bool validarTrailerA(string pLine, int pContadorAltas, int pContadorEmpleadoras)
        {
            int numEmpleadoras = Convert.ToInt32(pLine.Substring(2, 4));
            int numMovimientos = Convert.ToInt32(pLine.Substring(6, 5));
            string filler = pLine.Substring(11, 209);

            Regex rgxFiller = new Regex("0{209}");

            if (!(numEmpleadoras == pContadorEmpleadoras))
            {
                Logueo.Error("[AltaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el número de Empleadoras:" + numEmpleadoras + "] [\\LNAltaEmpleado.cs:línea 362]");
                return false;
            }
            if (!(numMovimientos == pContadorAltas))
            {
                Logueo.Error("[AltaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el número de Altas:" + numMovimientos + "] [\\LNAltaEmpleado.cs:línea 367]");
                return false;
            }
            if (!(rgxFiller.IsMatch(filler)))
            {
                Logueo.Error("[AltaEmpleado] [validarTrailerA] [hay un error en el Trailer A no corresponde el filler:" + filler + "] [\\LNAltaEmpleado.cs:línea 372]");
                return false;
            }

            return true;
        }

        #endregion

        private bool insertaDatos(string pNomArchivo)
        {
            bool result = false;
            Hashtable htFile = new Hashtable();
            htFile.Add("@descripcion", "Archivo de Alta Empleados");
            htFile.Add("@claveProceso", "ALTAEMPLEADO");
            htFile.Add("@nombre", pNomArchivo);
            htFile.Add("@tipoArchivo", ".txt");
            string idFile = BDsps.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, connArchivosEfectivale).Tables["Table"].Rows[0]["ID_Archivo"].ToString();
            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);
            ht.Add("@tblContent", dtContenidoFile);
            DataTable tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoAltas", ht, connArchivosEfectivale).Tables["Table"];

            if (tmpDatosInsert != null)
            {
                if (tmpDatosInsert.Rows.Count == 0)
                {
                    return enviarDatosParabilia(idFile);
                }
                result = enviaDatosEvertec(tmpDatosInsert, idFile);
            }

            return result;
        }

        private bool enviaDatosEvertec(DataTable pDtContenidoFile, string pIdArchivo)
        {
            XmlDocument xm = new XmlDocument();
            SqlConnection connection = new SqlConnection(connArchivosEfectivale);
            try
            {
                connection.Open();
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {
                    string response = null;
                    try
                    {
                        var row = pDtContenidoFile.Rows[contador];
                        if (generarCuentasCorreo(row["CorreoCFDI"].ToString().Trim(), row["CorreoEmpresaCFDI"].ToString().Trim()))
                        {
                            if (row["ID_EstatusEvertec"].ToString() != "2")
                            {
                                EntradaWSEvertecAlta datosEntrada = new EntradaWSEvertecAlta()
                                {
                                    Numero_Solicitud = row["Id_ArchivoDetalle"].ToString().PadRight(8),
                                    Tipo_Registro = "PRI",
                                    Numero_Cedula = (row["ClaveEmpleadora"].ToString().TrimEnd(' ') + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
                                    Personeria = "F",
                                    Nombre_Cliente = "NOMBRE".PadRight(20),
                                    Segundo_Nombre = "".PadRight(20),
                                    Primer_Apellido = "APELLIDO".PadRight(15),
                                    Segundo_Apellido = "".PadRight(15),
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
                                    Limite_Credito = PNConfig.Get("ALTAEMPLEADO", "LimiteCredito"),
                                    Moneda_Emisor = "MX",//por confirmar Evertec
                                    Sucursal_Retiro_Tarjeta = "01",
                                    Ciclo_Corte = PNConfig.Get("ALTAEMPLEADO", "CicloCorte"),//por confirmar Evertec
                                    Emisor = (PNConfig.Get("ALTAEMPLEADO", "Emisor") + row["ClaveEmisor"].ToString()).PadRight(8),
                                    Nombre_Embozar = row["Nombre"].ToString().Substring(0, 21),
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
                                    Codigo_Vendedor = PNConfig.Get("ALTAEMPLEADO", "CodigoVendedor").PadRight(3),
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
                                    Codigo_superfranquicia = "EE",
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
                                    Numero_cliente = (row["ClaveEmpleadora"].ToString().TrimEnd(' ') + row["RegistroEmpleado"].ToString().TrimEnd(' ')).PadRight(19),
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
                                    Numero_cuenta_Siscard = "".PadRight(13),
                                    Poliza_2 = "".PadRight(3),
                                    Numero_Poliza_2 = "".PadRight(16),
                                    Tipo_Vivienda = "".PadRight(1),
                                    Area_Actividad_economica = "".PadRight(3),
                                    Actividad_Economica = "".PadRight(4),
                                    Usuario = PNConfig.Get("ALTAEMPLEADO", "UsuarioPros").PadRight(10),
                                    Forma_Envio_Estados_Cuenta = "N",
                                    Numero_Documento_Garantia = "".PadRight(13),
                                    Valor_En_Garantia = "".PadRight(12)
                                };
                                response = WebService.WSAltaEmpleadoEvertec(datosEntrada);

                                ///////Registros Dummy
                                //string tmpNumTarjeta = "41526385967" + (new Random().Next(10000, 99999));
                                //string tmpNumCuenta = (new Random().Next(10000, 99999)).ToString();
                                //response = "<Resultado><Fecha>" + DateTime.Today.ToString("yyyyMMdd") + "</Fecha><Hora>" + DateTime.Now.ToString("HHmmss") +
                                //                    "</Hora><Cuenta>" + tmpNumCuenta + "</Cuenta><Tarjeta>" + tmpNumTarjeta + "</Tarjeta>" +
                                //                    "<Nombre>" + datosEntrada.Nombre_Embozar + "</Nombre><Identificacion>25863</Identificacion><Respuestas>" +
                                //                       "<Respuesta><Codigo>00</Codigo><Descripcion>Datos Correctos</Descripcion>" +
                                //                    "</Respuesta></Respuestas></Resultado>";
                                response = response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#xD;", "");
                                xm = new XmlDocument();
                                xm.LoadXml(response);
                                Logueo.EntradaSalida("[WS_AltaEmpleado.Evertec.Prospectación] [RECIBIDO] [enviaDatosEvertec] [NOMBRE:" + xm.GetElementsByTagName("Nombre")[0].InnerText + ";FECHA:"
                                    + xm.GetElementsByTagName("Fecha")[0].InnerText + ";HORA:"
                                    + xm.GetElementsByTagName("Hora")[0].InnerText + ";Código:"
                                    + xm.GetElementsByTagName("Codigo")[0].InnerText + ";Descripción"
                                    + xm.GetElementsByTagName("Descripcion")[0].InnerText + "]", "PROCNOC", true);
                                BDsps.insertarRespuestaEvertecAltas(xm, connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                string tarjetaTmp = xm.GetElementsByTagName("Tarjeta")[0].InnerText;
                                if (!string.IsNullOrEmpty(tarjetaTmp))
                                {
                                    registraApp_Web(row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                                            row["Correo"].ToString().TrimEnd(' '),
                                            row["CorreoCFDI"].ToString().TrimEnd(' '),
                                            row["Nombre"].ToString().TrimEnd(' '),
                                            row["RegistroEmpleado"].ToString().TrimEnd(' '),
                                            row["Telefono"].ToString().TrimEnd(' '),
                                            tarjetaTmp);
                                }
                            }
                            else
                            {
                                BDsps.insertarRespuestaEvertecAltasReproceso(connection, row["Id_ArchivoDetalle"].ToString()
                                            , row["ClaveEmpleadora"].ToString(), row["RegistroEmpleado"].ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[WS_AltaEmpleado.Evertec.Prospectación] [enviaDatosEvertec] [Resulto un error al convertir la respuesta " + response + " en XML:" + ex.Message + "] [" + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[WS_AltaEmpleado.Evertec.Prospectación] [enviaDatosEvertec] [Error al guardar respuesta Evertec] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
            }
            return enviarDatosParabilia(pIdArchivo);
        }

        private void registraApp_Web(string pEmpresa, string pCorreo, string pCorreoCFDI,
                                    string pNomEmpleado, string pNumEmpleado,
                                    string pTelefono, string pNumTarjeta)
        {
            Hashtable ht = new Hashtable();
            string datosSalida;
            ht.Add("@input_cliente", pEmpresa.Substring(0, 6));
            ht.Add("@input_cardholder_email", pCorreo);
            ht.Add("@input_cardholder_product_email", pCorreoCFDI);
            ht.Add("@input_cardholder_name", pNomEmpleado);
            ht.Add("@input_cardholder_last_name", "");
            ht.Add("@input_cardholder_employee_number", pNumEmpleado);
            ht.Add("@input_cardholder_job_title", "");
            ht.Add("@input_cardholder_phone", pTelefono);
            ht.Add("@input_cardholder_suburb", "");
            ht.Add("@input_cardholder_zip_code", "");
            ht.Add("@input_card_number", "******************");
            datosSalida = new JavaScriptSerializer().Serialize(ht);
            Logueo.EntradaSalida("[AltaEmpleado.Armit.SPpostgres] [ENVIADO] [registraApp_Web] [datos a enviar en sp: efectivale_backoffice_sp_create_cardholder" + datosSalida + "]", "PROCNOC", false);
            ht.Remove("@input_card_number");
            ht.Add("@input_card_number", pNumTarjeta);

            if (BDsps.EjecutarSP_Postgres("efectivale_backoffice_sp_create_cardholder", ht, PNConfig.Get("ALTAEMPLEADO", "BDReadEfectivale")) == null)
            {
                ht.Remove("@input_card_number");
                ht.Add("@input_card_number", "******************");
                datosSalida = new JavaScriptSerializer().Serialize(ht);
                Logueo.EntradaSalida("[AltaEmpleado.Armit.SPpostgres] [RECIBIDO] [registraApp_Web] [sp: efectivale_backoffice_sp_create_cardholder" + datosSalida + "]", "PROCNOC", true);
            }
        }

        private bool enviarDatosParabilia(string pIdArchivoEntrada)
        {
            string fechaVencimiento = null;
            bool procesoFechaVencimiento = true;
            string numTarjeta = null;
            string datosSalida = null;
            SqlConnection connection = new SqlConnection(PNConfig.Get("ALTAEMPLEADO", "BDReadAutorizador"));
            try
            {
                Hashtable htProcesar = new Hashtable();
                htProcesar.Add("@idArchivo", pIdArchivoEntrada);
                Logueo.Evento("[enviarDatosParabilia] con Id de Archivo: " + pIdArchivoEntrada);
                DataTable dtDatosProcesar = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosParabiliaAltas", htProcesar, connArchivosEfectivale).Tables["Table"];
                connection.Open();
                if (dtDatosProcesar != null)
                {
                    Logueo.Evento("[Si trae registros este sp: procnoc_travel_ObtieneRegistrosParabiliaAltas] cadena de conexion: " + connArchivosEfectivale);
                    for (int contador = 0; contador < dtDatosProcesar.Rows.Count; contador++)
                    {
                        var row = dtDatosProcesar.Rows[contador];
                        numTarjeta = row["Tarjeta"].ToString();
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
                                fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                                Logueo.Evento("[Eror en la fecha de vencimiento: " + exp.Message + "] [TRACE: " + exp.StackTrace + "]");
                            }
                        }
                        Logueo.Evento("[AltaEmpleado] [enviarDatosParabilia] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Inicio de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar);
                        ParamParabiliaAltas paramParabilia = new ParamParabiliaAltas()
                        {
                            ClaveColectiva = (row["ClaveEmpleadora"].ToString().TrimEnd(' ') + row["RegistroEmpleado"].ToString().TrimEnd(' ')),
                            Nombre = row["Nombre"].ToString(),
                            Tarjeta = row["Tarjeta"].ToString(),
                            IdArchivoDetalle = row["Id_ArchivoDetalle"].ToString(),
                            NumeroCuenta = row["Cuenta"].ToString(),
                            IdEmpleadoraPadre = row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                            FechaVencimientoTarjeta = fechaVencimiento,
                            correoCFDI = row["CorreoCFDI"].ToString().Trim(),
                            correo = row["Correo"].ToString().Trim(),
                            telefono = validaString(row["Telefono"])
                        };
                        datosSalida = new JavaScriptSerializer().Serialize(paramParabilia);
                        Logueo.EntradaSalida("[AltaEmpleado.SPparabilia] [ENVIADO] [enviarDatosParabilia] [" + datosSalida + "]", "PROCNOC", false);
                        DataTable respuestaParabilia = BDsps.insertarDatosParabiliaAltas(paramParabilia, connection).Tables["Table"];
                        var codigo = respuestaParabilia.Rows[0]["Codigo"].ToString();
                        var descripcion = respuestaParabilia.Rows[0]["Descripcion"].ToString();
                        Logueo.EntradaSalida("[AltaEmpleado.SPparabilia] [RECIBIDO] [enviarDatosParabilia] [CODIGO:" + codigo + ";RESPUESTA:" + descripcion + "]", "PROCNOC", true);
                        actualizaRegistrosBDPRocesadorEfectivaleAltas(codigo, paramParabilia.IdArchivoDetalle, paramParabilia.ClaveColectiva);
                        Logueo.Evento("[AltaEmpleado.SPparabilia] [enviarDatosParabilia] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar);
                    }
                }
                if (dtDatosProcesar.Rows.Count == 0)
                {
                    DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                }
                Logueo.Evento("[AltaEmpleado.SPparabilia] [enviarDatosParabilia] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Fin de Proceso de Archivo: " + nomArchivoProcesar);
                return crearArchivoSalida(fechaVencimiento, pIdArchivoEntrada);
            }
            catch (Exception ex)
            {
                Logueo.Error("[AltaEmpleado.SPparabilia] [enviarDatosParabilia] [Error al guardar datos parabilia:" + datosSalida + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        private string validaString(Object paramString)
        {
            if (paramString == null || paramString == "")
                return "";
            return paramString.ToString();
        }

        private void actualizaRegistrosBDPRocesadorEfectivaleAltas(string pCodigo, string pIdArchivoDetalle, string pClaveColectiva)
        {
            try
            {
                if (pCodigo.Equals("00"))
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "0");
                    Logueo.Evento("[AltaEmpleado] [Se creo la colectiva: " + pClaveColectiva + "]");
                }
                else
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "1");
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[AltaEmpleado] [actualizaRegistrosBDPRocesadorEfectivaleAltas] [Error al actualizar el registro detalle con id: " + pIdArchivoDetalle + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        public void actualizaRegsAltas(string pIdArchivoDetalle, string pIdentificador)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleAltas", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesAltas", htProcesar, connArchivosEfectivale);
        }

        private bool crearArchivoSalida(string fechaVencimiento, string pIdArchivoEntrada)
        {
            Hashtable ht = new Hashtable();
            ht.Add("@IdArchivo", pIdArchivoEntrada);
            DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaAltas", ht, connArchivosEfectivale).Tables["Table"];
            string nombreArchivoSalida = directorioSalida + "\\" + datosSalida.Rows[0]["NombreArchivo"].ToString().Replace("AT", "RE") + ".txt";
            Logueo.Evento("[AltaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Inicia Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida);
            StreamWriter streamSalida = File.CreateText(nombreArchivoSalida);
            try
            {
                streamSalida.WriteLine("13" + "EMP_RES " + datosSalida.Rows[0]["FechaEnvio"].ToString() + "".PadRight(57, '0'));

                string tmpClaveEmpleadora = datosSalida.Rows[0]["ClaveEmpleadora"].ToString();
                int numAltasEmpleadora = 0, numRechazosEmpleadora = 0, numTotAltasEmpleadora = 0, numTotRechazosEmpleadora = 0, numTotEmpleadoras = 1;
                streamSalida.WriteLine("14" + tmpClaveEmpleadora.PadRight(11) + "".PadRight(62, '0'));
                for (int contador = 0; contador < datosSalida.Rows.Count; contador++)
                {
                    var row = datosSalida.Rows[contador];
                    if (tmpClaveEmpleadora != row["ClaveEmpleadora"].ToString())
                    {
                        tmpClaveEmpleadora = row["ClaveEmpleadora"].ToString();
                        streamSalida.WriteLine("93" + numAltasEmpleadora.ToString().PadLeft(5, '0') + numRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(63, '0'));
                        streamSalida.WriteLine("14" + tmpClaveEmpleadora.PadRight(11) + "".PadRight(62, '0'));
                        numTotEmpleadoras += 1;
                        numAltasEmpleadora = 0;
                        numRechazosEmpleadora = 0;
                    }
                    string cuentaTmp = SeguridadCifrado.descifrar(row["Cuenta"].ToString());
                    string cuentaArchivoSalida = "";
                    if (!string.IsNullOrEmpty(cuentaTmp))
                        cuentaArchivoSalida = cuentaTmp.Substring((cuentaTmp.Length - 5), 5);

                    if (row["ID_EstatusEvertec"].ToString() == "2" && row["ID_EstatusParabilia"].ToString() == "2")
                    {
                        streamSalida.WriteLine("2201" + row["RegistroEmpleado"].ToString().PadRight(10) + cuentaArchivoSalida.PadRight(8) + SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                                     "000".PadRight(19) + fechaVencimiento + row["IdEmpleado"].ToString().PadRight(10));
                        numAltasEmpleadora += 1;
                        numTotAltasEmpleadora += 1;
                        enviarCorreoAlta(row["Correo"].ToString().Trim(), row["CorreoCFDI"].ToString().Trim(),
                                    row["Nombre"].ToString().Trim(),
                                    row["ClaveEmpleadora"].ToString().TrimEnd(' ') + row["RegistroEmpleado"].ToString().TrimEnd(' '));
                    }
                    else
                    {
                        string codResp = row["CodigoRP"].ToString().Count() > 2
                                        ? "99"
                                        : row["CodigoRP"].ToString();
                        streamSalida.WriteLine("2201" + row["RegistroEmpleado"].ToString().PadRight(10) + cuentaArchivoSalida.PadRight(8) + SeguridadCifrado.descifrar(row["Tarjeta"].ToString()).PadRight(16) +
                                   (codResp.Equals("00") ? "99" : codResp) + "0".PadRight(19) + fechaVencimiento + row["IdEmpleado"].ToString().PadRight(10));
                        numRechazosEmpleadora += 1;
                        numTotRechazosEmpleadora += 1;
                    }

                }

                streamSalida.WriteLine("93" + numAltasEmpleadora.ToString().PadLeft(5, '0') + numRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(63, '0'));
                streamSalida.WriteLine("94" + numTotEmpleadoras.ToString().PadLeft(4, '0') + numTotAltasEmpleadora.ToString().PadLeft(5, '0') + numTotRechazosEmpleadora.ToString().PadLeft(5, '0') + "".PadRight(59, '0'));
                streamSalida.Close();
                Logueo.Evento("[AltaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] Finaliza Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida);
                return true;
            }
            catch (Exception ex)
            {
                streamSalida.Close();
                Logueo.Error("[AltaEmpleado] [crearArchivoSalida] [Error al crear el archivo de salida con Identificador: " + pIdArchivoEntrada + "] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

        private string obtenerFechaVencimiento(string pNumTarjeta)
        {
            try
            {
                WSObtenerFechaVencimiento param = new WSObtenerFechaVencimiento()
                {
                    numeroTarjeta = SeguridadCifrado.descifrar(pNumTarjeta)
                };

                string response = WebService.WSObtenerFechaVencimiento(param);


                /////////DATOS DUMMY
                //string response = "<Resultado><NumeroCedula>" + DateTime.Today.ToString("yyyyMMdd") + "</NumeroCedula><InfoCuenta>" +
                //                              "<FechaVencimientoCuenta>20251225" +
                //                           "</FechaVencimientoCuenta></InfoCuenta></Resultado>";
                XmlDocument xm = new XmlDocument();

                xm.LoadXml(response);
                Logueo.EntradaSalida("[WS_AltaEmpleado.Evertec.ConsultaVigencia] [RECIBIDO] [obtenerFechaVencimiento] [" + response.Replace("\n", " ").Replace("\r", " ") + "]", "PROCNOC", true);
                return xm.GetElementsByTagName("fechaVencimientoCuenta")[0].InnerText;
            }
            catch (Exception exp)
            {
                DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                string fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                Logueo.Evento("[Eror en la fecha de vencimiento: " + exp.Message + "] [TRACE: " + exp.StackTrace + "]");
                return fechaVencimiento;
            }
        }

        private void enviarCorreoAlta(string correo, string mailCFDI, string nombre, string ClaveColectiva)
        {
            Hashtable ht = new Hashtable();
            String nombreEmpresa = null;
            ht.Add("@ClaveColectiva", ClaveColectiva);
            DataTable datosSalida = BDsps.EjecutarSP("Procnoc_Travel_ObtieneNombreColectivaPadre", ht, PNConfig.Get("ALTAEMPLEADO", "BDReadAutorizador")).Tables["Table"];
            if (datosSalida.Rows.Count > 0)
                nombreEmpresa = datosSalida.Rows[0]["NombreORazonSocial"].ToString();
            else
                Logueo.Evento("[AltaEmpleado] " + "[No se obtuvo el Nombre a Embozar de la Colectiva padre]");
            Correo datoCorreo = new Correo()
            {
                body = construirBody(correo, mailCFDI, nombre, nombreEmpresa),
                subject = PNConfig.Get("ALTAEMPLEADO", "SubjectMail"),
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
            try
            {
                string fileName = PNConfig.Get("ALTAEMPLEADO", "DirectorioPlantilla");

                var template = File.ReadAllText(fileName);
                template = template.Replace("[NOMBREEMPRESA]", nomEmpresa);
                template = template.Replace("[EMPRESA]", nomEmpresa);
                template = template.Replace("[MAILCFDI]", mailCFDI);
                template = template.Replace("[NOMBRE]", nombre);
                return template;
            }
            catch (Exception exp)
            {
                Logueo.Error("[AltaEmpleado] [construirBody] [El archivo html para el cuerpo del correo:" + correo + " no se encuentra][Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
                return null;
            }

        }

        private DataTable crearDataTable()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaEmpleados");
            var dc = new DataColumn("ClaveEmisor", Type.GetType("System.String"));
            var dc1 = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveTransaccion", Type.GetType("System.String"));
            var dc4 = new DataColumn("RegistroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("NumeroCuenta", Type.GetType("System.String"));
            var dc7 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc8 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc9 = new DataColumn("CorreoCFDI", Type.GetType("System.String"));
            var dc10 = new DataColumn("CorreoEmpresaCFDI", Type.GetType("System.String"));
            var dc11 = new DataColumn("IdEmpleado", Type.GetType("System.String"));
            var dc12 = new DataColumn("ID_EstatusEvertec", Type.GetType("System.String"));
            var dc13 = new DataColumn("ID_EstatusParabilia", Type.GetType("System.String"));
            var dc14 = new DataColumn("ReintentosEvertec", Type.GetType("System.String"));
            var dc15 = new DataColumn("ReintentosParabilia", Type.GetType("System.String"));

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

            return dtDatosnew;
        }

        #region OperacionesFile

        public void crearDirectorio()
        {
            directorioSalida = PNConfig.Get("ALTAEMPLEADO", "PathSalida");
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
