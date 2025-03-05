using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class responseAltaTarjeta : Response
    {
        public string IDSolicitud { get; set; }
        public string RegistroCliente { get; set; }
        public string CuentaCacao { get; set; }
        public string Tarjeta { get; set; }
        public string MotivoRechazo { get; set; }
        public string FechaVencimiento { get; set; }
        public string CLABE { get; set; }
    }
}
