using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class pagosIvasYComisiones
    {
        public string cantidad { get; set; }
        public string unidad { get; set; }
        public string claveProdServ{ get; set; }
        public string claveUnidad { get; set; }
        public string nombreProducto { get; set; }
        public string ivaImporte { get; set; }
        public string valorIntereses { get; set; }
        public string precioUnitario { get; set; }
        public string precioBase { get; set; }
        public string importeImpuesto { get; set; }
        public string impTipoFactor { get; set; }
        public string iva { get; set; }//se refiere el porcentaje

   //     Cantidad = 1, Unidad = "Servicios", ClaveProdServ = "84121500", ClaveUnidad = "E48", NombreProducto = "INTERES ORDINARIO", impImporte = impIvaOrd.ToString(), Total = valorInteres, PrecioUnitario = valorInteres, impBase = valorInteres.ToString(), impImpuesto = "002", impTipoFactor = "Tasa", impTasaOCuota = iva.ToString()
    }
}
