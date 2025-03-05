using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.Entidades.Response
{
    public class ResponseProceso
    {
        public Int32 CodRespuesta { get; set; }
        public String DescRespuesta { get; set; }
        public Int64 ID_Fichero { get; set; }
        public Int32 OperacionesSinErrores { get; set; }
        public Int32 OperacionesConErrores { get; set; }
    }
}
