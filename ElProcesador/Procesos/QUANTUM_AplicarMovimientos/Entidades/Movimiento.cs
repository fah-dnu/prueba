using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QUANTUM_AplicarMovimientos.Entidades
{
    class Movimiento
    {
        public String ID_Cliente  { get; set; }
        public String Consecutivo  { get; set; }
        public String ID_Concepto_Evento { get; set; }
        public String Observaciones  { get; set; }
        public String Factura  { get; set; }
        public String Tarjeta { get; set; }
        public DateTime FechaAutorizacion  { get; set; }
        public String Autorizacion   { get; set; }
        public Decimal Importe  { get; set; }

        public String ClaveCadenaComercial { get; set; }

        
    }
}
