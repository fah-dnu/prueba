using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RespuestaValidacionOperacionReversada
    {
        public string CodigoRespuesta { get; set; }
        public Int64 IdOperacion { get; set; }
        public int IdEstatusOperacion { get; set; }
        public int IdEstatusPostOperacion { get; set; }
    }
}
