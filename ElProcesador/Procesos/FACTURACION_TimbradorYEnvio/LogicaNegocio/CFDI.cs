using CommonProcesador;
using DALAutorizador.Entidades;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FACTURACION_Timbrador.LogicaNegocio
{



    public class CFDI
    {
        StringBuilder elXML = new StringBuilder();

        Factura laFacturaLocal;

        /// <summary>
        /// Instancia definición del objeto CFDI
        /// </summary>
        /// <param name="path">Ruta dentro del servidor con la plantilla XML de la factura</param>
        /// <param name="laFactura">Objeto Factura al que se e genera el CFDI</param>
        public CFDI(String path, Factura laFactura)
        {
            laFacturaLocal = laFactura;

            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        String line = sr.ReadLine();

                        if (line.Contains("<cfdi:Comprobante"))
                        {
                            line = EstableceComprobante(line, laFactura);
                        }

                        if (line.Contains("<cfdi:Emisor"))
                        {
                            line = EstableceEmisor(line, laFactura);
                        }

                        if (line.Contains("<cfdi:Receptor"))
                        {
                            line = EstableceReceptor(line, laFactura);
                        }

                        if (line.Contains("<cfdi:Conceptos></cfdi:Conceptos>"))
                        {
                            line = ReemplazaParam(line, "<cfdi:Conceptos></cfdi:Conceptos>", "<cfdi:Conceptos>");

                            foreach (DetalleFactura detalle in laFactura.losDetalles)
                            {
                                line += NuevoConcepto(detalle);
                            }

                            line += "</cfdi:Conceptos>";
                        }
                       
                        if (line.Contains("<cfdi:Impuestos"))
                        {
                            line = ReemplazaParam(line, "TotalImpuestosTrasladados=\"\"", 
                                "TotalImpuestosTrasladados=\"" + Decimal.Round(Decimal.Parse(laFactura.IVA.ToString()), 2) + "\"");
                        }
                        if (line.Contains("<cfdi:Traslado"))
                        {
                            line = ReemplazaParam(line, "Importe=\"\"", "Importe=\"" + Decimal.Round(Decimal.Parse(laFactura.IVA.ToString()), 2) + "\"");
                        }

                        elXML.Append(line);
                    }
                }

                elXML = CollapseSpaces(elXML);
                elXML = CollapseEquals(elXML);
                elXML = elXML.Replace("> <", "><");
                elXML = elXML.Replace(" />", "/>");
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
            }
        }

        /// <summary>
        /// Elimina caracteres en blanco, espacios, nulos y tabuladores en un StringBuilder
        /// </summary>
        /// <param name="sb">StringBuilder por depurar</param>
        /// <returns>StringBuilder con los caracteres eliminados</returns>
        static StringBuilder CollapseSpaces(StringBuilder sb)
        {
            StringBuilder result = new StringBuilder();

            return result.Append(Regex.Replace(sb.ToString(), @"\s+", " "));
        }

        /// <summary>
        /// Elimina caracteres en blanco, espacios, nulos y tabuladores que estén antes
        /// o después de un caracter '=' dentro de un StringBuilder
        /// </summary>
        /// <param name="_sb">StringBuilder por depurar</param>
        /// <returns>StringBuilder con los caracteres eliminados</returns>
        static StringBuilder CollapseEquals(StringBuilder _sb)
        {
            StringBuilder _result = new StringBuilder();

            return _result.Append(Regex.Replace(_sb.ToString(), @"\s*=\s*", "="));
        }

        /// <summary>
        /// Reemplaza dentro de una cadena el parámetro sin valor por el parámetro con valor
        /// </summary>
        /// <param name="cadenaBase">Cadena de entrada (TAG del XML)</param>
        /// <param name="param">Parámetro sin valor</param>
        /// <param name="valor">Parámetro con valor</param>
        /// <returns>Cadena modificada</returns>
        protected String ReemplazaParam(String cadenaBase, String param, String valor)
        {
            return cadenaBase.Replace(param, valor);
        }

        /// <summary>
        /// Sustituye el tag <Comprobante> del XML con sus correspondientes parámetros y valores
        /// </summary>
        /// <param name="entrada">Cadena de entrada (tag con los parámetros sin valor)</param>
        /// <param name="_factura">Factura de la que se toman los valores</param>
        /// <returns>Cadena modificada (tag <Comprobante>)</returns>
        protected String EstableceComprobante(String entrada, Factura _factura)
        {
            try
            {
                String comprobante = entrada;

                comprobante = ReemplazaParam(comprobante, "Sello=\"\"", "Sello=\"[Sello]\"");
                comprobante = ReemplazaParam(comprobante, "Serie=\"\"", "Serie=\"" + _factura.Serie + "\"");
                comprobante = ReemplazaParam(comprobante, "Folio=\"\"", "Folio=\"" + _factura.Folio + "\"");
                comprobante = ReemplazaParam(comprobante, "Fecha=\"\"", "Fecha=\"" + _factura.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss") + "\"");
                comprobante = ReemplazaParam(comprobante, "FormaPago=\"\"", "FormaPago=\"" + _factura.getParametro("@FormaPago") + "\"");
                comprobante = ReemplazaParam(comprobante, "NoCertificado=\"\"", "NoCertificado=\"[NoCertificado]\"");
                comprobante = ReemplazaParam(comprobante, "Certificado=\"\"", "Certificado=\"[Certificado]\"");
                comprobante = ReemplazaParam(comprobante, "SubTotal=\"\"", "SubTotal=\"" + Decimal.Round(_factura.SubTotal, 2) + "\"");
                comprobante = ReemplazaParam(comprobante, "Total=\"\"", "Total=\"" + (Decimal.Round(_factura.SubTotal, 2) + Decimal.Round(_factura.IVA, 2)) + "\"");
                comprobante = ReemplazaParam(comprobante, "MetodoPago=\"\"", "MetodoPago=\"" + _factura.getParametro("@MetodoPago") + "\"");
                comprobante = ReemplazaParam(comprobante, "TipoDeComprobante=\"\"", "TipoDeComprobante=\"" + _factura.getParametro("@TipoComprobante") + "\"");
                comprobante = ReemplazaParam(comprobante, "LugarExpedicion=\"\"", "LugarExpedicion=\"" + _factura.getParametro("@LugarExpedicion") + "\"");

                return comprobante;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                return "";
            }   
        }

        /// <summary>
        /// Sustituye el tag <Emisor> del XML con sus correspondientes parámetros y valores
        /// </summary>
        /// <param name="entrada">Cadena de entrada (tag con los parámetros sin valor)</param>
        /// <param name="_LaFactura">Factura de la que se toman los valores</param>
        /// <returns>Cadena modificada (tag <Emisor>)</returns>
        protected String EstableceEmisor(String entrada, Factura _LaFactura)
        {
            try
            {
                string emisor = entrada;

                emisor = ReemplazaParam(emisor, "Rfc=\"\"", "Rfc=\"" + _LaFactura.Emisora.RFC + "\"");
                emisor = ReemplazaParam(emisor, "Nombre=\"\"", "Nombre=\"" + _LaFactura.Emisora.NombreORazonSocial + " " +
                    _LaFactura.Emisora.APaterno + " " + _LaFactura.Emisora.AMaterno + "\"");
                emisor = ReemplazaParam(emisor, "RegimenFiscal=\"\"", "RegimenFiscal=\"" + laFacturaLocal.getParametro("@RegimenFiscal") + "\"");

                return emisor;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                return "";
            }
        }

        /// <summary>
        /// Sustituye el tag <Receptor> del XML con sus correspondientes parámetros y valores
        /// </summary>
        /// <param name="entrada">Cadena de entrada (tag con los parámetros sin valor)</param>
        /// <param name="_unaFactura">Factura de la que se toman los valores</param>
        /// <returns>Cadena modificada (tag <Receptor>)</returns>
        protected String EstableceReceptor(String entrada, Factura _unaFactura)
        {
            try
            {
                string receptor = entrada;

                receptor = ReemplazaParam(receptor, "Rfc=\"\"", "Rfc=\"" + _unaFactura.Receptora.RFC + "\"");
                receptor = ReemplazaParam(receptor, "Nombre=\"\"", "Nombre=\"" + _unaFactura.Receptora.NombreORazonSocial +
                    " " + _unaFactura.Receptora.APaterno + " " + _unaFactura.Receptora.AMaterno + "\"");
                receptor = ReemplazaParam(receptor, "UsoCFDI=\"\"", "UsoCFDI=\"" + laFacturaLocal.getParametro("@UsoCFDI") + "\"");

                return receptor;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                return "";
            }
        }

        public String NuevoConcepto(DetalleFactura eldetalle)
        {
            try
            {
                StringBuilder losConceptos = new StringBuilder();

                losConceptos.Append(" <cfdi:Concepto Cantidad=\"");
                losConceptos.Append(eldetalle.Cantidad);
                losConceptos.Append("\" Unidad=\"");
                losConceptos.Append(eldetalle.Unidad);
                losConceptos.Append("\" NoIdentificacion=\"");
                losConceptos.Append(eldetalle.SKU);
                losConceptos.Append("\" ClaveProdServ=\"");
                losConceptos.Append(eldetalle.ClaveProdServ.PadLeft(8, '0'));
                losConceptos.Append("\" ClaveUnidad=\"");
                losConceptos.Append(eldetalle.ClaveUnidad);
                losConceptos.Append("\" Descripcion=\"");
                losConceptos.Append(eldetalle.NombreProducto);
                losConceptos.Append("\" ValorUnitario=\"");
                losConceptos.Append((eldetalle.PrecioUnitario));
                losConceptos.Append("\" Importe=\"");
                losConceptos.Append((eldetalle.Total));
                losConceptos.Append("\">");
                losConceptos.Append("<cfdi:Impuestos>");
                losConceptos.Append("<cfdi:Traslados>");
                losConceptos.Append("<cfdi:Traslado ");
                losConceptos.Append(" Base=\"");
                losConceptos.Append((eldetalle.impBase));
                losConceptos.Append("\"");
                losConceptos.Append(" Impuesto=\"");
                losConceptos.Append(eldetalle.impImpuesto);
                losConceptos.Append("\"");
                losConceptos.Append(" TipoFactor=\"");
                losConceptos.Append(UppercaseFirst(eldetalle.impTipoFactor));
                losConceptos.Append("\"");
                losConceptos.Append(" TasaOCuota=\"");
                losConceptos.Append(eldetalle.impTasaOCuota);
                losConceptos.Append("0000\"");
                losConceptos.Append(" Importe=\"");
                losConceptos.Append(eldetalle.impImporte);
                losConceptos.Append("\"");
                losConceptos.Append("/>");
                losConceptos.Append("</cfdi:Traslados>");
                losConceptos.Append("</cfdi:Impuestos>");
                losConceptos.Append("</cfdi:Concepto>");

                return losConceptos.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
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
            return elXML.ToString();
        }
    }
}