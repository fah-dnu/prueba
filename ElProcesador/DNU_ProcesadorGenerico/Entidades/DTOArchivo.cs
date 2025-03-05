using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorGenerico.Entidades
{
    public class DTOArchivo
    {
        public DTOArchivo() 
        {
            Filas = new List<DTOFila>();
        }
        public DateTime ENC_Fecha { get; set; }
        public List<DTOFila> Filas { get; set; }
        public int PIE_NumeroRegistros { get; set; }
        public decimal PIE_ImporteTotal { get; set; }

        public class DTOFila 
        {
            public int Contador { get; set; }
            public string MedioPago { get; set; }
            public string TipoMedioPago { get; set; }
            public string ClaveConcepto { get; set; }
            public decimal Importe { get; set; }
            public string Ticket { get; set; }
            public string CodigoRespuesta { get; set; }
            public string NumeroAutorizacion { get; set; }
        }

        public string Url { get; set; }
    }
}
