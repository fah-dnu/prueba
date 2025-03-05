using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    public class AltasRequest : ParamParabiliaAltasV1
    {
        public string ClaveProducto { get; set; }
        public string SubProducto { get; set; }
        public string Calle { get; set; }
        public string NoExterior { get; set; }
        public string NoInterior { get; set; }
        public string Colonia { get; set; }
        public string DelegacionMun { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string CP { get; set; }
        public string Pais { get; set; }
    }

    public class ParamParabiliaAltasV1 {
        public string ClaveColectiva { get; set; }
        public string Nombre { get; set; }
        public string Tarjeta { get; set; }
        public string NumeroCuenta { get; set; }
        public string ClaveEmpresaPadre { get; set; }
        public string FechaVencimientoTarjeta { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public string MedioAcceso { get; set; }
        public string TipoMedioAcceso { get; set; }
        public string TarjetaTitular { get; set; }
        public string NomEmbozado { get; set; }
        public string TipoManufactura { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
    }
}
