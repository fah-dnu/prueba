using CommonProcesador;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALAutorizador.Utilidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class CFDIPago
    {
        StringBuilder elXML = new StringBuilder();

        Factura laFacturaPago;
        List<Factura> laFacturaOriginal = new List<Factura>();
        public List<FacturaRelacionada> lasFacturasRelacionadas = new  List<FacturaRelacionada>();
        decimal _sumaImportes = 0;





        public CFDIPago(List<Factura> FacturasAPagar, Factura FacturaPago, decimal SumaImportes)
        {
            try
            {
                _sumaImportes = SumaImportes;

                   laFacturaOriginal = FacturasAPagar;
                laFacturaPago = FacturaPago;
                //elXML= new StringBuilder(elNuevoXML);
                //  elXML.Append(" <?xml version=\"1.0\" encoding=\"UTF-8\"?> ");
                elXML.Append(" <cfdi:Comprobante ");
                elXML.Append(" xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" );
                elXML.Append(" xmlns:pago10 =\"http://www.sat.gob.mx/Pagos\"");
                elXML.Append(" xsi:schemaLocation =\"http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/Pagos http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos10.xsd\" ");
                elXML.Append(" LugarExpedicion=\"" + FacturaPago.getParametro("@LugarExpedicion") + "\"");
                elXML.Append(" Fecha=\"" + FacturaPago.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss") + "\"");
                elXML.Append(" Version=\"3.3\"");
                elXML.Append(" Folio=\"" + /*TODO:FacturaPago.Folio*/ DateTime.Now.ToString("HHmmss") + "\"");
                elXML.Append(" Serie=\"" + FacturaPago.Serie + "\"");
                elXML.Append(" NoCertificado=\""+ "[NoCertificado]" + "\"");
                elXML.Append(" Certificado=\"[Certificado]\"");
                elXML.Append(" TipoDeComprobante=\"P\"");
                elXML.Append(" Moneda=\"XXX\"");
                elXML.Append(" SubTotal=\"0\"");
                elXML.Append(" Total=\"0\"");
                elXML.Append(" Sello=\"[Sello]\">");
                
                elXML.Append(setEmisor(FacturaPago.Emisora));

                elXML.Append(SetReceptor(FacturaPago.Receptora));

                elXML.Append("<cfdi:Conceptos> ");

                foreach (DetalleFactura detalle in FacturaPago.losDetalles)
                {
                    elXML.Append(NuevoConcepto(detalle));
                    break;
                }


                elXML.Append("</cfdi:Conceptos> ");
                //  elXML.Append(SetImpuestoTrasladado(laFactura.getParametro("@IVADescripcion"), laFactura.getParametro("@FactorIVA"), laFactura.IVA.ToString(), laFactura.getParametro("@TipoFactor")));
                elXML.Append(SetComplementoPago(laFacturaOriginal));
                elXML.Append("</cfdi:Comprobante> ");



            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public String setEmisor(Colectiva elEmisor)
        {
            try
            {
                StringBuilder unEmisor = new StringBuilder();

                unEmisor.Append("<cfdi:Emisor ");
                unEmisor.Append("Rfc=\"" + elEmisor.RFC + "\" ");

                unEmisor.Append("Nombre=\"" + elEmisor.NombreORazonSocial + " " + elEmisor.APaterno + " " + elEmisor.AMaterno + "\" ");
                unEmisor.Append("RegimenFiscal=\"" + laFacturaPago.getParametro("@RegimenFiscal") + "\">");
                unEmisor.Append("</cfdi:Emisor> ");

                return unEmisor.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
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
                //unReceptor.Append(" UsoCFDI=\"" + laFacturaPago.getParametro("@UsoCFDI") + "\">");
                unReceptor.Append(" UsoCFDI=\"P01\">");
                unReceptor.Append("</cfdi:Receptor> ");

                return unReceptor.ToString();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return "";
            }

        }

        private String SetComplementoPago(List<Factura> lasFacturasPagar)
        {
            try
            {
                StringBuilder unComplemetoPago = new StringBuilder();


                unComplemetoPago.Append(" <cfdi:Complemento> ");
                unComplemetoPago.Append(" <pago10:Pagos Version=\"1.0\"> ");

                foreach (Factura unaFactura in lasFacturasPagar)
                {
                    if (_sumaImportes == 0)
                    {
                        Loguear.Evento("SE HA TERMINADO EL SALDO DE LA POLIZA DE PAGO", "Pagos");
                        break;
                    }

                    SaldoFactura losSaldos = new SaldoFactura();

                    losSaldos = DAOFactura.ObtieneSaldoFacturaPendientePago(unaFactura.ID_Factura);

                    decimal saldoPorAplicar = 0;

                    if (losSaldos.ImporteAdeudo <= _sumaImportes)
                    {
                        saldoPorAplicar = losSaldos.ImporteAdeudo;
                        _sumaImportes = _sumaImportes - losSaldos.ImporteAdeudo;
                    }
                    else if (losSaldos.ImporteAdeudo > _sumaImportes)
                    {
                        saldoPorAplicar = _sumaImportes;
                        _sumaImportes = 0;
                    }



                    losSaldos.Parcialidad = (losSaldos.Parcialidad + 1);

                    unComplemetoPago.Append(" <pago10:Pago  ");
                    unComplemetoPago.Append(" FechaPago=\"" + laFacturaPago.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss") + "\"  ");
                    unComplemetoPago.Append(" MonedaP=\"MXN\"  ");
                    unComplemetoPago.Append(" FormaDePagoP=\"" + laFacturaPago.getParametro("@FormaPago") + "\"  ");
                    unComplemetoPago.Append(" Monto=\"" + Decimal.Round(saldoPorAplicar, 2) + "\"> ");
                    unComplemetoPago.Append(" <pago10:DoctoRelacionado  ");
                    unComplemetoPago.Append(" IdDocumento=\"" + unaFactura.UUID + "\"  ");
                    unComplemetoPago.Append(" Serie=\"" + unaFactura.Serie + "\"  ");
                    unComplemetoPago.Append(" Folio=\"" + unaFactura.Folio /*DateTime.Now.ToString("HMMss") */+ "\"  ");
                    unComplemetoPago.Append(" NumParcialidad =\"" + (losSaldos.Parcialidad) + "\"  ");
                    unComplemetoPago.Append(" MetodoDePagoDR=\"" + laFacturaPago.MetodoPago + "\"  ");
                    unComplemetoPago.Append(" MonedaDR=\"MXN\"  ");
                    unComplemetoPago.Append(" ImpSaldoAnt=\"" + Decimal.Round((losSaldos.ImporteAdeudo), 2) + "\"  ");
                    unComplemetoPago.Append(" ImpSaldoInsoluto=\"" + Decimal.Round(losSaldos.ImporteAdeudo - saldoPorAplicar, 2) + "\"  ");
                    unComplemetoPago.Append(" ImpPagado=\"" + Decimal.Round(saldoPorAplicar, 2) + "\"/> ");
                    unComplemetoPago.Append(" </pago10:Pago> ");

                    FacturaRelacionada unaFacturaRelacionada = new FacturaRelacionada();

                    unaFacturaRelacionada.Folio = unaFactura.Serie + unaFactura.Folio;
                    unaFacturaRelacionada.ID_Factura = unaFactura.ID_Factura;
                    unaFacturaRelacionada.ID_FacturaPago = laFacturaPago.ID_Factura;
                    unaFacturaRelacionada.Metodo = laFacturaPago.MetodoPago;
                    unaFacturaRelacionada.MontoPagado = Decimal.Round(saldoPorAplicar, 2);
                    unaFacturaRelacionada.SaldoAnterior = Decimal.Round((losSaldos.ImporteAdeudo), 2);
                    unaFacturaRelacionada.SaldoPendiente = Decimal.Round(losSaldos.ImporteAdeudo - saldoPorAplicar, 2);
                    unaFacturaRelacionada.TotalFactura = unaFactura.ImporteTotal;
                    unaFacturaRelacionada.UUID = Guid.Parse(unaFactura.UUID);
                   
                    lasFacturasRelacionadas.Add(unaFacturaRelacionada);



                }

                unComplemetoPago.Append(" </pago10:Pagos> ");
                //unComplemetoPago.Append(" [TIMBRE] ");
                unComplemetoPago.Append(" </cfdi:Complemento> ");



                return unComplemetoPago.ToString();

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return "";
            }

        }


        //private String SetComplementoPago(List<Factura> lasFacturasPagar)
        //{
        //    try
        //    {
        //        StringBuilder unComplemetoPago = new StringBuilder();


        //        unComplemetoPago.Append(" <cfdi:Complemento> ");
        //        unComplemetoPago.Append(" <pago10:Pagos Version=\"1.0\"> ");

        //        foreach (Factura unaFactura in lasFacturasPagar)
        //        {

        //            unComplemetoPago.Append(" <pago10:Pago  ");
        //            unComplemetoPago.Append(" FechaPago=\"" + laFacturaPago.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss") + "\"  ");
        //            unComplemetoPago.Append(" MonedaP=\"MXN\"  ");
        //            unComplemetoPago.Append(" FormaDePagoP=\"" + laFacturaPago.getParametro("@FormaPago") + "\"  ");
        //            unComplemetoPago.Append(" Monto=\"" + Decimal.Round(laFacturaPago.ImporteTotal, 2) + "\"> ");
        //            unComplemetoPago.Append(" <pago10:DoctoRelacionado  ");
        //            unComplemetoPago.Append(" IdDocumento=\"" +/* unaFactura.UUID*/ Guid.NewGuid().ToString() + "\"  ");
        //            unComplemetoPago.Append(" Serie=\"" + unaFactura.Serie + "\"  ");
        //            unComplemetoPago.Append(" Folio=\"" + unaFactura.Folio + "\"  ");
        //            unComplemetoPago.Append(" NumParcialidad =\"" + "1" + "\"  ");
        //            unComplemetoPago.Append(" MetodoDePagoDR=\"" + unaFactura.getParametro("@MetodoPago") + "\"  ");
        //            unComplemetoPago.Append(" MonedaDR=\"MXN\"  ");
        //            unComplemetoPago.Append(" ImpSaldoAnt=\"" + Decimal.Round((unaFactura.ImporteTotal - unaFactura.ImportePagado), 2) + "\"  ");
        //            unComplemetoPago.Append(" ImpSaldoInsoluto=\"" + Decimal.Round((unaFactura.ImporteTotal - (unaFactura.ImportePagado + laSumaDeImportes)), 2) + "\"  ");
        //            unComplemetoPago.Append(" ImpPagado=\"" + Decimal.Round(unaFactura.ImportePagado + laSumaDeImportes, 2) + "\"/> ");
        //            unComplemetoPago.Append(" </pago10:Pago> ");
        //        }

        //        unComplemetoPago.Append(" </pago10:Pagos> ");
        //        //unComplemetoPago.Append(" [TIMBRE] ");
        //        unComplemetoPago.Append(" </cfdi:Complemento> ");



        //        return unComplemetoPago.ToString();
        //    }
        //    catch (Exception err)
        //    {
        //        Loguear.Error(err, "");
        //        return "";
        //    }

        //}



        public String NuevoConcepto(DetalleFactura eldetalle)
        {
            try
            {

                StringBuilder losConceptos = new StringBuilder();

                losConceptos.Append(" <cfdi:Concepto Cantidad=\"");
                losConceptos.Append("1");
               
              
                losConceptos.Append("\" ClaveProdServ=\"");
                losConceptos.Append("84111506");
                losConceptos.Append("\" ClaveUnidad=\"");
                losConceptos.Append("ACT");
                losConceptos.Append("\" Descripcion=\"");
                losConceptos.Append("Pago");
                losConceptos.Append("\" ValorUnitario=\"");
                //losConceptos.Append((eldetalle.PrecioUnitario));
                losConceptos.Append("0");
                losConceptos.Append("\" Importe=\"");
                losConceptos.Append("0");
                losConceptos.Append("\">");
               
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
