using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorIMILE.Entidades.Response
{

    public class ResponseLogIn
    {
        public string UserID { get; set; }
        public string UserTemp { get; set; }
        public string NombreUsuario { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Token { get; set; }
        public string CodRespuesta { get; set; }
        public string DescRespuesta { get; set; }
    }
}
