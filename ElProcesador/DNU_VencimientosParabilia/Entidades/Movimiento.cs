using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_VencimientosParabilia.Entidades
{
    public class Movimiento
    {

        public int IdEvento { get; set; }
        public long Id_Operacion { get; set; }
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
        public string ID_EstatusOperacion { get; set; }
        public string ID_EstatusPostOperacion { get; set; }
    }

    public static class StringExtensions
    {
        public static string ToMaskedField(this String field)
        {
            if (String.IsNullOrEmpty(field))
            {
                return field;
            }

            var prestep = String.Join(string.Empty, field.Select(s => '*').ToList());

            if (field.Length >= 16)
            {
                return String.Format("{0}{1}{2}", field.Substring(0,3), prestep.Substring(3, prestep.Length-7), field.Substring(field.Length - 4));
            }


            return String.Format("{0}{1}", prestep.Substring(0, prestep.Length - 4), field.Substring(field.Length - 4));
        }

    }




}
