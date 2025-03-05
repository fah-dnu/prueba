using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.Entidades
{
    public class WebhookMessage
    {
        public int ID_Colectiva { get; set; }
        public long ID_Operacion { get; set; }
        public Mensaje mensaje { get; set; }
        public String Prioridad { get; set; }
    }


    public class Mensaje
    {
        public String IdMovimiento { get; set; }
        public String Tarjeta { get; set; }
        public String NumeroCuenta { get; set; }
        public String Importe { get; set; }
        public String Moneda { get; set; }
        public String FechaHora { get; set; }
        public String NumeroAutorizacion { get; set; }
        public String POSEntryMode { get; set; }
        public String ClaveMovimiento { get; set; }
        public String CodigoProceso { get; set; }
        public String NumeroAfiliacionComercio { get; set; }
        public String NombreComercio { get; set; }
        public String MCC { get; set; }
        public String Nombre { get; set; }
        public String PrimerApellido { get; set; }
        public String SegundoApellido { get; set; }
        public String Telefono { get; set; }
        public String Correo { get; set; }
        public String TipoMedioAcceso { get; set; }
        public String MedioAcceso { get; set; }
        public String InstitucionOrdenante { get; set; }
        public String InstitucionBeneficiaria { get; set; }
        public String ClaveRastreo { get; set; }
        public String NombreOrdenante { get; set; }
        public String RfcCurpOrdenante { get; set; }
        public String NombreBeneficiario { get; set; }
        public String CuentaBeneficiario { get; set; }
        public String RfcCurpBeneficiario { get; set; }
        public String ConceptoPago { get; set; }
        public String ReferenciaNumerica { get; set; }
        public String TipoManufactura { get; set; }


    }



}
