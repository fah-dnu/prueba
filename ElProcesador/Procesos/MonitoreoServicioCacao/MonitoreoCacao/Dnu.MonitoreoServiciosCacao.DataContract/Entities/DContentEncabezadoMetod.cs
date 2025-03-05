using Dnu.MonitoreoServiciosCacao.DataContract.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.ConsumosServiciosCacao.DataContract.Entities
{
    public class DContentEncabezadoMetod
    {
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public DDetalleTestMetodo Detalle { get; set; }
    }
}
