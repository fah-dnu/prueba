using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class ArchivoXLSX
    {
        public string Nombre { get; set; }
        public string FechaGeneracion { get; set; }
        public int EstadosDeCuentaTotales { get; set; }
        public int EstadosDeCuentaCorrectos { get; set; }
        public int EstadosDeCuentaErroneos { get; set; }
        public int EstadosDeCuentaTimbrados { get; set; }
        public List<DetalleArchivoXLSX> Detalle { get; set; }


    }
}
