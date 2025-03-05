using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaBloqDesbloqueoTDC.Entidades
{
    public class Resultado
    {
        public string Tipo { get; set; }
        public Int64 ID_Cuenta { get; set; }
        public string Mensaje { get; set; }
        public string Bitacora { get; set; }
    }
}
