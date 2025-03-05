using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class Timbre
    {
        String _xmlTimbre = "";
        XmlDocument xDoc = new XmlDocument();
        XmlNodeList timbre;
        XmlNodeList lista;

        public Timbre(String xmlTimbre)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                _xmlTimbre = xmlTimbre;

                xDoc.LoadXml(_xmlTimbre);
                timbre = xDoc.GetElementsByTagName("tfd:TimbreFiscalDigital");
                //timbre.
                //lista = ((XmlElement)timbre[0]).GetElementsByTagName("tfd:TimbreFiscalDigital");
            }
            catch (Exception err)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [FACTURACION" + err + "]");
            }
        }


        public String version
        {
            get
            {
                if (ObtieneAtributo("Version").Length == 0)
                {
                    return ObtieneAtributo("version");
                }
                else
                {
                    return ObtieneAtributo("Version");

                }
            }
        }

        public String UUID
        {
            get
            {
                return ObtieneAtributo("UUID");
            }
        }


        public String FechaTimbrado

        {
            get
            {
                return ObtieneAtributo("FechaTimbrado");
            }

        }


        public String selloCFD
        {
            get
            {
                return ObtieneAtributo("SelloCFD");
            }
        }

        public String noCertificadoSAT
        {
            get
            {
                return ObtieneAtributo("NoCertificadoSAT");
            }
        }

        public String RfcProvCertif
        {
            get
            {
                return ObtieneAtributo("RfcProvCertif");
            }
        }


        public String selloSAT
        {
            get
            {
                return ObtieneAtributo("SelloSAT");
            }
        }

        public String ObtieneAtributo(String elAtributo)
        {
            try
            {
                string sAtributo = string.Empty;

                foreach (XmlElement nodo in timbre)
                {
                    sAtributo = nodo.GetAttribute(elAtributo);
                }

                return sAtributo;

            }
            catch (Exception err)
            {
                return "";
            }
        }
    }
}
