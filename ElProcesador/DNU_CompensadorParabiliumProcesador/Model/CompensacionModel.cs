using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumProcesador.Model
{
    public class CompensacionModel
    {
        public decimal T112_ImporteCompensacionPesos { get; set; }
        public decimal T112_ImporteCompensacionDolares { get; set; }
        public decimal T112_ImporteCompensacionLocal { get; set; }
        public string T112_CodigoMonedaLocal { get; set; }
        public decimal T112_CuotaIntercambio { get; set; }
        public decimal T112_IVA { get; set; }
        public string T112_FechaPresentacion { get; set; }
        public string T112_NombreArchivo { get; set; }
        public string T112_CodigoTx { get; set; }
        public string T112_Comercio { get; set; }
        public string T112_Ciudad { get; set; }
        public string T112_Pais { get; set; }
        public string T112_MCC { get; set; }
        public string T112_Moneda1 { get; set; }
        public string T112_Moneda2 { get; set; }
        public string T112_Referencia { get; set; }
        public string T112_FechaProc { get; set; }
        public string T112_FechaJuliana { get; set; }
        public string T112_FechaConsumo { get; set; }
        public string T112_Ciclo { get; set; }

    }
}
