using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class WebhookMessage
    {
        public long ID_Colectiva { get; set; }
        public long ID_Operacion { get; set; }
        public Mensaje mensaje { get; set; }
        public String Prioridad { get; set; }
    }


    public class Mensaje
    {
        public string IdMovimiento { get; set; }
        public string Tarjeta { get; set; }
        public string NumeroCuenta { get; set; }
        public string Importe { get; set; }
        public string Moneda { get; set; }
        public string FechaHora { get; set; }
        public string NumeroAutorizacion { get; set; }
        public string POSEntryMode { get; set; }
        public string ClaveMovimiento { get; set; }
        public string DescripcionMovimiento { get; set; }
        public string CodigoProceso { get; set; }
        public string NumeroAfiliacionComercio { get; set; }
        public string NombreComercio { get; set; }
        public string MCC { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string TipoMedioAcceso { get; set; }
        public string MedioAcceso { get; set; }
        public string InstitucionOrdenante { get; set; }
        public string InstitucionBeneficiaria { get; set; }
        public string ClaveRastreo { get; set; }
        public string NombreOrdenante { get; set; }
        public string RfcCurpOrdenante { get; set; }
        public string NombreBeneficiario { get; set; }
        public string CuentaBeneficiario { get; set; }
        public string RfcCurpBeneficiario { get; set; }
        public string ConceptoPago { get; set; }
        public string ReferenciaNumerica { get; set; }
        public string TipoManufactura { get; set; }
        public string NombreFichero { get; set; }
        public string FechaPresentacion { get; set; }
        public string ImporteOrigen { get; set; }
        public string DivisaOrigen { get; set; }
        public string ImporteDestino { get; set; }
        public string DivisaDestino { get; set; }


    }
}
