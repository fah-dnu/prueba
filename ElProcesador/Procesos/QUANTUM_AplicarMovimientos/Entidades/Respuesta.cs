using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QUANTUM_AplicarMovimientos.Entidades
{
   public class Respuesta
    {
        public Decimal Saldo { get; set; }
        public String Tarjeta { get; set; }
        public String Autorizacion { get; set; }
        public String XmlExtras { get; set; }
        public int CodigoRespuesta { get; set; }
        public String Descripcion { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
