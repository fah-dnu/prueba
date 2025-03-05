using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente.Entidades
{
    public class EncabezadoAlta
    {
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public EntradaWSEvertecAlta Detalle { get; set; }
    }
}
