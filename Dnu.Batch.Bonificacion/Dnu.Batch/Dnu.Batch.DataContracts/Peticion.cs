using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dnu.Batch.DataContracts
{
    public class Peticion
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string MedioPago { get; set; }
        public string TipoMedioPago { get; set; }
        public string SKU_Beneficio { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public float Importe { get; set; }
        public string Ticket { get; set; }
        public string Sucursal { get; set; }
        public string Afiliacion { get; set; }
        public string Terminal { get; set; }
        public string Operador { get; set; }
        public string SKUs { get; set; }
    }
}
