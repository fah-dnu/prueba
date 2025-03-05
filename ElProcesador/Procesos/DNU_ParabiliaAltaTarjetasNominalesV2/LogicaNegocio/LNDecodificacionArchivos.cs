using CommonProcesador;
using Dnu_AutorizadorCacao_NCliente.LogicaNegocio;
using DNU_ParabiliaAltaTarjetasNominales.BaseDatos;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNDecodificacionArchivos
    {
        LogueoAltaEmpleadoV2 logEmpelado;
        LNInsertarDatos lnInsertarDatos;
        private string directorio;
        private string directorioSalida;
        private DataTable dtContenidoFile;
        private DataTable dtContenidoFileProducto;
        private const string expRegFecha2 = @"^\d{4}((0[1-9])|(1[012]))((0[1-9]|[12]\d)|3[01])$";
        private string tipoColectiva;
       

        public LNDecodificacionArchivos(LogueoAltaEmpleadoV2 logEmpelado, string directorio, string directorioSalida, string tipoColectiva)
        {
            this.logEmpelado = logEmpelado;
            string connArchivosCacao = PNConfig.Get("ALTAEMPLEADODNU", "BDWriteProcesadorArchivosCacao");
            lnInsertarDatos = new LNInsertarDatos(this.logEmpelado, connArchivosCacao, directorio, directorioSalida);
            this.directorio = directorio;
            this.directorioSalida = directorioSalida;
            this.tipoColectiva = tipoColectiva;
            //    this.nomArchivoProcesar = nomArchivoProcesar;
        }

        internal void decodificarArchivo(string pPath, string pNomArchivo,string nomArchivoProcesar)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            Guid idLog = Guid.NewGuid();

            try
            {
                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Inicia Validacion del Archivo: " + pPath + "" + idLog + "]");
                if (validarContenido(InfoArchivo))
                {
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Insertando informacion]");
                    lnInsertarDatos.setDtContenidoFile(dtContenidoFile, dtContenidoFileProducto,  nomArchivoProcesar);

                    if (lnInsertarDatos.insertaDatos(pNomArchivo))
                    {
                        string rutaFinal = directorio + "\\Procesados\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        LNOperacionesArchivos.moverArchivo(InfoArchivo.FullName, rutaFinal);
                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados]");
                    }
                    else
                    {
                        string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                        LNOperacionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                    }
                }
                else
                {
                    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    LNOperacionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                LNOperacionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, decodificarArchivo, El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]" + "[" + idLog + "]");
                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
            }
        }

        private bool validarContenido(FileInfo pInfoArchivo)
        {
            int counter = 0, contadorAltasTotales = 0, contadorEmpleadoras = 0, contadorAltas = 0;
            string line, fechaEnvio = null, claveCliente = null, claveBIN = null;

            bool resultHeaderA = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;

            dtContenidoFile = LNCreacionTablas.crearDataTable();
            dtContenidoFileProducto = LNCreacionTablas.crearDataTableProducto();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 361 && line.Count() != 372 && line.Count() != 522)
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, ValidarContenido, La línea numero: " + (counter + 1) + " no tinen los 372 caracteres, \\LNAltaEmpleado.cs: línea 156]");
                    file.Close();
                    return false;
                }
                if (line.StartsWith("11"))
                {
                    resultHeaderA = validaHeaderA(line);
                    fechaEnvio = line.Substring(9, 8);
                    claveCliente = line.Substring(17, 10);
                    claveBIN = line.Substring(27, 8);
                    if (!resultHeaderA)
                    {
                        file.Close();
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Error en validacion encabezado]");

                        return false;

                    }
                }
                if (line.StartsWith("21"))
                {
                    resultDetalle = validarDetalle(line, counter, claveCliente, fechaEnvio, claveBIN);
                    contadorAltasTotales += 1;
                    contadorAltas += 1;
                    if (!resultDetalle)
                    {
                        file.Close();
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Error en validacion datos]");

                        return false;
                    }
                }
                if (line.StartsWith("92"))
                {
                    resultTrailerA = validarTrailerA(line, contadorAltasTotales, contadorEmpleadoras);
                    contadorAltasTotales = 0;
                    contadorEmpleadoras = 0;
                    if (!resultTrailerA)
                    {
                        file.Close();
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Error en validacion pie archivo]");

                        return false;
                    }
                }
                counter++;
            }
            file.Close();

            logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado Obteniendo contratos]");

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

            dtContenidoFile = LNCreacionTablas.crearDataTableCredito();

            StreamReader file = new StreamReader(pInfoArchivo.FullName);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() != 383)
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, ValidarContenido, La línea numero: " + (counter + 1) + " no tinen los 361 caracteres, \\LNAltaEmpleado.cs: línea 156]");
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
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + identificacionReg + ", \\LNAltaEmpleado.cs:línea 213]");
                return false;
            }
            if (!rgxFecha.IsMatch(fechaEnvio))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + fechaEnvio + ", \\LNAltaEmpleado.cs:línea 218]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validaHeaderA, hay un error en el Header A en el campo: " + filler + ", \\LNAltaEmpleado.cs:línea 223]");
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
                DataTable dtEmpleadora = BDsps.EjecutarSP("procnoc_travel_ObtieneEmpleadora", ht, PNConfig.Get("ALTAEMPLEADODNU", "BDReadAutorizador")).Tables["Table"];
                if (dtEmpleadora.Rows.Count > 0)
                {
                    result = true;
                }
                else
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validaHeaderB, La empleadora con clave " + claveEmpleadora + " no se encuentra dada de Alta, \\LNAltaEmpleado.cs:línea 249]");
                    result = false;
                }
            }
            else
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validaHeaderB, hay un error en el Header B con empleadora:" + claveEmpleadora + ", \\LNAltaEmpleado.cs:línea 243]");
            }

            return result;
        }

        private bool validarDetalle(string pLine, int pNumLinea, string pClaveCliente, string pFechaEnvio, string pClaveBIN)
        {
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
            if (pLine.Count() == 522 || pLine.Count() == 372)
            {
                subproducto = pLine.Substring(361, 11).Trim();
            }

            //Optional fields
            string calle = pLine.Substring(371, 40).Trim();
            string noExterior = pLine.Substring(411, 15).Trim();
            string noInteriror = pLine.Substring(426, 10).Trim();
            string colonia = pLine.Substring(436, 20).Trim();
            string delegacion = pLine.Substring(456, 15).Trim();
            string ciudad = pLine.Substring(471, 15).Trim();
            string estado = pLine.Substring(486, 15).Trim();
            string cp = pLine.Substring(501, 10).Trim();
            string pais = pLine.Substring(511, 10).Trim();

            Regex rgxClaveEmpresa = new Regex("[A-Za-z0-9 _]{11}");
            Regex rgxClaveSub = new Regex("[A-Za-z0-9 _]");
            Regex rgxAlfaNumerico10 = new Regex(@"[\w+\s*]{10}");
            Regex rgxNombre = new Regex(@"([a-zA-Z])*([\s])*([a-zA-Z])+");
            Regex rgxTelefono = new Regex(@"\d{10}");
            Regex rgxTipoTarjeta = new Regex("[A|T]");
            Regex rgxTarjetaTitular = new Regex(@"\d{16}");


            if ((!string.IsNullOrEmpty(claveEmpresa)) && !rgxClaveEmpresa.IsMatch(claveEmpresa))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave empresa no cumple el formato, \\LNAltaEmpleado.cs:línea 308]");
                return false;
            }

            if (!rgxAlfaNumerico10.IsMatch(regCliente))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el numero de cliente no cumple el formato, \\LNAltaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el nombre no cumple el formato: " + nombre + ", \\LNAltaEmpleado.cs:línea 318]");
                return false;
            }
            if (!rgxTelefono.IsMatch(telefono))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado] [validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Telefono no cumple el formato: " + telefono + ", \\LNAltaEmpleado.cs:línea 328]");
                return false;
            }
            if (!rgxTipoTarjeta.IsMatch(tipoTarjeta))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Tipo Tarjeta no cumple el formato: " + tipoTarjeta + "]");
                return false;
            }
            if (tipoTarjeta.Equals("A"))
            {
                if (!rgxTarjetaTitular.IsMatch(tarjetaTitular))
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la TarjetaTitular no cumple el formato: " + tarjetaTitular + "]");
                    return false;
                }
            }

            if (pLine.Count() == 372 || pLine.Count() == 522)
            {
                if (!rgxClaveSub.IsMatch(subproducto))
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave subproducto no cumple el formato, \\LNAltaEmpleado.cs:línea 414]");
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
                                                ? pClaveCliente.Trim()
                                                : claveEmpresa.Trim(),regCliente,
                nombre, primerApellido, segundoApellido,  nombreEmbozado,  telefono, correo, TipoMedioAcceso, medioAcceso,
                tipoTarjeta, tarjetaTitular, tipoMedioAccesoTitular, medioAccesoTitular,"1", "0","",subproducto, calle, noExterior,
                noInteriror, colonia, delegacion, ciudad, estado, cp, pais});

        
                return true;
            }
            catch (Exception exp)
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, Correos inválidos en la línea " + (pNumLinea + 1) + ", Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
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
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave empresa no cumple el formato, \\LNAltaEmpleado.cs:línea 308]");
                return false;
            }

            if (!rgxAlfaNumerico10.IsMatch(regCliente))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el numero de cliente no cumple el formato, \\LNAltaEmpleado.cs:línea 313]");
                return false;
            }
            if (!rgxNombre.IsMatch(nombre))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el nombre no cumple el formato: " + nombre + ", \\LNAltaEmpleado.cs:línea 318]");
                return false;
            }
            if (!rgxTelefono.IsMatch(telefono))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Telefono no cumple el formato: " + telefono + ", \\LNAltaEmpleado.cs:línea 328]");
                return false;
            }
            if (!rgxTipoTarjeta.IsMatch(tipoTarjeta))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Tipo Tarjeta no cumple el formato: " + tipoTarjeta + "]");
                return false;
            }
            if (!rgxClaveSubproducto.IsMatch(claveSubproducto))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la clave Subproducto no cumple el formato: " + claveSubproducto + "]");
                return false;
            }
            if (!rgxLimiteCredito.IsMatch(limiteCredito))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Limite de Credito no cumple el formato: " + limiteCredito + "]");
                return false;
            }
            if (!rgxDiaCorte.IsMatch(diaCorte))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " el Dia de corte no cumple el formato: " + diaCorte + "]");
                return false;
            }
            if (tipoTarjeta.Equals("A"))
            {
                if (!rgxTarjetaTitular.IsMatch(tarjetaTitular))
                {
                    logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, hay un error en la línea no. " + (pNumLinea + 1) + " la TarjetaTitular no cumple el formato: " + tarjetaTitular + "]");
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
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarDetalle, Correos inválidos en la línea " + (pNumLinea + 1) + ", Mensaje: " + exp.Message + " TRACE: " + exp.StackTrace + "]");
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
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarTrailerB, hay un error en el Trailer B no corresponde el numero de Altas por empleadora:" + numAltas + ", \\LNAltaEmpleado.cs:línea 341]");
                return false;
            }
            if (!rgxFiller.IsMatch(filler))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarTrailerB, hay un error en el Trailer B en el filler:" + filler + ", \\LNAltaEmpleado.cs:línea 346]");
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
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarTrailerA, hay un error en el Trailer A no corresponde el número de Altas:" + numMovimientos + ", \\LNAltaEmpleado.cs:línea 367]");
                return false;
            }
            if (!(rgxFiller.IsMatch(filler)))
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, validarTrailerA, hay un error en el Trailer A no corresponde el filler:" + filler + "]");
                return false;
            }

            return true;
        }


    }
}
