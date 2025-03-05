using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensacionT112_API_Cacao.Entidades
{
    public class RegistroACompensar
    {
        //public int idProcesoT112ApiCacao { get; set; } //
        public Int64 IdFicheroDetalle { get; set; } //
        public Int64 IdFichero { get; set; } //
        public String NombreFichero { get; set; }
        //public bool EsFicheroDuplicado { get; set; } //
        public String NumAutorizacion { get; set; } //
        public String NumTarjeta { get; set; } //
        public decimal ImporteCompensadoEnPesos { get; set; }//
        public decimal ImporteCompensadoEnDolares { get; set; } //
        public decimal ImporteCompensadoLocal { get; set; } //
        public string CodigoMonedaLocal { get; set; } //
        public decimal CuotaIntercambio { get; set; } //
        //public decimal IVA { get; set; } //
        public String T112_CodigoTx { get; set; } //
        public String T112_Comercio { get; set; } //
        public String T112_Ciudad { get; set; } //
        public String T112_Pais { get; set; } //
        public String T112_MCC { get; set; } //
        public String T112_Moneda1 { get; set; } //
        public String C040 { get; set; } //
        public String T112_Moneda2 { get; set; } //
        public String T112_Referencia { get; set; }//
        public String T112_FechaProc { get; set; } //
        public String T112_FechaJuliana { get; set; }
        public String T112_FechaConsumo { get; set; }
        public String T112_FechaPresentacion { get; set; }
        public String T112_Ciclo { get; set; }
    }
}
