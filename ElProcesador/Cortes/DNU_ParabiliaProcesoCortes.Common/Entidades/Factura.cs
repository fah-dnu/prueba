using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Entidades
{
    public class Factura
    {
        public Int64 ID_Factura { get; set; }
        public Int64 ID_FacturaPago { get; set; }
        public int ID_FormaPago { get; set; }
        public String DescripcionFactura { get; set; }
        public String FormaPago { get; set; }
        public String MetodoPago { get; set; }
        public String LugarExpedicion { get; set; }
        public DateTime FechaEmision { get; set; }
        public Colectiva Receptora { get; set; }
        public Colectiva Emisora { get; set; }
        public Decimal IVA { get; set; }
        public Decimal SubTotal { get; set; }
        public Decimal ImporteTotal { get; set; }
        public String XMLCFDI { get; set; }
        public String XMLTimbre { get; set; }
        public String Serie { get; set; }
        public String Folio { get; set; }
        public String CadenaOriginal { get; set; }
        public String Sello { get; set; }
        public String SelloCFD { get; set; }
        public String SelloSAT { get; set; }
        public DateTime FechaTimbrado { get; set; }
        public String Comentarios { get; set; }
        public Decimal IEPS { get; set; }

        public Decimal ImportePagado { get; set; }
        public Decimal ID_PolizaPago { get; set; }

        public Decimal ImportePorAplicar { get; set; }
        public Decimal IVARetenido { get; set; }
        public Decimal ISRRetenido { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public String URLCodigoBarras { get; set; }
        public String NoCertificadoSAT { get; set; }
        public String NoCertificadoEmisor { get; set; }
        public String elCertificado { get; set; }
        public String UUID { get; set; }
        public String DatosExtraEnBD { get; set; }
        public String CadenaOriginalTimbre { get; set; }
        public String DomicilioEmisor { get; set; }
        public String CiudadEstadoEmisor { get; set; }
        public String DomicilioReceptor { get; set; }
        public String CiudadEstadoReceptor { get; set; }
        public String NombreEmisor { get; set; }
        public String NombreReceptor { get; set; }
        public String RFCEmisor { get; set; }
        public String RFCReceptor { get; set; }
        public String TotalConLetra { get; set; }
        public String UrlQrCode { get; set; }
        public String Estatus { get; set; }
        public String UrlReporte { get; set; }
        public String TipoComprobante { get; set; }
        public String RequestorPAC { get; set; }
        public String UserPAC { get; set; }
        public String UsoCFDI { get; set; }
        public String UserPassPAC { get; set; }
        public String PACFacturacion { get; set; }
        public String RegimenFiscal { get; set; }
        public String RFCProvedorCertificado { get; set; }
        public String URLProvedorCertificado { get; set; }
        public String URLProvedorCertificadoObtieneXML { get; set; }
        public String RutaCerSAT { get; set; }
        public String RutaKeySAT { get; set; }
        public String PassCerSAT { get; set; }
        public String DomicilioFiscalReceptor { get; set; }
        public String RegimenFiscalReceptor { get; set; }


        public String getParametro(String nombreParametro)
        {
            try
            {

                String laRespuesta = "";


                if (this._parametrosCalculados.ContainsKey(nombreParametro))
                {
                    laRespuesta = this._parametrosCalculados[nombreParametro].Valor;
                }
                else
                {
                    if (this.Receptora.LosParametrosExtras.ContainsKey(nombreParametro))
                    {
                        laRespuesta = this.Receptora.LosParametrosExtras[nombreParametro].Valor;
                    }
                }


                return laRespuesta;

            }
            catch (Exception err)
            {
                throw new Exception("ERROR AL OBTENER EL PARAMETRO:" + nombreParametro);
            }
        }
        public Int32 ID_Periodo { get; set; }
        public Int64 ID_FacturaTipo { get; set; }
        private Dictionary<String, ParametroFacturaTipo> _parametrosCalculados = new Dictionary<String, ParametroFacturaTipo>();

        public Dictionary<String, ParametroFacturaTipo> ParametrosCalculados
        {
            get { return _parametrosCalculados; }
            set { _parametrosCalculados = value; }
        }

        //public String InformacionExtra { get; set; }
        private List<DetalleFactura> _losDetalles = new List<DetalleFactura>();

        public List<DetalleFactura> losDetalles
        {
            get { return _losDetalles; }
            set { _losDetalles = value; }
        }



        public FacturaTipo laFacturaTipo { get; set; }

        public String InformacionExtra()
        {
            StringBuilder laResp = new StringBuilder();

            try
            {
                if (Emisora != null)
                {
                    foreach (Parametro unParam in Emisora.LosParametrosExtras.Values)
                    {
                        if (unParam.ImpresoFacturaEmisor)
                        {
                            if (unParam.ImprimeDescripcion)
                            {
                                laResp.Append(unParam.Descripcion);
                                laResp.Append(": ");
                                laResp.Append(unParam.Valor);
                                laResp.Append("\n");
                            }
                            else
                            {
                                laResp.Append(unParam.Valor);
                                laResp.Append("\n");
                            }
                        }
                    }
                }

                if (Receptora != null)
                {
                    foreach (Parametro unParam in Receptora.LosParametrosExtras.Values)
                    {
                        if (unParam.ImpresoFacturaReceptor)
                        {
                            if (unParam.ImprimeDescripcion)
                            {
                                laResp.Append(unParam.Descripcion);
                                laResp.Append(": ");
                                laResp.Append(unParam.Valor);
                                laResp.Append("\n");
                            }
                            else
                            {
                                laResp.Append(unParam.Valor);
                                laResp.Append("\n");
                            }
                        }
                    }
                }

                foreach (ParametroFacturaTipo unParam in _parametrosCalculados.Values)
                {

                    if (unParam.ImprimeFactura)
                    {
                        if (unParam.ImprimeDescripcionFactura)
                        {
                            laResp.Append(unParam.Descripcion);
                            laResp.Append(": ");
                            laResp.Append(unParam.Valor);
                            laResp.Append("\n");
                        }
                        else
                        {
                            laResp.Append(unParam.Valor);
                            laResp.Append("\n");
                        }
                    }

                }

                return laResp.ToString();
            }
            catch (Exception err)
            {
                return "";
            }
        }



    }
}
