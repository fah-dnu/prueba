using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using DALCentralAplicaciones.Utilidades;


namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class Timbre
    {

        String _xmlTimbre = "";
        XmlDocument xDoc = new XmlDocument();
        XmlNodeList timbre;
        XmlNodeList lista;

        public Timbre(String xmlTimbre)
        {
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
                Loguear.Error(err, "FACTURACION");
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
                foreach (XmlElement nodo in timbre)
                {
                    return nodo.GetAttribute(elAtributo);
                }

                return "";

            }
            catch (Exception err)
            {
                return "";
            }
        }
    }
}