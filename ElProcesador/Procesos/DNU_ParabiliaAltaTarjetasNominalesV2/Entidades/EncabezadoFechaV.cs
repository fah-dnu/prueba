using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class EncabezadoFechaV
    {
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public WSObtenerFechaVencimiento Detalle { get; set; }
    }
}
