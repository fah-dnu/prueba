using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.Entidades.Response
{

    public class ResponseConsultarTarjetaOnb
    {
        public string IDSolicitud { get; set; }
        public string IDUsuario { get; set; }
        public Tarjetas[] Tarjetas { get; set; }
        public string CodRespuesta { get; set; }
        public string DescRespuesta { get; set; }
    }

    public class Tarjetas
    {
        public string Folio { get; set; }
        public string Tarjeta { get; set; }
        public DateTime FechaVigencia { get; set; }
        public string Status { get; set; }
        public string DescripcionStatus { get; set; }
        public string SaldoActual { get; set; }
        public string NombreEmbozado { get; set; }
        public string TipoManufactura { get; set; }
    }

}
