using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.Modelos.Clases
{
    class Operaciones
    {
        public string IdOperacion { get; set; }
        public string Tarjeta { get; set; }
        public string Meses { get; set; }
        public string MontoDiferido { get; set; }
        public string IdCuenta { get; set; }
        public string IdCorte { get; set; }
        public string IdCadenaComercial { get; set; }
        public DateTime FechaProximoCorte { get; set; }
        public string IdColectivaCuentahabiente { get; set; }
        public string IDTipoColectivaCuentahabiente { get; set; }
        public string ClaveCuentahabiente { get; set; }
        public string NombreCuentahabiente { get; set; }
        public string ClaveCliente { get; set; }
        //public string RFC { get; set; }
        public string Diferimiento { get; set; }
        public string TasaInteresMSI { get; set; }
        public string ClavePromocion { get; set; }
        public string ImporteOperacion { get; set; }
        public string Cargo { get; set; }
        public string Evento { get; set; }
        public string IdPoliza { get; set; }
        public DateTime FechaCompra { get; set; }
        public string IdEvento { get; set; }
        public string  Concepto { get; set; }
        public string IdEventoIntereses { get; set; }
        public string EventoIntereses { get; set; }
        public string IdProducto { get; set; }
        public string ConceptoMCI { get; set; }
        public string IdEventoBonificacion { get; set; }
        public string DescripcionEventoBonificacion { get; set; }
    }
}
