using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RespuestaProceso
    {
        public Int32 CodigoRespuesta { get; set; } 
        public String Descripcion { get; set; }

        public Int64 ID_Fichero { get; set; }

        public Int32 OperacionesSinErrores { get; set; }

        public Int32 OperacionesConErrores { get; set; }

    }
}
