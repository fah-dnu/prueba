using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.AutorizadorParabilia_NCliente.Entidades
{
    public class Bonificacion
    {
        public int IdEvento { get; set; }
        public string ClaveEvento { get; set; }
        public long IdColectiva { get; set; }
        public int IdTipoColectiva { get; set; }
        public string ClaveColectiva { get; set; }
        public string Importe { get; set; }
        public string Concepto { get; set; }
        public string Observaciones { get; set; }
        public Nullable<int> RefNumerica { get; set; }
        public Decimal SaldoTotal { get; set; }
        public string Cuenta { get; set; }
        public long IdEmisor { get; set; }
    }
}
