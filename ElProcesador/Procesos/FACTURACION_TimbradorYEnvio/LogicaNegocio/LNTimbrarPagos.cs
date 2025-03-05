using CommonProcesador;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALCentralAplicaciones.Entidades;
using DALCentralAplicaciones.Utilidades;
using FACTURACION_Timbrador.wsFacturacion;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class LNTimbrarPagos
    {
        static XslCompiledTransform _transformador = new XslCompiledTransform();

        static LNTimbrarPagos()
        {
            try
            {
                string ArchXSLT = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IdApplication"].ToString()), "FileXSLT").Valor;

                _transformador.Load(ArchXSLT);
            }
            catch (Exception err)
            {
            }

        }

        public static int TimbrarFactura(Factura laFactura,  String elXML, Usuario elUser)
        {
            try
            {
                //si el tipo de documento no requiere timbre entonces no lo genera.
               
                //DecodificaSelloDigital(, laFactura.Emisora);
                String elnoCeritifado = "";
                String ElCertificado = "";

               // laFactura.Folio = DateTime.Now.ToString("HMMss");

                string CadenaOriginal = ObtieneCadenaOriginal(elXML).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
                String elSello = ObtenerSelloPago(CadenaOriginal, laFactura.Emisora, out elnoCeritifado, out ElCertificado);
                CadenaOriginal = CadenaOriginal.Replace("[NoCertificado]", elnoCeritifado);
                laFactura.CadenaOriginal = CadenaOriginal;
                laFactura.Sello = elSello;
                laFactura.NoCertificadoEmisor = elnoCeritifado;
                laFactura.elCertificado = ElCertificado;


                wsFacturacion.FactWSFront unaOperacion = new wsFacturacion.FactWSFront();
                elXML = elXML.Replace("[Sello]", elSello);
                elXML = elXML.Replace("[NoCertificado]", elnoCeritifado);
                elXML = elXML.Replace("[Certificado]", ElCertificado);

                //  elXML = elXML.Replace("    ", " ").Replace("  ", "");

                Loguear.EntradaSalida(elXML, "", false);

                TransactionTag laRespuesta = unaOperacion.RequestTransaction(laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), "TIMBRAR", "MX", laFactura.Emisora.RFC, laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), laFactura.Emisora.ObtieneParametro("@UserNameMySuite"), elXML, "", "");
                //TransactionTag laRespuesta = unaOperacion.RequestTransaction(laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), "TIMBRAR", "MX", laFactura.Emisora.RFC, laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), laFactura.Emisora.ObtieneParametro("@UserNameMySuite"), elXML, "", "");
                //TransactionTag laRespuesta = unaOperacion.RequestTransaction("15379e78-a244-4ba9-bc93-220c05078657", "TIMBRAR", "MX", "AAA010101AAA", "15379e78-a244-4ba9-bc93-220c05078657", "MX.AAA010101AAA.Juan", elXML, "", "");

                Loguear.EntradaSalida(laRespuesta.ResponseData.ResponseData3 + ",  ", "", true);
                Loguear.EntradaSalida(laRespuesta.Response.Description + ",  ", "", true);

                if (laRespuesta.ResponseData.ResponseData3.Equals("OK", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Fue Autorizada y se genero el timbre.
                    Byte[] resp = Convert.FromBase64String(laRespuesta.ResponseData.ResponseData1);
                    String unTimbrazo = System.Text.Encoding.UTF8.GetString(resp);


                    Loguear.EntradaSalida(unTimbrazo, "", true);
                    Timbre elTimbre = new Timbre(unTimbrazo);
                    //Poner
                    elXML = elXML.Replace(" </cfdi:Complemento>", " </cfdi:Complemento>\r <cfdi:Complemento>\r" + unTimbrazo.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "") + "\r </cfdi:Complemento>");

                    laFactura.CadenaOriginalTimbre = "||" + elTimbre.version + "|" + elTimbre.UUID + "|" + elTimbre.FechaTimbrado + "|" + elTimbre.RfcProvCertif + "|" + elTimbre.selloCFD + "|" + elTimbre.noCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);
                    laFactura.LugarExpedicion = laFactura.LugarExpedicion;// laFactura.Emisora.DUbicacion.Asentamiento.ElMunicipio.DesMunicipio;
                    laFactura.XMLCFDI = elXML;
                    //MX.DDN1106065K2.DNU
                    //generar el archvio XML
                    //Genera el XML
                    
                    String unPath = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(laFactura.XMLCFDI);
                    xDoc.Save(unPath + "Pago_" + laFactura.Serie + laFactura.Folio + ".xml");
                   

                    laFactura.XMLTimbre = unTimbrazo;
                    laFactura.NoCertificadoSAT = elTimbre.noCertificadoSAT;
                    laFactura.SelloCFD = elTimbre.selloCFD;
                    laFactura.SelloSAT = elTimbre.selloSAT;
                    laFactura.UUID = elTimbre.UUID;
                    laFactura.FechaTimbrado = DateTime.Parse(elTimbre.FechaTimbrado);
                    laFactura.UrlQrCode = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "QR_" + laFactura.Serie + laFactura.Folio + ".png";


                    using (SqlConnection conn = BDAutorizador.BDEscritura)
                    {
                        conn.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {

                            try
                            {
                              //  DAOFactura.InsertaPago(laFactura, ID_PolizaPago, conn, transaccionSQL);

                                DAOFactura.ActualizaFactura(laFactura, conn, transaccionSQL);

                                DAOFactura.CambiaEstatusFacturaPago(laFactura.ID_Factura, 3, conn, transaccionSQL);

                                transaccionSQL.Commit();

                            }
                            catch (Exception Err)
                            {
                                Logueo.Error(Err.Message);
                                transaccionSQL.Rollback();

                            }
                         }
                    }


                }
                else
                {

                    Loguear.EntradaSalida(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3, "", true);
                    if (laRespuesta.ResponseData.ResponseData2.Trim().Length != 0)
                        throw new Exception(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3);
                    else
                        throw new Exception(laRespuesta.Response.Code + ": " + laRespuesta.Response.Description + "; " + laRespuesta.Response.Data);

                }
                //guardar los datos de la respuesta en la BD.

                return 0;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                throw err;
            }
        }

        public static string ObtenerSelloPago(String CadenaOriginal, Colectiva Emisor, out String NoCertificado, out String Certificado)
        {
            NoCertificado = "";
            Certificado = "";
            try
            {
                string ArchivoCertificado = Emisor.ObtieneParametro("@SAT_UbicacionCertificado");// @"D:\CodigoFuente\CentralAdministrativa\CentralAdministrativa\Core\Apps\Facturas\Emisor.cer";
                string key = Emisor.ObtieneParametro("@SAT_UbicacionKey");//@"D:\CodigoFuente\CentralAdministrativa\CentralAdministrativa\Core\Apps\Facturas\Emisor.key";
                string lPassword = Emisor.ObtieneParametro("@SAT_Pass");// @"12345678a";
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
                Logueo.Error(err.Message);
                return "";

            }
        }


    


        public static String ObtieneCadenaOriginal(String elCFDIXML)
        {
            //--FileXSLT

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CloseOutput = false;

            // Create the XmlWriter object and write some content.
            MemoryStream strm = new MemoryStream();
            // XmlWriter writer = XmlWriter.Create(strm, settings);
            XmlReader elXml = XmlReader.Create(new StringReader(elCFDIXML));
            StringBuilder laCadena = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(laCadena, settings);

            _transformador.Transform(elXml, writer);
            return laCadena.ToString().Replace("\t", "").Replace("\r", "").Replace("\n", "");

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


    }
}