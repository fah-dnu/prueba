using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALAutorizador.LogicaNegocio;
using DALCentralAplicaciones.Utilidades;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace FACTURACION_Sender.LogicaNegocio
{
    public class LNSender
    {

        public static Boolean EnviaSugerencias()
        {
            try
            {
                //Obtiene las Facturas
                List<Factura> lasFacturas = DAOFactura.ObtieneFacturasSugerenciasProcNoc();
                //Las timbra

                foreach (Factura unaFac in lasFacturas)
                {
                    // Factura LaFactura = DAOFactura.ObtieneFactura(ID_Factura, this.Usuario, Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()));

                    //Enviar
                    String laFacturaPDF = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".pdf";
                    String LaFacturaXML = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".xml";
                    String elHtml = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "Mailhtml").Valor + "MailParaEnvioFacturasSugeridas.html";

                    StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(elHtml));


                    if (unaFac.Emisora.Email.Trim().Length != 0)
                    {

                        //Genera los Archivos
                        GenerarArchivos(unaFac);

                        //Atache los archivos
                        Attachment uno, dos;
                        uno = new Attachment(laFacturaPDF);
                        try
                        {
                            if (unaFac.laFacturaTipo.GeneraXML)
                            {
                                dos = new Attachment(LaFacturaXML);
                            }
                            else
                            {
                                dos = null;
                            }
                        }
                        catch (Exception err)
                        {
                            Loguear.Error(err, "Procesador Nocturno");
                            dos = null;
                        }


                        if (unaFac.laFacturaTipo.GeneraXML)
                        {
                            Emailing.Send(unaFac.Emisora.Email, ElCuerpodelMail.ToString(), "Sugerencia de Factura Emitida para " + unaFac.Receptora.NombreORazonSocial, uno, dos);

                            if (uno != null)
                                uno.Dispose();

                            if (dos != null)
                                dos.Dispose();

                            uno = null;
                            dos = null;

                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 7);

                            Loguear.EntradaSalida("Se envio la Sugerencia de Factura [" + unaFac.ID_Factura + "] a los Correos: " + unaFac.Emisora.Email, "Procesador Nocturno", false);
                        }
                        else
                        {
                            Emailing.Send(unaFac.Emisora.Email, ElCuerpodelMail.ToString(), "Sugerencia de Reporte Emitida para " + unaFac.Receptora.NombreORazonSocial, uno, dos);

                            if (uno != null)
                                uno.Dispose();

                            if (dos != null)
                                dos.Dispose();

                            uno = null;
                            dos = null;

                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 7);

                            Loguear.EntradaSalida("Se envio la Sugerencia de Reporte [" + unaFac.ID_Factura + "] a los Correos: " + unaFac.Emisora.Email, "Procesador Nocturno", false);
                        }

                    }
                    else
                    {
                        Loguear.Evento("El Emisor: " + unaFac.Emisora.NombreORazonSocial + ", No tiene Email registrado, no se envio la Factura [" + unaFac.ID_Factura + "]", "Procesador Nocturno");
                    }

                }

                return true;
            }
            catch (Exception err)
            {
                Loguear.Error(err, "Procesador Nocturno");
            }

            return false;
        }

        public static Boolean EnviaFacturasTimbradasXML()
        {
            try
            {
                //Obtiene las Facturas
                List<Factura> lasFacturas = DAOFactura.ObtieneFacturasTimbradasProcNoc();
                //Las timbra

                foreach (Factura unaFac in lasFacturas)
                {
                    // Generar
                    // GenerarArchivos(unaFac);

                    //Enviar
                    //String laFacturaPDF = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".pdf";
                    //String LaFacturaXML = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".xml";

                    String laFacturaPDF;

                    if (unaFac.TipoComprobante.ToUpper().Equals("INGRESO") || unaFac.TipoComprobante.ToUpper().Equals("I"))
                    {

                        laFacturaPDF = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".pdf";
                    } else  if (unaFac.TipoComprobante.ToUpper().Equals("PAGO") || unaFac.TipoComprobante.ToUpper().Equals("P"))
                    {

                        laFacturaPDF = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Pago_" + unaFac.Serie + unaFac.Folio + ".pdf";
                    }
                    else
                    {
                        laFacturaPDF = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Reporte_" + unaFac.Serie + unaFac.Folio + ".pdf";
                    }

                    String LaFacturaXML = "";
                    if (unaFac.TipoComprobante.ToUpper().Equals("INGRESO") || unaFac.TipoComprobante.ToUpper().Equals("I"))
                    {
                        // laFacturaPDF = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + LaFacturaToSend.Serie + LaFacturaToSend.Folio + ".pdf";
                        LaFacturaXML = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Factura_" + unaFac.Serie + unaFac.Folio + ".xml";
                    } else if (unaFac.TipoComprobante.ToUpper().Equals("PAGO") || unaFac.TipoComprobante.ToUpper().Equals("P"))
                    {

                        LaFacturaXML = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Pago_" + unaFac.Serie + unaFac.Folio + ".xml";
                    }
                    else
                    {
                        // laFacturaPDF = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Informe_" + LaFacturaToSend.Serie + LaFacturaToSend.Folio + ".pdf";
                        LaFacturaXML = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "Reporte_" + unaFac.Serie + unaFac.Folio + ".xml";
                    }



                    String elHtml = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "Mailhtml").Valor + "MailParaEnvioFacturas.html";

                    StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(elHtml));


                    if (unaFac.Receptora.Email.Trim().Length != 0)
                    {



                        //Genera los Archivos
                        GenerarArchivos(unaFac);

                        //Atache los archivos
                        Attachment uno, dos;
                        uno = new Attachment(laFacturaPDF);
                        try
                        {

                            dos = new Attachment(LaFacturaXML);
                        }
                        catch (Exception err)
                        {
                            Loguear.Error(err, "Procesador Nocturno");
                            dos = null;
                        }

                        String losEmail = unaFac.Receptora.Email + ";" + unaFac.Emisora.Email;

                        //Emailing.Send(losEmail, ElCuerpodelMail.ToString(), "Factura Emitida por " + unaFac.Emisora.NombreORazonSocial, uno, dos);

                        //LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 6);
                        //if (uno != null)
                        //    uno.Dispose();

                        //if (dos != null)
                        //    dos.Dispose();

                        //uno = null;
                        //dos = null;

                        //Loguear.EntradaSalida("Se envio la Factura [" + unaFac.Folio + "] a los Correos: " + unaFac.Receptora.Email, "Procesador Nocturno", false);

                        if (unaFac.TipoComprobante.ToUpper().Equals("INGRESO") || unaFac.TipoComprobante.ToUpper().Equals("I"))
                        {
                            
                            Emailing.Send(losEmail, ElCuerpodelMail.ToString(), "Factura Emitida por " + unaFac.Emisora.NombreORazonSocial, uno, dos);

                            if (uno != null)
                                uno.Dispose();

                            if (dos != null)
                                dos.Dispose();

                            uno = null;
                            dos = null;

                            // X.Js.AddScript("#{frmSendMail}.setVisible(false);");

                            Loguear.EntradaSalida("Se envio la Factura [" + unaFac.Folio + "] a los Correos: " + losEmail, "Procesador Noctunro", false);

                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 6);
                            // X.Msg.Notify("Envio de Email", "La Factura se ha enviado con <br />  <br /> <b> E X I T O </b> <br />  <br /> ").Show();
                        } else if (unaFac.TipoComprobante.ToUpper().Equals("PAGO") || unaFac.TipoComprobante.ToUpper().Equals("P"))
                        {

                            Emailing.Send(losEmail, ElCuerpodelMail.ToString(), "Pago Emitido por " + unaFac.Emisora.NombreORazonSocial, uno, dos);

                            if (uno != null)
                                uno.Dispose();

                            if (dos != null)
                                dos.Dispose();

                            uno = null;
                            dos = null;

                            // X.Js.AddScript("#{frmSendMail}.setVisible(false);");

                            Loguear.EntradaSalida("Se envio la PAGO [" + unaFac.Folio + "] a los Correos: " + losEmail, "Procesador Noctunro", false);

                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 6);
                            // X.Msg.Notify("Envio de Email", "La Factura se ha enviado con <br />  <br /> <b> E X I T O </b> <br />  <br /> ").Show();
                        }
                        else
                        {
                            Emailing.Send(losEmail, ElCuerpodelMail.ToString(), "Informe Emitido por " + unaFac.Emisora.NombreORazonSocial, uno, dos);

                            if (uno != null)
                                uno.Dispose();

                            if (dos != null)
                                dos.Dispose();

                            uno = null;
                            dos = null;

                            // X.Js.AddScript("#{frmSendMail}.setVisible(false);");

                            Loguear.EntradaSalida("Se envio el Informe [" + unaFac.Folio + "] a los Correos: " + losEmail, "Procesador Nocturno", false);

                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 8);
                            //  X.Msg.Notify("Envio de Email", "El Informe se ha enviado con <br />  <br /> <b> E X I T O </b> <br />  <br /> ").Show();

                        }
                    }
                    else
                    {
                        Loguear.Evento("El Receptor: " + unaFac.Receptora.NombreORazonSocial + ", No tiene Email registrado, no se envio la Factura [" + unaFac.Folio + "]", "Procesador Nocturno");
                    }
                }

                return true;
            }
            catch (Exception err)
            {
                Loguear.Error(err, "Procesador Nocturno");
            }

            return false;
        }


        protected static void GenerarArchivos(Factura lafactura)
        {
            try
            {
                DataTable Dt = new DataTable();//

                CrystalDecisions.CrystalReports.Engine.ReportDocument report = new CrystalDecisions.CrystalReports.Engine.ReportDocument();

                String Path2 = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLosRPT").Valor;
                String unPath = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;

                //genera el QR
                String elCodigoQR = "?re=" + lafactura.Emisora.RFC + "&rr=" + lafactura.Receptora.RFC + "&tt=" + lafactura.ImporteTotal.ToString("N6").Replace(",", "") + "&id=" + lafactura.UUID;
                String NombreQR = unPath + "QR_" + lafactura.Serie + lafactura.Folio + ".png";
                var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
                var qrCode = qrEncoder.Encode(elCodigoQR);


                var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
                using (var stream = new FileStream(NombreQR, FileMode.Create))
                    renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);

                //Genera el XML
                try
                {
                    if ((lafactura.TipoComprobante.ToUpper().Equals("INGRESO")) || lafactura.TipoComprobante.ToUpper().Equals("I"))
                    {
                        String unPath2 = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;
                        XmlDocument xDoc = new XmlDocument();
                        Loguear.Evento(" intentar arbir el Reporte: " + Path2 + lafactura.UrlReporte, "Procesador");
                        xDoc.LoadXml(lafactura.XMLCFDI);
                        xDoc.Save(unPath2 + "Factura_" + lafactura.Serie + lafactura.Folio + ".xml");
                    }
                }
                catch (Exception err)
                {
                }


                //genera el PDF

                if (lafactura.TipoComprobante.ToUpper().Equals("INGRESO") || lafactura.TipoComprobante.ToUpper().Equals("I"))
                {
                    Loguear.Evento(" intentar arbir el Reporte: " + Path2 + lafactura.UrlReporte, "Procesador");
                    report.Load(Path2 + lafactura.UrlReporte);
                    Dt = DAOFactura.ObtieneDataSetFactura(lafactura.ID_Factura).Tables[0];
                    report.SetDataSource(Dt);
                    String NombreArchivo = "Factura_" + lafactura.Serie + lafactura.Folio + ".pdf";
                    report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, unPath + NombreArchivo);

                    report.Close();
                }
                else if (lafactura.TipoComprobante.ToUpper().Equals("PAGO") || lafactura.TipoComprobante.ToUpper().Equals("P"))
                {
                    Loguear.Evento(" intentar arbir el Reporte: " + Path2 + lafactura.UrlReporte, "Procesador");
                    report.Load(Path2 + lafactura.UrlReporte);
                    Dt = DAOFactura.ObtieneDataSetFactura(lafactura.ID_Factura).Tables[0];
                    report.SetDataSource(Dt);
                    String NombreArchivo = "Pago_" + lafactura.Serie + lafactura.Folio + ".pdf";
                    report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, unPath + NombreArchivo);

                    report.Close();
                }
                else
                {
                    Loguear.Evento(" intentar arbir el Reporte: " + Path2 + lafactura.UrlReporte, "Procesador");
                    report.Load(Path2 + lafactura.UrlReporte);
                    Dt = DAOFactura.ObtieneDataSetFactura(lafactura.ID_Factura).Tables[0];
                    report.SetDataSource(Dt);
                    String NombreArchivo = "Reporte_" + lafactura.Serie + lafactura.Folio + ".pdf";
                    report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, unPath + NombreArchivo);

                    report.Close();
                }

            }
            catch (Exception err)
            {
                Loguear.Error(err, "");
                throw err;
            }
            finally
            {

            }
        }



        //protected static void GenerarArchivos(Factura lafactura)
        //{
        //    try
        //    {
        //        DataTable Dt = new DataTable();//

        //        CrystalDecisions.CrystalReports.Engine.ReportDocument report = new CrystalDecisions.CrystalReports.Engine.ReportDocument();

        //        String Path2 = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLosRPT").Valor;
        //        String unPath = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;

        //        //genera el QR
        //        String elCodigoQR = "?re=" + lafactura.Emisora.RFC + "&rr=" + lafactura.Receptora.RFC + "&tt=" + lafactura.ImporteTotal.ToString("N6").Replace(",", "") + "&id=" + lafactura.UUID;
        //        String NombreQR = unPath + "QR_" + lafactura.Serie + lafactura.Folio + ".png";
        //        var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
        //        var qrCode = qrEncoder.Encode(elCodigoQR);


        //        var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
        //        using (var stream = new FileStream(NombreQR, FileMode.Create))
        //            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);

        //       //Genera XML
        //        if (lafactura.XMLCFDI.Trim().Length != 0)
        //        {
        //            String unPath2 = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;
        //            XmlDocument xDoc = new XmlDocument();
        //            xDoc.LoadXml(lafactura.XMLCFDI);
        //            xDoc.Save(unPath2 + "Factura_" + lafactura.Serie + lafactura.Folio + ".xml");
        //        }
        //        //genera el PDF
        //        report.Load(Path2 + "unaFactura.rpt");
        //        Dt = DAOFactura.ObtieneDataSetFactura(lafactura.ID_Factura).Tables[0];
        //        report.SetDataSource(Dt);
        //        String NombreArchivo = "Factura_" + lafactura.Serie + lafactura.Folio + ".pdf";
        //        report.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, unPath + NombreArchivo);

        //        report.Close();

        //    }
        //    catch (Exception err)
        //    {

        //        Loguear.Error(err,"Procesador Nocturno");
        //        throw err;
        //    }
        //    finally
        //    {

        //    }
        //}

    }
}
