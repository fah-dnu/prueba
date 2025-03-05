using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class responseSolicitudDigital : Response
    {//
        public string IDSolicitud { get; set; }
        public string Clave { get; set; }
        public string Identificador { get; set; }
        public string Fecha { get; set; }
    }
}
