using CommonProcesador;
using CrystalDecisions.Shared;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.ReporteDebito;
using DNU_ParabiliaProcesoCortes.ReporteTipado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_Multipacs;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNOperacionesCorte
    {
        //esto es para pruebas
        public static bool GeneracionPDFSinCorte(string id, string cadenaConexion, string fecha, string ruta, string rutaImagen, bool envioCorreo,
            RespuestaSolicitud respuestaSolicitud, LNEnvioCorreo _lnEnvioCorreo, DAOCortes _daoCortes, string nombrePdfCredito, string nombrePdfPrepago)
        {
            //  //funciona correcto//
            bool pdfGeneradoCorrectamente = false;
            if (!string.IsNullOrEmpty(id))
            {
                Cuentas cuentaAProcesar = null;
                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                {
                    conn2.Open();
                    servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);
                    List<Cuentas> cuentas = servicioDatos_.Obtiene_Set_deCuentas(fecha, id, true);
                    if (cuentas.Count > 0)
                    {
                        cuentaAProcesar = cuentas[0];
                    }
                    conn2.Close();
                }
                if (cuentaAProcesar != null)
                {
                    DateTime Hoy = cuentaAProcesar.FechaCorteAnterior;
                    string mes = Hoy.ToString("MM");
                    if (mes.Length == 1)
                    {
                        mes = "0" + mes;
                    }
                    string anio = Hoy.ToString("yyyy");
                    if (ruta[ruta.Length - 1] == '\\')
                    {
                        ruta = ruta + "\\";
                    }

                    ruta = ruta + cuentaAProcesar.ClaveCliente + "\\" + cuentaAProcesar.ClaveCuentahabiente + "\\" + cuentaAProcesar.ID_Cuenta + "\\" + anio + "\\" + mes + "\\";
                    //el envio de correo solo sirve en pruebas, proque aqui deberia de traer la ruta de la base no generarla
                    if (envioCorreo)
                    {
                        ruta = ruta + "EstadoDeCuenta.pdf";
                        if (File.Exists(ruta))
                        {
                            List<String> ArchivosCorreo = new List<String>();
                            ArchivosCorreo.Add(ruta);
                            if (!string.IsNullOrEmpty(cuentaAProcesar.CorreoCuentahabiente))
                            {
                                string fechaCreacionCorreo = Hoy.ToString("dd-MM-yyyy");
                                Correo _correo = new Correo
                                {
                                    asunto = "Estado de cuenta del" + fechaCreacionCorreo,
                                    correoReceptor = cuentaAProcesar.CorreoCuentahabiente,
                                    mensaje = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                    archivos = ArchivosCorreo,
                                    titulo = PNConfig.Get("PROCESAEDOCUENTA", "AsuntoCorreo"),
                                    correoEmisor = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo"),
                                    cuerpoMensaje = PNConfig.Get("PROCESAEDOCUENTA", "BodyCorreoElectronico"),
                                    host = PNConfig.Get("PROCESAEDOCUENTA", "HostCorreo"),
                                    password = PNConfig.Get("PROCESAEDOCUENTA", "PasswordCorreo"),
                                    puerto = PNConfig.Get("PROCESAEDOCUENTA", "PuertoCorreo"),
                                    usuario = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioCorreo")
                                }; //"Estado De Cuenta Cacao" };
                                pdfGeneradoCorrectamente = _lnEnvioCorreo.envioCorreo(_correo, respuestaSolicitud);
                                if (pdfGeneradoCorrectamente)
                                {
                                    respuestaSolicitud.codigoRespuesta = "0000";
                                    respuestaSolicitud.descripcionRespuesta = "Correcto";
                                }
                            }
                            else
                            {
                                respuestaSolicitud = new RespuestaSolicitud { codigoRespuesta = "0002", descripcionRespuesta = "El cuentahabiente no cuenta con correo" };
                            }
                            //insertar registro de correo y generacion en la base de datos
                            if (pdfGeneradoCorrectamente)
                            {
                                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                                {
                                    conn2.Open();
                                    servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);
                                    _daoCortes.ActualizarDatoCorreoYPDF(true, true, Convert.ToInt64(id), conn2);
                                    conn2.Close();
                                }
                            }

                        }
                        else
                        {
                            respuestaSolicitud.codigoRespuesta = "0003";
                            respuestaSolicitud.descripcionRespuesta = "No existe el Estado de cuenta";

                        }
                    }
                    else
                    {
                        if (cuentaAProcesar.ClaveCorteTipo != "MTD001")//credito
                        {
                            rutaImagen = rutaImagen + cuentaAProcesar.ClaveCliente;
                            LNOperaciones.crearDirectorio(rutaImagen);
                            LNOperaciones.crearDirectorio(ruta);
                            //DataTable dt = new DataTable("TablaCliente");
                            DataSet ds = _daoCortes.ObtenerDatosPagos(id, cadenaConexion);
                            ds = _daoCortes.ObtenerDatosCH(id, cadenaConexion, null, ds);
                            //  DataSetOperacionesEdoCuenta dataSet = new DataSetOperacionesEdoCuenta();
                            EdoCuentaTip estadoDeCuenta = new EdoCuentaTip();
                            estadoDeCuenta.SetDataSource(ds);
                            estadoDeCuenta.SetParameterValue("imagenLogo", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                            estadoDeCuenta.SetParameterValue("CAT", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\CAT.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                            estadoDeCuenta.SetParameterValue("UNE", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\UNE.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");

                            estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + (string.IsNullOrEmpty(nombrePdfCredito) ? "EstadoDeCuenta.pdf" : nombrePdfCredito));
                            estadoDeCuenta.Close();
                            estadoDeCuenta.Dispose();
                            pdfGeneradoCorrectamente = true;
                        }
                        else
                        {//debito
                            rutaImagen = rutaImagen + cuentaAProcesar.ClaveCliente;
                            LNOperaciones.crearDirectorio(rutaImagen);
                            LNOperaciones.crearDirectorio(ruta);
                            //DataTable dt = new DataTable("TablaCliente");
                            DataSet ds = _daoCortes.ObtenerDatosPagosDebito(id, cadenaConexion);
                            ds = _daoCortes.ObtenerDatosCHDebito(id, cadenaConexion, null, ds);
                            //  DataSetOperacionesEdoCuenta dataSet = new DataSetOperacionesEdoCuenta();
                            EdoCuentaTipDebito estadoDeCuenta = new EdoCuentaTipDebito();
                            estadoDeCuenta.SetDataSource(ds);
                            estadoDeCuenta.SetParameterValue("imagenLogo", rutaImagen + "\\logo.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                            estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + (string.IsNullOrEmpty(nombrePdfPrepago) ? "ResumenDeMovimientos.pdf" : nombrePdfPrepago));
                            estadoDeCuenta.Close();
                            estadoDeCuenta.Dispose();
                            pdfGeneradoCorrectamente = true;
                        }
                    }
                }
                else
                {
                    if (envioCorreo)
                    {
                        respuestaSolicitud = new RespuestaSolicitud { codigoRespuesta = "0001", descripcionRespuesta = "No se encontró la cuenta para procesar" };
                    }
                }
                //ReportDocument cryRpt = new ReportDocument();
                ////////  //fin
                //  //    ExportOptions CrExportOptions;
                //  //    DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
                //  //    PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
                //  //    CrDiskFileDestinationOptions.DiskFileName = "c:\\csharp.net-informations.pdf";
                //  //    CrExportOptions = estadoDeCuenta.expo
                //  //    {
                //  //        CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                //  //        CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                //  //        CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
                //  //        CrExportOptions.FormatOptions = CrFormatTypeOptions;
                //  //    }
                //  //    cryRpt.Export();

            }
            return pdfGeneradoCorrectamente;
        }

        public static bool generarFactura(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<String> archivos, Cuentas cuenta, XslCompiledTransform _transformador)
        {
            DateTime Hoy = cuenta.Fecha_Corte;//DateTime.Now;

            bool facturaGeneradoCorrectamente = false;
            LNOperaciones lnOperaciones = new LNOperaciones();
            LNOperaciones.crearDirectorio(ruta);
            CFDI xmlMySuit = new CFDI(factura, facturaEnBlanco);
            string emisor = factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            string key = factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            string password = factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");
            String noCertificado = "";
            //
            //Timbrado //
            if (factura.PACFacturacion.Contains("Facturama"))
            {
                Timbrado timbrado = new Timbrado();
                var timbre = timbrado.Facturama(factura);

                if (timbre.Error == null) {
                    return true;
                } else
                {
                    return false;
                }                
            }
            
            String certificado = "";
            string elXML = xmlMySuit.ToString(); //File.ReadAllText(textXML);
            string CadenaOriginal = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            factura.CadenaOriginal = CadenaOriginal;
            String elSello = LNOperacionesEdoCuenta.ObtenerSelloFactura(CadenaOriginal, out noCertificado, out certificado, emisor, key, password);
            factura.Sello = elSello;
            elXML = elXML.Replace("[Sello]", elSello);
            elXML = elXML.Replace("[NoCertificado]", noCertificado);
            elXML = elXML.Replace("[Certificado]", certificado);
            factura.NoCertificadoEmisor = noCertificado;

            //text = text.Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            // CFDI xmlMySuit = new CFDI("");
            //    string xml = xmlMySuit.ToString();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(elXML);
            xDoc.Save(ruta + "Factura_Original.xml");
            // archivos.Add(ruta + "Factura_Timbrada.xml");
            wsFacturacionMySuit.FactWSFront unaOperacion = new wsFacturacionMySuit.FactWSFront();
            com.mysuitetest.www.FactWSFront unaOperacionTest = new com.mysuitetest.www.FactWSFront();
            // string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
            // TransactionTag laRespuesta = unaOperacion.RequestTransaction("", "TIMBRAR", "MX", "", "", "", xml, "", "");
            string cadenaMySuit = PNConfig.Get("PROCESAEDOCUENTA", "RutaSoapMySuit");
            dynamic laRespuesta = null;
            if (!facturaEnBlanco)
            {
                if (cadenaMySuit.ToLower().Contains("mysuitetest"))
                {
                    laRespuesta = unaOperacionTest.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", /*factura.Emisora.RFC*/"AAA010101AAA", factura.RequestorPAC, factura.UserPAC, elXML, "", "");
                     //laRespuesta = unaOperacionTest.RequestTransaction("0cfbbd00-ac21-47d9-8776-956c0f44254c", "TIMBRAR", "MX", "SAMP570517JB1", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");

                }
                else
                {
                    laRespuesta = unaOperacion.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", factura.Emisora.RFC, factura.RequestorPAC, factura.UserPAC, elXML, "", "");
                    //                    laRespuesta = unaOperacion.RequestTransaction("0cfbbd00-ac21-47d9-8776-956c0f44254c", "TIMBRAR", "MX", "SAMP570517JB1", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");

                }

                //com.mysuitetest.www.TransactionTag laRespuesta = unaOperacionTest.RequestTransaction("0cfbbd00-ac21-47d9-8776-956c0f44254c", "TIMBRAR", "MX", "SAMP570517JB1", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");
                if (laRespuesta.ResponseData.ResponseData3.Equals("OK", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Fue Autorizada y se genero el timbre.
                    Byte[] resp = Convert.FromBase64String(laRespuesta.ResponseData.ResponseData1);
                    String unTimbrazo = System.Text.Encoding.UTF8.GetString(resp);
                    Timbre elTimbre = new Timbre(unTimbrazo);
                    //Poner
                    elXML = elXML.Replace("</cfdi:Conceptos>", "</cfdi:Conceptos>\r <cfdi:Complemento>\r" + unTimbrazo.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "") + "\r </cfdi:Complemento>");
                    xDoc.LoadXml(elXML);
                    // Hoy = DateTime.Now;
                    // string fecha = Hoy.ToString("ddMMyyyyHHmmss");
                    xDoc.Save(ruta + "Factura_Timbrada.xml");//+ fecha + ".xml");
                    archivos.Add(ruta + "Factura_Timbrada.xml");
                    facturaGeneradoCorrectamente = true;

                    factura.CadenaOriginalTimbre = "||" + elTimbre.version + "|" + elTimbre.UUID + "|" + elTimbre.FechaTimbrado + "|" + elTimbre.RfcProvCertif + "|" + elTimbre.selloCFD + "|" + elTimbre.noCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);
                    //factura.LugarExpedicion = factura.LugarExpedicion;// laFactura.Emisora.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio;
                    factura.XMLCFDI = elXML;
                    //MX.DDN1106065K2.DNU
                    //generar el archvio XML
                    //Genera el XML
                    //if (laFactura.TipoComprobante.ToUpper().Equals("INGRESO") || laFactura.TipoComprobante.ToUpper().Equals("I"))
                    //{
                    //    String unPath = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;
                    //    XmlDocument xDoc = new XmlDocument();
                    //    xDoc.LoadXml(laFactura.XMLCFDI);
                    //    xDoc.Save(unPath + "Factura_" + laFactura.Serie + laFactura.Folio + ".xml");
                    //}

                    factura.XMLTimbre = unTimbrazo;
                    factura.NoCertificadoSAT = elTimbre.noCertificadoSAT;
                    factura.SelloCFD = elTimbre.selloCFD;
                    factura.SelloSAT = elTimbre.selloSAT;
                    factura.UUID = elTimbre.UUID;
                    factura.FechaTimbrado = DateTime.Parse(elTimbre.FechaTimbrado);
                    factura.RFCProvedorCertificado = elTimbre.RfcProvCertif;
                    //  laFactura.UrlQrCode = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "QR_" + laFactura.Serie + laFactura.Folio + ".png";

                    //DAOFactura.ActualizaFactura(laFactura);


                }
                else
                {

                    //Loguear.EntradaSalida(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3, "", true);
                    if (laRespuesta.ResponseData.ResponseData2.Trim().Length != 0)
                        throw new Exception(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3);
                    else
                        throw new Exception(laRespuesta.Response.Code + ": " + laRespuesta.Response.Description + "; " + laRespuesta.Response.Data);

                }
            }
            else
            {
                facturaGeneradoCorrectamente = true;
            }
            return facturaGeneradoCorrectamente;
        }
    }
}
