using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente.Entidades
{
    public class EntradaWSEvertecBaja
    {
        public string numeroTarjeta { get; set; }
        public string estadoDeseado { get; set; }
        public string motivoCancelacion { get; set; }
        public string usuarioSiscard { get; set; }
    }
}
