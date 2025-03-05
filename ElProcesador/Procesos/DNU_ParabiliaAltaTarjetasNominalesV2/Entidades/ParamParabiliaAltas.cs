using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class ParamParabiliaAltas
    {
        public string ClaveColectiva { get; set; }
        public string Nombre { get; set; }
        public string IdArchivoDetalle { get; set; }
        public string Tarjeta { get; set; }
        public string NumeroCuenta { get; set; }
        public string IdEmpleadoraPadre { get; set; }
        public string FechaVencimientoTarjeta { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public string medioAcceso { get; set; }
        public string tipoMedioAcceso { get; set; }
        public string tarjetaTitular { get; set; }
        public string tipoMedioAccesoTitular { get; set; }
        public string medioAccesoTitular { get; set; }
        public string tipoManufactura { get; set; }
        public string nombreEmbozar { get; set; }
        public string LimiteCredito { get; set; }
        public string CicloCorte { get; set; }
        public string SubProducto { get; set; }
    }
}
