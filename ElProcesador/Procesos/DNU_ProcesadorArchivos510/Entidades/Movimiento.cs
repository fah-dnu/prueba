using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos510.Entidades
{
    class Movimiento
    {

        public int IdEvento { get; set; }
        public string ClaveEvento { get; set; }
        public string ClaveMA { get; set; }
        public string TipoMA { get; set; }
        public long IdColectiva { get; set; }
        public int IdTipoColectiva { get; set; }
        public string ClaveColectiva { get; set; }
        public string Importe { get; set; }
        public string Concepto { get; set; }
        public string Observaciones { get; set; }
    }
}
