using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos.Entidades
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
            public string Registro { get; set; }
         
        }

        public string Url { get; set; }
    }
}
