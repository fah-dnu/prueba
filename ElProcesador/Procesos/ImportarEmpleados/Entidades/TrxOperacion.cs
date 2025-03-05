using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportarEmpleados.Entidades
{
    public enum TipoOperacion
    {
        Requerimiento = 0,
        Reverso = 1,
    };

    public class TrxOperacion
    {

        public string Monto { get; set; }
        public TipoOperacion elTipoOperacion { get; set; }
        public DateTime HoraTransaccion { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string TarjetaCredito { get; set; }
        public string FechaVencimiento { get; set; }
        public string Track2 { get; set; }
        public string CVV2 { get; set; }
        public string CodigoMoneda { get; set; }
        public string Adquirente { get; set; }
        public string DatosPlastico { get; set; }
        public string Beneficiario { get; set; }
        public string MedioAcceso { get; set; }
        public string TipoMedioAcceso { get; set; }
        public string Telefono { get; set; }
        public string NIP { get; set; }
        public string ProccesingCode { get; set; }
        public string POSEntryMode { get; set; }
        public string Ticket { get; set; }
        public string Operador { get; set; }
        public string SKU { get; set; }
        public string Afiliacion { get; set; }
        public string Referencia { get; set; }
        public string Sucursal { get; set; }
        public string Terminal { get; set; }
        public String ECI { get; set; }
        public String CardType { get; set; }
        public String XID { get; set; }
        public String CAVV { get; set; }
        public String Status { get; set; }
        public String Origen { get; set; }


        public TrxOperacion()
        { }

        public TrxOperacion(string pmontoTransferencia, DateTime pfechaTransmision, string ptelefonica, string ptelefono, string pidClienteTitular,
            string porigen, string pnumeroTarjeta, string pfechaVencimiento, string pnombreTarjetahabiente, string pcvv2, string pidTarjeta,
            string pnip, string pmedioAcceso, string ptipoMedioAcceso, string pProccesingCode,
            string pTicket, string poperador, string pSKU, string pBeneficiario, string pReferencia, string pafiliacion, string psucursal, string pterminal, string pPOSEM,
            String pECI,
            String pCardType,
            String pXID,
            String pCAVV,
            String pStatus
            )
        {
            try
            {

                string mes = pfechaVencimiento.Substring(0, 2);
                string anio = pfechaVencimiento.Substring(2, 2);
                Monto = pmontoTransferencia;
                HoraTransaccion = pfechaTransmision;
                FechaTransaccion = pfechaTransmision;
                Beneficiario = ptelefonica;


                if (pnumeroTarjeta.Contains("@"))
                {
                    TarjetaCredito = "0000000000000000=0000";
                }
                else
                {
                    TarjetaCredito = pnumeroTarjeta + "=" + mes + anio;
                }

                CVV2 = pcvv2;
                Telefono = ptelefono;
                Origen = porigen;
                NIP = NIP.PadLeft(8, '0');
                MedioAcceso = pmedioAcceso.PadLeft(20, ' ');
                Afiliacion = pafiliacion;
                TipoMedioAcceso = ptipoMedioAcceso;
                ProccesingCode = ProccesingCode;
                POSEntryMode = pPOSEM;
                Ticket = pTicket;
                Operador = poperador;
                SKU = pSKU;
                Sucursal = psucursal.PadLeft(8, ' ');
                Terminal = pterminal.PadLeft(16, ' ');
                Beneficiario = pBeneficiario;
                Referencia = pReferencia.PadLeft(50, ' ');
                ECI = pECI;
                CardType = pCardType;
                XID = pXID;
                CAVV = pCAVV;
                Status = pStatus;


            }
            catch (Exception Ex)
            {
                new Exception("Ha sucedido un error al intentar obtener la Tarjeta del usuario actual");
            }
        }

    }
}
