using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoAnualidad.Entidades
{
    public class Cuentas
    {
        public Int64 ID_Corte { get; set; }
        public Int64 ID_Cuenta { get; set; }
        public Int64 id_CadenaComercial { get; set; }
        public Int64 ID_CuentaHabiente { get; set; }
        public Int32 ID_TipoColectiva { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public string ClaveEventoAgrupador { get; set; }
        public string ClaveCorteTipo { get; set; }
        public string Tarjeta { get; set; }
        public string ClaveCliente { get; set; }
        public string ClaveCuentahabiente { get; set; }
        public string NombreCuentahabiente { get; set; }
        public string CorreoCuentahabiente { get; set; }
        public string RFCCuentahabiente { get; set; }
        public DateTime FechaCorteAnterior { get; set; }
        public string RFCCliente { get; set; }
        public string NombreORazonSocial { get; set; }
        public Int64 id_colectivaCliente { get; set; }
        //public string CP { get; set; }
        //public string Subproducto { get; set; }
        public string CumpleAnio { get; set; }
    }
}
