using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class Correo
    {
        public string correoEmisor { get; set; }
        public string correoReceptor { get; set; }
        public List<String> receptoresCopia { get; set; }
        public string asunto { get; set; }
        public string mensaje { get; set; }
        public List<string> archivos = new List<string>(); //lista de archivos a enviar
        public string titulo{ get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public string cuerpoMensaje { get; set; }
        public string puerto { get; set; }
        public string host { get; set; }

    }
}
