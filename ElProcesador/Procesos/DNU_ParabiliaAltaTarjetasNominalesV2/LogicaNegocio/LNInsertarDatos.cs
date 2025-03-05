using CommonProcesador;
using DNU.Cifrado.DES;
using Dnu_AutorizadorCacao_NCliente.LogicaNegocio;
using DNU_ParabiliaAltaTarjetasNominales.BaseDatos;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using Interfases.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNInsertarDatos
    {
        LogueoAltaEmpleadoV2 logEmpelado;
        string connArchivos;
        private string directorio;
        private string directorioSalida;
        string nomArchivoProcesar;

        public LNInsertarDatos(LogueoAltaEmpleadoV2 logEmpelado, string connArchivos, string directorio, string directorioSalida)
        {
            this.logEmpelado = logEmpelado;
            this.connArchivos = connArchivos;
            this.directorio = directorio;
            this.directorioSalida = directorioSalida;
        }

        private DataTable dtContenidoFile;
        private DataTable dtContenidoFileProducto;
        internal void setDtContenidoFile(DataTable dtContenidoFile, DataTable dtContenidoFileProducto, string nomArchivoProcesar)
        {
            this.dtContenidoFile = dtContenidoFile;
            this.dtContenidoFileProducto = dtContenidoFileProducto;
            this.nomArchivoProcesar = nomArchivoProcesar;
        }


        internal bool insertaDatos(string pNomArchivo, bool esCredito = false, Guid? idLog = null)
        {
            bool result = false;
            Hashtable htFile = new Hashtable();
            htFile.Add("@descripcion", "Archivo de Alta Empleados");
            htFile.Add("@claveProceso", "ALTAEMPLEADODNU");
            htFile.Add("@nombre", pNomArchivo);
            htFile.Add("@tipoArchivo", ".txt");
            string idFile = BDsps.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, connArchivos).Tables["Table"].Rows[0]["ID_Archivo"].ToString();
            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);
            DataTable tmpDatosInsert = null;

            logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Bulk info]");
            bool insertarDatosBulk = BDsps.bulkInsertarDatosArchivoDetalle(dtContenidoFile, connArchivos, "ArchivoDetalleAltas", idFile);
            logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Actualizando info bulk]");
            tmpDatosInsert = BDsps.EjecutarSP("procnoc_travel_InsertaContenidoAltas", ht, connArchivos).Tables["Table"];

            string tipoManuFactura = pNomArchivo.Substring(10, 3);

            if (tmpDatosInsert != null)
            {
                if (tmpDatosInsert.Rows.Count == 0)
                {
                    return enviarDatosParabiliaVerBovedadigital(idFile, pNomArchivo);//en este caso el corte de debito se hara dentro del sps
                }
                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Creando tarjeta]");
                if (tipoManuFactura.ToUpper().Equals("_V_"))
                {
                    result = enviaDatosBovedaDigial(tmpDatosInsert, idFile, pNomArchivo, dtContenidoFileProducto);
                }
                else
                {
                    result = enviaDatosBovedaDigial(tmpDatosInsert, idFile, pNomArchivo, dtContenidoFileProducto);
                }
            }

            return result;
        }

        private bool enviaDatosBovedaDigial(DataTable pDtContenidoFile, string pIdArchivo, string nomFile, DataTable tContenidoFileProducto)
        {
            XmlDocument xm = new XmlDocument();
            SqlConnection connection = new SqlConnection(connArchivos);
            SqlConnection connectionParabilia = new SqlConnection(PNConfig.Get("ALTAEMPLEADODNU", "BDReadAutorizador"));
            DataTable respValidarTarjetaTitular = null;
            bool respValidarTitular = true;
            string tarjetaTitular = null;
            try
            {


                connection.Open();
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {
                    string response = null;
                    string codRespuesta = "1";
                    string desRespuesta = "Datos Incorrectos";
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
                                string tarjeta = "";
                                Random rand = new Random();
                                int numero = rand.Next(25);
                                char letra = (char)(((int)'A') + numero);
                                var random = new Random();
                                string idSolicitud = random.Next(1, 99999999).ToString();
                                idSolicitud = letra + idSolicitud.PadLeft(9, '0');
                                string credencialesBase64ToString = "";//Encoding.UTF8.GetString(Convert.FromBase64String(credenciales));

                                string urlBovedaDigital = PNConfig.Get("ALTAEMPLEADODNU", "URLBovedaDigital");
                                string credenciales = PNConfig.Get("ALTAEMPLEADODNU", "CredencialesBovedaDigital");
                                credenciales = AzureExtensions.ObtenerValorSecretoAzure(credenciales, "", "", logEmpelado);
                                string clave = PNConfig.Get("ALTAEMPLEADODNU", "SecretKeyDW");
                                string longitudClave = PNConfig.Get("ALTAEMPLEADODNU", "InitializationVectorDW");

                                credencialesBase64ToString = Encoding.UTF8.GetString(Convert.FromBase64String(credenciales));

                                string user = credencialesBase64ToString.Split(':')[0];
                                string pass = credencialesBase64ToString.Split(':')[1];
                                string tempTipoMF = nomFile.Substring(10, 2);
                                if (tempTipoMF.ToUpper().Equals("_V"))
                                {
                                    tempTipoMF = "VIR";
                                }
                                else
                                {
                                    tempTipoMF = "FIS";
                                }
                                RequestAddLot requestAddlot = new RequestAddLot
                                {
                                    items = "1",
                                    lot_type_key = "Sol",
                                    manufacturing_type_key = tempTipoMF,
                                    sub_bins_group_key = row["Subproducto"].ToString(),
                                    user = user,
                                    lot_key = idSolicitud
                                   
                                };
                                Response respuestaWebService = new Response();
                                logEmpelado.Info("[api/AltaTarjetaBD/][Entrada]" + JsonConvert.SerializeObject(requestAddlot));
                                LNConexionWSExternos lnConexionWSExternos = new LNConexionWSExternos();

                                Hashtable htHeader = new Hashtable();
                                htHeader.Add("credentials", credenciales);

                                var responseWs =
                                    lnConexionWSExternos.PostWithLoginDW(requestAddlot, user, pass, credenciales,
                                    $"{urlBovedaDigital}", respuestaWebService, htHeader, logEmpelado, $"{urlBovedaDigital}/api/lots").GetAwaiter().GetResult();


                                logEmpelado.Info(" [api/AltaTarjetaBD/][Entrada]" + respuestaWebService.CodRespuesta);

                                var bodyResponse = JsonConvert.DeserializeObject<ResponseAddLot>(responseWs);
                                if (respuestaWebService.CodRespuesta != HttpStatusCode.OK.ToString() &&
                                    respuestaWebService.CodRespuesta != HttpStatusCode.Created.ToString() &&
                                    respuestaWebService.CodRespuesta != HttpStatusCode.Accepted.ToString())
                                {
                                    var bodyResponseError = JsonConvert.DeserializeObject<ResponseWs>(responseWs);
                                    if (string.IsNullOrEmpty(bodyResponseError.code_response))
                                    {
                                        respuestaWebService.CodRespuesta = "9999";
                                        respuestaWebService.DescRespuesta = "Error desconocido, consultar con administrador";
                                        logEmpelado.Error("[api/AltaTarjetaBD/] [" + JsonConvert.SerializeObject(respuestaWebService) + "]");
                                        return false;
                                    }
                                    else
                                    {
                                        respuestaWebService.CodRespuesta = bodyResponseError.code_response;
                                        respuestaWebService.DescRespuesta = bodyResponseError.desc_response;
                                        logEmpelado.Error("[api/AltaTarjetaBD/] [" + JsonConvert.SerializeObject(respuestaWebService) + "]");
                                        return false;
                                    }

                                }
                                else
                                {
                                    tarjeta = bodyResponse.values[0].encrypted_value.ToString();
                                    if (!string.IsNullOrEmpty(tarjeta))
                                    {
                                        codRespuesta = "00";
                                        desRespuesta = "Datos Correctos";
                                    }
                                }

                                //decript tarjeta
                                string secretKey = AzureExtensions.ObtenerValorSecretoAzure(clave, "", "", logEmpelado);// Configuration["secretKey"];
                                string initializationVector = AzureExtensions.ObtenerValorSecretoAzure(longitudClave, "", "", logEmpelado); //Configuration["initializationVector"];
                                Cifrador cifrador = new Cifrador(secretKey, initializationVector);
                                tarjeta = cifrador.Descifrar(tarjeta);
                                String numeroCuenta = "";
                                try
                                {
                                    numeroCuenta = bodyResponse.values[0].encrypted_account;
                                    if (!string.IsNullOrEmpty(numeroCuenta))
                                    {
                                        numeroCuenta = cifrador.Descifrar(numeroCuenta);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    numeroCuenta = "";
                                    logEmpelado.Error("[api/AltaTarjetaPluginEvertec/] [" + ex.Message + "] [" + ex.StackTrace + "]");

                                }

                                string tmpNumTarjeta = tarjeta; 
                                string tmpNumCuenta = numeroCuenta;
                                String tmpNombreEmbozo = row["NombreEmbozado"].ToString().Length > 21 ?
                                        row["NombreEmbozado"].ToString().Substring(0, 21) :
                                        row["NombreEmbozado"].ToString().PadRight(21);

                                response = "<Resultado><Fecha>" + DateTime.Today.ToString("yyyyMMdd") + "</Fecha><Hora>" + DateTime.Now.ToString("HHmmss") +
                                                    "</Hora><Cuenta>" + tmpNumCuenta + "</Cuenta><Tarjeta>" + tmpNumTarjeta + "</Tarjeta>" +
                                                    "<Nombre>" + tmpNombreEmbozo + "</Nombre><Identificacion>" + idSolicitud + "</Identificacion><Respuestas>" +
                                                       "<Respuesta><Codigo>" + codRespuesta + "</Codigo><Descripcion>" + desRespuesta + "</Descripcion>" +
                                                    "</Respuesta></Respuestas></Resultado>";

                                response = response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#xD;", "");
                                xm = new XmlDocument();
                                xm.LoadXml(response);
                                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [WS_AltaEmpleado.CACAO.Prospectación] [RECIBIDO] [enviaDatosEvertec] [NOMBRE:" + xm.GetElementsByTagName("Nombre")[0].InnerText + ";FECHA:"
                                    + xm.GetElementsByTagName("Fecha")[0].InnerText + ";HORA:"
                                    + xm.GetElementsByTagName("Hora")[0].InnerText + ";Código:"
                                    + xm.GetElementsByTagName("Codigo")[0].InnerText + ";Descripción"
                                    + xm.GetElementsByTagName("Descripcion")[0].InnerText + "]", "PROCNOC", true + "]");

                                BDsps.insertarRespuestaAltas(xm, connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Tarjeta generada]");

                            }
                            else
                            {
                                BDsps.insertarRespuestaValidarTitular(respValidarTarjetaTitular.Rows[0]["Codigo"].ToString(),
                                                        respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString(),
                                                        connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [WS_AltaEmpleado.CACAO.Prospectación] [validarTarjetaTitular] [Codigo: " + respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() +
                                                                "] [Descripcion: " + respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString() + "]");
                            }

                        }
                        else
                        {
                            BDsps.insertarRespuestaAltasReproceso(connection, row["Id_ArchivoDetalle"].ToString()
                                        , string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), row["RegistroEmpleado"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Resulto un error al convertir la respuesta " + response + " en XML:" + ex.Message + ", " + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Error al guardar respuesta CACAO, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
                connectionParabilia.Close();
            }
            return enviarDatosParabiliaVerBovedadigital(pIdArchivo, nomFile);
        }

        private bool enviarDatosParabiliaVerBovedadigital(string pIdArchivoEntrada, string nomArchivo)
        {
            logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Guardabdo info tarjeta]");

            string fechaVencimiento = null;
            bool procesoFechaVencimiento = true;
            string numTarjeta = null, tipoTarjeta = null, tarjetaTitular = null;
            string datosSalida = null;
            DataTable respuestaParabilia;

            SqlConnection connection = new SqlConnection(PNConfig.Get("ALTAEMPLEADODNU", "BDReadAutorizador"));
            try
            {
                Hashtable htProcesar = new Hashtable();
                htProcesar.Add("@idArchivo", pIdArchivoEntrada);
                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [enviarDatosParabilia, con Id de Archivo: " + pIdArchivoEntrada + "]");
                DataTable dtDatosProcesar = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosAltas", htProcesar, connArchivos).Tables["Table"];
                connection.Open();
                if (dtDatosProcesar != null)
                {
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [El SP procnoc_travel_ObtieneRegistrosCACAOAltas trae " + dtDatosProcesar.Rows.Count + " registros]");
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
                                fechaVencimiento = "";//obtenerFechaVencimiento(numTarjeta);
                                procesoFechaVencimiento = false;
                            }
                            catch (Exception exp)
                            {
                                DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(5).ToString("yyyyMMdd");
                                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [Error en la fecha de vencimiento: " + exp.Message + ", TRACE: " + exp.StackTrace + "]");
                                //Logueo.Evento("[Eror en la fecha de vencimiento: " + exp.Message + "] [TRACE: " + exp.StackTrace + "]");
                            }
                        }
                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Inicio de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                        
                        string tempTipoMF = nomArchivo.Substring(10, 2);
                        if (tempTipoMF.ToUpper().Equals("_V"))
                        {
                            tempTipoMF = "V";
                        }
                        else
                        {
                            tempTipoMF = "F";
                        }
                        fechaVencimiento = GetFechaVencimiento();
                        AltasRequest paramParabilia = new AltasRequest()
                        {
                            ClaveColectiva = ((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' ')) + row["RegistroEmpleado"].ToString().TrimEnd(' ')),
                            Nombre = row["Nombre"].ToString(),
                            ClaveEmpresaPadre = string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                               : row["ClaveEmpleadora"].ToString().TrimEnd(' '),
                            FechaVencimientoTarjeta = fechaVencimiento,
                            correo = row["Correo"].ToString().Trim(),
                            telefono = validaString(row["Telefono"]),
                            TipoMedioAcceso = row["TipoMedioAcceso"].ToString(),
                            MedioAcceso = row["MedioAcceso"].ToString(),
                            Tarjeta = row["Tarjeta"].ToString(),
                            NumeroCuenta = "",
                            NomEmbozado = row["NombreEmbozado"].ToString(),

                            PrimerApellido = row["PrimerApellido"].ToString(),
                            SegundoApellido = row["SegundoApellido"].ToString(),
                            ClaveProducto = "",
                            TipoManufactura = tempTipoMF,
                            SubProducto = row["SubProducto"].ToString(),

                            //Datos Dirección
                            Calle = row["Calle"].ToString(),
                            NoExterior = row["NoExterior"].ToString(),
                            NoInterior = row["NoInterior"].ToString(),
                            Colonia = row["Colonia"].ToString(),
                            DelegacionMun = row["DelegacionMun"].ToString(),
                            Ciudad = row["Ciudad"].ToString(),
                            Estado = row["Estado"].ToString(),
                            CP = row["CP"].ToString(),
                            Pais = row["Pais"].ToString()
                        };

                        string idArchivo = row["Id_ArchivoDetalle"].ToString();

                        datosSalida = new JavaScriptSerializer().Serialize(paramParabilia);
                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, ENVIADO, enviarDatosParabilia, " + datosSalida + ", ", "PROCNOC", false + "]");
                   
                        respuestaParabilia = BDsps.insertarDatosParabiliaAltasV2(paramParabilia, connection).Tables["Table"];

                        if (paramParabilia.correo.Contains("|"))
                        {
                            paramParabilia.correo = paramParabilia.correo.Split('|')[0];
                        }
                        var codigo = respuestaParabilia.Rows[0]["Codigo"].ToString();
                        var descripcion = respuestaParabilia.Rows[0]["Descripcion"].ToString();
                        if (codigo == "00")
                        {
                            string nuevacuenta = respuestaParabilia.Rows[0]["ctaInterna"].ToString();//respuestaParabilia.Rows[0]["ctaCacao"].ToString();
                            string idMedioAcceso = respuestaParabilia.Rows[0]["idMa"].ToString();
                            string tipoMedioAcceso = respuestaParabilia.Rows[0]["tipoMA"].ToString();
                            var tablaAltaCuenta = BDsps.insertarDatosMA(nuevacuenta, idMedioAcceso, tipoMedioAcceso, connection).Tables["Table"];
                            codigo = tablaAltaCuenta.Rows[0]["tipo"].ToString();
                           
                            if (codigo == "correcto")
                            {
                                codigo = "00";
                            }
                            else
                            {
                                descripcion = tablaAltaCuenta.Rows[0]["error"].ToString();
                                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Error al guardar datos de MA, Codigo:" + codigo + " Mensaje: " + descripcion + " Fila:" + contador + "]");
                            }

                        }
                        else
                        {
                            logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Error al guardar datos parabilia, Codigo:" + codigo + " Mensaje: " + descripcion + " Fila:" + contador + "]");
                        }

                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia,RECIBIDO, enviarDatosParabilia, CODIGO:" + codigo + ";RESPUESTA:" + descripcion + ", ", "PROCNOC", true + "]");
                        actualizaRegistrosBDPRocesadorAltas(codigo, idArchivo, paramParabilia.ClaveColectiva);
                        logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Registro " + (contador + 1) + " del Archivo: " + nomArchivoProcesar + "]");
                    }
                }
                if (dtDatosProcesar != null && dtDatosProcesar.Rows.Count == 0)
                {
                    DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(3).ToString("yyyyMMdd");
                }
                logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, ID_ARCHIVO:" + pIdArchivoEntrada + " Fin de Proceso de Archivo: " + nomArchivoProcesar + "]");
                return crearArchivoSalida(fechaVencimiento, pIdArchivoEntrada);
            }
            catch (Exception ex)
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado.SPparabilia, enviarDatosParabilia, Error al guardar datos parabilia:" + datosSalida + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        
        private string GetFechaVencimiento()
        {
            DateTime fechainicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            string fechaVencimiento = fechainicio.AddMonths(1).AddDays(-1).AddYears(5).ToString("yyyyMMdd");

            return fechaVencimiento;
        }
        
        private string validaString(Object paramString)
        {
            if (paramString == null || paramString == "")
                return "";
            return paramString.ToString();
        }

        private void actualizaRegistrosBDPRocesadorAltas(string pCodigo, string pIdArchivoDetalle, string pClaveColectiva)
        {
            try
            {
                if (pCodigo.Equals("00"))
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "0");
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Se creo la colectiva: " + pClaveColectiva + "]");
                }
                else
                {
                    actualizaRegsAltas(pIdArchivoDetalle, "1");
                }
            }
            catch (Exception ex)
            {
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, actualizaRegistrosBDPRocesadorCacaoAltas, Error al actualizar el registro detalle con id: " + pIdArchivoDetalle + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
        }

        public void actualizaRegsAltas(string pIdArchivoDetalle, string pIdentificador)
        {
            Hashtable htProcesar = new Hashtable();
            htProcesar.Add("@IdArchivoDetalleAltas", pIdArchivoDetalle);
            htProcesar.Add("@identificador", pIdentificador);
            BDsps.EjecutarSP("procnoc_travel_ActualizaDetallesAltas", htProcesar, connArchivos);
        }
      
        private bool crearArchivoSalida(string fechaVencimiento, string pIdArchivoEntrada)
        {

            Hashtable ht = new Hashtable();
            bool validaError = false;
            ht.Add("@IdArchivo", pIdArchivoEntrada);
            DataTable datosSalida = BDsps.EjecutarSP("procnoc_travel_ObtieneRegistrosArchivoSalidaCACAOAltas", ht, connArchivos).Tables["Table"];
            //string nombreArchivoSalida = directorioSalida + "\\" + datosSalida.Rows[0]["NombreArchivo"].ToString().Replace("AT", "RE") + ".txt";
            string nombreArchivoSalida = directorioSalida + "\\PROCESADO_" + datosSalida.Rows[0]["NombreArchivo"].ToString() + ".txt";
            logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado] [ID_ARCHIVO:" + pIdArchivoEntrada + "] [Inicia Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
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
                        cuentaArchivoSalida = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CTAINT");
                    }
                    catch (Exception ex)
                    {
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + ", AltaEmpleado, crearArchivoSalida, No se pudo obtener la clave del tipo CACAO: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                    }
                    try
                    {
                        cuentaClabe = BDsps.obtieneClaveMACacaoFromTarjeta(SeguridadCifrado.descifrar(row["Tarjeta"].ToString()), "CLABE");
                    }
                    catch (Exception ex)
                    {
                        logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, crearArchivoSalida, No se pudo obtener la CLABE: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
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
                    LNOperacionesArchivos.moverArchivo(nombreArchivoSalida, pathArchivosInvalidos);
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, Archivo " + pathArchivosInvalidos + " finalizo de procesar y se envía a carpeta \\Erroneos]");
                }
                else
                {
                    logEmpelado.Info("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, ID_ARCHIVO:" + pIdArchivoEntrada + " Finaliza Generación de Archivo de Respuesta del Archivo" + nomArchivoProcesar + " con nombre: " + nombreArchivoSalida + "]");
                }

                return true;
            }
            catch (Exception ex)
            {
                streamSalida.Close();
                logEmpelado.Error("[" + "" + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + "" + "] [AltaEmpleado, crearArchivoSalida, Error al crear el archivo de salida con Identificador: " + pIdArchivoEntrada + ", Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                return false;
            }
        }

    }
}
