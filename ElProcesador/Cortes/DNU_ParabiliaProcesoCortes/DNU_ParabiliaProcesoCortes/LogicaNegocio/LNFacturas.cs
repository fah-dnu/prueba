//using DALAutorizador.Entidades;
using CommonProcesador;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Entidades;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNFacturas
    {

        DAOFacturas _daoFacturas;
        public LNFacturas()
        {
            _daoFacturas = new DAOFacturas();
        }
        public bool insertarFactura(Factura factura)
        {

            return true;
        }
        public static void generaCodigoQR(string ruta, string texto)
        {

            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            QrCode qrCode = new QrCode();
            qrEncoder.TryEncode(texto, out qrCode);

            GraphicsRenderer renderer = new GraphicsRenderer(new FixedCodeSize(400, QuietZoneModules.Zero), Brushes.Black, Brushes.White);

            MemoryStream ms = new MemoryStream();

            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, ms);
            var imageTemporal = new Bitmap(ms);
            var imagen = new Bitmap(imageTemporal, new Size(new Point(200, 200)));
            //Guardar en el disco duro la imagen (Carpeta del proyecto)
            imagen.Save(ruta, ImageFormat.Png);

        }

        public static Factura obtenerDatosFactura(Dictionary<string, Parametro> todosLosParametros, Decimal sumaComisiones, Decimal ivaComision, Decimal iva, Decimal ivaIntereses, decimal impIvaOrd, decimal impIvaMor, bool facturaEnBlanco, Cuentas cuenta, List<DetalleFactura> listaDetallesFactura, List<DetalleFactura> listaDetallesFacturaXML, LNValidacionesCampos _validaciones, string folioFactura, Cuentas cuentaEmisorPAC)
        {
            Factura laFactura = new Factura();
            string CP = cuentaEmisorPAC.CP;
            laFactura.LugarExpedicion = (!string.IsNullOrEmpty(CP))?CP:_validaciones.validaParametroDiccionario(todosLosParametros, "@LugarExpedicion") ? todosLosParametros["@LugarExpedicion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "LugarExpedicion"); // "03240";
            laFactura.MetodoPago = _validaciones.validaParametroDiccionario(todosLosParametros, "@MetodoPago") ? todosLosParametros["@MetodoPago"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "MetodoPago"); //"PUE";
            laFactura.TipoComprobante = "I";
            laFactura.FormaPago = _validaciones.validaParametroDiccionario(todosLosParametros, "@FormaPago") ? todosLosParametros["@FormaPago"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "FormaPago"); //"03";
            DateTime Hoy = DateTime.Now;
            string fechaConvetida = Hoy.ToString("dd/MM/yyyy HH:mm:ss");  //Hoy.ToString("dd/MM/yyyy hh:mm:ss tt");//HH
            laFactura.FechaEmision =Convert.ToDateTime(fechaConvetida); //DateTime.ParseExact(fechaConvetida, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);//Convert.ToDateTime(fechaConvetida);
            laFactura.RequestorPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@RequestorPACTimbre") ? todosLosParametros["@RequestorPACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "RequestorPACTimbre");
            laFactura.UserPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@UserNamePACTimbre") ? todosLosParametros["@UserNamePACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "UserNamePACTimbre");
            laFactura.UserPassPAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@UserPassPACTimbre") ? todosLosParametros["@UserPassPACTimbre"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PassUserNamePACTimbre");
            string nombrePAC = _validaciones.validaParametroDiccionario(todosLosParametros, "@PACFacturacion") ? todosLosParametros["@PACFacturacion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
            laFactura.URLProvedorCertificado = obtenerURLPACFacturacion(nombrePAC,laFactura);
            laFactura.Folio = folioFactura;// "5491XXXXXXXX6877";
            Dictionary<String, ParametroFacturaTipo> larespuesta = new Dictionary<String, ParametroFacturaTipo>();
            ParametroFacturaTipo unValorRegla = new ParametroFacturaTipo();
            string usoCFDI = _validaciones.validaParametroDiccionario(todosLosParametros, "@UsoCFDI") ? todosLosParametros["@UsoCFDI"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "UsoCFDI");                                                                                                                                           //"601";
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
            Colectiva emisor = new Colectiva { RFC = string.IsNullOrEmpty(cuentaEmisorPAC.RFCCliente) ? PNConfig.Get("PROCESAEDOCUENTA", "RFCEmisorPACTimbre"): cuentaEmisorPAC.RFCCliente/*cuenta.RFCCliente*//*"AAA010101AAA"*/, NombreORazonSocial = cuentaEmisorPAC.NombreORazonSocial /*"Cadena DNU Mexico Central"*/ , ID_Colectiva = cuentaEmisorPAC.id_colectivaCliente };
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
                laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscalFisica");//616
                string appelidoPaterno = string.IsNullOrEmpty(cuenta.ApellidoPaternoCuentahabiente) ? "" : " " + cuenta.ApellidoPaternoCuentahabiente;
                string appelidoMaterno = string.IsNullOrEmpty(cuenta.ApellidoMaternoCuentahabiente) ? "" : " " + cuenta.ApellidoMaternoCuentahabiente;
                receptor.NombreORazonSocial = receptor.NombreORazonSocial + appelidoPaterno + appelidoMaterno;
            }
            else
            {
                laFactura.RegimenFiscalReceptor = PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal");
            }
            laFactura.DomicilioFiscalReceptor = cuenta.CPCliente;//laFactura.LugarExpedicion;//receptor.RFC == "XAXX010101000" ? laFactura.LugarExpedicion : "01030";
           
            unValorRegla = new ParametroFacturaTipo();
            unValorRegla.Nombre = "@RegimenFiscal";//emisor
            unValorRegla.Valor = laFactura.RegimenFiscal;//_validaciones.validaParametroDiccionario(todosLosParametros, "@RegimenFiscal") ? todosLosParametros["@RegimenFiscal"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "RegimenFiscal"); //"03";
            larespuesta.Add(unValorRegla.Nombre, unValorRegla);
            laFactura.Emisora = emisor;
            laFactura.Receptora = receptor;
            laFactura.PACFacturacion = _validaciones.validaParametroDiccionario(todosLosParametros, "@PACFacturacion") ? todosLosParametros["@PACFacturacion"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "PACDefault");
            //  List<DetalleFactura> listaDetallesFactura = new List<DetalleFactura>();
            laFactura.RutaCerSAT= _validaciones.validaParametroDiccionario(todosLosParametros, "@RutaCerSAT") ? todosLosParametros["@RutaCerSAT"].Valor : PNConfig.Get("PROCESAEDOCUENTA", "CertificadoSAT");
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
                    if (valorInteres > 0)
                    {
                        //INTERES EXENTO
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

        public static Factura obtenerDatosFacturaCFDI4(Dictionary<string, Parametro> todosLosParametros, Decimal sumaComisiones, Decimal ivaComision, Decimal iva, Decimal ivaIntereses, decimal impIvaOrd, decimal impIvaMor, bool facturaEnBlanco, Cuentas cuenta, List<DetalleFactura> listaDetallesFactura, List<DetalleFactura> listaDetallesFacturaXML, LNValidacionesCampos _validaciones, string folioFactura, Cuentas cuentaEmisorPAC,
            List<DetalleFactura> listaDetallesFacturaExtra)
        {
            Factura laFactura = new Factura();
            string CP = cuentaEmisorPAC.CP;
            if (string.IsNullOrEmpty(CP)) {
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
            laFactura.URLProvedorCertificado = obtenerURLPACFacturacion(nombrePAC, laFactura);
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

        public static string obtenerURLPACFacturacion(string nombrePAC, Factura laFactura)
        {
            string urlPAC = "";
            if (nombrePAC.ToLower() == "mysuite")
            {
                urlPAC = PNConfig.Get("PROCESAEDOCUENTA", "RutaSoapMySuit");
            }
            else if (nombrePAC.ToLower() == "facturama")
            {
                urlPAC = PNConfig.Get("PROCESAEDOCUENTA", "URLPACFacturama");
                laFactura.URLProvedorCertificadoObtieneXML = PNConfig.Get("PROCESAEDOCUENTA", "URLPACFacturamaObtenerXML");
            }
            return urlPAC;
        }
    }

}
