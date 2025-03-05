using CommonProcesador;
using CrystalDecisions.Shared;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Reportes;
using DNU_ParabiliaProcesoCortes.ReporteTipado;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Utilidades;
using FACTURACION_Timbrador.LogicaNegocio;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
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
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + err + " FACTURACION]");
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

            // Create the XmlWriter object and write some content.
            MemoryStream strm = new MemoryStream();
            // XmlWriter writer = XmlWriter.Create(strm, settings);
            XmlReader elXml = XmlReader.Create(new StringReader(elCFDIXML));
            StringBuilder laCadena = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(laCadena, settings);

            _transformador.Transform(elXml, writer);
            return laCadena.ToString().Replace("\t", "").Replace("\r", "").Replace("\n", "");

        }
        public static bool edoCuentaPDF(List<string> archivos, string userCR, string passCR, string hostCR, string databaseCR, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo,bool noTimbrar)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [GeneraEstadoCuentaCredito] generando carpeta:" + ruta + "]");
                LNOperaciones.crearDirectorio(ruta);
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, ds);
                dynamic estadoDeCuenta;
                if (noTimbrar) {
                    estadoDeCuenta = new EdoCuentaTipSinTimbre();
                } else {
                    estadoDeCuenta = new EdoCuentaTip();
                }//
                estadoDeCuenta.SetDataSource(ds);
                estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                estadoDeCuenta.SetParameterValue("CAT", imagenCAT);
                estadoDeCuenta.SetParameterValue("UNE", imagenUNE);
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [error al genrerar PDF " + ex.Message + " " + ex.StackTrace + "]");
                return false;
            }

        }



    }
}
