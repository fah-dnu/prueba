#define PIEK
#define PIEKMSI
#define ADICIONALES
using CommonProcesador;
using CrystalDecisions.ReportAppServer;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.ReportDefModel;
using CrystalDecisions.Shared;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.ReporteDebito.trafalgar;

using DNU_ParabiliaProcesoCortes.ReporteTipado;
using Executer.Utilidades;
using FACTURACION_Timbrador.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNOperacionesEdoCuenta
    {

        public static string ObtenerSelloFactura(String CadenaOriginal, out String NoCertificado, out String Certificado, string rutaCert, string rutaKey, string pass)
        {
            NoCertificado = "";
            Certificado = "";
            try
            {
                string ArchivoCertificado = rutaCert;// @"D:\CodigoFuente\CentralAdministrativa\CentralAdministrativa\Core\Apps\Facturas\Emisor.cer";
                string key = rutaKey;//@"D:\CodigoFuente\CentralAdministrativa\CentralAdministrativa\Core\Apps\Facturas\Emisor.key";
                string lPassword = pass;// @"12345678a";
                string Digestion = "";
                string strCadenaOriginal = CadenaOriginal;//"*** Cadena Original Utilizando XSLT del SAT";

                // Open Certificado
                X509Certificate2 certificado = new X509Certificate2(ArchivoCertificado);

                // Obtengo Datos
                NoCertificado = FromHex(certificado.SerialNumber);
                strCadenaOriginal = strCadenaOriginal.Replace("[NoCertificado]", NoCertificado);
                strCadenaOriginal = strCadenaOriginal.Replace("    ", " ").Replace("  ", "");
                // SAH1
                //SHA1 oSHA1 = SHA1CryptoServiceProvider.Create();
                SHA256 oSHA1 = SHA256CryptoServiceProvider.Create();
                Byte[] textOriginal = Encoding.UTF8.GetBytes(strCadenaOriginal);
                Byte[] hash = oSHA1.ComputeHash(textOriginal);
                StringBuilder oSB = new StringBuilder();
                foreach (byte i in hash)
                    oSB.AppendFormat("{0:x2}", i);
                Digestion = oSB.ToString();

                // leer KEY
                SecureString lSecStr = new SecureString();
                lSecStr.Clear();
                foreach (char c in lPassword.ToCharArray())
                    lSecStr.AppendChar(c);

                Byte[] pLlavePrivadaenBytes = System.IO.File.ReadAllBytes(key);

                // Uso clSeguridad para Leer Certificado
                RSACryptoServiceProvider lrsa = opensslkey.DecodeEncryptedPrivateKeyInfo(pLlavePrivadaenBytes, lSecStr);
                SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();

                Byte[] bytesFirmados = lrsa.SignData(System.Text.Encoding.UTF8.GetBytes(strCadenaOriginal), hasher);

                // Obtengo Sello
                string sellodigital = Convert.ToBase64String(bytesFirmados);


                Certificado = Convert.ToBase64String(certificado.RawData);

                //// Resultado //
                //Console.WriteLine("-----------------------------------------");
                //Console.WriteLine("Sello=" + sellodigital);
                ////Console.WriteLine("NoCertificado=" + NoCertificado);
                //Console.WriteLine("Certificado=" + Certificado);
                //Console.WriteLine("-----------------------------------------");

                return sellodigital;


            }
            catch (Exception err)
            {
                Loguear.Error(err + " FACTURACION");
                return "";

            }

        }
        public static String FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return System.Text.Encoding.UTF8.GetString(raw);
        }

        public static String ObtieneCadenaOriginal(String elCFDIXML, XslCompiledTransform _transformador)
        {
            //--FileXSLT

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CloseOutput = false;
            Logueo.Evento("[GeneraEstadoCuentaCredito] xml writer");

            // Create the XmlWriter object and write some content.
            MemoryStream strm = new MemoryStream();
            // XmlWriter writer = XmlWriter.Create(strm, settings);
            XmlReader elXml = XmlReader.Create(new StringReader(elCFDIXML));
            Logueo.Evento("[GeneraEstadoCuentaCredito] xml string reader");
            StringBuilder laCadena = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(laCadena, settings);

            _transformador.Transform(elXml, writer);
            return laCadena.ToString().Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("&amp;", "&");

        }

        public static bool edoCuentaPDF(List<string> archivos, string userCR, string passCR, string hostCR, string databaseCR, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar)
        {
            try
            {
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, ds);
                dynamic estadoDeCuenta;
                string tarjeta = "";
                int numeroTablaDatosCH = 1;
                bool visualizaSubReporte = false;
                DataSet dsSubreporte = null;
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, null);
#if !PIEK
                    estadoDeCuenta = new EdoCuentaCreditoSinDatos();
#else
                    estadoDeCuenta = new EdoCuentaTipSinDatos();
#endif
                    tarjeta = ds.Tables[0].Rows[0]["tarjeta"].ToString();
                    numeroTablaDatosCH = 0;
                }
                else
                {
#if !PIEK
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaCreditoSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaCredito();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();

#else
#if PIEKMSI
                    dsSubreporte = _daoCortes.ObtenerDatosSubreporteMSI(ID_Corte.ToString(), null, conn);
                    if (dsSubreporte.Tables[0].Rows.Count > 0)
                    {
                        visualizaSubReporte = true;
                        // estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                    }
#else
                    dsSubreporte=new DataSet();
#endif
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaTipSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaTip();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();
#endif
                }//
                if (!tarjeta.Contains("*"))
                {
                    tarjeta = tarjeta.Substring(0, 6) + "******" + tarjeta.Substring(12, 4);
                }
                ds.Tables[numeroTablaDatosCH].Rows[0]["tarjeta"] = tarjeta;


                estadoDeCuenta.SetDataSource(ds);

#if PIEK

                // estadoDeCuenta.SetParameterValue("visualizaSubReporte", 0);
#if PIEKMSI
                if (numeroTablaDatosCH != 0)
                {
                    estadoDeCuenta.Subreports[0].SetDataSource(dsSubreporte);
                    //  if (numeroTablaDatosCH != 0)
                    // {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", visualizaSubReporte ? 1 : 0);
                    // }
                }
#else
                if (numeroTablaDatosCH != 0)
                {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", 0);
                }
#endif
#endif

                //#if PIEK
                estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                estadoDeCuenta.SetParameterValue("CAT", imagenCAT);
                estadoDeCuenta.SetParameterValue("UNE", imagenUNE);
                //#endif
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF " + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }

        public static bool edoCuentaText(List<string> archivos, string userCR, string passCR, string hostCR, string databaseCR, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar, Cuentas cuenta, int numeroFolio, int numeroCuentas)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                DateTime Hoy = DateTime.Now;
                string fechaConvetida = Hoy.ToString("ddMMyyyy");
                ruta = ruta + "\\EstadosDeCuentaTxt" + fechaConvetida + "\\";
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                nombreArchivo = nombreArchivo.Replace("pdf", "txt");
                nombreArchivo = nombreArchivo.Replace(".txt", " " + cuenta.NombreORazonSocial + " " + cuenta.Subproducto + ".txt");
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCHTxt(ID_Corte.ToString(), null, conn, ds);
                dynamic estadoDeCuenta;
                bool contieneMovimientos = true;
                int numeroMovimientos = 0;
                if (ds.Tables[0].Rows.Count == 0)
                {
                    contieneMovimientos = false;
                    ds = _daoCortes.ObtenerDatosCHTxt(ID_Corte.ToString(), null, conn, null);
                }
                else
                {
                    numeroMovimientos = ds.Tables["DataTablePagos"].Rows.Count;
                }
                //header
                //solo en el primer registro
                if (!File.Exists(ruta + nombreArchivo))
                {
                    sb.AppendLine(String.Format("{0}{1}{2}{3}",
                    "01".PadRight(2),
                    cuenta.Subproducto.PadRight(10),
                    cuenta.Fecha_Corte.ToString("yyyyMMdd").PadRight(8),
                    numeroCuentas.ToString().PadLeft(6, '0')));
                }

                //Datos tarjetahabiente
                sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}",
                   "02".PadRight(2),
                   ds.Tables["DataTableCH"].Rows[0]["nombre"].ToString().PadRight(30),
                   ds.Tables["DataTableCH"].Rows[0]["apellidoPaterno"].ToString().PadRight(30),
                   ds.Tables["DataTableCH"].Rows[0]["apellidoMaterno"].ToString().PadRight(30),
                   ds.Tables["DataTableCH"].Rows[0]["calle"].ToString().PadRight(50),
                   ds.Tables["DataTableCH"].Rows[0]["numExterior"].ToString().PadRight(20),
                   ds.Tables["DataTableCH"].Rows[0]["numIterior"].ToString().PadRight(20),
                   ds.Tables["DataTableCH"].Rows[0]["colonia"].ToString().PadRight(50),
                   ds.Tables["DataTableCH"].Rows[0]["ciudad"].ToString().PadRight(50),
                   ds.Tables["DataTableCH"].Rows[0]["estado"].ToString().PadRight(30),
                   ds.Tables["DataTableCH"].Rows[0]["cp"].ToString().PadRight(5)
                 ));
                //Datos corte
                string tarMask = ds.Tables["DataTableCH"].Rows[0]["tarjeta"].ToString().PadRight(16);

                tarMask = tarMask.Substring(0, 6) + "******"+ tarMask.Substring(11, 4);

                sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}" +
                    "{20}{21}{22}",
                  "03".PadRight(2),
                  cuenta.RFCCliente.ToString().PadRight(13),
                  ds.Tables["DataTableCH"].Rows[0]["Proxima_FechaDePago"].ToString().PadRight(8),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["Pago_Minimo"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["Pago_ParaNoIntereses"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["saldoAnterior"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["TotalComprasDisposiciones"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["ComisionPagoTardioFaltaDePago"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["interesesCobrados"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["iva"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["TotalPagosReembolsos"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["Saldo_Insoluto"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["Saldo_Insoluto"].ToString(), 10),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["tasaOrdAnual"].ToString(), 4, false),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["tasaOrdMensual"].ToString(), 4, false),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["tasaMorAnual"].ToString(), 4, false),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["tasaMorMensual"].ToString(), 4, false),
                  tarMask,
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["LimiteDeCredito"].ToString(), 9, false),
                  operacionCadenasOperadores(ds.Tables["DataTableCH"].Rows[0]["creditoDisponible"].ToString(), 10),
                  ds.Tables["DataTableCH"].Rows[0]["PagoMinimoMeses"].ToString().PadRight(3),
                  ds.Tables["DataTableCH"].Rows[0]["fechaInicialCorte"].ToString().PadRight(8),
                  ds.Tables["DataTableCH"].Rows[0]["fechaInicialCorte"].ToString().PadRight(8)
                ));
                //Datos Operaciones
                if (contieneMovimientos)
                {//
                    foreach (DataRow row in ds.Tables["DataTablePagos"].Rows)
                    {
                        sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                           "04".PadRight(2),
                           row["Fecha"].ToString().Replace("-", "").PadRight(8),
                            row["Concepto"].ToString().PadRight(50),
                           (" ").PadRight(13),//RFC Comercio
                           (" ").PadRight(50),//Población
                           row["OtrasMonedas"].ToString().PadRight(3),
                          operacionCadenasOperadores(row["OtrasDivisas"].ToString(), 10),
                          operacionCadenasOperadores(row["Cargo"].ToString(), 10)
                     ));
                    }
                }
                //Pie informe
                if (contieneMovimientos)
                {
                    sb.AppendLine(String.Format("{0}{1}",
                       "05".PadRight(2),
                       numeroMovimientos.ToString().PadLeft(3, '0')
                ));
                }
                else
                {
                    sb.AppendLine(String.Format("{0}{1}",
                      "05".PadRight(2),
                      "0".PadLeft(3, '0')));
                }

                File.AppendAllText(ruta + nombreArchivo, sb.ToString());

                //if (File.Exists(ruta + nombreArchivo))
                //{
                //    File.AppendAllText(ruta + nombreArchivo, sb.ToString());
                //    // File.AppendAllLines(ruta + nombreArchivo, new String[] { sb.ToString() });
                //}
                //else
                //{
                //    using (StreamWriter sw = File.CreateText(ruta + nombreArchivo))
                //    {
                //        sw.WriteLine(sb.ToString());
                //    }
                //}
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar txt " + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }
        private static string operacionCadenasOperadores(string cadena, int longitud, bool espacio = true)
        {
            string cadenaNueva = "";
            if (cadena.Contains('-'))
            {
                cadenaNueva = "-" + cadena.Replace(",", "").Replace(".", "").Replace("$", "").Replace(" ", "").Replace("-", "").PadLeft(longitud - 1, '0');
            }
            else if (espacio)
            {
                cadenaNueva = " " + cadena.Replace(",", "").Replace(".", "").Replace("$", "").Replace(" ", "").Replace("-", "").PadLeft(longitud - 1, '0');

            }
            else
            {
                cadenaNueva = cadena.Replace(",", "").Replace(".", "").Replace("$", "").Replace(" ", "").Replace("-", "").PadLeft(longitud, '0');

            }
            return cadenaNueva;
        }


        public static bool edoCuentaPDFExterno(string id, SqlConnection conn, string ruta, DAOCortes _daoCortes, string nombreArchivo, Factura laFacturaExt, DetalleArchivoXLSX detalleRegistro = null)
        {
            try
            {
                //ruta = ruta + "Procesados\\";
                bool noTimbrar = true;
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                DataSet ds = _daoCortes.ObtenerDatosCHExterno(id, null, conn);// _daoCortes.ObtenerDatosPagos(id, null, conn);
                ds = _daoCortes.ObtenerDatosPagosDebitoExterno(id, null, conn, ds, detalleRegistro);
                DataTable datosComisiones = _daoCortes.ObtenerDatosComisionesRelevantes(id, null, conn);
                DataTable datosObjetados = _daoCortes.ObtenerDatosCargosObjetados(id, null, conn);


                //dynamic estadoDeCuenta;
                EdoCuentaTrafalgar estadoDeCuenta = new EdoCuentaTrafalgar();

                estadoDeCuenta.Subreports[0].SetDataSource(datosObjetados);
                estadoDeCuenta.Subreports[1].SetDataSource(datosComisiones);

                string tarjeta = "";
                int numeroTablaDatosCH = 1;
                //if (ds.Tables[1].Rows.Count == 0)
                //{
                //    ds = _daoCortes.ObtenerDatosCH(id, null, conn, null);

                //    //tarjeta = ds.Tables[0].Rows[0]["tarjeta"].ToString();
                //    //numeroTablaDatosCH = 0;
                //}



                estadoDeCuenta.SetDataSource(ds);

                // string fecha= laFacturaExt.FechaTimbrado.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
                string fecha = laFacturaExt.FechaTimbrado.ToString("yyyy-MM-dd'T'HH:mm:ss");
                //asignando parametros
                estadoDeCuenta.SetParameterValue("Folio", string.IsNullOrEmpty(laFacturaExt.Folio) ? "" : laFacturaExt.Folio);
                estadoDeCuenta.SetParameterValue("Certificado", string.IsNullOrEmpty(laFacturaExt.NoCertificadoEmisor) ? "" : laFacturaExt.NoCertificadoEmisor);
                estadoDeCuenta.SetParameterValue("selloEmisor", string.IsNullOrEmpty(laFacturaExt.SelloCFD) ? "" : laFacturaExt.SelloCFD);
                estadoDeCuenta.SetParameterValue("selloReceptor", string.IsNullOrEmpty(laFacturaExt.SelloSAT) ? "" : laFacturaExt.SelloSAT);
                estadoDeCuenta.SetParameterValue("NoSerieCerSAT", string.IsNullOrEmpty(laFacturaExt.NoCertificadoSAT) ? "" : laFacturaExt.NoCertificadoSAT);
                estadoDeCuenta.SetParameterValue("FechaHoraCertificacion", string.IsNullOrEmpty(fecha) ? "" : fecha);
                estadoDeCuenta.SetParameterValue("CadenaOriginal", string.IsNullOrEmpty(laFacturaExt.CadenaOriginal) ? "" : laFacturaExt.CadenaOriginal);
                estadoDeCuenta.SetParameterValue("ImagenSAT", string.IsNullOrEmpty(laFacturaExt.UrlQrCode) ? "" : laFacturaExt.UrlQrCode);
                estadoDeCuenta.SetParameterValue("numeroReportes", 0);
                //imprimiendo pdf
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + "\\" + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                // archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF externo" + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }
        public static bool edoCuentaPDFExternoConSubReporte(string id, SqlConnection conn, string ruta, DAOCortes _daoCortes, string nombreArchivo, Factura laFacturaExt, ClienteExterno clienteExterno, DetalleArchivoXLSX detalleRegistro = null)
        {
            try
            {
                // ruta = ruta + "Procesados\\";
                bool noTimbrar = true;
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }

                //dynamic estadoDeCuenta;
                EdoCuentaTrafalgar estadoDeCuenta = new EdoCuentaTrafalgar();
                DataSet ds = new DataSet();
                for (int i = 0; i < clienteExterno.cuentas.Count; i++)
                {
                    CuentaAhorroCLABE cuentaYCLABE = clienteExterno.cuentas[i];
                    if (i == 0)
                    {
                        ds = _daoCortes.ObtenerDatosCHExterno(id, cuentaYCLABE, null, conn);// _daoCortes.ObtenerDatosPagos(id, null, conn);
                        ds = _daoCortes.ObtenerDatosPagosDebitoExterno(id, cuentaYCLABE, null, conn, ds, detalleRegistro);
                        DataTable datosComisiones = _daoCortes.ObtenerDatosComisionesRelevantes(id, null, conn);
                        DataTable datosObjetados = _daoCortes.ObtenerDatosCargosObjetados(id, null, conn);
                        estadoDeCuenta.Subreports[0].SetDataSource(datosObjetados);
                        estadoDeCuenta.Subreports[1].SetDataSource(datosComisiones);
                    }
                    else
                    {
                        String rutaReporte = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                        DataSet dsVariable = new DataSet();
                        dsVariable = _daoCortes.ObtenerDatosCHExterno(id, cuentaYCLABE, null, conn);// _daoCortes.ObtenerDatosPagos(id, null, conn);
                        dsVariable = _daoCortes.ObtenerDatosPagosDebitoExterno(id, cuentaYCLABE, null, conn, dsVariable, detalleRegistro);

                        //// CrystalDecisions.CrystalReports.Engine.ReportClass rep = new CrystalDecisions.CrystalReports.Engine.ReportClass();
                        //ReportClientDocumentWrapper doc = (ReportClientDocumentWrapper)estadoDeCuenta.ReportClientDocument;//;.ReportClientDocument;
                        //CrystalDecisions.ReportAppServer.ReportDefModel.Section sec = doc.ReportDefController.ReportDefinition.ReportFooterArea.Sections[2];
                        //// doc.SubreportController.ImportSubreport("EdoCuentaTrafalgarSubReporteDetalle", rutaReporte + "EdoCuentaTrafalgarSubReporteDetalle.rpt", sec);
                        //// // estadoDeCuenta.OpenSubreport("EdoCuentaTrafalgarSubReporteDetalle").SetDataSource(dsVariable);
                        //// estadoDeCuenta.Subreports["EdoCuentaTrafalgarSubReporteDetalle"].SetDataSource(dsVariable);
                        //doc.SubreportController.ImportSubreport("EdoCuentaTrafalgarSubReporteDetalle", /*rutaReporte*/@"C:\DNUNet\CortesBranch\Branched_Proceso_Cortes\Development-PCI\Cortes\DNU CORTE DE CUENTAS\DNU_ParabiliaProcesoCortes\DNU_ParabiliaProcesoCortes\ReporteDebito\trafalgar\" + "EdoCuentaTrafalgarSubReporteDetalle.rpt", sec);
                        //estadoDeCuenta.Subreports["EdoCuentaTrafalgarSubReporteDetalle"].SetDataSource(dsVariable);


                        CrystalDecisions.ReportAppServer.ReportDefModel.Section sec = new CrystalDecisions.ReportAppServer.ReportDefModel.Section();
                        sec.Kind = CrAreaSectionKindEnum.crAreaSectionKindReportFooter;
                        sec.Name = String.Format("DynamicSection{0}", (i));
                        sec.Width = estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.DetailArea.Sections[0].Width;
                        // sec.Format.BackgroundColor = 16777215;// System.Drawing.Color.White;
                        // sec.Height = 1000;
                        //sec.Format.EnableUnderlaySection = true;
                        //sec.Format.EnableSuppressIfBlank = true;
                        //sec.Format.EnablePrintAtBottomOfPage = true;
                        //sec.Format.EnableUnderlaySection = true;
                        //sec.Format.EnableKeepTogether = true;
                        // sec.ReportObjects.Border.BorderColor =16777215;

                        //import
                        estadoDeCuenta.ReportClientDocument.ReportDefController.ReportSectionController.Add(sec, estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea, (i));
                        estadoDeCuenta.ReportClientDocument.SubreportController.ImportSubreport(sec.Name, rutaReporte/*@"C:\DNUNet\CortesBranch\Branched_Proceso_Cortes\Development-PCI\Cortes\DNU CORTE DE CUENTAS\DNU_ParabiliaProcesoCortes\DNU_ParabiliaProcesoCortes\ReporteDebito\trafalgar\"*/ + "EdoCuentaTrafalgarSubReporteDetalle.rpt", sec);
                        //estadoDeCuenta.ReportClientDocument.SubreportController.ImportSubreport("EdoCuentaTrafalgarSubReporteDetalle", @"C:\DNUNet\CortesBranch\Branched_Proceso_Cortes\Development-PCI\Cortes\DNU CORTE DE CUENTAS\DNU_ParabiliaProcesoCortes\DNU_ParabiliaProcesoCortes\ReporteDebito\trafalgar\" + "EdoCuentaTrafalgarSubReporteDetalle.rpt", sec);

                        //  estadoDeCuenta.ReportClientDocument.SubreportController.ImportSubreportEx("EdoCuentaTrafalgarSubReporteDetalle", /*rutaReporte*/@"C:\DNUNet\CortesBranch\Branched_Proceso_Cortes\Development-PCI\Cortes\DNU CORTE DE CUENTAS\DNU_ParabiliaProcesoCortes\DNU_ParabiliaProcesoCortes\ReporteDebito\trafalgar\" + "EdoCuentaTrafalgarSubReporteDetalle.rpt", sec, 0, 0, sec.Width, 100);


                        //format
                        //   ReportDocument reportDocument = estadoDeCuenta.ReportDefinition.ReportObjects;
                        //  ReportObject reportObject = (ReportObject)estadoDeCuenta.ReportDefinition.ReportObjects["DynamicSection"+i];
                        estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Sections[i].ReportObjects[0].Border.BorderColor = 16777215;
                        //quitando bordes
                        foreach (ISCRReportObject repObj in estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Sections[i].ReportObjects)

                        {

                            // Check to see if it is subreport type

                            if (repObj.Kind == CrReportObjectKindEnum.crReportObjectKindSubreport)

                            {

                                // clone the report object

                                ISCRReportObject newObj = repObj.Clone(true);

                                //modify the line style

                                newObj.Border.BottomLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.TopLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.RightLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.LeftLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                // tell the report to update the report object.

                                estadoDeCuenta.ReportClientDocument.ReportDefController.ReportObjectController.Modify(repObj, newObj);

                            }

                        }



                        //
                        // estadoDeCuenta.ReportClientDocument.ReportDefController.ReportObjectController.Modify(m_boReportObject, m_boReportObjectModified);





                        //  estadoDeCuenta.ReportDefinition.ReportObjects[String.Format("DynamicSection{0}", (i))].Border.BorderColor=Color.White;
                        // estadoDeCuenta.Subreports[String.Format("DynamicSection{0}", (i))];
                        // estadoDeCuenta.ReportClientDocument.ReportDefController.ReportSectionController.SetProperty(sec, CrystalDecisions.ReportAppServer.Controllers.CrReportSectionPropertyEnum.crReportSectionPropertyFormat, (i));
                        //estadoDeCuenta.ReportDefinition.ReportObjects[0].ObjectFormat.EnableCanGrow = true;
                        // estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Sections[i].Format.EnablePrintAtBottomOfPage = false;
                        //  estadoDeCuenta.ReportClientDocument.ReportDefController.ReportObjectController..

                        //source
                        estadoDeCuenta.Subreports[sec.Name].SetDataSource(dsVariable);
                        //estadoDeCuenta.Subreports["EdoCuentaTrafalgarSubReporteDetalle"].SetDataSource(dsVariable);

                        // Dim obj As SubreportObject = estadoDeCuenta.ReportDefinition.ReportObjects['Test'].ObjectFormat.EnableCanGrow = true;
                        //obj.ObjectFormat.EnableCanGrow = True
                        //estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Format.

                        //margenes
                        //CrystalDecisions.CrystalReports.Engine.PrintOptions boPrintOptions = boReportDocument.PrintOptions;
                        //CrystalDecisions.Shared.PageMargins boPageMargins = boPrintOptions.PageMargins;

                        //int margin = 0;

                        //boPageMargins.bottomMargin = margin;
                        //boPageMargins.leftMargin = margin;
                        //boPageMargins.rightMargin = margin;
                        //boPageMargins.topMargin = margin;

                        //boPrintOptions.ApplyPageMargins(boPageMargins);
                    }

                }
                estadoDeCuenta.SetDataSource(ds);
                // string fecha= laFacturaExt.FechaTimbrado.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
                string fecha = laFacturaExt.FechaTimbrado.ToString("yyyy-MM-dd'T'HH:mm:ss");
                //asignando parametros
                estadoDeCuenta.SetParameterValue("Folio", string.IsNullOrEmpty(laFacturaExt.Folio) ? "" : laFacturaExt.Folio);
                estadoDeCuenta.SetParameterValue("Certificado", string.IsNullOrEmpty(laFacturaExt.NoCertificadoEmisor) ? "" : laFacturaExt.NoCertificadoEmisor);
                estadoDeCuenta.SetParameterValue("selloEmisor", string.IsNullOrEmpty(laFacturaExt.SelloCFD) ? "" : laFacturaExt.SelloCFD);
                estadoDeCuenta.SetParameterValue("selloReceptor", string.IsNullOrEmpty(laFacturaExt.SelloSAT) ? "" : laFacturaExt.SelloSAT);
                estadoDeCuenta.SetParameterValue("NoSerieCerSAT", string.IsNullOrEmpty(laFacturaExt.NoCertificadoSAT) ? "" : laFacturaExt.NoCertificadoSAT);
                estadoDeCuenta.SetParameterValue("FechaHoraCertificacion", string.IsNullOrEmpty(fecha) ? "" : fecha);
                estadoDeCuenta.SetParameterValue("CadenaOriginal", string.IsNullOrEmpty(laFacturaExt.CadenaOriginal) ? "" : laFacturaExt.CadenaOriginal);
                estadoDeCuenta.SetParameterValue("ImagenSAT", string.IsNullOrEmpty(laFacturaExt.UrlQrCode) ? "" : laFacturaExt.UrlQrCode);
                estadoDeCuenta.SetParameterValue("numeroReportes", 2);
                //imprimiendo pdf
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + "\\" + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();

                // archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF externo" + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }

        public static bool edoCuentaPDFCredito(List<string> archivos, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar)
        {
            try
            {
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, ds);
                dynamic estadoDeCuenta;
                string tarjeta = "";
                int numeroTablaDatosCH = 1;
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, null);
#if !PIEK
                    estadoDeCuenta = new EdoCuentaCreditoSinDatos();
#else
                    estadoDeCuenta = new EdoCuentaTipSinDatos();
#endif
                    tarjeta = ds.Tables[0].Rows[0]["tarjeta"].ToString();
                    numeroTablaDatosCH = 0;
                }
                else
                {
#if !PIEK
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaCreditoSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaCredito();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();

#else
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaTipSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaTip();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();
#endif
                }//
                if (!tarjeta.Contains("*"))
                {
                    tarjeta = tarjeta.Substring(0, 6) + "******" + tarjeta.Substring(12, 4);
                }
                ds.Tables[numeroTablaDatosCH].Rows[0]["tarjeta"] = tarjeta;


                estadoDeCuenta.SetDataSource(ds);
                //#if PIEK
                estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                estadoDeCuenta.SetParameterValue("CAT", imagenCAT);
                estadoDeCuenta.SetParameterValue("UNE", imagenUNE);
                //#endif
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF " + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }


        public static bool edoCuentaPDFV2(List<string> archivos, string userCR, string passCR, string hostCR, string databaseCR, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar, List<CuentaAdicional> cuentasAdicionales)
        {
            try
            {
#if ADICIONALES
                if (cuentasAdicionales != null && cuentasAdicionales.Count > 0)
                {

                    foreach (CuentaAdicional cuentaAdicional in cuentasAdicionales)
                    {
                        try
                        {
                            LNOperaciones.crearDirectorio(cuentaAdicional.RutaEdoCuenta);
                            DataSet dsAdicional = _daoCortes.ObtenerDatosCHAdicional(ID_Corte.ToString(), cuentaAdicional.ID_ColectivaAdicional, null, conn);
                            dsAdicional = _daoCortes.ObtenerDatosPagosAdicional(ID_Corte.ToString(), cuentaAdicional.ID_CuentaCompensacion,null, conn, dsAdicional);
                            EdoCuentaTarAdicional EstadoDeCuentaAdicional = new EdoCuentaTarAdicional();
                            string tarjetaAdicional = dsAdicional.Tables[0].Rows[0]["tarjeta"].ToString();
                            if (!tarjetaAdicional.Contains("*"))
                            {
                                tarjetaAdicional = tarjetaAdicional.Substring(0, 6) + "******" + tarjetaAdicional.Substring(12, 4);
                            }
                            dsAdicional.Tables[0].Rows[0]["tarjeta"] = tarjetaAdicional;
                            EstadoDeCuentaAdicional.SetDataSource(dsAdicional);
                            EstadoDeCuentaAdicional.SetParameterValue("imagenLogo", logo);
                            EstadoDeCuentaAdicional.SetParameterValue("CAT", imagenCAT);
                            EstadoDeCuentaAdicional.SetParameterValue("UNE", imagenUNE);
                            EstadoDeCuentaAdicional.SetParameterValue("visualizaSubReporte",0);


                            EstadoDeCuentaAdicional.ExportToDisk(ExportFormatType.PortableDocFormat, cuentaAdicional.RutaEdoCuenta + nombreArchivo);
                            EstadoDeCuentaAdicional.Close();
                            EstadoDeCuentaAdicional.Dispose();
                            cuentaAdicional.Archivos.Add(cuentaAdicional.RutaEdoCuenta + nombreArchivo);//

                        }
                        catch (Exception exAd)
                        {
                            Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF Adicional " + exAd.Message + " " + exAd.StackTrace);

                        }

                    }
                }
#endif
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, ds);
                dynamic estadoDeCuenta;
                string tarjeta = "";
                int numeroTablaDatosCH = 1;
                bool visualizaSubReporte = false;
                DataSet dsSubreporte = null;
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, null);
#if !PIEK
                    estadoDeCuenta = new EdoCuentaCreditoSinDatos();
#else
                    estadoDeCuenta = new EdoCuentaTipSinDatos();
#endif
                    tarjeta = ds.Tables[0].Rows[0]["tarjeta"].ToString();
                    numeroTablaDatosCH = 0;
                }
                else
                {
#if !PIEK
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaCreditoSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaCredito();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();

#else
#if PIEKMSI
                    dsSubreporte = _daoCortes.ObtenerDatosSubreporteMSI(ID_Corte.ToString(), null, conn);
                    if (dsSubreporte.Tables[0].Rows.Count > 0)
                    {
                        visualizaSubReporte = true;
                        // estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                    }
#else
                    dsSubreporte=new DataSet();
#endif
                    if (noTimbrar)
                    {
                        estadoDeCuenta = new EdoCuentaTipSinTimbre();
                    }
                    else
                    {
                        estadoDeCuenta = new EdoCuentaTip();
                    }
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();
#endif
                }//
                if (!tarjeta.Contains("*"))
                {
                    tarjeta = tarjeta.Substring(0, 6) + "******" + tarjeta.Substring(12, 4);
                }
                ds.Tables[numeroTablaDatosCH].Rows[0]["tarjeta"] = tarjeta;



                estadoDeCuenta.SetDataSource(ds);




#if PIEK

                // estadoDeCuenta.SetParameterValue("visualizaSubReporte", 0);
#if PIEKMSI
                if (numeroTablaDatosCH != 0)//en el pdf sin movimientos no se agrego subreporte
                {
                    estadoDeCuenta.Subreports[0].SetDataSource(dsSubreporte);
                    //  if (numeroTablaDatosCH != 0)
                    // {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", visualizaSubReporte ? 1 : 0);
                    // }
                }
                else {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", visualizaSubReporte ? 1 : 0);
                }
#else
                if (numeroTablaDatosCH != 0)
                {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", 0);
                }
#endif
#endif


                //adicionales
#if ADICIONALES
                int i = 0;
                if (cuentasAdicionales != null && cuentasAdicionales.Count > 0)
                {

                    foreach (CuentaAdicional cuentaAdicional in cuentasAdicionales)
                    {
                        i++;

                        String rutaReporte = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                        // DataSet dsVariable = new DataSet();
                        DataSet dsVariable = _daoCortes.ObtenerDatosCHAdicional(ID_Corte.ToString(), cuentaAdicional.ID_ColectivaAdicional, null, conn);
                        dsVariable = _daoCortes.ObtenerDatosPagosAdicional(ID_Corte.ToString(), cuentaAdicional.ID_CuentaCompensacion, null, conn, dsVariable);
                        string tarjetaAdicional = dsVariable.Tables[0].Rows[0]["tarjeta"].ToString();
                        if (!tarjetaAdicional.Contains("*"))
                        {
                            tarjetaAdicional = tarjetaAdicional.Substring(0, 6) + "******" + tarjetaAdicional.Substring(12, 4);
                        }
                        dsVariable.Tables[0].Rows[0]["tarjeta"] = tarjetaAdicional;
                        CrystalDecisions.ReportAppServer.ReportDefModel.Section sec = new CrystalDecisions.ReportAppServer.ReportDefModel.Section();
                        sec.Kind = CrAreaSectionKindEnum.crAreaSectionKindReportFooter;
                        sec.Name = String.Format("DynamicSection{0}", (i));
                        sec.Width = estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.DetailArea.Sections[0].Width;


                        //import
                        estadoDeCuenta.ReportClientDocument.ReportDefController.ReportSectionController.Add(sec, estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea, (i));
                        estadoDeCuenta.ReportClientDocument.SubreportController.ImportSubreport(sec.Name, rutaReporte/*@"C:\DNUNet\CortesBranch\Branched_Proceso_Cortes\Development-PCI\Cortes\DNU CORTE DE CUENTAS\DNU_ParabiliaProcesoCortes\DNU_ParabiliaProcesoCortes\ReporteDebito\trafalgar\"*/ + "EdoCuentaTarAdicionalSubreporte.rpt", sec);
                        estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Sections[i].ReportObjects[0].Border.BorderColor = 16777215;
                        //quitando bordes
                        foreach (ISCRReportObject repObj in estadoDeCuenta.ReportClientDocument.ReportDefController.ReportDefinition.ReportFooterArea.Sections[i].ReportObjects)

                        {


                            if (repObj.Kind == CrReportObjectKindEnum.crReportObjectKindSubreport)

                            {

                                // clone the report object

                                ISCRReportObject newObj = repObj.Clone(true);

                                //modify the line style

                                newObj.Border.BottomLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.TopLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.RightLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                newObj.Border.LeftLineStyle = CrystalDecisions.ReportAppServer.ReportDefModel.CrLineStyleEnum.crLineStyleNoLine;

                                // tell the report to update the report object.

                                estadoDeCuenta.ReportClientDocument.ReportDefController.ReportObjectController.Modify(repObj, newObj);

                            }

                        }

                        //source
                        estadoDeCuenta.Subreports[sec.Name].SetDataSource(dsVariable);
                    }
                }

#endif


                //#if PIEK
                estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                estadoDeCuenta.SetParameterValue("CAT", imagenCAT);
                estadoDeCuenta.SetParameterValue("UNE", imagenUNE);

#if PIEK
    #if PIEKMSI
                if (numeroTablaDatosCH != 0)//en el pdf sin movimientos no se agrego subreporte
                {
              
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", visualizaSubReporte ? 1 : 0);
             
                }
    #else
                if (numeroTablaDatosCH != 0)
                {
                    estadoDeCuenta.SetParameterValue("visualizaSubReporte", 0);
                }
    #endif
#endif
                //#endif
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF " + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }

    }
}
