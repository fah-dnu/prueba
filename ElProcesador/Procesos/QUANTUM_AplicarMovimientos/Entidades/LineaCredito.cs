using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QUANTUM_ModificarLineasCredito.Entidades
{
    public class LineaCredito
    {

        public String ID_Cliente { get; set; }
        public String Tarjeta { get; set; }
        public Decimal LimiteCredito { get; set; }
        public int DiasVigencia { get; set; }
        public String Consecutivo { get; set; }
    }
}
