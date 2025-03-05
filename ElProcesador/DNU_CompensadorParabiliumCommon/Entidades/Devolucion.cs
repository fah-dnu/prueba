using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Devolucion
    {
        public long ID_Devolucion { get; set; }
        public string tipo { get; set; }
        public string claveMa { get; set; }
        public string referencia { get; set; }
        public decimal importe { get; set; }
        public string moneda { get; set; }
        public string autorizacion { get; set; }
        public long ID_FicheroDetalle { get; set; }
        public long ID_Operacion { get; set; }
        public int? Estatus { get; set; }
    }

    public class DevolucionModel : Devolucion
    {
        public int ID_EstatusOperacion { get; set; }
        public int ID_EstatusPostOperacion { get; set; }
        public long ID_Poliza { get; set; }
        public long ID_Evento { get; set; }
        public string ClaveEvento { get; set; }
        public string ClaveColectiva { get; set; }
        public string FechaOperacion { get; set; }
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
                return String.Format("{0}{1}{2}", field.Substring(0, 3), prestep.Substring(3, prestep.Length - 7), field.Substring(field.Length - 4));
            }


            return String.Format("{0}{1}", prestep.Substring(0, prestep.Length - 4), field.Substring(field.Length - 4));
        }

    }
}
