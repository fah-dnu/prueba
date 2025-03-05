using CommonProcesador;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALAutorizador.LogicaNegocio;
using DALCentralAplicaciones.Entidades;
using DALCentralAplicaciones.Utilidades;
using FACTURACION_Timbrador.wsFacturacion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class LNTimbrados
    {
        static XslCompiledTransform _transformador = new XslCompiledTransform();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Boolean TimbrarFacturasCreadas()
        {
            try
            {
                int resp = 0;

                //Obtiene las Facturas
                List<Factura> lasFacturas = DAOFactura.ObtieneFacturasPorTimbrarProcNoc();

                foreach (Factura unaFac in lasFacturas)
                {
                    if (unaFac.TipoComprobante.ToUpper().Equals("INGRESO") || unaFac.TipoComprobante.ToUpper().Equals("I"))
                    {
                        CFDI XmlFactura = new CFDI(Configuracion.Get(
                                Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()),
                                "FileXML_CFDI3.3").Valor, unaFac);
                        try
                        {
                            resp = TimbrarFactura(unaFac, XmlFactura.ToString(), new Usuario() { ClaveUsuario = "Procesador Nocturno" });
                        }
                        catch (Exception)
                        {
                            resp = -1;
                        }
                    }
                    else if (unaFac.TipoComprobante.ToUpper().Equals("PAGO") || unaFac.TipoComprobante.ToUpper().Equals("P"))
                    {
                        Pago unPago = DAOFactura.ObtienePolizaEImporteFacturaPago(unaFac.ID_Factura);
                        bool bResp = LNPagos.TimbrarPago(unPago.ID_Poliza, unPago.ImportePAgado,
                            new Usuario() { ClaveUsuario = "Procesador Nocturno" },
                            Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()));
                        resp = bResp ? 0 : -1;
                    }
                    else
                    {
                        CFDI XmlFactura = new CFDI(Configuracion.Get(
                                Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()),
                                "FileXML_CFDI3.3").Valor, unaFac);
                        try
                        {
                            resp = TimbrarFactura(unaFac, XmlFactura.ToString(), new Usuario() { ClaveUsuario = "Procesador Nocturno" });
                        }
                        catch (Exception)
                        {
                            resp = -1;
                        }
                    }
                                        

                    if (resp == 0)
                    {
                        if (unaFac.TipoComprobante.ToUpper().Equals("INGRESO") || unaFac.TipoComprobante.ToUpper().Equals("I"))
                        {
                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 3);//Cambia el estatus a Timbrado
                        }
                        else if (unaFac.TipoComprobante.ToUpper().Equals("PAGO") || unaFac.TipoComprobante.ToUpper().Equals("P"))
                        {
                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 3);//Cambia el estatus a Timbrado
                        }
                        else
                        {
                            LNFactura.CambiaEstatusFactura(unaFac.ID_Factura, 5);//Cambia el estatus a confirmada
                        }
                    }
                }

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error(err.ToString());
                return false;
            }
        }

        static LNTimbrados()
        {
            try
            {
                string ArchXSLT = Configuracion.Get(new Guid(ConfigurationManager.AppSettings["IdApplication"].ToString()), "FileXSLT").Valor;

                _transformador.Load(ArchXSLT);
            }
            catch (Exception err)
            {
            }

        }

        public static int TimbrarFactura(Factura laFactura, String elXML, Usuario elUser)
        {
            try
            {
                //si el tipo de documento no requiere timbre entonces no lo genera.
                if (!laFactura.TipoComprobante.ToUpper().Equals("INGRESO"))
                {
                    if (!laFactura.TipoComprobante.ToUpper().Equals("I"))
                    {
                        DAOFactura.ActualizaFacturaSinTimbre(laFactura);
                        return 0;
                    }
                }

                String elnoCeritifado = "";
                String ElCertificado = "";

                string CadenaOriginal = ObtieneCadenaOriginal(elXML).Replace("    |    |", "||").Replace("    ||  ", "||").Replace("    ", " ").Replace("  ", "");
                String elSello = ObtenerSelloFactura(CadenaOriginal, laFactura.Emisora, out elnoCeritifado, out ElCertificado);
                CadenaOriginal = CadenaOriginal.Replace("[NoCertificado]", elnoCeritifado);
                laFactura.CadenaOriginal = CadenaOriginal;
                laFactura.Sello = elSello;
                laFactura.NoCertificadoEmisor = elnoCeritifado;
                laFactura.elCertificado = ElCertificado;

                FactWSFront unaOperacion = new FactWSFront();
                elXML = elXML.Replace("[Sello]", elSello);
                elXML = elXML.Replace("[NoCertificado]", elnoCeritifado);
                elXML = elXML.Replace("[Certificado]", ElCertificado);

                Loguear.EntradaSalida(elXML, "", false);
                TransactionTag laRespuesta = unaOperacion.RequestTransaction(laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), "TIMBRAR", "MX", laFactura.Emisora.RFC, laFactura.Emisora.ObtieneParametro("@RequestorIDMySuite"), laFactura.Emisora.ObtieneParametro("@UserNameMySuite"), elXML, "", "");
                Loguear.EntradaSalida(laRespuesta.ResponseData.ResponseData3 + ",  ", "", true);

                if (laRespuesta.ResponseData.ResponseData3.Equals("OK", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Fue Autorizada y se genero el timbre.
                    Byte[] resp = Convert.FromBase64String(laRespuesta.ResponseData.ResponseData1);
                    String unTimbrazo = Encoding.UTF8.GetString(resp);

                    Loguear.EntradaSalida(unTimbrazo, "", true);
                    Timbre elTimbre = new Timbre(unTimbrazo);

                    elXML = elXML.Replace("</cfdi:Impuestos></cfdi:Comprobante>", "</cfdi:Impuestos>\r <cfdi:Complemento>\r" + 
                        unTimbrazo.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "")
                        + "\r </cfdi:Complemento>" + "\r </cfdi:Comprobante>");

                    laFactura.CadenaOriginalTimbre = "||" + elTimbre.version + "|" + elTimbre.UUID + "|" + elTimbre.FechaTimbrado + "|" + elTimbre.RfcProvCertif + "|" + elTimbre.selloCFD + "|" + elTimbre.noCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);
                    laFactura.LugarExpedicion = laFactura.LugarExpedicion;
                    laFactura.XMLCFDI = elXML;
                    
                    //Genera el XML
                    if (laFactura.TipoComprobante.ToUpper().Equals("INGRESO") || laFactura.TipoComprobante.ToUpper().Equals("I"))
                    {
                        String unPath = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor;
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(laFactura.XMLCFDI);
                        xDoc.Save(unPath + "Factura_" + laFactura.Serie + laFactura.Folio + ".xml");
                    }

                    laFactura.XMLTimbre = unTimbrazo;
                    laFactura.NoCertificadoSAT = elTimbre.noCertificadoSAT;
                    laFactura.SelloCFD = elTimbre.selloCFD;
                    laFactura.SelloSAT = elTimbre.selloSAT;
                    laFactura.UUID = elTimbre.UUID;
                    laFactura.FechaTimbrado = DateTime.Parse(elTimbre.FechaTimbrado);
                    laFactura.UrlQrCode = Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "urlLasFacturas").Valor + "QR_" + laFactura.Serie + laFactura.Folio + ".png";

                    DAOFactura.ActualizaFactura(laFactura);
                }
                else
                {

                    Loguear.EntradaSalida(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3, "", true);
                    if (laRespuesta.ResponseData.ResponseData2.Trim().Length != 0)
                        throw new Exception(laRespuesta.ResponseData.ResponseData2 + ": " + laRespuesta.ResponseData.ResponseData3);
                    else
                        throw new Exception(laRespuesta.Response.Code + ": " + laRespuesta.Response.Description + "; " + laRespuesta.Response.Data);

                }

                return 0;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                throw err;
            }
        }

        public static string ObtenerSelloFactura(String CadenaOriginal, Colectiva Emisor, out String NoCertificado, out String Certificado)
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


        private string generaSello(String CadenaOriginal, Colectiva Emisor, out String NoCertificado, out String Certificado)
        {
            string strSello = "";
            string strPathLlave = Emisor.ObtieneParametro("@SAT_UbicacionCertificado");// CFDI.Properties.Settings.Default.llave;
            string strLlavePwd = Emisor.ObtieneParametro("@SAT_UbicacionKey");// CFDI.Properties.Settings.Default.clave;
            string strCadenaOriginal = CadenaOriginal;// generaCadena(); // Aquí ya haber generado la cadena original

            System.Security.SecureString passwordSeguro = new System.Security.SecureString();
            passwordSeguro.Clear();

            foreach (char c in strLlavePwd.ToCharArray())
                passwordSeguro.AppendChar(c);

            byte[] llavePrivadaBytes = System.IO.File.ReadAllBytes(strPathLlave);
            RSACryptoServiceProvider rsa = opensslkey.DecodeEncryptedPrivateKeyInfo(llavePrivadaBytes, passwordSeguro);

            NoCertificado = rsa.ToString();
            Certificado = rsa.ToString();
            // Obtengo Datos
            //  NoCertificado = FromHex(certificado.SerialNumber);
            // strCadenaOriginal = strCadenaOriginal.Replace("[NoCertificado]", NoCertificado);
            strCadenaOriginal = strCadenaOriginal.Replace("    ", " ").Replace("  ", "");

            SHA1Managed sha = new SHA1Managed();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(strCadenaOriginal);
            byte[] digest = sha.ComputeHash(bytes);

            RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);
            RSAFormatter.SetHashAlgorithm("SHA1");
            byte[] SignedHashValue = RSAFormatter.CreateSignature(digest);

            SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();

            byte[] bytesFirmados = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(strCadenaOriginal), hasher);
            strSello = Convert.ToBase64String(bytesFirmados);  // Y aquí está el sello
            string r = Convert.ToBase64String(SignedHashValue);// Y aquí está el sello 2 

            return strSello;
            //return r;
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