using CommonProcesador;
using DNU_Multipacs;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.Contratos;
using DNU_ParabiliaProcesoCortes.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio.LNCortes
{
    class LNFacturacion : Facturacion
    {
        //XslCompiledTransform _transformador;
        //public LNFacturacion(XslCompiledTransform _transformador)
        //{
        //    this._transformador = _transformador;
        //}
        public override bool generarFacturaV4(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<string> archivos, Cuentas cuenta, XslCompiledTransform _transformador, string nombreArchivo, bool rutaProcesado, string nombreXML = null)
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
                // factura.Receptora.NombreORazonSocial = "JIMENEZ ESTRADA & SALAS A A";
                factura.Receptora.NombreORazonSocial = "JIMENEZ ESTRADA SALAS A A";
                factura.Emisora.NombreORazonSocial = "JIMENEZ ESTRADA SALAS A A";
                factura.LugarExpedicion = "01030";
                factura.DomicilioFiscalReceptor = "01030";
                factura.RegimenFiscalReceptor = "601";
                factura.UsoCFDI = "G03";

            }


            if (factura.Receptora.RFC == "XAXX010101000")
            {
                factura.RegimenFiscalReceptor = "616";
                factura.UsoCFDI = "S01";
                factura.DomicilioFiscalReceptor = factura.LugarExpedicion; 
            }


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
            if (!string.IsNullOrEmpty(nombreXML))
            {
                xDoc.Save(ruta + nombreXML + "_Factura_Original.xml");
            }
            else
            {
                xDoc.Save(ruta + "Factura_Original.xml");
            }
          //  elXML= elXML.Replace("&amp;","&");

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
                    if (!string.IsNullOrEmpty(nombreXML))
                    {
                        xDoc.Save(ruta + nombreXML + "_Factura_Timbrada.xml");//+ fecha + ".xml");
                        archivos.Add(ruta + nombreXML + "_Factura_Timbrada.xml");
                    }
                    else
                    {
                        xDoc.Save(ruta + "Factura_Timbrada.xml");//+ fecha + ".xml");
                        archivos.Add(ruta + "Factura_Timbrada.xml");
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

        public override Factura obtenerDatosFacturaCFDI4(Dictionary<string, Parametro> todosLosParametros, decimal sumaComisiones, decimal ivaComision, decimal iva, decimal ivaIntereses, decimal impIvaOrd, decimal impIvaMor, bool facturaEnBlanco, Cuentas cuenta, List<DetalleFactura> listaDetallesFactura, List<DetalleFactura> listaDetallesFacturaXML, LNValidacionesCampos _validaciones, string folioFactura, Cuentas cuentaEmisorPAC, List<DetalleFactura> listaDetallesFacturaExtra)
        {
            Factura laFactura = new Factura();
            string CP = cuentaEmisorPAC.CP;
            if (string.IsNullOrEmpty(CP))
            {
                CP = cuenta.CPCliente;
            }
            laFactura.LugarExpedicion = (!string.IsNullOrEmpty(CP)) ? CP : _validaciones.validaParametroDiccionario(todosLosParametros, "@LugarExpedicion") ? todosLosParametros["@LugarExpedicion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "LugarExpedicion"); // "03240";
            laFactura.MetodoPago = _validaciones.validaParametroDiccionario(todosLosParametros, "@MetodoPago") ? todosLosParametros["@MetodoPago"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "MetodoPago"); //"PUE";
            laFactura.TipoComprobante = "I";
            laFactura.FormaPago = _validaciones.validaParametroDiccionario(todosLosParametros, "@FormaPago") ? todosLosParametros["@FormaPago"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "FormaPago"); //"03";
            DateTime Hoy = DateTime.Now;
            string fechaConvetida = Hoy.ToString("dd/MM/yyyy HH:mm:ss");  //Hoy.ToString("dd/MM/yyyy hh:mm:ss tt");//HH
            laFactura.FechaEmision = Convert.ToDateTime(fechaConvetida); //DateTime.ParseExact(fechaConvetida, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);//Convert.ToDateTime(fechaConvetida);
            laFactura.RequestorPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@RequestorPACTimbre") ? todosLosParametros["@RequestorPACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "RequestorPACTimbre");
            laFactura.UserPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@UserNamePACTimbre") ? todosLosParametros["@UserNamePACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "UserNamePACTimbre");
            laFactura.UserPassPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@UserPassPACTimbre") ? todosLosParametros["@UserPassPACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PassUserNamePACTimbre");
            string nombrePAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@PACFacturacion") ? todosLosParametros["@PACFacturacion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
            laFactura.URLProvedorCertificado = FuncionesFacturas.obtenerURLPACFacturacion(nombrePAC, laFactura);//obtenerURLPACFacturacion(nombrePAC, laFactura);
            laFactura.Folio = folioFactura;// "5491XXXXXXXX6877";
            Dictionary<String, ParametroFacturaTipo> larespuesta = new Dictionary<String, ParametroFacturaTipo>();
            ParametroFacturaTipo unValorRegla = new ParametroFacturaTipo();
            string usoCFDI = cuenta.UsoCFDI; //_validaciones.validaParametroDiccionario(todosLosParametros, "@UsoCFDI") ? todosLosParametros["@UsoCFDI"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "UsoCFDI");                                                                                                                                           //"601";
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@UsoCFDI";//receptor
            unValorRegla.Valor = usoCFDI; //"G03";
            laFactura.UsoCFDI = usoCFDI;
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@IVADescripcion";
            unValorRegla.Valor = "";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@FactorIVA";
            unValorRegla.Valor = iva.ToString();
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@TipoFactor";
            unValorRegla.Valor = "Tasa";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@FormaPago";
            unValorRegla.Valor = laFactura.FormaPago;// "03";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@MetodoPago";
            unValorRegla.Valor = laFactura.MetodoPago;//"PUE";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@TipoComprobante";
            unValorRegla.Valor = laFactura.TipoComprobante;
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@LugarExpedicion";
            unValorRegla.Valor = laFactura.LugarExpedicion;// "03240";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            unValorRegla = new ParametroFacturaTipo();
            laFactura.IVA = ivaIntereses + ivaComision;
            laFactura.ParametrosCalculados = larespuesta;
            Colectiva emisor = new Colectiva { RFC = string.IsNullOrEmpty(cuentaEmisorPAC.RFCCliente) ? PNConfig.Get("PROCESAEDOCUENTA", "RFCEmisorPACTimbre") : cuentaEmisorPAC.RFCCliente/*cuenta.RFCCliente*//*"AAA010101AAA"*/, NombreORazonSocial = cuentaEmisorPAC.NombreORazonSocial /*"Cadena DNU Mexico Central"*/ , ID_Colectiva = cuentaEmisorPAC.id_colectivaCliente };
            Colectiva receptor = new Colectiva { RFC = string.IsNullOrEmpty(cuenta.RFCCuentahabiente) ? "XAXX010101000" : cuenta.RFCCuentahabiente, NombreORazonSocial = cuenta.NombreCuentahabiente.Trim(), ID_Colectiva = cuenta.ID_CuentaHabiente };
            laFactura.RegimenFiscal = _validaciones.validaParametroDiccionario(todosLosParametros, "@RegimenFiscal") ? todosLosParametros["@RegimenFiscal"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal");
            if (emisor.RFC.Length == 13)//persona fisica
            {
                laFactura.RegimenFiscal = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscalFisica");
            }
            else
            {
                laFactura.RegimenFiscal = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal");

            }
            if (receptor.RFC.Length == 13)//persona fisica
            {
                // laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscalFisica");//616
                string appelidoPaterno = string.IsNullOrEmpty(cuenta.ApellidoPaternoCuentahabiente) ? "" : " " + cuenta.ApellidoPaternoCuentahabiente;
                string appelidoMaterno = string.IsNullOrEmpty(cuenta.ApellidoMaternoCuentahabiente) ? "" : " " + cuenta.ApellidoMaternoCuentahabiente;
                receptor.NombreORazonSocial = receptor.NombreORazonSocial + appelidoPaterno + appelidoMaterno;
            }
            else
            {
                // laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal");
            }
            laFactura.RegimenFiscalReceptor = cuenta.RegimenFiscal;
            laFactura.DomicilioFiscalReceptor = cuenta.CPFiscal;//cuenta.CPCliente;//laFactura.LugarExpedicion;//receptor.RFC == "XAXX010101000" ? laFactura.LugarExpedicion : "01030";

            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@RegimenFiscal";//emisor
            unValorRegla.Valor = laFactura.RegimenFiscal;//_validaciones.validaParametroDiccionario(todosLosParametros, "@RegimenFiscal") ? todosLosParametros["@RegimenFiscal"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal"); //"03";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            laFactura.Emisora = emisor;
            laFactura.Receptora = receptor;
            laFactura.PACFacturacion = _validaciones.validaParametroDiccionario(todosLosParametros, "@PACFacturacion") ? todosLosParametros["@PACFacturacion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
            //  List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
            laFactura.RutaCerSAT = _validaciones.validaParametroDiccionario(todosLosParametros, "@RutaCerSAT") ? todosLosParametros["@RutaCerSAT"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
            laFactura.RutaKeySAT = _validaciones.validaParametroDiccionario(todosLosParametros, "@RutaKeySAT") ? todosLosParametros["@RutaKeySAT"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "LlaveCertificadoSAT");
            laFactura.PassCerSAT = _validaciones.validaParametroDiccionario(todosLosParametros, "@PassCerSAT") ? todosLosParametros["@PassCerSAT"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PassCertificadoSAT");
            if (sumaComisiones > 0)
            {
                DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "COMISION COBRADA", impImporte = ivaComision.ToString(), Total = sumaComisiones, PrecioUnitario = sumaComisiones, impBase = sumaComisiones.ToString(), impImpuesto = "002", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString("###,###,##0.000000") };
                listaDetallesFacturaXML.Add(detalle);
            }
            foreach (string parametro in todosLosParametros.Keys)
            {
                Decimal valorInteres = 0;
                if (parametro == "@INTORD")
                {
                    valorInteres = Convert.ToDecimal(todosLosParametros[parametro].Valor);
                    if (listaDetallesFacturaExtra != null)
                    {
                        foreach (DetalleFactura detalle in listaDetallesFacturaExtra)
                        {
                            //impIvaOrd= impIvaOrd + Convert.ToDecimal(detalle.impImporte);
                            //aui se comento este dato porque el importe de los diferimientos ya se sumo antes de entra a este metodo
                            //se hizo la suma de los interees ordinarios mas los intereses de difermiento
                            valorInteres = valorInteres + detalle.Total;
                        }
                    }

                    if (valorInteres > 0)
                    {
                        //INTERES EXENTO
                        //impIvaOrd es
                        sumaComisiones = sumaComisiones + valorInteres;
                        DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "INTERES ORDINARIO", impImporte = impIvaOrd.ToString(), Total = valorInteres, PrecioUnitario = valorInteres, impBase = valorInteres.ToString(), impImpuesto = "002", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString("###,###,##0.000000") };
                        listaDetallesFacturaXML.Add(detalle);
                    }
                }
                if (parametro == "@INTMOR")
                {
                    valorInteres = Convert.ToDecimal(todosLosParametros[parametro].Valor);
                    if (valorInteres > 0)
                    {
                        //INTERES GRAVABLE
                        sumaComisiones = sumaComisiones + valorInteres;
                        DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "INTERES MORATORIO", impImporte = impIvaMor.ToString(), Total = valorInteres, PrecioUnitario = valorInteres, impBase = valorInteres.ToString(), impImpuesto = "002", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString("###,###,##0.000000") };
                        listaDetallesFacturaXML.Add(detalle);
                    }
                }

            }
            decimal totalOtrosConceptos = Convert.ToDecimal(0.00);
            if (facturaEnBlanco)
            {
                //  totalOtrosConceptos = Convert.ToDecimal(0.01);
                //  DetalleFactura detalle = new DetalleFactura { Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "Otros conceptos no afectados para ISR e IVA", impImporte = "0.00", Total = Convert.ToDecimal(0.01), PrecioUnitario = Convert.ToDecimal(0.01), impBase = "0", impImpuesto = "0", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString() };
                //   listaDetallesFactura.Add(detalle);
            }

            laFactura.losDetalles = listaDetallesFacturaXML;
            laFactura.SubTotal = Convert.ToDecimal(sumaComisiones.ToString("########0.00"));
            laFactura.SubTotal = laFactura.SubTotal + totalOtrosConceptos;
            return laFactura;
        }
    }
}
