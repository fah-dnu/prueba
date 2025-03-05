using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    public class ParametrosSFTP
    {
        public string rutaOriginal { get; set; }
        public string rutaNuevoArchivo { get; set; }
        public string rutaCert { get; set; }
        public string rutaKey { get; set; }
        public string passKey { get; set; }
        public string cliente { get; set; }

    }
}
