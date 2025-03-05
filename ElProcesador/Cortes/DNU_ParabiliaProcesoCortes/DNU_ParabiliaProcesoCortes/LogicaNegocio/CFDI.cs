using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Entidades;
//using DALAutorizador.Entidades;
//using DALAutorizador.Entidades;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class CFDI
    {
        StringBuilder elXML = new StringBuilder();
        Factura laFacturaLocal;



       public CFDI(Factura laFactura, bool facturaEnBlanco)
        {
            try
            {

                laFacturaLocal = laFactura;
                //elXML= new StringBuilder(elNuevoXML);
                //  elXML.Append(" <?xml version=\"1.0\" encoding=\"UTF-8\"?> ");
                elXML.Append(" <cfdi:Comprobante xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                elXML.Append(" xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\"");
                elXML.Append(" xmlns:tfd=\"http://www.sat.gob.mx/TimbreFiscalDigital\"");
                elXML.Append(" xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/3");
                elXML.Append(" http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd");
                elXML.Append(" http://www.sat.gob.mx/TimbreFiscalDigital");
                elXML.Append(" http://www.sat.gob.mx/sitio_internet/TimbreFiscalDigital/TimbreFiscalDigital.xsd\"");
                elXML.Append(" Version=\"3.3\"");
                elXML.Append(" Sello=\"[Sello]\"");
                //elXML.Append(" Serie=\"" + laFactura.Serie + "\"");
                elXML.Append(" Folio=\"" + laFactura.Folio + "\"");
                elXML.Append(" Fecha=\"" + laFactura.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss") + "\"");
                elXML.Append(" FormaPago=\"" + laFactura.FormaPago + "\"");

                //elXML.Append(" FormaPago=\"" + laFactura.getParametro("@FormaPago") + "\"");
                elXML.Append(" NoCertificado=\"[NoCertificado]\"");
                elXML.Append(" Certificado=\"[Certificado]\"");
                elXML.Append(" SubTotal=\"" + Decimal.Round(laFactura.SubTotal, 2) + "\"");
                //  elXML.Append(" SubTotal=\"" + (laFactura.SubTotal) + "\"");
                // elXML.Append(" Descuento=\"0.00\"");
                elXML.Append(" Total=\"" + (Decimal.Round(laFactura.SubTotal, 2) + Decimal.Round(laFactura.IVA, 2)) + "\"");
                //elXML.Append(" Total=\"" + (laFactura.ImporteTotal) + "\"");
                //elXML.Append(" MetodoPago=\"" + laFactura.getParametro("@MetodoPago") + "\"");
                elXML.Append(" MetodoPago=\"" + laFactura.MetodoPago + "\"");
                elXML.Append(" TipoDeComprobante=\"" + laFactura.TipoComprobante/*laFactura.getParametro("@TipoComprobante")*/ + "\"");
                elXML.Append(" TipoCambio=\"1\"");
                elXML.Append(" Moneda=\"MXN\"");
                elXML.Append(" LugarExpedicion=\"" + laFactura.LugarExpedicion/*laFactura.getParametro("@LugarExpedicion")*/ + "\">");

                elXML.Append(setEmisor(laFactura.Emisora));

                elXML.Append(SetReceptor(laFactura.Receptora));

                elXML.Append("<cfdi:Conceptos> ");

                if (laFactura.losDetalles.Count > 1)
                {
                    foreach (DetalleFactura detalle in laFactura.losDetalles)
                    {
                        elXML.Append(NuevoConcepto(detalle));
                    }
                }
                else
                {
                    foreach (DetalleFactura detalle in laFactura.losDetalles)
                    {
                        elXML.Append(NuevoConcepto(detalle, facturaEnBlanco));
                    }
                }


                elXML.Append("</cfdi:Conceptos> ");
                if (laFactura.losDetalles.Count >= 1)
                {
                    elXML.Append(SetImpuestoTrasladado(""/*laFactura.getParametro("@IVADescripcion")*/, /*laFactura.getParametro("@FactorIVA")*/"0.16", laFactura.IVA.ToString(), "Tasa"/*laFactura.getParametro("@TipoFactor")*/));
                }
                elXML.Append("</cfdi:Comprobante> ");



            }
            catch (Exception err)
            {
                Logueo.Error("FACTURACION" + err.Message + " " + err.StackTrace);

                // Loguear.Error(err, "FACTURACION");
            }
        }

        String setEmisor(Colectiva elEmisor)
        {
            try
            {
                StringBuilder unEmisor = new StringBuilder();

                unEmisor.Append("<cfdi:Emisor ");
                unEmisor.Append("Rfc=\"" + elEmisor.RFC + "\" ");

                unEmisor.Append("Nombre=\"" + elEmisor.NombreORazonSocial + " " + elEmisor.APaterno + " " + elEmisor.AMaterno + "\" ");
                unEmisor.Append("RegimenFiscal=\"" + laFacturaLocal.RegimenFiscal /*laFacturaLocal.getParametro("@RegimenFiscal")*/ + "\">");
                //unEmisor.Append("<cfdi:DomicilioFiscal ");
                //unEmisor.Append(" calle=\"" + elEmisor.DFacturacion.Calle + "\" ");
                //unEmisor.Append(" noExterior=\"" + elEmisor.DFacturacion.NumExterior + "\" ");
                //unEmisor.Append(" colonia=\"" + elEmisor.DFacturacion.Asentamiento.DesAsentamiento + "\" ");
                //unEmisor.Append(" localidad=\"" + elEmisor.DFacturacion.Asentamiento.DesAsentamiento + "\" ");
                //unEmisor.Append(" municipio=\"" + elEmisor.DFacturacion.Asentamiento.ElMunicipio.DesMunicipio + "\" ");
                //unEmisor.Append(" estado=\"" + elEmisor.DFacturacion.Asentamiento.ElEstado.Descripcion + "\" ");
                //unEmisor.Append(" pais=\"MEXICO\"");
                //unEmisor.Append(" codigoPostal=\"" + elEmisor.DFacturacion.Asentamiento.CodigoPostal + "\" /> ");
                //unEmisor.Append(" <cfdi:ExpedidoEn ");
                //unEmisor.Append(" calle=\"" + elEmisor.DUbicacion.Calle + "\" ");
                //unEmisor.Append(" noExterior=\"" + elEmisor.DUbicacion.NumExterior + "\" ");
                //unEmisor.Append(" colonia=\"" + elEmisor.DUbicacion.Asentamiento.DesAsentamiento + "\" ");
                //unEmisor.Append(" localidad=\"" + elEmisor.DUbicacion.Asentamiento.DesAsentamiento + "\" ");
                //unEmisor.Append(" municipio=\"" + elEmisor.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio + "\" ");
                //unEmisor.Append(" estado=\"" + elEmisor.DUbicacion.Asentamiento.ElEstado.Descripcion + "\" ");
                //unEmisor.Append(" pais=\"MEXICO\"  />");
                //unEmisor.Append(" <cfdi:RegimenFiscal Regimen=\"" + elEmisor.ObtieneParametro("@RegimenFiscal") + "\" /> ");
                unEmisor.Append("</cfdi:Emisor> ");

                return unEmisor.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error("FACTURACION" + err.Message + " " + err.StackTrace);
                return "";
            }

        }

        private String SetReceptor(Colectiva Receptora)
        {
            try
            {
                StringBuilder unReceptor = new StringBuilder();

                unReceptor.Append(" <cfdi:Receptor ");
                unReceptor.Append(" Rfc=\"" + Receptora.RFC + "\" ");
                unReceptor.Append(" Nombre=\"" + Receptora.NombreORazonSocial + " " + Receptora.APaterno + " " + Receptora.AMaterno + "\" ");
                unReceptor.Append(" UsoCFDI=\"" + laFacturaLocal.UsoCFDI/*laFacturaLocal.getParametro("@UsoCFDI")*/ + "\">");
                //unReceptor.Append(" <cfdi:Domicilio ");
                //unReceptor.Append(" calle=\"" + Receptora.DFacturacion.Calle + "\" ");
                //unReceptor.Append(" noExterior=\"" + Receptora.DFacturacion.NumExterior + "\" ");
                //unReceptor.Append(" noInterior=\"" + Receptora.DFacturacion.NumInterior + "\" ");
                //unReceptor.Append(" colonia=\"" + Receptora.DFacturacion.Asentamiento.DesAsentamiento + "\" ");
                //unReceptor.Append(" localidad=\"" + Receptora.DFacturacion.Asentamiento.DesAsentamiento + "\" ");
                //unReceptor.Append(" municipio=\"" + Receptora.DFacturacion.Asentamiento.ElMunicipio.DesMunicipio + "\" ");
                //unReceptor.Append(" estado=\"" + Receptora.DFacturacion.Asentamiento.ElEstado.Descripcion + "\" ");
                //unReceptor.Append(" pais=\"MEXICO\"");
                //unReceptor.Append(" codigoPostal=\"" + Receptora.DFacturacion.Asentamiento.CodigoPostal + "\" /> ");
                unReceptor.Append("</cfdi:Receptor> ");

                return unReceptor.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error("FACTURACION" + err.Message + " " + err.StackTrace);

                return "";
            }

        }

        public String SetImpuestoTrasladado(String impuesto, String Tasa, String Importe, String TipoFactor)
        {
            StringBuilder elIVA = new StringBuilder();

            elIVA.Append(" <cfdi:Impuestos TotalImpuestosTrasladados=\"" + Decimal.Round(Decimal.Parse(Importe), 2) + "\"> ");
            // elIVA.Append(" <cfdi:Impuestos TotalImpuestosTrasladados=\"" + (Importe) + "\"> ");
            elIVA.Append(" <cfdi:Traslados> ");
            // elIVA.Append(" <cfdi:Traslado impuesto=\"" + impuesto + "\" TipoFactor=\"" + TipoFactor +  "\" TasaOCuota=\"" + Tasa + "\" importe=\"" + Importe + "\" /> ");
            elIVA.Append(" <cfdi:Traslado Impuesto=\"" + "002" + "\" TipoFactor=\"" + "Tasa" + "\" TasaOCuota=\"" + "0.160000" + "\" Importe=\"" + Decimal.Round(Decimal.Parse(Importe), 2) + "\" /> ");
            //elIVA.Append(" <cfdi:Traslado Impuesto=\"" + "002" + "\" TipoFactor=\"" + "Tasa" + "\" TasaOCuota=\"" + "0.160000" + "\" Importe=\"" + ((Importe)) + "\" /> ");
            elIVA.Append(" </cfdi:Traslados> ");
            elIVA.Append(" </cfdi:Impuestos> ");
            return elIVA.ToString();
        }

        String NuevoConcepto(DetalleFactura eldetalle, bool facturaEnBlanco = false)
        {
            try
            {
                StringBuilder losConceptos = new StringBuilder();

                losConceptos.Append(" <cfdi:Concepto Cantidad=\"");
                losConceptos.Append(eldetalle.Cantidad);
                losConceptos.Append("\" Unidad=\"");
                losConceptos.Append(eldetalle.Unidad);
                //losConceptos.Append("\" NoIdentificacion=\"");
                ////losConceptos.Append(noIdentificacion); 'ClaveProdServ' 
                //losConceptos.Append(eldetalle.SKU);
                losConceptos.Append("\" ClaveProdServ=\"");
                losConceptos.Append(eldetalle.ClaveProdServ.PadLeft(8, '0'));
                losConceptos.Append("\" ClaveUnidad=\"");
                losConceptos.Append(eldetalle.ClaveUnidad);
                losConceptos.Append("\" Descripcion=\"");
                losConceptos.Append(eldetalle.NombreProducto);
                losConceptos.Append("\" ValorUnitario=\"");
                //  losConceptos.Append(Decimal.Round(eldetalle.PrecioUnitario,2));
                losConceptos.Append((eldetalle.PrecioUnitario));
                losConceptos.Append("\" Importe=\"");
                // losConceptos.Append(Decimal.Round(eldetalle.Total,2));
                losConceptos.Append((eldetalle.Total));
                losConceptos.Append("\">");
                if (!facturaEnBlanco)
                {
                    losConceptos.Append("<cfdi:Impuestos>");
                    losConceptos.Append("<cfdi:Traslados>");
                    losConceptos.Append("<cfdi:Traslado ");
                    losConceptos.Append(" Base = \"");
                    //losConceptos.Append(Decimal.Round(Decimal.Parse(eldetalle.impBase),2));
                    losConceptos.Append((eldetalle.impBase));
                    losConceptos.Append("\"");
                    losConceptos.Append(" Impuesto= \"");
                    losConceptos.Append(eldetalle.impImpuesto);
                    losConceptos.Append("\"");
                    losConceptos.Append(" TipoFactor= \"");
                    losConceptos.Append(UppercaseFirst(eldetalle.impTipoFactor));
                    losConceptos.Append("\"");
                    losConceptos.Append(" TasaOCuota= \"");
                    losConceptos.Append(eldetalle.impTasaOCuota);
                    losConceptos.Append("\"");
                    losConceptos.Append(" Importe=\"");
                    //losConceptos.Append(Decimal.Round(Decimal.Parse(eldetalle.impImporte),2));
                    losConceptos.Append(eldetalle.impImporte);
                    losConceptos.Append("\"");
                    losConceptos.Append("/>");
                    losConceptos.Append("</cfdi:Traslados >");
                    losConceptos.Append("</cfdi:Impuestos >");

                }
                losConceptos.Append("</cfdi:Concepto>");
                return losConceptos.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error("FACTURACION" + err.Message + " " + err.StackTrace);
                return "";
            }
        }

        String NuevoConceptoSinImporte(DetalleFactura eldetalle)
        {
            try
            {
                StringBuilder losConceptos = new StringBuilder();

                losConceptos.Append(" <cfdi:Concepto Cantidad=\"");
                losConceptos.Append(eldetalle.Cantidad);
                losConceptos.Append("\" Unidad=\"");
                losConceptos.Append(eldetalle.Unidad);
                //losConceptos.Append("\" NoIdentificacion=\"");
                ////losConceptos.Append(noIdentificacion); 'ClaveProdServ' 
                //losConceptos.Append(eldetalle.SKU);
                losConceptos.Append("\" ClaveProdServ=\"");
                losConceptos.Append(eldetalle.ClaveProdServ.PadLeft(8, '0'));
                losConceptos.Append("\" ClaveUnidad=\"");
                losConceptos.Append(eldetalle.ClaveUnidad);
                losConceptos.Append("\" Descripcion=\"");
                losConceptos.Append(eldetalle.NombreProducto);
                losConceptos.Append("\" ValorUnitario=\"");
                //  losConceptos.Append(Decimal.Round(eldetalle.PrecioUnitario,2));
                losConceptos.Append((eldetalle.PrecioUnitario));
                losConceptos.Append("\" Importe=\"");
                // losConceptos.Append(Decimal.Round(eldetalle.Total,2));
                losConceptos.Append((eldetalle.Total));
                losConceptos.Append("\">");
                losConceptos.Append("<cfdi:Impuestos>");
                losConceptos.Append("<cfdi:Traslados>");
                losConceptos.Append("<cfdi:Traslado ");
                losConceptos.Append(" Base = \"");
                //losConceptos.Append(Decimal.Round(Decimal.Parse(eldetalle.impBase),2));
                losConceptos.Append((eldetalle.impBase));
                losConceptos.Append("\"");
                losConceptos.Append(" Impuesto= \"");
                losConceptos.Append(eldetalle.impImpuesto);
                losConceptos.Append("\"");
                losConceptos.Append(" TipoFactor= \"");
                losConceptos.Append(UppercaseFirst(eldetalle.impTipoFactor));
                losConceptos.Append("\"");
                //losConceptos.Append(" TasaOCuota= \"");
                //losConceptos.Append(eldetalle.impTasaOCuota);
                //losConceptos.Append("0000\"");
                //losConceptos.Append(" Importe=\"");
                ////losConceptos.Append(Decimal.Round(Decimal.Parse(eldetalle.impImporte),2));
                //losConceptos.Append(eldetalle.impImporte);
                //losConceptos.Append("\"");
                losConceptos.Append("/>");
                losConceptos.Append("</cfdi:Traslados >");
                losConceptos.Append("</cfdi:Impuestos >");
                losConceptos.Append("</cfdi:Concepto>");

                return losConceptos.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error("FACTURACION" + err.Message + " " + err.StackTrace);
                return "";
            }
        }

        static string UppercaseFirst(string s)
        {

            s = s.ToLower();
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }


        public override string ToString()
        {

            elXML.Replace("&", "&amp;");
            return elXML.ToString();

        }
    }
}
