using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.MonitoreoServiciosCacao.DataContract.Entities
{
    public class DContentEncabezado
    {
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public DDetalleTarjeta Detalle { get; set; }
    }
}
