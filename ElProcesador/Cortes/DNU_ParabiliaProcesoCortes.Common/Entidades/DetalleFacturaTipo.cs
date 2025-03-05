using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Entidades
{
    public class DetalleFacturaTipo
    {
        public Int64 ID_DetalleFacturaTipo { get; set; }
        public Int64 ID_Colectiva { get; set; }
        public Int64 ID_CadenaComercial { get; set; }
        public String CadenaComercial { get; set; }
        public Int32 ID_Evento { get; set; }
        public Int32 ID_Producto { get; set; }
        public String FormulaTotal { get; set; }
        public String FormulaCantidad { get; set; }
        public String FormulaPrecioUnitario { get; set; }
        public Int32 ID_FacturaTipo { get; set; }
        public Int64 ID_TipoCuenta { get; set; }
        public Decimal PrecioUnitario { get; set; }
        public Decimal Total { get; set; }
        public String NombreProducto { get; set; }
        public Decimal IvaProducto { get; set; }
        public String SKU { get; set; }
        public String Unidad { get; set; }
        public String ClaveProdServ { get; set; }
        public String ClaveUnidad { get; set; }
        public String ClaveImpuesto { get; set; }
        public String TipoFactor { get; set; }
        public String TasaOCuota { get; set; }

    }
}
