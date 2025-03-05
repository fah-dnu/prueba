
#define PIEK
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
using Renci.SshNet;
using Renci.SshNet.Common;
using DNU_SFTP.Services;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNOperacionesCorte
    {

        public static bool GeneracionPDFTimbreSinCorteV2(string id, string cadenaConexion, string fecha, string ruta, string rutaImagen, bool envioCorreo,
            RespuestaSolicitud respuestaSolicitud, LNEnvioCorreo _lnEnvioCorreo, DAOCortes _daoCortes, string nombrePdfCredito, string nombrePdfPrepago, bool esDebito, DAOFacturas _daoFacturas = null, XslCompiledTransform _transformador = null)
        {
            //  //funciona correcto//
            bool pdfGeneradoCorrectamente = false;
            bool xmlTimbrado = false;
            if (!string.IsNullOrEmpty(id))

            {

                if (!esDebito)
                {
                    LNValidacionesCampos _validaciones = new LNValidacionesCampos();
                    Logueo.Evento("Buscando factura relacionada al corte");
                    //credito
                    Response errores = new Response();
                    try
                    {
                        cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizadorTimbreManual");
                        cadenaConexion = cadenaConexion.Replace("10.0.0.5", "20.225.175.141");
                        using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                        {
                            //  DataTable tablaNuevoCorte1 = _daoCortes.ObtenerFacturaV1("20577", ruta, conn2);
                            DataTable dt = _daoCortes.ObtenerDatosCortes("EJECUTOR_ObtieneDatosParaTxt", null, conn2);
                            id = dt.Rows[0]["ID_CORTE"].ToString();
                            Logueo.Evento("Buscando factura relacionada al corte");
                            conn2.Open();
                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");
                            //sin timbtrar4
                            rutaImagen = rutaImagen.Replace("D:", "C:");
                            //pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", true);
                            //pdf timbrado ya hecho
                            // pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", false);


                            //timbrando
                            DataTable tablaNuevoCorte = _daoCortes.ObtenerFacturaV1(id, ruta, conn2);
                            if (_validaciones.BusquedaSinErrores(errores, tablaNuevoCorte))
                            {
                                //obteniendo los detalles
                                String idFactura = tablaNuevoCorte.Rows[0]["ID_Factura"].ToString();
                                Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");
                                Decimal IVAFac1 = Convert.ToDecimal(tablaNuevoCorte.Rows[0]["IVA"].ToString());
                                DataTable tablaDetalles = _daoCortes.ObtenerFacturaDetalles(idFactura, ruta, conn2);
                                if (_validaciones.BusquedaSinErrores(errores, tablaDetalles))
                                {
                                    Factura laFactura = new Factura();
                                    List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                                    foreach (DataRow row in tablaDetalles.Rows)
                                    {
                                        Decimal ivaB = Convert.ToDecimal(row["IVA"].ToString());
                                        Decimal iva = Convert.ToDecimal(row["ImporteIVA"].ToString());
                                        Decimal total = Convert.ToDecimal(row["Total"].ToString());
                                        String decimasTotal = total.ToString("########0.00");
                                        DetalleFactura detalle = new DetalleFactura { Cantidad = Convert.ToInt32(row["Cantidad"].ToString()), Unidad = row["Unidad"].ToString(), ClaveProdServ = row["ClaveProdServ"].ToString(), ClaveUnidad = row["Clave"].ToString(), NombreProducto = row["Descripcion"].ToString(), impImporte = iva.ToString("########0.00"), Total = Convert.ToDecimal(decimasTotal), PrecioUnitario = Convert.ToDecimal(decimasTotal), impBase = total.ToString("########0.00"), impImpuesto = row["ClaveImpuesto"].ToString(), impTipoFactor = row["claveTF"].ToString(), impTasaOCuota = ivaB.ToString("##0.000000") };
                                        listaDetallesFactura.Add(detalle);
                                    }
                                    laFactura.losDetalles = listaDetallesFactura;

                                    laFactura.LugarExpedicion = tablaNuevoCorte.Rows[0]["LugarExpedicion"].ToString();
                                    laFactura.MetodoPago = tablaNuevoCorte.Rows[0]["MetodoPago"].ToString();
                                    laFactura.TipoComprobante = tablaNuevoCorte.Rows[0]["TipoComprobante"].ToString();
                                    laFactura.FormaPago = tablaNuevoCorte.Rows[0]["formaDePago"].ToString();
                                    DateTime Hoy = DateTime.Now;
                                    string fechaConvetida = Hoy.ToString("dd/MM/yyyy HH:mm:ss");  //Hoy.ToString("dd/MM/yyyy hh:mm:ss tt");//HH
                                    laFactura.FechaEmision = Convert.ToDateTime(fechaConvetida);
                                    //DateTime fechaEmision = Convert.ToDateTime(tablaNuevoCorte.Rows[0]["FechaEmision"].ToString());
                                    //  string fechaConvetida = fechaEmision.ToString("dd/MM/yyyy HH:mm:ss");  //Hoy.ToString("dd/MM/yyyy hh:mm:ss tt");//HH
                                    //   laFactura.FechaEmision = Convert.ToDateTime(fechaConvetida); //DateTime.ParseExact(fechaConvetida, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);//Convert.ToDateTime(fechaConvetida);
                                    laFactura.RequestorPAC = PNConfig.Get("PROCESAEDOCUENTA", "RequestorPACTimbre");
                                    laFactura.UserPAC = PNConfig.Get("PROCESAEDOCUENTA", "UserNamePACTimbre");
                                    laFactura.UserPassPAC = PNConfig.Get("PROCESAEDOCUENTA", "PassUserNamePACTimbre");
                                    string nombrePAC = PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
                                    laFactura.URLProvedorCertificado = LNFacturas.obtenerURLPACFacturacion(nombrePAC, laFactura);
                                    laFactura.Folio = tablaNuevoCorte.Rows[0]["Folio"].ToString();
                                    Dictionary<String, ParametroFacturaTipo> larespuesta = new Dictionary<String, ParametroFacturaTipo>();
                                    ParametroFacturaTipo unValorRegla = new ParametroFacturaTipo();
                                    laFactura.UsoCFDI = tablaNuevoCorte.Rows[0]["UsoCFDI"].ToString();
                                    Decimal IVAFac = Convert.ToDecimal(tablaNuevoCorte.Rows[0]["IVA"].ToString());
                                    laFactura.IVA = Convert.ToDecimal(IVAFac.ToString("########0.00"));
                                    Colectiva emisor = new Colectiva { RFC = tablaNuevoCorte.Rows[0]["rfcColectivaEmisora"].ToString(), NombreORazonSocial = tablaNuevoCorte.Rows[0]["nombreColectivaEmisora"].ToString().Trim() /*"Cadena DNU Mexico Central"*/ , ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaEmisora"].ToString()) };
                                    //Colectiva receptor = new Colectiva { RFC = /*string.IsNullOrEmpty(tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString()) ?*/ "XAXX010101000" /*: tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString()*/, NombreORazonSocial = tablaNuevoCorte.Rows[0]["nombreColectivaReceptora"].ToString(), ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaReceptora"].ToString()) };
                                    string nombre = tablaNuevoCorte.Rows[0]["nombreColectivaReceptora"].ToString();
                                    string rfcR = tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString();
                                    if ((!string.IsNullOrEmpty(rfcR)) && rfcR.Length == 13)//persona fisica
                                    {
                                        laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscalFisica");//616
                                        string appelidoPaterno = string.IsNullOrEmpty(tablaNuevoCorte.Rows[0]["APaterno"].ToString()) ? "" : " " + tablaNuevoCorte.Rows[0]["APaterno"].ToString();
                                        string appelidoMaterno = string.IsNullOrEmpty(tablaNuevoCorte.Rows[0]["AMaterno"].ToString()) ? "" : " " + tablaNuevoCorte.Rows[0]["AMaterno"].ToString();
                                        nombre = nombre + appelidoPaterno + appelidoMaterno;
                                    }
                                    else
                                    {
                                        laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal");
                                    }
                                    Colectiva receptor = new Colectiva { RFC = string.IsNullOrEmpty(rfcR) ? "XAXX010101000" : tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString(), NombreORazonSocial = nombre.Trim(), ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaReceptora"].ToString()) };

                                    laFactura.RegimenFiscal = tablaNuevoCorte.Rows[0]["RegimenFiscal"].ToString();
                                    laFactura.Emisora = emisor;
                                    laFactura.Receptora = receptor;
                                    laFactura.PACFacturacion = PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
                                    //  List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                                    laFactura.RutaCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                                    laFactura.RutaKeySAT = PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
                                    laFactura.PassCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");


                                    //laFactura.losDetalles = listaDetallesFacturaXML;
                                    Decimal subtotal = Convert.ToDecimal(tablaNuevoCorte.Rows[0]["SubTotal"].ToString());
                                    laFactura.SubTotal = Convert.ToDecimal(subtotal.ToString("########0.00"));
                                    string rutaPDF = tablaNuevoCorte.Rows[0]["rutaEstadoDeCuenta"].ToString();
                                    rutaPDF = rutaPDF.Replace("EstadoDeCuenta.pdf", "");
                                    //timbrando
                                    bool sinDeuda = false;
                                    string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); ;
                                    Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] generando factura timbrada");
                                    //prod manual
                                    ruta = ruta.Replace("D:", "C:");
                                    rutaPDF = rutaPDF.Replace("D:", "C:");
                                    rutaImagen = rutaImagen.Replace("D:", "C:");
                                    //no prod
                                    //if (true)//LNOperacionesCorte.generarFactura(laFactura, rutaPDF, sinDeuda, "", new List<string>(), new Cuentas(), new XslCompiledTransform()))
                                    //pro
                                    if (LNOperacionesCorte.generarFactura(laFactura, rutaPDF, sinDeuda, "", new List<string>(), new Cuentas(), _transformador, "", false))
                                    {//una vez que se genera la factura ya no hay forma de hacer rollback
                                        try
                                        {

                                            string codigoQR = urlAccesoServicioSAT + "?id=" + laFactura.UUID + "&re=" + (laFactura.Emisora.RFC.Length > 13 ? laFactura.Emisora.RFC.Substring(0, 13) : laFactura.Emisora.RFC) + "&rr=" + (laFactura.Receptora.RFC.Length > 13 ? laFactura.Receptora.RFC.Substring(0, 13) : laFactura.Receptora.RFC) + "&tt=" +
                                            (Decimal.Round(laFactura.SubTotal, 2) + Decimal.Round(laFactura.IVA, 2)) + "&fe=" + laFactura.Sello.Substring((laFactura.Sello.Length - 8), 8);
                                            string rutaImagenQR = ruta + "imagenQR.png";
                                            LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                            laFactura.UrlQrCode = sinDeuda ? "" : rutaImagenQR;
                                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] Insertando factura");
                                            DataTable tablaDatoFacturaInserta = _daoFacturas.ActualizaFactura(Convert.ToInt64(idFactura), laFactura, 0, conn2, null, sinDeuda);
                                            errores = new Response();
                                            string nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
                                            if (_validaciones.BusquedaSinErrores(errores, tablaDatoFacturaInserta))
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] factura insertada");

                                                xmlTimbrado = true;

                                                string rutaImagenLogo = @"C:\ArchivosCortesPIEK\upload\edocta_00\logo_JYU.jpg";
                                                string rutaImagenUNE = @"C:\ArchivosCortesPIEK\upload\edocta_00\UNE_JYU.jpg";
                                                string rutaImagenCAT = @"C:\ArchivosCortesPIEK\upload\edocta_00\CAT_JYU.jpg";


                                                //generando el pdf
                                                // rutaPDF = "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\";
                                                //  pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), rutaPDF, _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", nombreEstado, sinDeuda);
                                                pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), ruta, _daoCortes, conn2, rutaImagen, "", rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, nombreEstado, false);

                                                // pdfGeneradoCorrectamente = true;
                                                if (pdfGeneradoCorrectamente)
                                                {
                                                    //ruta prod
                                                    rutaPDF = rutaPDF.Replace("C:", "D:");
                                                    _daoCortes.ActualizarDatoCorreoYPDF(pdfGeneradoCorrectamente, false, Convert.ToInt64(id), conn2, null, (pdfGeneradoCorrectamente ? (rutaPDF + nombreEstado) : (rutaPDF)));

                                                }
                                            }
                                            else
                                            {
                                                Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] error al actualizar factura, fac:" + id + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] error al insertar factura " + ex.Message + " " + ex.StackTrace);
                                        }

                                    }

                                }
                                else
                                {
                                    Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual Detalles] No se encontro el corte");

                                }

                                // return laFactura;
                            }
                            else
                            {
                                Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] No se encontro el corte");

                            }
                            // servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);

                            conn2.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                    }
                    return xmlTimbrado;
                }

                //debito
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
                                pdfGeneradoCorrectamente = _lnEnvioCorreo.envioCorreo(_correo, cuentaAProcesar, respuestaSolicitud);
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
                            //EdoCuentaTipDebito estadoDeCuenta = new EdoCuentaTipDebito();
                            //estadoDeCuenta.SetDataSource(ds);
                            //estadoDeCuenta.SetParameterValue("imagenLogo", rutaImagen + "\\logo.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                            //estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + (string.IsNullOrEmpty(nombrePdfPrepago) ? "ResumenDeMovimientos.pdf" : nombrePdfPrepago));
                            //estadoDeCuenta.Close();
                            //estadoDeCuenta.Dispose();
                            //pdfGeneradoCorrectamente = true;
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


        //esto es para pruebas
        public static bool GeneracionPDFSinCorte(string id, string cadenaConexion, string fecha, string ruta, string rutaImagen, bool envioCorreo,
            RespuestaSolicitud respuestaSolicitud, LNEnvioCorreo _lnEnvioCorreo, DAOCortes _daoCortes, string nombrePdfCredito, string nombrePdfPrepago, bool esDebito, DAOFacturas _daoFacturas = null)
        {
            //  //funciona correcto//
            bool pdfGeneradoCorrectamente = false;
            bool xmlTimbrado = false;
            if (!string.IsNullOrEmpty(id))

            {

                if (!esDebito)
                {
                    LNValidacionesCampos _validaciones = new LNValidacionesCampos();
                    Logueo.Evento("Buscando factura relacionada al corte");
                    //credito
                    Response errores = new Response();
                    try
                    {
                        cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizadorTimbreManual");
                        using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                        {
                            Logueo.Evento("Buscando factura relacionada al corte");
                            conn2.Open();
                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");
                            //sin timbtrar4
                            rutaImagen = rutaImagen.Replace("D:", "C:");
                            //pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", true);
                            //pdf timbrado ya hecho
                            pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", false);


                            //timbrando
                            DataTable tablaNuevoCorte = _daoCortes.ObtenerFactura(id, ruta, conn2);
                            if (_validaciones.BusquedaSinErrores(errores, tablaNuevoCorte))
                            {
                                //obteniendo los detalles
                                String idFactura = tablaNuevoCorte.Rows[0]["ID_Factura"].ToString();
                                Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");

                                DataTable tablaDetalles = _daoCortes.ObtenerFacturaDetalles(idFactura, ruta, conn2);
                                if (_validaciones.BusquedaSinErrores(errores, tablaDetalles))
                                {
                                    Factura laFactura = new Factura();
                                    List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                                    foreach (DataRow row in tablaDetalles.Rows)
                                    {
                                        Decimal iva = Convert.ToDecimal(row["ImporteIVA"].ToString());
                                        Decimal total = Convert.ToDecimal(row["Total"].ToString());
                                        String decimasTotal = total.ToString("########0.00");
                                        DetalleFactura detalle = new DetalleFactura { Cantidad = Convert.ToInt32(row["Cantidad"].ToString()), Unidad = row["Unidad"].ToString(), ClaveProdServ = row["ClaveProdServ"].ToString(), ClaveUnidad = row["Clave"].ToString(), NombreProducto = row["Descripcion"].ToString(), impImporte = iva.ToString("########0.00"), Total = Convert.ToDecimal(decimasTotal), PrecioUnitario = Convert.ToDecimal(decimasTotal), impBase = total.ToString("########0.00"), impImpuesto = row["ClaveImpuesto"].ToString(), impTipoFactor = row["Clave"].ToString(), impTasaOCuota = row["IVA"].ToString() };
                                        listaDetallesFactura.Add(detalle);
                                    }
                                    laFactura.losDetalles = listaDetallesFactura;

                                    laFactura.LugarExpedicion = tablaNuevoCorte.Rows[0]["LugarExpedicion"].ToString();
                                    laFactura.MetodoPago = tablaNuevoCorte.Rows[0]["MetodoPago"].ToString();
                                    laFactura.TipoComprobante = tablaNuevoCorte.Rows[0]["TipoComprobante"].ToString();
                                    laFactura.FormaPago = tablaNuevoCorte.Rows[0]["formaDePago"].ToString();
                                    DateTime fechaEmision = Convert.ToDateTime(tablaNuevoCorte.Rows[0]["FechaEmision"].ToString());
                                    string fechaConvetida = fechaEmision.ToString("dd/MM/yyyy HH:mm:ss");  //Hoy.ToString("dd/MM/yyyy hh:mm:ss tt");//HH
                                    laFactura.FechaEmision = Convert.ToDateTime(fechaConvetida); //DateTime.ParseExact(fechaConvetida, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);//Convert.ToDateTime(fechaConvetida);
                                    laFactura.RequestorPAC = PNConfig.Get("PROCESAEDOCUENTA", "RequestorPACTimbre");
                                    laFactura.UserPAC = PNConfig.Get("PROCESAEDOCUENTA", "UserNamePACTimbre");
                                    laFactura.UserPassPAC = PNConfig.Get("PROCESAEDOCUENTA", "PassUserNamePACTimbre");
                                    string nombrePAC = PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
                                    laFactura.URLProvedorCertificado = LNFacturas.obtenerURLPACFacturacion(nombrePAC, laFactura);
                                    laFactura.Folio = tablaNuevoCorte.Rows[0]["Folio"].ToString();
                                    Dictionary<String, ParametroFacturaTipo> larespuesta = new Dictionary<String, ParametroFacturaTipo>();
                                    ParametroFacturaTipo unValorRegla = new ParametroFacturaTipo();
                                    laFactura.UsoCFDI = tablaNuevoCorte.Rows[0]["UsoCFDI"].ToString();
                                    Decimal IVAFac = Convert.ToDecimal(tablaNuevoCorte.Rows[0]["IVA"].ToString());
                                    laFactura.IVA = Convert.ToDecimal(IVAFac.ToString("########0.00"));
                                    Colectiva emisor = new Colectiva { RFC = tablaNuevoCorte.Rows[0]["rfcColectivaEmisora"].ToString(), NombreORazonSocial = tablaNuevoCorte.Rows[0]["nombreColectivaEmisora"].ToString() /*"Cadena DNU Mexico Central"*/ , ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaEmisora"].ToString()) };
                                    //Colectiva receptor = new Colectiva { RFC = /*string.IsNullOrEmpty(tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString()) ?*/ "XAXX010101000" /*: tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString()*/, NombreORazonSocial = tablaNuevoCorte.Rows[0]["nombreColectivaReceptora"].ToString(), ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaReceptora"].ToString()) };
                                    Colectiva receptor = new Colectiva { RFC = string.IsNullOrEmpty(tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString()) ? "XAXX010101000" : tablaNuevoCorte.Rows[0]["rfcColectivaReceptora"].ToString(), NombreORazonSocial = tablaNuevoCorte.Rows[0]["nombreColectivaReceptora"].ToString(), ID_Colectiva = Convert.ToInt64(tablaNuevoCorte.Rows[0]["idColectivaReceptora"].ToString()) };

                                    laFactura.RegimenFiscal = tablaNuevoCorte.Rows[0]["RegimenFiscal"].ToString();
                                    laFactura.Emisora = emisor;
                                    laFactura.Receptora = receptor;
                                    laFactura.PACFacturacion = PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
                                    //  List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
                                    laFactura.RutaCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                                    laFactura.RutaKeySAT = PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
                                    laFactura.PassCerSAT = PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");


                                    //laFactura.losDetalles = listaDetallesFacturaXML;
                                    Decimal subtotal = Convert.ToDecimal(tablaNuevoCorte.Rows[0]["SubTotal"].ToString());
                                    laFactura.SubTotal = Convert.ToDecimal(subtotal.ToString("########0.00"));
                                    string rutaPDF = tablaNuevoCorte.Rows[0]["rutaEstadoDeCuenta"].ToString();
                                    //timbrando
                                    bool sinDeuda = false;
                                    string urlAccesoServicioSAT = PNConfig.Get("PROCESAEDOCUENTA", "URLAccesoAlServicioSAT"); ;
                                    Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] generando factura timbrada");
                                    //prod manual
                                    ruta = ruta.Replace("D:", "C:");
                                    rutaPDF = rutaPDF.Replace("D:", "C:");
                                    rutaImagen = rutaImagen.Replace("D:", "C:");
                                    //no prod
                                    //if (true)//LNOperacionesCorte.generarFactura(laFactura, rutaPDF, sinDeuda, "", new List<string>(), new Cuentas(), new XslCompiledTransform()))
                                    //pro
                                    if (LNOperacionesCorte.generarFactura(laFactura, rutaPDF, sinDeuda, "", new List<string>(), new Cuentas(), new XslCompiledTransform(), "", false))
                                    {//una vez que se genera la factura ya no hay forma de hacer rollback
                                        try
                                        {

                                            string codigoQR = urlAccesoServicioSAT + "?id=" + laFactura.UUID + "&re=" + (laFactura.Emisora.RFC.Length > 13 ? laFactura.Emisora.RFC.Substring(0, 13) : laFactura.Emisora.RFC) + "&rr=" + (laFactura.Receptora.RFC.Length > 13 ? laFactura.Receptora.RFC.Substring(0, 13) : laFactura.Receptora.RFC) + "&tt=" +
                                            (Decimal.Round(laFactura.SubTotal, 2) + Decimal.Round(laFactura.IVA, 2)) + "&fe=" + laFactura.Sello.Substring((laFactura.Sello.Length - 8), 8);
                                            string rutaImagenQR = rutaPDF + "imagenQR.png";
                                            LNFacturas.generaCodigoQR(rutaImagenQR, codigoQR);
                                            laFactura.UrlQrCode = sinDeuda ? "" : rutaImagenQR;
                                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] Insertando factura");
                                            DataTable tablaDatoFacturaInserta = _daoFacturas.ActualizaFactura(Convert.ToInt64(idFactura), laFactura, 0, conn2, null, sinDeuda);
                                            errores = new Response();
                                            string nombreEstado = PNConfig.Get("PROCESAEDOCUENTA", "NombreArchivoPDFCredito");
                                            if (_validaciones.BusquedaSinErrores(errores, tablaDatoFacturaInserta))
                                            {
                                                Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] factura insertada");

                                                xmlTimbrado = true;
                                                //generando el pdf
                                                // rutaPDF = "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\";
                                                pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), rutaPDF, _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", nombreEstado, sinDeuda);
                                                // pdfGeneradoCorrectamente = true;
                                                if (pdfGeneradoCorrectamente)
                                                {
                                                    //ruta prod
                                                    rutaPDF = rutaPDF.Replace("C:", "D:");
                                                    _daoCortes.ActualizarDatoCorreoYPDF(pdfGeneradoCorrectamente, false, Convert.ToInt64(id), conn2, null, (pdfGeneradoCorrectamente ? (rutaPDF + nombreEstado) : (rutaPDF)));

                                                }
                                            }
                                            else
                                            {
                                                Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] error al actualizar factura, fac:" + id + " " + errores.CodRespuesta + " " + errores.DescRespuesta);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] error al insertar factura " + ex.Message + " " + ex.StackTrace);
                                        }

                                    }

                                }
                                else
                                {
                                    Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual Detalles] No se encontro el corte");

                                }

                                // return laFactura;
                            }
                            else
                            {
                                Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] No se encontro el corte");

                            }
                            // servicioDatos servicioDatos_ = new servicioDatos(conn2, cadenaConexion);

                            conn2.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                    }
                    return xmlTimbrado;
                }

                //debito
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
                                pdfGeneradoCorrectamente = _lnEnvioCorreo.envioCorreo(_correo, cuentaAProcesar, respuestaSolicitud);
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
                            //EdoCuentaTipDebito estadoDeCuenta = new EdoCuentaTipDebito();
                            //estadoDeCuenta.SetDataSource(ds);
                            //estadoDeCuenta.SetParameterValue("imagenLogo", rutaImagen + "\\logo.jpg");//\\ArchivosCortesCacao\\Facturas\\dnuLogo.png");
                            //estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + (string.IsNullOrEmpty(nombrePdfPrepago) ? "ResumenDeMovimientos.pdf" : nombrePdfPrepago));
                            //estadoDeCuenta.Close();
                            //estadoDeCuenta.Dispose();
                            //pdfGeneradoCorrectamente = true;
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

        //genracion pdf timbrado sin timbrar
        public static bool GeneracionPDFSinCorteSinTimbrar(string id, string cadenaConexion, string fecha, string ruta, string rutaImagen, bool envioCorreo,
            RespuestaSolicitud respuestaSolicitud, LNEnvioCorreo _lnEnvioCorreo, DAOCortes _daoCortes, string nombrePdfCredito, string nombrePdfPrepago, bool esDebito, DAOFacturas _daoFacturas = null)
        {
            //  //funciona correcto//
            bool pdfGeneradoCorrectamente = false;
            bool xmlTimbrado = false;
            if (!string.IsNullOrEmpty(id))

            {

                if (!esDebito)
                {
                    LNValidacionesCampos _validaciones = new LNValidacionesCampos();
                    Logueo.Evento("Buscando factura relacionada al corte");
                    //credito
                    Response errores = new Response();
                    try
                    {

#if PIEK
                        cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizadorTimbreManual");
                        cadenaConexion = cadenaConexion.Replace("10.0.0.5", "20.225.175.141");
#else
                        cadenaConexion = DBProcesadorArchivo.strBDEscrituraArchivo;

#endif
                        //   cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizadorTimbreManual");
                        using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                        {
                            Logueo.Evento("Buscando factura relacionada al corte");
                            conn2.Open();
                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");
                            //sin timbtrar4
                            rutaImagen = rutaImagen.Replace("D:", "C:");
                            //pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", true);
                            //pdf sintimbre


                            DataTable dt = _daoCortes.ObtenerDatosCortes("EJECUTOR_ObtieneDatosParaTxt", null, conn2);
                            foreach (DataRow row in dt.Rows)
                            {
                                id = row["ID_CORTE"].ToString();
                                //  pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaText(new List<string>(), "", "", "", "", Convert.ToInt64(id), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", false, new Cuentas { Subproducto = "Prueba", NombreORazonSocial = "kap", Fecha_Corte = DateTime.Now, RFCCliente = "CNF120614443" }, 0, 31);
                                //  pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDFDebito(new List<string>(), "", "", "", "", Convert.ToInt64(id), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDDManual\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", true);


#if PIEK
                                //timbre
                                string rutaRaiz = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalida");
                                rutaRaiz = rutaRaiz.Replace("Facturas\\", "");

                                string clave= row["Clave"].ToString();
                                string factura = row["ID_Factura"].ToString();
                                bool noImprimirFactura = false;
                                if (factura=="0") {
                                    noImprimirFactura = true;
                                }

                                string rutaImagenLogo = @"upload\edocta_10\Logo_AKB.jpg";
                                string rutaImagenUNE = @"upload\edocta_10\UNE_AKB.jpg";
                                string rutaImagenCAT = @"upload\edocta_10\CAT_AKB.jpg";

                                if (clave == "JYUTC00")
                                {
                                     rutaImagenLogo = @"upload\edocta_00\Logo_JYU.jpg";
                                     rutaImagenUNE = @"upload\edocta_00\UNE_JYU.jpg";
                                     rutaImagenCAT = @"upload\edocta_00\CAT_JYU.jpg";
                                }
                                rutaImagenLogo = rutaRaiz + rutaImagenLogo;
                                rutaImagenUNE = rutaRaiz + rutaImagenUNE;
                                rutaImagenCAT = rutaRaiz + rutaImagenCAT;

                                string rutaArchivo= row["RutaEstadoDeCuenta"].ToString();
                                rutaArchivo = Path.GetFullPath(rutaArchivo);
                                String[] spearator = { "ArchivosCortesPIEK" };
                                Int32 count = 2;

                                // using the method
                                String[] strlist = rutaArchivo.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);
                                rutaArchivo = strlist[1].Replace("EstadoDeCuenta.pdf", "");
                                //  @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\

                                // string[] parteRuta = rutaArchivo.Split("ArchivosCortesPIEK");

                                //C:\ArchivosCortesPIEK\Facturas\40965800\0010284667\2023\07\EstadoDeCuenta.pdf

                                //   String dir= cuenta.ClaveCliente + "\\" + cuenta.Tarjeta.Substring(6) + "\\" + anio + "\\" + mes + "\\";


                                pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDFV2(new List<string>(), "", "", "", "", Convert.ToInt64(id), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\"+ rutaArchivo, _daoCortes, conn2, rutaImagen, "", rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, "EstadoDeCuenta.pdf", noImprimirFactura, new List<CuentaAdicional>());
                                //pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDFV2(new List<string>(), "", "", "", "", Convert.ToInt64(id), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\", _daoCortes, conn2, rutaImagen, "", rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, "EstadoDeCuenta.pdf", false,new List<CuentaAdicional>());
#else
                               // pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDFV(new List<string>(), "", "", "", "", Convert.ToInt64(id), @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\", _daoCortes, conn2, rutaImagen, "", rutaImagenLogo, rutaImagenUNE, rutaImagenCAT, "EstadoDeCuenta.pdf", true);

                                //trafa
                                pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDFExterno(id, conn2, @"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\EDCmanual\", _daoCortes, "TrafaMov", new Factura());
#endif
                            }

                            conn2.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                    }
                    return xmlTimbrado;
                }


            }
            return pdfGeneradoCorrectamente;
        }

        //esto es para pruebas
        public static bool GeneracionTXTSinCorte(string id, string cadenaConexion, string fecha, string ruta, string rutaImagen, bool envioCorreo,
            RespuestaSolicitud respuestaSolicitud, LNEnvioCorreo _lnEnvioCorreo, DAOCortes _daoCortes, string nombrePdfCredito, string nombrePdfPrepago, bool esDebito, DAOFacturas _daoFacturas = null)
        {
            //  //funciona correcto//
            bool pdfGeneradoCorrectamente = false;
            bool xmlTimbrado = false;
            if (!string.IsNullOrEmpty(id))

            {

                if (!esDebito)
                {
                    LNValidacionesCampos _validaciones = new LNValidacionesCampos();
                    Logueo.Evento("Buscando factura relacionada al corte");
                    //credito
                    Response errores = new Response();
                    try
                    {
                        cadenaConexion = PNConfig.Get("PROCESAEDOCUENTA", "BDReadAutorizadorTimbreManual");
                        using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                        {
                            Logueo.Evento("Buscando factura relacionada al corte");
                            conn2.Open();
                            Logueo.Evento("[GeneraEstadoCuentaCreditoTimbrado] obteniendo detalles");
                            //sin timbtrar4
                            rutaImagen = rutaImagen.Replace("D:", "C:");
                            //pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaPDF(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", true);
                            //pdf timbrado ya hecho
                            pdfGeneradoCorrectamente = LNOperacionesEdoCuenta.edoCuentaText(new List<string>(), "", "", "", "", Convert.ToInt64(id), "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\estdos prod\\Nueva carpeta\\", _daoCortes, conn2, rutaImagen, "", "C:\\Users\\Osvaldo\\Documents\\sistema dnu\\Cortes\\credito\\imagenes\\kapital\\Edocuenta\\Logo_Mesa.jpg", "", "", "EstadoDeCuenta.pdf", false, new Cuentas { Subproducto = "Prueba", NombreORazonSocial = "kap", Fecha_Corte = DateTime.Now, RFCCliente = "CNF120614443" }, 0, 1);



                            conn2.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCreditoTimbrado] [Error al generar el la factura manual] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");

                    }
                    return xmlTimbrado;
                }
            }
            return pdfGeneradoCorrectamente;
        }

        public static bool generarFactura(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<String> archivos, Cuentas cuenta, XslCompiledTransform _transformador, string nombreArchivo, bool rutaProcesado, string nombreXML = null)
        {
            DateTime Hoy = cuenta.Fecha_Corte;//DateTime.Now;

            bool facturaGeneradoCorrectamente = false;
            LNOperaciones lnOperaciones = new LNOperaciones();
            Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo pac");
            string cadenaMySuit = PNConfig.Get("PROCESAEDOCUENTA", "RutaSoapMySuit");
            Logueo.Evento("[GeneraEstadoCuentaCredito] pac obtenido");
            //if (!ruta.Contains(".pdf"))
            //{
            //    LNOperaciones.crearDirectorio(ruta);
            //}
            //else
            //{
            //    //_transformador = new XslCompiledTransform();
            //    //_transformador.Load("C:\\ArchivosCortesCacao\\ArchivosXSLT\\CadenaOriginal_3_3.xslt");
            //}
            Logueo.Evento("[GeneraEstadoCuentaCredito]nombre archivo" + nombreArchivo);
            nombreArchivo = nombreArchivo.Replace(".pdf", "");
            if (rutaProcesado)
            {
                ruta = ruta + "Procesados\\";
            }
            if (!ruta.Contains(".pdf"))
            {
                LNOperaciones.crearDirectorio(ruta);
            }
            else
            {
                //_transformador = new XslCompiledTransform();
                //_transformador.Load("C:\\ArchivosCortesCacao\\ArchivosXSLT\\CadenaOriginal_3_3.xslt");
            }

            //pruebas
            if (cadenaMySuit.ToLower().Contains("mysuitetest"))
            {
                Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo pac prubas");
                factura.Receptora.RFC = "JES900109Q90";
                factura.Emisora.RFC = "JES900109Q90";
                factura.Receptora.NombreORazonSocial = "JIMENEZ & ESTRADA SALAS A A";
                factura.Emisora.NombreORazonSocial = "JIMENEZ ESTRADA SALAS A A";
                factura.LugarExpedicion = "01030";
                factura.DomicilioFiscalReceptor = "01030";
                factura.RegimenFiscalReceptor = "601";

            }

#if PIEK
            //pruebas
            if (cadenaMySuit.ToLower().Contains("https://www.mysuite.com") && (factura.Emisora.NombreORazonSocial == "GRUPO PIEK" || factura.Emisora.NombreORazonSocial == "PIEK" || factura.Emisora.NombreORazonSocial == "PRODUCTOS INTEGRALES EN COMUNICACION"))
            {
                factura.Emisora.RFC = "PIC9508303ZA";
                factura.Emisora.NombreORazonSocial = "PRODUCTOS INTEGRALES EN COMUNICACION";
                factura.LugarExpedicion = "52105";// "01030";
                //factura.DomicilioFiscalReceptor = "52105"; //;"01030";
                if (factura.Receptora.RFC == "XAXX010101000")
                {
                    //factura.Receptora.RFC = "XAXX010101000";
                    factura.RegimenFiscalReceptor = "616";
               //     factura.UsoCFDI = "S01";
                    factura.DomicilioFiscalReceptor = factura.LugarExpedicion; //;"01030";
                }
            }
#else

            if (cadenaMySuit.ToLower().Contains("https://www.mysuite.com"))//&& (factura.Emisora.NombreORazonSocial.ToUpper().Contains("TRAF")))
            {
                Logueo.Evento("[GeneraEstadoCuentaCredito] obteniendo pac prod");
                //  factura.Receptora.RFC = "JES900109Q90";
                factura.Emisora.RFC = "TDI190405HF5";
                factura.Emisora.NombreORazonSocial = "TRAFALGAR DIGITAL, INSTITUCION DE FONDOS DE PAGO ELECTRONICO";
                factura.LugarExpedicion = "06500";// "01030";
                //factura.DomicilioFiscalReceptor = "52105"; //;"01030";
                if (factura.Receptora.RFC == "XAXX010101000")
                {
                    //factura.Receptora.RFC = "XAXX010101000";
                    factura.RegimenFiscalReceptor = "616";
                    //     factura.UsoCFDI = "S01";
                    factura.DomicilioFiscalReceptor = factura.LugarExpedicion; //;"01030";
                }
            }

            //string emisor = @"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\SELLOS TRAFALGAR DIGITAL SA CV1\SELLOS TRAFALGAR DIGITAL SA CV\CSD_TDI190405HF5_20190605193310\00001000000500158972.cer"; //factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            //string key = @"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\SELLOS TRAFALGAR DIGITAL SA CV1\SELLOS TRAFALGAR DIGITAL SA CV\CSD_TDI190405HF5_20190605193310\CSD_CDMX_TDI190405HF5_20190605_193242.key"; //factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            //string password = "TDI945HF";//factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");


#endif

            Logueo.Evento("[GeneraEstadoCuentaCredito]genreando CFDI");
            CFDI xmlMySuit = new CFDI(factura, facturaEnBlanco);
            string emisor = factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            string key = factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            string password = factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");
            String noCertificado = "";

            //string elXML1 = xmlMySuit.ToString(); //File.ReadAllText(textXML);
            //string CadenaOriginal1 = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML1, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            //factura.CadenaOriginal = CadenaOriginal1;

            //
            //Timbrado //
            if (factura.PACFacturacion.ToLower().Contains("facturama"))
            {
                Timbrado timbrado = new Timbrado();
                var timbre = timbrado.Facturama(factura, ruta);

                if (timbre.Error == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            String certificado = "";
            string elXML = xmlMySuit.ToString(); //File.ReadAllText(textXML);
            Logueo.Evento("[GeneraEstadoCuentaCredito]obteniendo cadena original");
            string CadenaOriginal = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            factura.CadenaOriginal = CadenaOriginal;
            //  Logueo.Evento("[GeneraEstadoCuentaCredito]cadena:"+ factura.CadenaOriginal);
            Logueo.Evento("[GeneraEstadoCuentaCredito]obteniendo sello");
            String elSello = LNOperacionesEdoCuenta.ObtenerSelloFactura(CadenaOriginal, out noCertificado, out certificado, emisor, key, password);
            factura.Sello = elSello;
            elXML = elXML.Replace("[Sello]", elSello);
            elXML = elXML.Replace("[NoCertificado]", noCertificado);
            elXML = elXML.Replace("[Certificado]", certificado);
            factura.NoCertificadoEmisor = noCertificado;

            Logueo.Evento("[GeneraEstadoCuentaCredito]datos obtenidos");

            //text = text.Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            // CFDI xmlMySuit = new CFDI("");
            //    string xml = xmlMySuit.ToString();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(elXML);
            if (!string.IsNullOrEmpty(nombreXML))
            {
                xDoc.Save(ruta + nombreXML + "_Factura_Original.xml");
            }
            else
            {
                xDoc.Save(ruta + "Factura_Original.xml");
            }
            // elXML = elXML.Replace("&amp;", "&");
            // archivos.Add(ruta + "Factura_Timbrada.xml");
            wsFacturacionMySuit.FactWSFront unaOperacion = new wsFacturacionMySuit.FactWSFront();
            com.mysuitetest.www.FactWSFront unaOperacionTest = new com.mysuitetest.www.FactWSFront();
            // string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
            // TransactionTag laRespuesta = unaOperacion.RequestTransaction("", "TIMBRAR", "MX", "", "", "", xml, "", "");

            dynamic laRespuesta = null;
            if (!facturaEnBlanco)
            {
                if (cadenaMySuit.ToLower().Contains("mysuitetest"))
                {
                    //  laRespuesta = unaOperacionTest.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", /*factura.Emisora.RFC*/"AAA010101AAA", factura.RequestorPAC, factura.UserPAC, elXML, "", "");
                    laRespuesta = unaOperacionTest.RequestTransaction("0cfbbd00-ac21-47d9-8776-956c0f44254c", "TIMBRAR", "MX", "SAMP570517JB1", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");

                }
                else
                {
                    laRespuesta = unaOperacion.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", "DDN1106065K2" /*factura.Emisora.RFC*/, factura.RequestorPAC, factura.UserPAC, elXML, "", "");

                    //laRespuesta = unaOperacion.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", factura.Emisora.RFC, factura.RequestorPAC, factura.UserPAC, elXML, "", "");
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
                    if (!string.IsNullOrEmpty(nombreXML))
                    {
                        xDoc.Save(ruta + nombreXML + "_Factura_Timbrada.xml");//+ fecha + ".xml");
                                                                              // archivos.Add(ruta + nombreXML + "_Factura_Timbrada.xml");
                    }
                    else
                    {
                        xDoc.Save(ruta + "Factura_Timbrada.xml");//+ fecha + ".xml");
                                                                 // archivos.Add(ruta + "Factura_Timbrada.xml");
                    }

                    facturaGeneradoCorrectamente = true;

                    factura.CadenaOriginalTimbre = "||" + elTimbre.version + "|" + elTimbre.UUID + "|" + elTimbre.FechaTimbrado + "|" + elTimbre.RfcProvCertif + "|" + elTimbre.selloCFD + "|" + elTimbre.noCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);
                    factura.LugarExpedicion = factura.LugarExpedicion;// laFactura.Emisora.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio;
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
                    try
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] Timbre error al insertar factura externa, corte:" + laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3);

                        Logueo.Error("[GeneraEstadoCuentaCredito] Timbre error al insertar factura externa detalle, corte:" + laRespuesta.Response.Data);
                        Logueo.Error("[GeneraEstadoCuentaCredito] Timbre error al insertar factura externa detalle, corte:" + elXML);

                    }
                    catch (Exception ex) { }
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

        public static bool generarFacturaV4(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<String> archivos, Cuentas cuenta, XslCompiledTransform _transformador, string nombreArchivo, bool rutaProcesado)
        {
            DateTime Hoy = cuenta.Fecha_Corte;//DateTime.Now;
            string cadenaMySuit = PNConfig.Get("PROCESAEDOCUENTA", "RutaSoapMySuit");
            bool facturaGeneradoCorrectamente = false;
            LNOperaciones lnOperaciones = new LNOperaciones();
            nombreArchivo = nombreArchivo.Replace(".pdf", "");
            if (rutaProcesado)
            {
                ruta = ruta + "Procesados\\";
            }
            if (!ruta.Contains(".pdf"))
            {
                LNOperaciones.crearDirectorio(ruta);
            }
            else
            {
                //_transformador = new XslCompiledTransform();
                //_transformador.Load("C:\\ArchivosCortesCacao\\ArchivosXSLT\\CadenaOriginal_3_3.xslt");
            }

            //pruebas
            if (cadenaMySuit.ToLower().Contains("mysuitetest"))
            {
                factura.Receptora.RFC = "JES900109Q90";
                factura.Emisora.RFC = "JES900109Q90";
                factura.Receptora.NombreORazonSocial = "JIMENEZ ESTRADA SALAS A A";
                factura.Emisora.NombreORazonSocial = "JIMENEZ ESTRADA SALAS A A";
                factura.LugarExpedicion = "01030";
                factura.DomicilioFiscalReceptor = "01030";
                factura.RegimenFiscalReceptor = "601";

            }

#if PIEK
            //pruebas
            if (cadenaMySuit.ToLower().Contains("https://www.mysuite.com") && (factura.Emisora.NombreORazonSocial == "GRUPO PIEK" || factura.Emisora.NombreORazonSocial == "PIEK QA"))
            {
                factura.Emisora.RFC = "PIC9508303ZA";
                factura.Emisora.NombreORazonSocial = "PRODUCTOS INTEGRALES EN COMUNICACION";
                factura.LugarExpedicion = "52105";// "01030";
                //factura.DomicilioFiscalReceptor = "52105"; //;"01030";
                if (factura.Receptora.RFC == "XAXX010101000")
                {
                    //factura.Receptora.RFC = "XAXX010101000";
                  //  factura.RegimenFiscalReceptor = "616";
                  //  factura.UsoCFDI = "S01";
                  //  factura.DomicilioFiscalReceptor = factura.LugarExpedicion; //;"01030";
                }
            }
#endif


            CFDI4 xmlMySuit = new CFDI4(factura, facturaEnBlanco);

            string emisor = factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            string key = factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            string password = factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");

            ////pruebas productivas traf
            ////prod
            //string emisor = @"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\SELLOS TRAFALGAR DIGITAL SA CV1\SELLOS TRAFALGAR DIGITAL SA CV\CSD_TDI190405HF5_20190605193310\00001000000500158972.cer"; //factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            //string key = @"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\SELLOS TRAFALGAR DIGITAL SA CV1\SELLOS TRAFALGAR DIGITAL SA CV\CSD_TDI190405HF5_20190605193310\CSD_CDMX_TDI190405HF5_20190605_193242.key"; //factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            //string password = "TDI945HF";//factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");



            String noCertificado = "";

            //string elXML1 = xmlMySuit.ToString(); //File.ReadAllText(textXML);
            //string CadenaOriginal1 = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML1, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            //factura.CadenaOriginal = CadenaOriginal1;

            //
            //Timbrado //
            if (factura.PACFacturacion.ToLower().Contains("facturama"))
            {
                Timbrado timbrado = new Timbrado();
                var timbre = timbrado.Facturama(factura, ruta);

                if (timbre.Error == null)
                {
                    return true;
                }
                else
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
            CadenaOriginal = CadenaOriginal.Replace("[NoCertificado]", noCertificado);
            factura.CadenaOriginal = CadenaOriginal;
            factura.NoCertificadoEmisor = noCertificado;

            //text = text.Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
            // CFDI xmlMySuit = new CFDI("");
            //    string xml = xmlMySuit.ToString();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(elXML);
            xDoc.Save(ruta + nombreArchivo + "Factura_Original.xml");
            // archivos.Add(ruta + "Factura_Timbrada.xml");
            wsFacturacionMySuit.FactWSFront unaOperacion = new wsFacturacionMySuit.FactWSFront();
            com.mysuitetest.www.FactWSFront unaOperacionTest = new com.mysuitetest.www.FactWSFront();
            // string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
            // TransactionTag laRespuesta = unaOperacion.RequestTransaction("", "TIMBRAR", "MX", "", "", "", xml, "", "");

            dynamic laRespuesta = null;
            if (!facturaEnBlanco)
            {
                if (cadenaMySuit.ToLower().Contains("mysuitetest"))
                {
                    //  laRespuesta = unaOperacionTest.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", factura.Emisora.RFC, factura.RequestorPAC, factura.UserPAC, elXML, "", "");
                    laRespuesta = unaOperacionTest.RequestTransaction("0c320b03-d4f1-47bc-9fb4-77995f9bf33e", "TIMBRAR", "MX", factura.Emisora.RFC, "0c320b03-d4f1-47bc-9fb4-77995f9bf33e", "MX.JES900109Q90.Juan", elXML, "", "");
                    //laRespuesta = unaOperacionTest.RequestTransaction("0cfbbd00-ac21-47d9-8776-956c0f44254c", "TIMBRAR", "MX", AAA010101AAA "SAMP570517JB1", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");
                    //MX.DDN1106065K2.DDN1106065K2
                }
                else
                {
                    //prueba prod traf
                    //laRespuesta = unaOperacion.RequestTransaction("8276653d-bc43-46f5-b7bd-b41f6a207cab", "TIMBRAR", "MX", "DDN1106065K2", "8276653d-bc43-46f5-b7bd-b41f6a207cab", "MX.DDN1106065K2. DDN1106065K2", elXML, "", "");


                    laRespuesta = unaOperacion.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", "DDN1106065K2" /*factura.Emisora.RFC*/, factura.RequestorPAC, factura.UserPAC, elXML, "", "");
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
                    xDoc.Save(ruta + nombreArchivo + "Factura_Timbrada.xml");//+ fecha + ".xml");
                    archivos.Add(ruta + "Factura_Timbrada.xml");
                    facturaGeneradoCorrectamente = true;

                    factura.CadenaOriginalTimbre = "||" + elTimbre.version + "|" + elTimbre.UUID + "|" + elTimbre.FechaTimbrado + "|" + elTimbre.RfcProvCertif + "|" + elTimbre.selloCFD + "|" + elTimbre.noCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);
                    factura.LugarExpedicion = factura.LugarExpedicion;// laFactura.Emisora.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio;
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
                    try
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] Timbre error al insertar factura externa, corte:" + laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3);

                        Logueo.Error("[GeneraEstadoCuentaCredito] Timbre error al insertar factura externa detalle, corte:" + laRespuesta.Response.Data);

                    }
                    catch (Exception ex) { }
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

        public static bool generarArchivoTemporalSFTP(string pathRemoteFile, string pathLocalFile)
        {
            FileInfo fi = new FileInfo(pathLocalFile);
            LNOperaciones.crearDirectorio(fi.DirectoryName);
            bool archivoGenerado = false;
            //string servidorSFTP = "45.32.4.114";//PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
            //string puertoSFTP = "22";//PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
            //string usuarioSFTP = "UserEmbozoSFTP";//PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
            //string passUsuarioSFTP = "4bhuzrrW3WeFNr";//PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            string servidorSFTP = PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
            string puertoSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
            string usuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
            string passUsuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            string carpetaEstadosDecuenta = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
            string carpetaInicial = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP");
            //string rutaTxt = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaGenEdC") ? Todos_losParametros["@RutaGenEdC"].Valor : ruta;

            try
            {
                KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(usuarioSFTP);
                keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                Renci.SshNet.ConnectionInfo conInfo = new Renci.SshNet.ConnectionInfo(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, keybAuth);


                // using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                using (SftpClient sftp = new SftpClient(conInfo))
                {
                    //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                    //{
                    sftp.Connect();
                    // string directoryMain = sftp.WorkingDirectory;// + "/" + carpetaInicial;



                    try
                    {
                        archivoGenerado = SFTPService.DownloadFileWithoutConnection(sftp, pathRemoteFile, pathLocalFile);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar certificado:" + ex.Message);
                    }
                    if (!archivoGenerado)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar certificados");

                    }


                    sftp.Disconnect();
                }
            }
            catch (Exception exEdo)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al generar certificado temproal sftp:" + exEdo.Message);

            }

            return archivoGenerado;
        }

        internal static void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        {
            string passUsuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            foreach (AuthenticationPrompt Prompt in e.Prompts)
            {
                if (Prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    Prompt.Response = passUsuarioSFTP;
                }
            }
        }


        public static bool generarFacturaPrueba(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<String> archivos, Cuentas cuenta, XslCompiledTransform _transformador, string id)
        {
            bool facturaGeneradoCorrectamente = false;
            if (!string.IsNullOrEmpty(id))
                try
                {
                    DateTime Hoy = cuenta.Fecha_Corte;//DateTime.Now;


                    LNOperaciones lnOperaciones = new LNOperaciones();
                    if (!ruta.Contains(".pdf"))
                    {
                        LNOperaciones.crearDirectorio(ruta);
                    }
                    else
                    {
                        //_transformador = new XslCompiledTransform();
                        //_transformador.Load("C:\\ArchivosCortesCacao\\ArchivosXSLT\\CadenaOriginal_3_3.xslt");
                    }
                    CFDI xmlMySuit = new CFDI(factura, facturaEnBlanco);
                    //string emisor = @"C:\ArchivosCortesPIEK\Facturas\upload\firmaSAT\00001000000510154087.cer"; //factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                    //string key = @"C:\ArchivosCortesPIEK\Facturas\upload\firmaSAT\PIC9508303ZA.key"; //factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
                    //string password = "12345678a";//factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");

                    //prod
                    string emisor = @"C:\Users\Osvi\Documents\DNU\requerimientos\tester\pruebas timbre\Sello Digital UNO\Sello Digital UNO\00001000000511696605.cer"; //factura.RutaCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
                    string key = @"C:\Users\Osvi\Documents\DNU\requerimientos\tester\pruebas timbre\Sello Digital UNO\Sello Digital UNO\CSD_UNO_UNO111111UP0_20220301_134734.key"; //factura.RutaKeySAT;//PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
                    string password = "Nopagare2022";//factura.PassCerSAT;//PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");


                    String noCertificado = "";

                    //string elXML1 = xmlMySuit.ToString(); //File.ReadAllText(textXML);
                    //string CadenaOriginal1 = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML1, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
                    //factura.CadenaOriginal = CadenaOriginal1;

                    ////
                    ////Timbrado //
                    //if (factura.PACFacturacion.ToLower().Contains("facturama"))
                    //{
                    //    Timbrado timbrado = new Timbrado();
                    //    var timbre = timbrado.Facturama(factura, ruta);

                    //    if (timbre.Error == null)
                    //    {
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}

                    String certificado = "";
                    string elXML = File.ReadAllText(@"C:\Users\Osvi\Documents\DNU\Cortes\credito\documentos\pruebaXML\EstadodeCuenta_sample_Ultra.xml");
                    string CadenaOriginal = LNOperacionesEdoCuenta.ObtieneCadenaOriginal(elXML, _transformador).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
                    factura.CadenaOriginal = CadenaOriginal;
                    String elSello = LNOperacionesEdoCuenta.ObtenerSelloFactura(CadenaOriginal, out noCertificado, out certificado, emisor, key, password);
                    factura.Sello = elSello;
                    elXML = elXML.Replace("[Sello]", elSello);
                    elXML = elXML.Replace("[NoCertificado]", noCertificado);
                    elXML = elXML.Replace("[Certificado]", certificado);
                    factura.NoCertificadoEmisor = noCertificado;
                    //
                    //text = text.Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
                    //CFDI xmlMySuit = new CFDI("");
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

                            laRespuesta = unaOperacionTest.RequestTransaction("0c320b03-d4f1-47bc-9fb4-77995f9bf33e", "TIMBRAR", "MX", "JES900109Q90", "0c320b03-d4f1-47bc-9fb4-77995f9bf33e", "MX.JES900109Q90.Juan", elXML, "", "");

                            // laRespuesta = unaOperacionTest.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", /*factura.Emisora.RFC*/"AAA010101AAA", factura.RequestorPAC, factura.UserPAC, elXML, "", "");
                            //laRespuesta = unaOperacionTest.RequestTransaction("cb6b2b32-2d5c-4c82-adc0-e1375c95a1a2", "TIMBRAR", "MX", "JES900109Q90", "cb6b2b32-2d5c-4c82-adc0-e1375c95a1a2", "MX.DDN1106065K2.DDN1106065K2", elXML, "", "");

                        }
                        else
                        {
                            //prueba prod

                            //8276653d-bc43-46f5-b7bd-b41f6a207cab
                            //MX.DDN1106065K2. DDN1106065K2
                            //prueba prod
                            laRespuesta = unaOperacion.RequestTransaction("8276653d-bc43-46f5-b7bd-b41f6a207cab", "TIMBRAR", "MX", "DDN1106065K2", "8276653d-bc43-46f5-b7bd-b41f6a207cab", "MX.DDN1106065K2. DDN1106065K2", elXML, "", "");

                            //laRespuesta = unaOperacion.RequestTransaction(factura.RequestorPAC, "TIMBRAR", "MX", factura.Emisora.RFC, factura.RequestorPAC, factura.UserPAC, elXML, "", "");

                            //no sirve
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
                            factura.LugarExpedicion = factura.LugarExpedicion;// laFactura.Emisora.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio;
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
                }
                catch (Exception ex)
                {
                    return false;
                }
            return facturaGeneradoCorrectamente;
        }

        public static bool descargarArchivoAWSSFTP(string pathLocalFile, List<String> listaDirectoriosDia)
        {
            Logueo.Evento("[GeneraEstadoCuentaDebito] Iniciando proceso descarga de archivo aws sftp");
            FileInfo fi = new FileInfo(pathLocalFile);
            LNOperaciones.crearDirectorio(fi.DirectoryName);
            string directorioBase = pathLocalFile;
            bool archivoGenerado = false;
            string servidorSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
            string puertoSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
            string usuarioSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
            string passUsuarioSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            string carpetaEstadosDecuentaAWS = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
            string carpetaInicialAWS = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP");
            string nameFile = PNConfig.Get("PROCESAEDOCUENTA", "PrivateKeySFTP");
            string directorioSalidaCTA = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalidaArchivos");//salida archivos

            DateTime fechaActual = DateTime.Now;
            string fecha = fechaActual.ToString("ddMMyy");
            //fecha = "290622";
            //carpetaEstadosDecuentaAWS = fecha + fecha;
            //Validando pdf en casoi de que ya existan para ya no volverlos a cargar
            directorioSalidaCTA = directorioSalidaCTA; //+ "Procesados\\";
            string[] archivos = new string[0];
            bool archivosProcesados = false;
            string[] listaCarpetasAws = carpetaEstadosDecuentaAWS.Split('/');
            string ultimaCarpetasAws = listaCarpetasAws[listaCarpetasAws.Length - 2] + "\\";
            // directorioBase = directorioBase + file.Name;



            //string servidorSFTPAWS = "s-c6d8c08a547747c38.server.transfer.us-east-1.amazonaws.com"; //PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
            //string puertoSFTPAWS = "22"; //PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
            //string usuarioSFTPAWS = "User_trfcards_edocta"; //PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
            //string passUsuarioSFTPAWS = "#P4r4b1l1um..2022"; //PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            //string carpetaEstadosDecuentaAWS = @"C:\ArchivosCortesTFR\CargaArchivos\Xsolicitud"; //PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
            //string carpetaInicialAWS = "/TRFCARDS/EdoCta/"; //PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP");
            //string nameFile = @"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\amazon\pkAWS";
            ////string rutaTxt = _validaciones.validaParametroDiccionario(Todos_losParametros, "@RutaGenEdC") ? Todos_losParametros["@RutaGenEdC"].Valor : ruta;

            PrivateKeyFile keyFile = new PrivateKeyFile(nameFile, passUsuarioSFTPAWS);
            var keyFiles = new[] { keyFile };

            try
            {
                Logueo.Evento("[GeneraEstadoCuentaDebito] conectando con sftp");
                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(usuarioSFTPAWS, passUsuarioSFTPAWS));
                methods.Add(new PrivateKeyAuthenticationMethod(usuarioSFTPAWS, keyFiles));

                KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(usuarioSFTPAWS);
                keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                Renci.SshNet.ConnectionInfo conInfo = new Renci.SshNet.ConnectionInfo(servidorSFTPAWS, Convert.ToInt32(puertoSFTPAWS), usuarioSFTPAWS, methods.ToArray()); //new Renci.SshNet.ConnectionInfo(servidorSFTPAWS, Convert.ToInt32(puertoSFTPAWS), usuarioSFTPAWS, keybAuth);


                //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                using (SftpClient sftp = new SftpClient(conInfo))
                {
                    //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                    //{
                    sftp.Connect();
                    Logueo.Evento("[GeneraEstadoCuentaDebito] conexion exitosa");
                    // string directoryMain = sftp.WorkingDirectory;// + "/" + carpetaInicial;

                    var files = sftp.ListDirectory(carpetaEstadosDecuentaAWS);

                    foreach (var file in files)
                    {
                        // Console.WriteLine(file.Name);
                        if (file.Name.Contains(fecha))
                        {
                            Logueo.Evento("[GeneraEstadoCuentaDebito] directorio encontrado:" + file.Name);
                            archivosProcesados = false;
                            //validamos si hay arhivos procesados PDF si es asi ya no se carga nada
                            string rutaProcesados = directorioSalidaCTA + "Correctos\\";
                            if (Directory.Exists(rutaProcesados + file.Name))
                            {
                                archivos = Directory.GetFiles(rutaProcesados + file.Name);
                                if (archivos.Length >= 1)
                                {
                                    archivosProcesados = true;

                                }
                            }


                            // directorioBase = directorioBase + file.Name;

                            string rutaDespositoArchivosAProcesar = directorioBase + ultimaCarpetasAws + file.Name;
                            if (archivosProcesados)//Directory.Exists(directorioBase + file.Name))
                            {

                                Logueo.Evento("[GeneraEstadoCuentaDebito] directorio ya procesado:" + file.Name);

                                //  Directory.CreateDirectory(ruta);
                            }
                            else
                            {
                                LNOperaciones.crearDirectorio(rutaDespositoArchivosAProcesar);
                                listaDirectoriosDia.Add(file.Name);//file.Name);//nombre de la carpeta
                            }



                        }
                    }
                    Logueo.Evento("[GeneraEstadoCuentaDebito] se encontararon" + listaDirectoriosDia.Count + " directorios");
                    foreach (string directorio in listaDirectoriosDia)
                    {
                        var filesDate = sftp.ListDirectory(carpetaEstadosDecuentaAWS + directorio);
                        try
                        {
                            directorioBase = directorioBase + ultimaCarpetasAws + directorio;
                            foreach (var file in filesDate)
                            {
                                if (!file.Name.ToLower().Contains("procesado"))
                                {
                                    Logueo.Evento("[GeneraEstadoCuentaDebito] descargando archivo" + file.Name + " de directorio: " + directorio);
                                    archivoGenerado = SFTPService.DownloadFileWithoutConnection(sftp, file.FullName, directorioBase + "\\" + file.Name);
                                    Logueo.Evento("[GeneraEstadoCuentaDebito] archivo archivo");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar archivo:" + ex.Message);
                        }
                        directorioBase = pathLocalFile;
                    }

                    if (!archivoGenerado)
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar archivos");

                    }


                    sftp.Disconnect();
                }
            }
            catch (Exception exEdo)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar archivo temproal sftp:" + exEdo.Message);

            }

            return archivoGenerado;
        }

        public static bool subirArhivosAWSSFTP(List<String> listaArchivos, string fechaFormateada)
        {
            Logueo.Evento("[GeneraEstadoCuentaDebito] Iniciando proceso de carga de archivo aws sftp");

            bool archivoGenerado = false;
            string servidorSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "ServidorSFTP");
            string puertoSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "PuertoSFTP");
            string usuarioSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "UsuarioSFTP");
            string passUsuarioSFTPAWS = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
            string carpetaEstadosDecuentaAWS = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaEstadosDecuenta");
            string carpetaInicialAWS = PNConfig.Get("PROCESAEDOCUENTA", "CarpetaInicialSFTP");
            string nameFile = PNConfig.Get("PROCESAEDOCUENTA", "PrivateKeySFTP");
            carpetaEstadosDecuentaAWS = carpetaEstadosDecuentaAWS.Replace("CSV", "PDF");
            string carpetaEstadosDecuentaAWSXML = carpetaEstadosDecuentaAWS.Replace("PDF", "XML");
            string carpetaEstadosDecuentaAWSXLS = carpetaEstadosDecuentaAWS.Replace("PDF", "XLS");
            // fecha = "290622";
            //carpetaEstadosDecuentaAWS = fecha + fecha;
            PrivateKeyFile keyFile = new PrivateKeyFile(nameFile, passUsuarioSFTPAWS);
            var keyFiles = new[] { keyFile };

            try
            {
                Logueo.Evento("[GeneraEstadoCuentaDebito] conectando con sftp");
                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(usuarioSFTPAWS, passUsuarioSFTPAWS));
                methods.Add(new PrivateKeyAuthenticationMethod(usuarioSFTPAWS, keyFiles));

                KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(usuarioSFTPAWS);
                keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                Renci.SshNet.ConnectionInfo conInfo = new Renci.SshNet.ConnectionInfo(servidorSFTPAWS, Convert.ToInt32(puertoSFTPAWS), usuarioSFTPAWS, methods.ToArray()); //new Renci.SshNet.ConnectionInfo(servidorSFTPAWS, Convert.ToInt32(puertoSFTPAWS), usuarioSFTPAWS, keybAuth);


                //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                using (SftpClient sftp = new SftpClient(conInfo))
                {
                    //using (SftpClient sftp = new SftpClient(servidorSFTP, Convert.ToInt32(puertoSFTP), usuarioSFTP, passUsuarioSFTP))
                    //{
                    sftp.Connect();
                    Logueo.Evento("[GeneraEstadoCuentaDebito] conexion exitosa");
                    string directoryMain = sftp.WorkingDirectory;
                    if (SFTPService.CreateDirectoryWithoutConnection(carpetaEstadosDecuentaAWS + fechaFormateada, sftp))
                    {
                        sftp.ChangeDirectory(directoryMain);
                        if (SFTPService.CreateDirectoryWithoutConnection(carpetaEstadosDecuentaAWSXML + fechaFormateada, sftp))
                        {
                            sftp.ChangeDirectory(directoryMain);
                            if (SFTPService.CreateDirectoryWithoutConnection(carpetaEstadosDecuentaAWSXML + fechaFormateada, sftp))
                            {
                                sftp.ChangeDirectory(directoryMain);
                                //  Logueo.Evento("[GeneraEstadoCuentaDebito] se encontararon" + listaDirectoriosDia.Count + " directorios");
                                foreach (string archivo in listaArchivos)
                                {
                                    try
                                    {
                                        if (archivo.Contains(".pdf"))
                                        {
                                            sftp.ChangeDirectory(carpetaEstadosDecuentaAWS + fechaFormateada);
                                        }
                                        else if (archivo.Contains(".xls"))
                                        {
                                            sftp.ChangeDirectory(carpetaEstadosDecuentaAWSXLS + fechaFormateada);
                                        }
                                        else
                                        {
                                            sftp.ChangeDirectory(carpetaEstadosDecuentaAWSXML + fechaFormateada);
                                        }
                                        Logueo.Evento("[GeneraEstadoCuentaDebito] cargando archivo" + archivo);
                                        archivoGenerado = SFTPService.CreateFileWithoutConnection(sftp, archivo);//ene ste punto ya se cargo la carpeta(working directory) en la crecion del directorio por eso ya no se pasa en el parametro
                                        Logueo.Evento("[GeneraEstadoCuentaDebito] archivo cargado");
                                        if (!archivoGenerado)
                                        {
                                            Logueo.Error("[GeneraEstadoCuentaCredito] error al cargar archivo" + archivo);

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logueo.Error("[GeneraEstadoCuentaCredito] error al cargar archivo:" + ex.Message);
                                    }
                                    sftp.ChangeDirectory(directoryMain);
                                }
                            }
                            else
                            {
                                Logueo.Error("[GeneraEstadoCuentaCredito] error al crear directorio xls:" + carpetaEstadosDecuentaAWS + fechaFormateada);
                            }
                        }
                        else
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] error al crear directorio xml:" + carpetaEstadosDecuentaAWS + fechaFormateada);
                        }
                    }
                    else
                    {
                        Logueo.Error("[GeneraEstadoCuentaCredito] error al crear directorio:" + carpetaEstadosDecuentaAWS + fechaFormateada);
                    }



                    sftp.Disconnect();
                }
            }
            catch (Exception exEdo)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al bajar archivo temproal sftp:" + exEdo.Message);

            }

            return archivoGenerado;
        }
    }
}
