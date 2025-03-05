using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_MovimientoManualMasivo.Entidades
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
        public string Tarjeta { get; set; }
        public long IdEmisor { get; set; }
        public int IdTipoColectivaEmisor { get; set; }
        public string valorMA { get; set; }
        public Nullable<int> IdTipoColectivaMA { get; set; }
        public string email { get; set; }
        public string origen { get; set; }
        public string destino { get; set; }
        public string cuentaOrigen { get; set; }
        public string cuentaDestino { get; set; }
    }
}
