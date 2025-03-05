using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.Entidades.Request
{
    public class RequestConsultarTarjetaOnb
    {
        public string Folio { get; set; }
        public string IDUsuario { get; set; }
        public string IDSolicitud { get; set; }
    }
}
