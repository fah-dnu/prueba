using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class EncabezadoAlta
    {
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public EntradaWSCACAOAlta Detalle { get; set; }
    }
}
