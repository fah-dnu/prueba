using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Movimiento
    {
        public Int64 IDFichero { get; set; }
        public string Ruta { get; set; }

        public Int64 IDFicheroDetalle { get; set; }
        public int IdEvento { get; set; }
        public string ClaveEvento { get; set; }
        public string ClaveMA { get; set; }
        public string TipoMA { get; set; }
        public string MonedaOriginal { get; set; }
        public long IdColectiva { get; set; }
        public int IdTipoColectiva { get; set; }
        public string ClaveColectiva { get; set; }
        public string Importe { get; set; }
        public string ImporteMonedaOriginal { get; set; }
        public string Concepto { get; set; }
        public string Observaciones { get; set; }
        public string Autorizacion { get; set; }
        public string ReferenciaNumerica { get; set; }
        public string FechaOperacion { get; set; }
        public string Ticket { get; set; }
        public string T112_ImporteCompensadoPesos { get; set; }
        public string T112_ImporteCompensadoDolar { get; set; }
        public string T112_ImporteCompensadoLocal { get; set; }
        public string T112_CodigoMonedaLocal { get; set; }
        public string T112_CuotaIntercambio { get; set; }
        public string T112_IVA { get; set; }
        public string T112_NombreArchivo { get; set; }
        public string T112_FechaPresentacion { get; set; }
        //Nuevos Campos
        public string T112_CodigoTx { get; set; }
        public string T112_Comercio { get; set; }
        public string T112_Ciudad { get; set; }
        public string T112_Pais { get; set; }
        public string T112_MCC { get; set; }
        public string T112_Referencia { get; set; }
        public string T112_FechaProc { get; set; }
        public string T112_FechaJuliana { get; set; }
        public string T112_FechaConsumo { get; set; }
        public string T112_Ciclo { get; set; }
        public string GrupoCuenta { get; set; }

        //Nuevos Campos Registro compensacion
        public string CodigoFuncion { get; set; }
        public string Reverso { get; set; }
        public string Bin { get; set; }
        public string ImporteOrigen { get; set; }
        public string ImporteDestino { get; set; }
        public string MonedaCuotaIntercambio { get; set; }
        public string MonedaIva { get; set; }
        public string ImporteMCCR { get; set; }
        public string ImporteCompensacion { get; set; }
        public string CodigoProceso { get; set; }
        public string Representacion { get; set; }
        public string MonedaDestino { get; set; }
        /// <summary>
        /// Propiedad que obtiene el valor de C045 dividio entre 100.
        /// </summary>
        public string T112_Imp_45 { get; set; }
        /// <summary>
        /// Propiedad que obtiene el valor de C077 dividido ente 100.
        /// </summary>
        public string T112_Imp_77 { get; set; }

        /// <summary>
        /// Propiedad que obtiene la moneda de la cuenta asociada a la tarjeta
        /// </summary>
        public string MonedaCuenta { get; set; }

        /// <summary>
        ///´Propiedad que obtiene una segunda referencia
        /// </summary>
        public string Referencia2 { get; set; }

        //Param for generate clavePlugin in Dashboard report
        public string SufijoArchivo { get; set; }
    }
}
